# Migration: Password Hashing từ SHA256 sang PBKDF2

## Tổng quan
Dự án đã được nâng cấp để sử dụng `Microsoft.AspNet.Identity.PasswordHasher` (PBKDF2) thay vì băm mật khẩu thủ công bằng SHA256 + Salt.

## Thay đổi chính

### 1. PasswordHelper.cs
- ✅ Sử dụng `PasswordHasher` từ Microsoft.Identity
- ✅ Hash mới không cần trường `Salt` riêng (salt tích hợp trong hash)
- ✅ Hỗ trợ tương thích ngược với hash cũ thông qua `VerifyPasswordV2()`
- ✅ Auto-upgrade password sang format mới khi user đăng nhập

### 2. Files đã cập nhật
**Controllers:**
- `AccountController.cs` - Đăng nhập công khai
- `Admin/AccountsController.cs` - CRUD tài khoản admin
- `Admin/AuthController.cs` - Đăng nhập/đăng ký admin
- `Admin/RecruitersController.cs` - CRUD nhà tuyển dụng

**Services:**
- `AccountService.cs` - Register, Authenticate, ResetPassword
- `MyAccountService.cs` - ChangePassword, VerifyPassword

### 3. Database Schema
- Cột `Salt` trong bảng `Accounts` đã được định nghĩa là **NULLABLE**
- ✅ Tài khoản mới: `Salt = NULL`, hash lưu theo PBKDF2
- ✅ Tài khoản cũ: `Salt != NULL`, hash theo SHA256 (tương thích)

## Workflow Migration

### Giai đoạn 1: Code Deployment (✅ Hoàn thành)
```
1. Code mới đã set Salt = null cho tất cả accounts mới
2. Login flow tự động rehash password cũ sang format mới
3. Không ảnh hưởng đến users hiện tại
```

### Giai đoạn 2: Database Verification (Bước tiếp theo)
Chạy script migration để:
1. Verify cột `Salt` có nullable
2. Kiểm tra tiến độ migration
3. Xem số lượng accounts còn format cũ

```sql
-- Chạy script này:
sqlcmd -S <server> -d JOBPORTAL_EN -i Database\Migration_PasswordHelper_PBKDF2_20251124.sql
```

Hoặc mở SQL Server Management Studio và chạy file:
```
Database\Migration_PasswordHelper_PBKDF2_20251124.sql
```

### Giai đoạn 3: Monitoring (30-90 ngày)
- Theo dõi số lượng accounts chưa migrate
- Mỗi lần user đăng nhập, password tự động upgrade
- Không cần thao tác thủ công

**Query kiểm tra:**
```sql
-- Xem tỷ lệ migration
SELECT 
    CASE 
        WHEN Salt IS NULL THEN 'New (PBKDF2)'
        ELSE 'Legacy (SHA256)'
    END AS HashType,
    COUNT(*) AS Count,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM Accounts) AS DECIMAL(5,2)) AS Percentage
FROM dbo.Accounts
GROUP BY CASE WHEN Salt IS NULL THEN 'New (PBKDF2)' ELSE 'Legacy (SHA256)' END;
```

### Giai đoạn 4: Cleanup (Tùy chọn - sau 90 ngày)
Khi **tất cả** accounts đã migrate (Salt = NULL):
1. Backup database
2. Drop cột `Salt`:
```sql
ALTER TABLE dbo.Accounts DROP COLUMN Salt;
```

⚠️ **Chỉ thực hiện khi:**
- Không còn account nào có `Salt != NULL`
- Đã backup đầy đủ
- Chấp nhận mất khả năng khôi phục password cũ

## Kiểm tra nhanh

### Xác nhận code hoạt động đúng:
1. Tạo tài khoản mới → Kiểm tra DB: `Salt` phải là `NULL`
2. Đăng nhập bằng tài khoản cũ → Sau login, `Salt` sẽ chuyển thành `NULL`
3. Đổi password → `Salt` phải là `NULL`

### Test cases:
- ✅ Register tài khoản mới
- ✅ Login với tài khoản cũ (SHA256)
- ✅ Login với tài khoản mới (PBKDF2)
- ✅ Forgot Password / Reset Password
- ✅ Change Password trong MyAccount
- ✅ Admin tạo account mới
- ✅ Admin reset password user

## Rollback Plan
Nếu có vấn đề, khôi phục bằng cách:
1. Revert code về commit trước
2. Restore database từ backup
3. Accounts đã upgrade vẫn hoạt động (hash tương thích ngược)

## FAQs

**Q: Tại sao không xóa cột Salt ngay?**  
A: Để hỗ trợ tài khoản cũ chưa đăng nhập. Chỉ xóa sau khi tất cả accounts đã migrate.

**Q: Password cũ có còn hoạt động không?**  
A: Có, `VerifyPasswordV2()` vẫn verify được password cũ và tự động upgrade.

**Q: Có cần reset password tất cả users không?**  
A: Không bắt buộc. Password tự động upgrade khi user đăng nhập lần kế.

**Q: Nếu user không đăng nhập lâu thì sao?**  
A: Giữ cột `Salt` trong DB. Sau 90 ngày, có thể gửi email yêu cầu reset password.

## Liên hệ
Nếu có vấn đề, kiểm tra file log hoặc debug tại:
- `PasswordHelper.VerifyPasswordV2()` 
- `AccountController.Login()`
- `AccountService.Authenticate()`

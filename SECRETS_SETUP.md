# 🔐 Hướng Dẫn Cấu Hình Secrets Local

## ⚠️ QUAN TRỌNG
File `Web.config` trong repository chỉ chứa placeholders. Bạn cần cấu hình secrets riêng cho môi trường local của mình.

## 📋 Cách Thiết Lập

### Phương án 1: Sử dụng Web.config.local (Khuyến nghị)
1. Copy file `Project_Recruiment_Huce/Web.config.local.example` thành `Web.config.local`
2. Điền các giá trị thực vào file `Web.config.local`
3. File này đã được thêm vào `.gitignore` và sẽ không bị push lên GitHub

### Phương án 2: Chỉnh sửa Web.config trực tiếp
1. Mở `Project_Recruiment_Huce/Web.config`
2. Thay thế các placeholders bằng giá trị thực
3. **KHÔNG ĐƯỢC commit và push file này lên GitHub**

## 🔑 Credentials Cần Thiết

### 1. Google OAuth (Đăng nhập Google)
```xml
<add key="GoogleClientId" value="YOUR_GOOGLE_CLIENT_ID_HERE" />
<add key="GoogleClientSecret" value="YOUR_GOOGLE_CLIENT_SECRET_HERE" />
```

**Cách lấy:**
1. Truy cập https://console.cloud.google.com/apis/credentials
2. Tạo OAuth 2.0 Client ID
3. Thêm Authorized redirect URIs:
   - `http://localhost:44300/signin-google`
   - `https://yourdomain.com/signin-google` (production)

### 2. Email Configuration (Gửi Email Phỏng Vấn)
```xml
<add key="SenderEmail" value="YOUR_EMAIL@gmail.com" />
<add key="Password" value="YOUR_GMAIL_APP_PASSWORD" />
```

**Cách lấy Gmail App Password:**
1. Truy cập https://myaccount.google.com/apppasswords
2. Tạo App Password mới
3. Copy password (16 ký tự) và paste vào config

⚠️ **Lưu ý:** Phải bật 2-Factor Authentication trước khi tạo App Password

### 3. VietQR API (Tạo QR Code Thanh Toán)
```xml
<add key="VietQR:ClientId" value="YOUR_VIETQR_CLIENT_ID" />
<add key="VietQR:ApiKey" value="YOUR_VIETQR_API_KEY" />
```

**Cách lấy:**
1. Đăng ký tài khoản tại https://my.vietqr.io/
2. Lấy Client ID và API Key từ Dashboard

### 4. Payment Information (Thông Tin Thanh Toán)
```xml
<add key="Payment:AccountNumber" value="YOUR_ACCOUNT_NUMBER" />
<add key="Payment:AccountName" value="YOUR_ACCOUNT_NAME" />
```

Điền thông tin tài khoản ngân hàng để nhận thanh toán.

### 5. Database Connection
```xml
<connectionStrings>
  <add name="JOBPORTAL_ENConnectionString" 
       connectionString="Data Source=YOUR_SERVER;Initial Catalog=JOBPORTAL_EN;..." />
</connectionStrings>
```

Cập nhật `Data Source` và `Password` theo SQL Server của bạn.

## 🚫 .gitignore

File `.gitignore` đã được cấu hình để bảo vệ:
- `Web.config.local` - Chứa secrets thực
- Các file backup của Web.config

## 🔄 Khi Deploy Production

1. **KHÔNG BAO GIỜ** commit secrets thực vào Git
2. Sử dụng biến môi trường hoặc Azure Key Vault
3. Cấu hình secrets trực tiếp trên server production
4. Sử dụng Web.config transforms (Web.Release.config)

## 📞 Hỗ Trợ

Nếu gặp vấn đề với cấu hình:
1. Kiểm tra tất cả placeholders đã được thay thế
2. Verify Google OAuth redirect URIs
3. Kiểm tra Gmail App Password còn hiệu lực
4. Kiểm tra SQL Server connection string

## ✅ Checklist Trước Khi Chạy

- [ ] Đã copy và điền Web.config.local
- [ ] Google OAuth credentials đã cấu hình
- [ ] Gmail App Password đã tạo và điền
- [ ] Database connection string đã cập nhật
- [ ] SQL Server đang chạy
- [ ] Đã chạy database migrations

---
**Lưu ý:** File này có thể commit lên GitHub vì không chứa secrets thực.

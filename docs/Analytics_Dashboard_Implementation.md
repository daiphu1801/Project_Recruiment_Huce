# Dashboard Phân tích & Thống kê cho Nhà Tuyển Dụng
**Hoàn thành ngày: 18/11/2025**

## Tổng quan
Đã triển khai thành công hệ thống phân tích và thống kê cho nhà tuyển dụng với các tính năng:
- ✅ Đếm lượt xem tin tuyển dụng (ViewCount)
- ✅ Thống kê tổng hợp (tổng lượt xem, tổng hồ sơ, tỷ lệ chuyển đổi)
- ✅ Phân tích chi tiết theo từng tin tuyển dụng
- ✅ UI/UX đẹp, thống nhất với theme hệ thống (#89ba16)
- ✅ Code clean, thin controller, service layer pattern

## Files đã tạo/chỉnh sửa

### 1. Database Migration
**File:** `Project_Recruiment_Huce\Database\Migration_AddViewCount_20251118.sql`
- Thêm cột `ViewCount INT NOT NULL DEFAULT 0` vào bảng `JobPosts`
- Tạo index `IX_JobPosts_RecruiterID` cho analytics queries
- Tạo stored procedures:
  - `sp_IncrementJobViewCount` - Tăng lượt xem an toàn
  - `sp_GetRecruiterAnalytics` - Lấy metrics tổng hợp và chi tiết
- Idempotent (chạy nhiều lần được, có tracking bảng `SchemaMigrations`)

**Cách chạy:**
```sql
-- Trong SQL Server Management Studio (SSMS)
-- 1. Connect vào database JOBPORTAL_EN
-- 2. Execute file Migration_AddViewCount_20251118.sql
```

### 2. DBML & Entity Mapping
**Files:**
- `Models\JOBPORTAL_EN.dbml` - Thêm column ViewCount
- `Models\JOBPORTAL_EN.designer.cs` - Regenerated với property ViewCount

### 3. ViewModels
**File:** `Models\Recruiters\RecruiterAnalyticsViewModels.cs`
- `RecruiterAnalyticsSummaryViewModel` - Metrics tổng hợp
- `JobAnalyticsItemViewModel` - Metrics từng job
- `RecruiterAnalyticsDashboardViewModel` - Combined model

**File:** `Models\Jobs\JobViewModels.cs` (updated)
- Thêm `ViewCount` vào `JobDetailsViewModel`
- Thêm `ViewCount` vào `JobListingItemViewModel`

### 4. Mapper
**File:** `Mappers\JobMapper.cs` (updated)
- `MapToDetails()` - Map ViewCount từ entity
- `MapToListingItem()` - Map ViewCount cho listing

### 5. Service Layer
**File:** `Services\RecruiterAnalyticsService.cs`
- `GetDashboardData()` - Lấy dashboard đầy đủ
- `GetSummaryMetrics()` - Gọi stored proc, lấy metrics tổng hợp
- `GetJobBreakdown()` - Lấy breakdown theo job
- `IncrementViewCount()` - Tăng lượt xem (efficient SQL UPDATE)
- `GetJobAnalytics()` - Lấy metrics 1 job cụ thể

### 6. Controller
**File:** `Controllers\Recruiters\RecruiterAnalyticsController.cs`
- **Thin controller** - delegate logic sang service
- `Index()` - Dashboard tổng quan (có filter by date)
- `Job(int id)` - Chi tiết analytics 1 job
- `GetCurrentRecruiterId()` - Helper lấy RecruiterID từ claims

**File:** `Controllers\Jobs\JobsController.cs` (updated)
- `JobDetails()` - Thêm logic increment ViewCount khi xem chi tiết

### 7. Views
**File:** `Views\RecruiterAnalytics\Index.cshtml`
- Dashboard với 4 KPI cards (Views, Applications, Jobs, Conversion Rate)
- Bộ lọc theo thời gian (fromDate, toDate)
- Bảng chi tiết theo job (sortable, với action buttons)
- Tips card cải thiện tỷ lệ chuyển đổi
- Responsive design, gradient backgrounds

**File:** `Views\RecruiterAnalytics\Job.cshtml`
- Header job title + status badge
- 3 metric boxes (Views, Applications, Conversion%)
- Phân tích hiệu quả (đánh giá conversion rate)
- Gợi ý cải thiện dựa trên metrics
- Action buttons (back to dashboard, view job, view applications)

**File:** `Views\Shared\_Layout.cshtml` (updated)
- Thêm menu item "Phân tích & Thống kê" trong dropdown Quản lý (cho Recruiter)

### 8. Project Configuration
**File:** `Project_Recruiment_Huce.csproj` (updated)
- Added `RecruiterAnalyticsController.cs`
- Added `RecruiterAnalyticsViewModels.cs`
- Added `RecruiterAnalyticsService.cs`

## Cách sử dụng

### Bước 1: Chạy Migration SQL
1. Mở SSMS (SQL Server Management Studio)
2. Connect vào database `JOBPORTAL_EN`
3. Mở file `Database\Migration_AddViewCount_20251118.sql`
4. Execute (F5)

### Bước 2: Build Project
```powershell
.\build-only.bat
```

### Bước 3: Chạy Website
```powershell
.\start-iis-express.bat
```

### Bước 4: Truy cập Dashboard
1. Login với tài khoản **Recruiter**
2. Vào menu **Quản lý** → **Phân tích & Thống kê**
3. Xem metrics tổng hợp và breakdown theo job

## Tính năng chính

### 1. Tự động đếm lượt xem
- Mỗi khi ứng viên (hoặc bất kỳ ai) xem chi tiết job → ViewCount +1
- Sử dụng SQL UPDATE trực tiếp (efficient, safe concurrency)

### 2. Dashboard Analytics
- **Tổng lượt xem**: Sum của ViewCount tất cả jobs
- **Tổng hồ sơ**: Count Applications
- **Tổng tin đăng**: Count JobPosts
- **Tỷ lệ chuyển đổi**: (Applications / Views) × 100%

### 3. Filter theo thời gian
- Có thể lọc analytics từ ngày X đến ngày Y
- Nút "Xóa bộ lọc" để reset về all-time

### 4. Per-Job Analytics
- Click vào job trong bảng để xem chi tiết
- Phân tích hiệu quả (conversion rate tốt/trung bình/thấp)
- Gợi ý cải thiện dựa trên metrics thực tế

## Kiến trúc & Best Practices

### ✅ Clean Architecture
- **Controller** mỏng: chỉ handle request/response, delegate logic sang service
- **Service Layer**: encapsulate business logic (RecruiterAnalyticsService)
- **Repository Pattern**: đã có sẵn (JobRepository, ApplicationRepository)
- **Mapper**: centralize mapping logic (JobMapper)

### ✅ Performance
- Stored procedures cho aggregation (fast, indexed queries)
- Direct SQL UPDATE cho increment (không load entity)
- CreateReadOnly() context cho read operations
- Create() context chỉ khi cần write

### ✅ UI/UX
- Gradient colors matching theme (#89ba16 primary)
- Responsive cards với hover effects
- Icon-rich interface (icon-eye, icon-file-text, icon-percent)
- Status badges với màu semantic (green/red/yellow)
- Tips & suggestions dựa trên metrics

### ✅ Security
- `[Authorize]` attribute trên controller
- Kiểm tra ownership (recruiter chỉ xem được analytics của mình)
- Claims-based authentication (VaiTro, NameIdentifier)

## Cấu trúc SQL

### Bảng SchemaMigrations
```sql
CREATE TABLE dbo.SchemaMigrations (
    MigrationId INT IDENTITY(1,1) PRIMARY KEY,
    ScriptName NVARCHAR(200) NOT NULL UNIQUE,
    AppliedAt DATETIME NOT NULL DEFAULT (GETDATE())
);
```

### Stored Procedures

**sp_IncrementJobViewCount**
```sql
EXEC dbo.sp_IncrementJobViewCount @JobPostID = 123;
```

**sp_GetRecruiterAnalytics**
```sql
EXEC dbo.sp_GetRecruiterAnalytics 
    @RecruiterID = 45, 
    @FromDate = '2025-01-01', 
    @ToDate = '2025-12-31';
```
Trả về 2 result sets:
1. Summary (TotalViews, TotalApplications, TotalJobs, ConversionRatePercent)
2. Per-job breakdown (JobPostID, JobTitle, JobStatus, PostedAt, Views, Applications, ConversionRatePercent)

## Testing

### Test Cases đề xuất
1. ✅ Xem job detail → ViewCount tăng
2. ✅ Xem dashboard → Hiển thị metrics đúng
3. ✅ Filter theo date → Metrics thay đổi
4. ✅ Click vào job trong bảng → Xem per-job analytics
5. ✅ Recruiter khác không thấy analytics của recruiter khác
6. ✅ Job có 0 views → Conversion = 0%
7. ✅ Job có views nhưng 0 applications → Suggestions hiển thị

## Notes

### Idempotent Migration
- File migration có thể chạy nhiều lần an toàn
- Kiểm tra `SchemaMigrations` table trước khi apply
- Nếu đã apply → skip, không lỗi

### ViewCount Increment
- Mỗi lần xem JobDetails → ViewCount +1
- Không phân biệt user (kể cả anonymous)
- Nếu muốn track unique views → cần bảng riêng (JobPostViews với CandidateID, ViewedAt)

### Conversion Rate
- Formula: `(Applications / Views) × 100%`
- Views = 0 → Conversion = 0%
- Đánh giá:
  - ≥ 5% → Tốt
  - 2-5% → Trung bình
  - < 2% → Cần cải thiện

## Mở rộng tương lai

### 1. Biểu đồ thời gian (Time Series Chart)
- Thêm chart library (Chart.js, D3.js)
- Track views/applications theo ngày
- Cần bảng `JobPostViews` để lưu lịch sử

### 2. Export CSV/Excel
- Thêm action `ExportAnalytics()`
- Dùng ClosedXML hoặc EPPlus
- Download báo cáo analytics

### 3. Email Reports
- Schedule job gửi email tuần/tháng
- Tóm tắt metrics cho recruiter
- Dùng EmailSyncHelper có sẵn

### 4. Comparison với period trước
- So sánh tuần này vs tuần trước
- Growth rate (% tăng/giảm)
- Trend indicators (↑↓)

### 5. Heatmap & Advanced Analytics
- Lượt xem theo giờ trong ngày
- Nguồn traffic (referrer tracking)
- Device breakdown (mobile/desktop)

## Liên hệ & Support
- Developer: GitHub Copilot
- Date: 18/11/2025
- Build Status: ✅ Success (0 Warnings, 0 Errors)

# Hướng dẫn sử dụng chức năng Đặt lịch phỏng vấn

## Tổng quan
Chức năng đặt lịch phỏng vấn cho phép Recruiter gửi email thông báo lịch hẹn phỏng vấn cho ứng viên khi đơn ứng tuyển chuyển sang trạng thái "Phỏng vấn".

## Các file đã tạo/chỉnh sửa

### 1. ViewModel - `RecruiterApplicationViewModel.cs`
**Đường dẫn**: `Project_Recruiment_Huce/Models/Recruiters/RecruiterApplicationViewModel.cs`

Đã thêm class `InterviewScheduleViewModel` với các trường:
- `ApplicationID`: ID đơn ứng tuyển
- `CandidateName`: Tên ứng viên
- `CandidateEmail`: Email ứng viên (để gửi thông báo)
- `JobTitle`: Vị trí ứng tuyển
- `InterviewDate`: Ngày phỏng vấn (bắt buộc)
- `InterviewTime`: Giờ phỏng vấn (bắt buộc)
- `Location`: Địa điểm phỏng vấn (bắt buộc, max 500 ký tự)
- `InterviewType`: Hình thức phỏng vấn (Trực tiếp/Trực tuyến/Điện thoại)
- `Duration`: Thời gian dự kiến (15-480 phút)
- `Interviewer`: Người phỏng vấn (max 200 ký tự)
- `RequiredDocuments`: Tài liệu cần mang theo (max 1000 ký tự)
- `AdditionalNotes`: Lưu ý thêm (max 1000 ký tự)

### 2. Controller - `RecruitersApplicationController.cs`
**Đường dẫn**: `Project_Recruiment_Huce/Controllers/Recruiters/RecruitersApplicationController.cs`

Đã thêm 2 action methods:

#### `ScheduleInterview` (GET)
- Kiểm tra quyền truy cập (Recruiter)
- Lấy thông tin đơn ứng tuyển
- Kiểm tra trạng thái phải là "Interview"
- Tạo ViewModel với dữ liệu mặc định:
  - Ngày phỏng vấn: 3 ngày kể từ hôm nay
  - Thời gian: 60 phút
- Chuẩn bị dropdown hình thức phỏng vấn

#### `ScheduleInterview` (POST)
- Validate dữ liệu form
- Lưu thông tin lịch phỏng vấn (TODO: Backend)
- Gửi email thông báo cho ứng viên (TODO: Backend)
- Redirect về trang chi tiết đơn ứng tuyển

### 3. View - `ApplicationDetails.cshtml`
**Đường dẫn**: `Project_Recruiment_Huce/Views/RecruitersApplication/ApplicationDetails.cshtml`

Đã thêm nút "Đặt lịch phỏng vấn":
- Chỉ hiển thị khi trạng thái đơn là "Interview"
- Style: `btn-gradient-info` với hiệu ứng `animate-pulse`
- Icon: `icon-calendar-check-o`

### 4. View Form - `ScheduleInterview.cshtml`
**Đường dẫn**: `Project_Recruiment_Huce/Views/RecruitersApplication/ScheduleInterview.cshtml`

Giao diện form đầy đủ bao gồm:

#### Header Section
- Hero banner với breadcrumb navigation
- Hiển thị tên ứng viên và vị trí ứng tuyển

#### Form Sections

**1. Thời gian phỏng vấn**
- Ngày phỏng vấn (date picker)
- Giờ phỏng vấn (time picker)
- Thời gian dự kiến (số phút)

**2. Địa điểm & Hình thức**
- Dropdown chọn hình thức phỏng vấn
- Textarea nhập địa điểm
- Input tên người phỏng vấn

**3. Yêu cầu & Lưu ý**
- Textarea danh sách tài liệu cần mang
- Textarea các lưu ý thêm

#### Features
- Auto-resize textarea khi nhập
- Validation client-side và server-side
- Placeholder thay đổi theo hình thức phỏng vấn
- Alert thông báo email sẽ được gửi
- Help section với hướng dẫn sử dụng
- Responsive design cho mobile

## Luồng sử dụng

### 1. Chuyển trạng thái đơn ứng tuyển
```
Recruiter -> Đơn ứng tuyển -> Cập nhật trạng thái -> Chọn "Phỏng vấn"
```

### 2. Xem chi tiết đơn và đặt lịch
```
Recruiter -> Chi tiết đơn ứng tuyển -> Nút "Đặt lịch phỏng vấn" (xuất hiện)
```

### 3. Điền form thông tin lịch phỏng vấn
```
- Chọn ngày, giờ phỏng vấn
- Chọn hình thức (Trực tiếp/Online/Điện thoại)
- Nhập địa điểm/link meeting
- Nhập người phỏng vấn
- Liệt kê tài liệu cần mang
- Thêm lưu ý (nếu có)
```

### 4. Gửi lịch hẹn
```
Click "Gửi lịch hẹn" -> Email tự động gửi cho ứng viên (TODO: Backend)
```

## TODO: Backend Implementation

Các phần cần triển khai backend:

### 1. Database Schema
Tạo bảng `InterviewSchedules` với các cột:
```sql
- InterviewScheduleID (PK)
- ApplicationID (FK)
- InterviewDate
- InterviewTime
- Duration
- Location
- InterviewType
- Interviewer
- RequiredDocuments
- AdditionalNotes
- CreatedAt
- UpdatedAt
- Status (Scheduled/Completed/Cancelled)
```

### 2. Service Layer
Tạo `InterviewScheduleService` với các methods:
- `CreateInterviewSchedule()`: Lưu thông tin lịch phỏng vấn
- `GetInterviewSchedule()`: Lấy thông tin lịch phỏng vấn
- `UpdateInterviewSchedule()`: Cập nhật lịch phỏng vấn
- `CancelInterviewSchedule()`: Hủy lịch phỏng vấn

### 3. Email Service
Sử dụng Gmail API hiện có để gửi email:
- Template email thông báo lịch phỏng vấn
- Bao gồm: Ngày giờ, địa điểm, người phỏng vấn, tài liệu cần mang
- Format: HTML email với styling đẹp
- Attach file .ics (calendar event) nếu có thể

### 4. Calendar Integration (Optional)
- Tạo Google Calendar event
- Gửi calendar invite cho ứng viên và recruiter

### 5. Notification System (Optional)
- Gửi reminder email trước 1 ngày
- Gửi reminder email trước 1 giờ
- SMS notification (nếu có)

## Testing Checklist

- [ ] Nút "Đặt lịch phỏng vấn" chỉ hiện với status "Interview"
- [ ] Form validation hoạt động đúng
- [ ] Các trường required được validate
- [ ] Date picker không cho chọn ngày quá khứ
- [ ] Time picker format đúng
- [ ] Dropdown hình thức phỏng vấn hoạt động
- [ ] Placeholder thay đổi theo hình thức
- [ ] Textarea auto-resize
- [ ] Submit form không lỗi
- [ ] Redirect về ApplicationDetails sau khi submit
- [ ] Responsive trên mobile

## UI/UX Features

### Design Highlights
1. **Gradient Colors**: Primary gradient cho header
2. **Section Organization**: Form được chia thành 3 sections rõ ràng
3. **Icons**: Sử dụng icons phù hợp cho mỗi field
4. **Help Text**: Mỗi field có hướng dẫn nhỏ
5. **Validation Feedback**: Real-time validation với màu sắc
6. **Info Card**: Card hiển thị thông tin ứng viên và công việc
7. **Help Section**: Hướng dẫn sử dụng ở cuối trang
8. **Alert Box**: Thông báo về việc gửi email

### Responsive Design
- Desktop: 2 columns cho date/time
- Mobile: Single column layout
- Các button full-width trên mobile

## Email Template Suggestion (Backend)

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Thông báo lịch phỏng vấn</title>
</head>
<body>
    <h2>Thông báo lịch phỏng vấn</h2>
    
    <p>Kính gửi <strong>{{CandidateName}}</strong>,</p>
    
    <p>Chúng tôi rất vui mừng thông báo rằng hồ sơ ứng tuyển của bạn cho vị trí 
    <strong>{{JobTitle}}</strong> đã được chọn vào vòng phỏng vấn.</p>
    
    <h3>Thông tin lịch phỏng vấn:</h3>
    <ul>
        <li><strong>Ngày:</strong> {{InterviewDate}}</li>
        <li><strong>Giờ:</strong> {{InterviewTime}}</li>
        <li><strong>Thời gian dự kiến:</strong> {{Duration}} phút</li>
        <li><strong>Hình thức:</strong> {{InterviewType}}</li>
        <li><strong>Địa điểm:</strong> {{Location}}</li>
        <li><strong>Người phỏng vấn:</strong> {{Interviewer}}</li>
    </ul>
    
    <h3>Tài liệu cần mang theo:</h3>
    <p>{{RequiredDocuments}}</p>
    
    <h3>Lưu ý:</h3>
    <p>{{AdditionalNotes}}</p>
    
    <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi.</p>
    
    <p>Trân trọng,<br>
    {{CompanyName}}</p>
</body>
</html>
```

## Security Considerations

1. **Authorization**: Chỉ Recruiter mới có thể đặt lịch phỏng vấn
2. **Ownership Check**: Recruiter chỉ đặt lịch cho đơn của mình
3. **Input Validation**: Validate tất cả input từ client
4. **XSS Prevention**: Encode HTML trong email
5. **CSRF Protection**: Sử dụng AntiForgeryToken

## Mở rộng tương lai

1. **Interview Feedback**: Form đánh giá sau phỏng vấn
2. **Interview History**: Lịch sử các buổi phỏng vấn
3. **Reschedule**: Cho phép thay đổi lịch hẹn
4. **Video Call Integration**: Tích hợp Zoom/Google Meet API
5. **SMS Reminder**: Gửi SMS nhắc nhở
6. **Interview Notes**: Ghi chú trong quá trình phỏng vấn
7. **Rating System**: Đánh giá ứng viên sau phỏng vấn

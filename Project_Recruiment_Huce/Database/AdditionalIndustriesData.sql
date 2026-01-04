-- ==========================================
-- File: AdditionalIndustriesData.sql
-- Mục đích: Thêm dữ liệu mẫu các ngành nghề đa dạng
-- Hướng dẫn: Chạy SAU KHI chạy CleanAndInsertSampleData.sql
-- ==========================================

USE JOBPORTAL_EN;
GO

PRINT '=== Thêm Companies đa ngành nghề ==='

-- Photos cho công ty mới
IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'vingroup.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat) VALUES ('vingroup.jpg', '/Content/uploads/recruiter/vingroup.jpg', 120, 'jpg');
IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'viettel.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat) VALUES ('viettel.jpg', '/Content/uploads/recruiter/viettel.jpg', 130, 'jpg');
IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'fpt.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat) VALUES ('fpt.jpg', '/Content/uploads/recruiter/fpt.jpg', 110, 'jpg');
IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'agribank.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat) VALUES ('agribank.jpg', '/Content/uploads/recruiter/agribank.jpg', 100, 'jpg');
IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'vinamilk.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat) VALUES ('vinamilk.jpg', '/Content/uploads/recruiter/vinamilk.jpg', 95, 'jpg');

DECLARE @VinPhoto INT, @ViettelPhoto INT, @FPTPhoto INT, @AgribankPhoto INT, @VinamilkPhoto INT;
SELECT @VinPhoto = PhotoID FROM ProfilePhotos WHERE FileName = 'vingroup.jpg';
SELECT @ViettelPhoto = PhotoID FROM ProfilePhotos WHERE FileName = 'viettel.jpg';
SELECT @FPTPhoto = PhotoID FROM ProfilePhotos WHERE FileName = 'fpt.jpg';
SELECT @AgribankPhoto = PhotoID FROM ProfilePhotos WHERE FileName = 'agribank.jpg';
SELECT @VinamilkPhoto = PhotoID FROM ProfilePhotos WHERE FileName = 'vinamilk.jpg';

-- Thêm 5 công ty đa ngành
IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyName = N'Tập đoàn Vingroup')
    INSERT INTO Companies (CompanyName, TaxCode, Industry, Address, Phone, CompanyEmail, Website, Description, PhotoID, ActiveFlag)
    VALUES (N'Tập đoàn Vingroup', '0101245678', N'Bất động sản & Đa ngành', N'Số 7 Bằng Lăng 1, Vinhomes Riverside, Long Biên, Hà Nội', '0243974xxxx', 'hr@vingroup.net', 'https://vingroup.net',
        N'<p>Vingroup là tập đoàn kinh tế tư nhân lớn nhất Việt Nam với các lĩnh vực bất động sản, bán lẻ, du lịch, y tế, giáo dục.</p>', @VinPhoto, 1);

IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyName = N'Tập đoàn Công nghiệp - Viễn thông Quân đội Viettel')
    INSERT INTO Companies (CompanyName, TaxCode, Industry, Address, Phone, CompanyEmail, Website, Description, PhotoID, ActiveFlag)
    VALUES (N'Tập đoàn Công nghiệp - Viễn thông Quân đội Viettel', '0100109106', N'Viễn thông & Công nghệ', N'Số 1 Giang Văn Minh, Ba Đình, Hà Nội', '0243456xxxx', 'hr@viettel.com.vn', 'https://viettel.com.vn',
        N'<p>Viettel là tập đoàn viễn thông và công nghệ lớn nhất Việt Nam, hoạt động tại nhiều quốc gia.</p>', @ViettelPhoto, 1);

IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyName = N'Tập đoàn FPT')
    INSERT INTO Companies (CompanyName, TaxCode, Industry, Address, Phone, CompanyEmail, Website, Description, PhotoID, ActiveFlag)
    VALUES (N'Tập đoàn FPT', '0101248141', N'Công nghệ thông tin', N'FPT Tower, 10 Phạm Văn Bạch, Cầu Giấy, Hà Nội', '0243562xxxx', 'hr@fpt.com.vn', 'https://fpt.com.vn',
        N'<p>FPT là tập đoàn công nghệ hàng đầu Việt Nam với các dịch vụ CNTT, viễn thông và giáo dục.</p>', @FPTPhoto, 1);

IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyName = N'Ngân hàng Nông nghiệp và Phát triển Nông thôn Việt Nam')
    INSERT INTO Companies (CompanyName, TaxCode, Industry, Address, Phone, CompanyEmail, Website, Description, PhotoID, ActiveFlag)
    VALUES (N'Ngân hàng Nông nghiệp và Phát triển Nông thôn Việt Nam', '0100686174', N'Ngân hàng - Tài chính', N'Số 2 Láng Hạ, Đống Đa, Hà Nội', '0243831xxxx', 'hr@agribank.com.vn', 'https://agribank.com.vn',
        N'<p>Agribank là ngân hàng thương mại lớn nhất Việt Nam về quy mô mạng lưới và nhân sự.</p>', @AgribankPhoto, 1);

IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyName = N'Công ty Cổ phần Sữa Việt Nam - Vinamilk')
    INSERT INTO Companies (CompanyName, TaxCode, Industry, Address, Phone, CompanyEmail, Website, Description, PhotoID, ActiveFlag)
    VALUES (N'Công ty Cổ phần Sữa Việt Nam - Vinamilk', '0300588569', N'Thực phẩm & Đồ uống', N'10 Tân Trào, Quận 7, TP.HCM', '0287989xxxx', 'hr@vinamilk.com.vn', 'https://vinamilk.com.vn',
        N'<p>Vinamilk là công ty sữa lớn nhất Việt Nam và nằm trong top 50 công ty sữa lớn nhất thế giới.</p>', @VinamilkPhoto, 1);

-- Lấy ID công ty
DECLARE @VinCo INT, @ViettelCo INT, @FPTCo INT, @AgribankCo INT, @VinamilkCo INT;
SELECT @VinCo = CompanyID FROM Companies WHERE CompanyName = N'Tập đoàn Vingroup';
SELECT @ViettelCo = CompanyID FROM Companies WHERE CompanyName LIKE N'%Viettel%';
SELECT @FPTCo = CompanyID FROM Companies WHERE CompanyName = N'Tập đoàn FPT';
SELECT @AgribankCo = CompanyID FROM Companies WHERE CompanyName LIKE N'%Agribank%';
SELECT @VinamilkCo = CompanyID FROM Companies WHERE CompanyName LIKE N'%Vinamilk%';

PRINT '=== Thêm Accounts & Recruiters ==='

DECLARE @PwdHash NVARCHAR(255) = 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=';

-- Tài khoản recruiter
IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'vingroupHR')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, ActiveFlag, FullName) VALUES ('vingroupHR', @PwdHash, 'hr@vingroup.net', '0906111111', 'Recruiter', 1, N'Trần Minh Đức');
IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'viettelHR')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, ActiveFlag, FullName) VALUES ('viettelHR', @PwdHash, 'hr@viettel.com.vn', '0906222222', 'Recruiter', 1, N'Nguyễn Văn Hùng');
IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'fptHR')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, ActiveFlag, FullName) VALUES ('fptHR', @PwdHash, 'hr@fpt.com.vn', '0906333333', 'Recruiter', 1, N'Lê Thị Hương');
IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'agribankHR')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, ActiveFlag, FullName) VALUES ('agribankHR', @PwdHash, 'hr@agribank.com.vn', '0906444444', 'Recruiter', 1, N'Phạm Văn Tùng');
IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'vinamilkHR')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, ActiveFlag, FullName) VALUES ('vinamilkHR', @PwdHash, 'hr@vinamilk.com.vn', '0906555555', 'Recruiter', 1, N'Hoàng Thị Mai');

DECLARE @VinAcc INT, @ViettelAcc INT, @FPTAcc INT, @AgribankAcc INT, @VinamilkAcc INT;
SELECT @VinAcc = AccountID FROM Accounts WHERE Username = 'vingroupHR';
SELECT @ViettelAcc = AccountID FROM Accounts WHERE Username = 'viettelHR';
SELECT @FPTAcc = AccountID FROM Accounts WHERE Username = 'fptHR';
SELECT @AgribankAcc = AccountID FROM Accounts WHERE Username = 'agribankHR';
SELECT @VinamilkAcc = AccountID FROM Accounts WHERE Username = 'vinamilkHR';

-- Recruiters
IF NOT EXISTS (SELECT 1 FROM Recruiters WHERE AccountID = @VinAcc)
    INSERT INTO Recruiters (AccountID, CompanyID, FullName, PositionTitle, CompanyEmail, Phone, ActiveFlag, SubscriptionType) 
    VALUES (@VinAcc, @VinCo, N'Trần Minh Đức', N'HR Director', 'duc.tm@vingroup.net', '0906111111', 1, 'Lifetime');
IF NOT EXISTS (SELECT 1 FROM Recruiters WHERE AccountID = @ViettelAcc)
    INSERT INTO Recruiters (AccountID, CompanyID, FullName, PositionTitle, CompanyEmail, Phone, ActiveFlag, SubscriptionType) 
    VALUES (@ViettelAcc, @ViettelCo, N'Nguyễn Văn Hùng', N'Talent Manager', 'hung.nv@viettel.com.vn', '0906222222', 1, 'Monthly');
IF NOT EXISTS (SELECT 1 FROM Recruiters WHERE AccountID = @FPTAcc)
    INSERT INTO Recruiters (AccountID, CompanyID, FullName, PositionTitle, CompanyEmail, Phone, ActiveFlag, SubscriptionType) 
    VALUES (@FPTAcc, @FPTCo, N'Lê Thị Hương', N'Senior HR', 'huong.lt@fpt.com.vn', '0906333333', 1, 'Lifetime');
IF NOT EXISTS (SELECT 1 FROM Recruiters WHERE AccountID = @AgribankAcc)
    INSERT INTO Recruiters (AccountID, CompanyID, FullName, PositionTitle, CompanyEmail, Phone, ActiveFlag, SubscriptionType) 
    VALUES (@AgribankAcc, @AgribankCo, N'Phạm Văn Tùng', N'HR Manager', 'tung.pv@agribank.com.vn', '0906444444', 1, 'Monthly');
IF NOT EXISTS (SELECT 1 FROM Recruiters WHERE AccountID = @VinamilkAcc)
    INSERT INTO Recruiters (AccountID, CompanyID, FullName, PositionTitle, CompanyEmail, Phone, ActiveFlag, SubscriptionType) 
    VALUES (@VinamilkAcc, @VinamilkCo, N'Hoàng Thị Mai', N'HRBP', 'mai.ht@vinamilk.com.vn', '0906555555', 1, 'Lifetime');

DECLARE @VinRec INT, @ViettelRec INT, @FPTRec INT, @AgribankRec INT, @VinamilkRec INT;
SELECT @VinRec = RecruiterID FROM Recruiters WHERE AccountID = @VinAcc;
SELECT @ViettelRec = RecruiterID FROM Recruiters WHERE AccountID = @ViettelAcc;
SELECT @FPTRec = RecruiterID FROM Recruiters WHERE AccountID = @FPTAcc;
SELECT @AgribankRec = RecruiterID FROM Recruiters WHERE AccountID = @AgribankAcc;
SELECT @VinamilkRec = RecruiterID FROM Recruiters WHERE AccountID = @VinamilkAcc;

PRINT '=== Thêm JobPosts đa ngành ==='

-- VINGROUP Jobs (Bất động sản, Bán lẻ, Y tế)
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'VIN001', @VinRec, @VinCo, N'Kiến trúc sư dự án', N'<p>Thiết kế kiến trúc cho các dự án bất động sản cao cấp.</p>', N'<ul><li>5+ năm kinh nghiệm</li><li>Thành thạo AutoCAD, Revit</li></ul>', 35000000, 55000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 45, GETDATE()), 'Published', DATEADD(DAY, -10, GETDATE()), 456
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'VIN001');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'VIN002', @VinRec, @VinCo, N'Quản lý cửa hàng VinMart', N'<p>Quản lý vận hành cửa hàng tiện lợi VinMart+.</p>', N'<ul><li>3+ năm quản lý bán lẻ</li></ul>', 18000000, 28000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 30, GETDATE()), 'Published', DATEADD(DAY, -8, GETDATE()), 234
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'VIN002');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'VIN003', @VinRec, @VinCo, N'Bác sĩ đa khoa - Vinmec', N'<p>Khám và điều trị bệnh nhân tại hệ thống Vinmec.</p>', N'<ul><li>Bằng Bác sĩ Y khoa</li><li>Chứng chỉ hành nghề</li></ul>', 40000000, 70000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 60, GETDATE()), 'Published', DATEADD(DAY, -12, GETDATE()), 567
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'VIN003');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'VIN004', @VinRec, @VinCo, N'Giáo viên Tiếng Anh - Vinschool', N'<p>Giảng dạy Tiếng Anh cho học sinh Vinschool.</p>', N'<ul><li>IELTS 7.5+ hoặc tương đương</li><li>Có kinh nghiệm giảng dạy</li></ul>', 20000000, 35000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 40, GETDATE()), 'Published', DATEADD(DAY, -5, GETDATE()), 345
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'VIN004');

-- VIETTEL Jobs (Viễn thông, Công nghệ)
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'VTT001', @ViettelRec, @ViettelCo, N'Kỹ sư mạng viễn thông', N'<p>Vận hành và tối ưu mạng viễn thông 4G/5G.</p>', N'<ul><li>Kinh nghiệm mạng viễn thông</li><li>CCNA/CCNP</li></ul>', 25000000, 40000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 50, GETDATE()), 'Published', DATEADD(DAY, -15, GETDATE()), 489
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'VTT001');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'VTT002', @ViettelRec, @ViettelCo, N'Chuyên viên an ninh mạng', N'<p>Bảo vệ hạ tầng mạng và phát hiện xâm nhập.</p>', N'<ul><li>Kinh nghiệm Security</li><li>CEH, CISSP ưu tiên</li></ul>', 30000000, 50000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 45, GETDATE()), 'Published', DATEADD(DAY, -10, GETDATE()), 378
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'VTT002');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'VTT003', @ViettelRec, @ViettelCo, N'Nhân viên kinh doanh B2B', N'<p>Phát triển khách hàng doanh nghiệp cho dịch vụ Viettel.</p>', N'<ul><li>Kỹ năng bán hàng</li><li>Giao tiếp tốt</li></ul>', 15000000, 25000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 35, GETDATE()), 'Published', DATEADD(DAY, -7, GETDATE()), 256
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'VTT003');

-- FPT Jobs (CNTT, Outsourcing)
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'FPT001', @FPTRec, @FPTCo, N'Java Developer (Onsite Japan)', N'<p>Làm việc tại Nhật Bản, phát triển phần mềm cho khách hàng Nhật.</p>', N'<ul><li>3+ năm Java</li><li>Tiếng Nhật N3+</li></ul>', 45000000, 70000000, 'VND', N'Tokyo, Japan', 'Full-time', DATEADD(DAY, 60, GETDATE()), 'Published', DATEADD(DAY, -20, GETDATE()), 678
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'FPT001');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'FPT002', @FPTRec, @FPTCo, N'Fresher .NET Developer', N'<p>Đào tạo và phát triển lập trình viên .NET mới ra trường.</p>', N'<ul><li>Sinh viên mới tốt nghiệp CNTT</li><li>Có kiến thức C#</li></ul>', 10000000, 15000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 40, GETDATE()), 'Published', DATEADD(DAY, -8, GETDATE()), 890
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'FPT002');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'FPT003', @FPTRec, @FPTCo, N'Business Analyst', N'<p>Phân tích yêu cầu và viết tài liệu cho dự án outsource.</p>', N'<ul><li>3+ năm BA</li><li>Tiếng Anh tốt</li></ul>', 25000000, 40000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 45, GETDATE()), 'Published', DATEADD(DAY, -12, GETDATE()), 423
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'FPT003');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'FPT004', @FPTRec, @FPTCo, N'Comtor (Tiếng Nhật)', N'<p>Phiên dịch và hỗ trợ giao tiếp cho dự án Nhật.</p>', N'<ul><li>Tiếng Nhật N2+</li><li>Hiểu biết IT</li></ul>', 20000000, 35000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 38, GETDATE()), 'Published', DATEADD(DAY, -6, GETDATE()), 312
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'FPT004');

-- AGRIBANK Jobs (Ngân hàng, Tài chính)
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'AGB001', @AgribankRec, @AgribankCo, N'Chuyên viên Tín dụng', N'<p>Thẩm định và quản lý hồ sơ cho vay.</p>', N'<ul><li>Tốt nghiệp Tài chính/Ngân hàng</li><li>Kỹ năng phân tích</li></ul>', 15000000, 25000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 35, GETDATE()), 'Published', DATEADD(DAY, -10, GETDATE()), 345
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AGB001');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'AGB002', @AgribankRec, @AgribankCo, N'Giao dịch viên', N'<p>Phục vụ khách hàng tại quầy giao dịch.</p>', N'<ul><li>Ngoại hình ưa nhìn</li><li>Giao tiếp tốt</li></ul>', 12000000, 18000000, 'VND', N'Toàn quốc', 'Full-time', DATEADD(DAY, 30, GETDATE()), 'Published', DATEADD(DAY, -5, GETDATE()), 567
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AGB002');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'AGB003', @AgribankRec, @AgribankCo, N'Kiểm soát viên nội bộ', N'<p>Kiểm tra và đánh giá hoạt động chi nhánh.</p>', N'<ul><li>5+ năm ngân hàng</li><li>Chứng chỉ CIA ưu tiên</li></ul>', 25000000, 40000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 45, GETDATE()), 'Published', DATEADD(DAY, -15, GETDATE()), 234
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AGB003');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'AGB004', @AgribankRec, @AgribankCo, N'Chuyên viên IT Ngân hàng', N'<p>Phát triển và bảo trì hệ thống Core Banking.</p>', N'<ul><li>Kinh nghiệm phát triển Java/Oracle</li></ul>', 22000000, 35000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 42, GETDATE()), 'Published', DATEADD(DAY, -8, GETDATE()), 389
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AGB004');

-- VINAMILK Jobs (Thực phẩm, Sản xuất)
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'VNM001', @VinamilkRec, @VinamilkCo, N'Kỹ sư Sản xuất', N'<p>Vận hành và tối ưu dây chuyền sản xuất sữa.</p>', N'<ul><li>Kỹ thuật Thực phẩm/Hóa học</li><li>3+ năm kinh nghiệm</li></ul>', 18000000, 28000000, 'VND', N'Bình Dương', 'Full-time', DATEADD(DAY, 40, GETDATE()), 'Published', DATEADD(DAY, -12, GETDATE()), 298
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'VNM001');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'VNM002', @VinamilkRec, @VinamilkCo, N'Nhân viên QC/QA', N'<p>Kiểm tra chất lượng sản phẩm sữa.</p>', N'<ul><li>Công nghệ Thực phẩm</li><li>Tỉ mỉ, cẩn thận</li></ul>', 12000000, 18000000, 'VND', N'Bình Dương', 'Full-time', DATEADD(DAY, 35, GETDATE()), 'Published', DATEADD(DAY, -7, GETDATE()), 267
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'VNM002');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'VNM003', @VinamilkRec, @VinamilkCo, N'Trưởng nhóm Marketing', N'<p>Xây dựng và triển khai chiến dịch marketing sản phẩm.</p>', N'<ul><li>5+ năm Marketing FMCG</li></ul>', 30000000, 45000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 50, GETDATE()), 'Published', DATEADD(DAY, -15, GETDATE()), 412
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'VNM003');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, ViewCount)
SELECT 'VNM004', @VinamilkRec, @VinamilkCo, N'Giám sát bán hàng khu vực', N'<p>Quản lý đội ngũ bán hàng và đại lý.</p>', N'<ul><li>3+ năm kinh nghiệm Sales FMCG</li></ul>', 20000000, 30000000, 'VND', N'Đà Nẵng', 'Full-time', DATEADD(DAY, 38, GETDATE()), 'Published', DATEADD(DAY, -9, GETDATE()), 334
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'VNM004');

PRINT '=== Thêm Applications ==='

-- Lấy danh sách Candidates
DECLARE @Cand1 INT, @Cand2 INT, @Cand3 INT, @Cand4 INT, @Cand5 INT, @Cand6 INT, @Cand7 INT, @Cand8 INT;
SELECT @Cand1 = CandidateID FROM Candidates WHERE FullName = N'Nguyễn Văn An';
SELECT @Cand2 = CandidateID FROM Candidates WHERE FullName = N'Trần Thị Bích';
SELECT @Cand3 = CandidateID FROM Candidates WHERE FullName = N'Lê Văn Cường';
SELECT @Cand4 = CandidateID FROM Candidates WHERE FullName = N'Phạm Thị Dung';
SELECT @Cand5 = CandidateID FROM Candidates WHERE FullName = N'Hoàng Văn Đức';
SELECT @Cand6 = CandidateID FROM Candidates WHERE FullName = N'Trần Thị Hoa';
SELECT @Cand7 = CandidateID FROM Candidates WHERE FullName = N'Nguyễn Thị Giang';
SELECT @Cand8 = CandidateID FROM Candidates WHERE FullName = N'Trần Văn Khánh';

-- Thêm Applications cho các jobs mới
DECLARE @JobID INT;

-- VIN001 Applications
SELECT @JobID = JobPostID FROM JobPosts WHERE JobCode = 'VIN001';
IF @JobID IS NOT NULL AND @Cand1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand1)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand1, @JobID, 'Under review', '/Content/uploads/resumes/cv1.jpg', DATEADD(DAY, -5, GETDATE()));
IF @JobID IS NOT NULL AND @Cand3 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand3)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand3, @JobID, 'Shortlisted', '/Content/uploads/resumes/cv3.jpg', DATEADD(DAY, -4, GETDATE()));

-- VTT001 Applications
SELECT @JobID = JobPostID FROM JobPosts WHERE JobCode = 'VTT001';
IF @JobID IS NOT NULL AND @Cand5 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand5)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand5, @JobID, 'Interviewed', '/Content/uploads/resumes/cv5.jpg', DATEADD(DAY, -10, GETDATE()));
IF @JobID IS NOT NULL AND @Cand2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand2)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand2, @JobID, 'Under review', '/Content/uploads/resumes/cv2.webp', DATEADD(DAY, -3, GETDATE()));

-- FPT001 Applications
SELECT @JobID = JobPostID FROM JobPosts WHERE JobCode = 'FPT001';
IF @JobID IS NOT NULL AND @Cand1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand1)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand1, @JobID, 'Offered', '/Content/uploads/resumes/cv1.jpg', DATEADD(DAY, -15, GETDATE()));
IF @JobID IS NOT NULL AND @Cand3 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand3)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand3, @JobID, 'Shortlisted', '/Content/uploads/resumes/cv3.jpg', DATEADD(DAY, -12, GETDATE()));
IF @JobID IS NOT NULL AND @Cand4 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand4)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand4, @JobID, 'Rejected', '/Content/uploads/resumes/cv4.webp', DATEADD(DAY, -18, GETDATE()));

-- FPT002 Applications (Fresher)
SELECT @JobID = JobPostID FROM JobPosts WHERE JobCode = 'FPT002';
IF @JobID IS NOT NULL AND @Cand2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand2)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand2, @JobID, 'Under review', '/Content/uploads/resumes/cv2.webp', DATEADD(DAY, -2, GETDATE()));
IF @JobID IS NOT NULL AND @Cand6 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand6)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand6, @JobID, 'Shortlisted', '/Content/uploads/resumes/cv1.jpg', DATEADD(DAY, -3, GETDATE()));

-- AGB001 Applications
SELECT @JobID = JobPostID FROM JobPosts WHERE JobCode = 'AGB001';
IF @JobID IS NOT NULL AND @Cand7 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand7)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand7, @JobID, 'Under review', '/Content/uploads/resumes/cv2.webp', DATEADD(DAY, -6, GETDATE()));

-- VNM003 Applications
SELECT @JobID = JobPostID FROM JobPosts WHERE JobCode = 'VNM003';
IF @JobID IS NOT NULL AND @Cand6 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand6)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand6, @JobID, 'Interviewed', '/Content/uploads/resumes/cv1.jpg', DATEADD(DAY, -8, GETDATE()));
IF @JobID IS NOT NULL AND @Cand8 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Applications WHERE JobPostID = @JobID AND CandidateID = @Cand8)
    INSERT INTO Applications (CandidateID, JobPostID, Status, ResumeFilePath, AppliedAt) VALUES (@Cand8, @JobID, 'Under review', '/Content/uploads/resumes/cv3.jpg', DATEADD(DAY, -4, GETDATE()));

PRINT '=== Thống kê ==='
SELECT 'Tổng số công ty' AS Info, COUNT(*) AS Total FROM Companies;
SELECT 'Tổng số JobPosts' AS Info, COUNT(*) AS Total FROM JobPosts;
SELECT 'Tổng số Applications' AS Info, COUNT(*) AS Total FROM Applications;

SELECT c.Industry, COUNT(jp.JobPostID) AS JobCount
FROM Companies c
LEFT JOIN JobPosts jp ON c.CompanyID = jp.CompanyID
GROUP BY c.Industry
ORDER BY JobCount DESC;

PRINT '=== HOÀN TẤT! ==='
GO

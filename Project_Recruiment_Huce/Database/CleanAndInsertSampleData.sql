-- ==========================================================
-- File: JOBPORTAL_EN_SeedData_VN.sql
-- Mục đích: Xóa dữ liệu cũ và chèn dữ liệu mẫu tiếng Việt
-- Lưu ý: Giữ nguyên Status/Role tiếng Anh để khớp với Code C#
-- ==========================================================

USE JOBPORTAL_EN;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

PRINT '=== Bước 1: Dọn dẹp dữ liệu cũ (Clean Data) ===';

-- Xóa theo đúng thứ tự ràng buộc khóa ngoại (Foreign Keys)
DELETE FROM SavedJobs;
DELETE FROM Applications;
DELETE FROM ResumeFiles;
DELETE FROM JobPostDetails; -- Bổ sung xóa bảng chi tiết nếu có
DELETE FROM JobPosts;
DELETE FROM Candidates;
DELETE FROM Recruiters WHERE RecruiterID > 3; -- Giữ lại recruiter mặc định (nếu có)
DELETE FROM Accounts WHERE Role = 'Recruiter' AND AccountID > 5;
DELETE FROM Accounts WHERE Role = 'Candidate';
DELETE FROM Companies WHERE CompanyID > 3; 
DELETE FROM ProfilePhotos WHERE PhotoID > 7;

PRINT '=== Bước 2: Chèn dữ liệu ảnh (Photos) ===';

-- Logo công ty
IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'apple.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat, UploadedAt)
    VALUES ('apple.jpg', '/Content/uploads/recruiter/apple.jpg', 150, 'jpg', GETDATE());

IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'amazon.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat, UploadedAt)
    VALUES ('amazon.jpg', '/Content/uploads/recruiter/amazon.jpg', 180, 'jpg', GETDATE());

IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'cocacola.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat, UploadedAt)
    VALUES ('cocacola.jpg', '/Content/uploads/recruiter/cocacola.jpg', 120, 'jpg', GETDATE());

-- Avatar nhà tuyển dụng
IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'avatar_rec1.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat, UploadedAt)
    VALUES ('avatar_rec1.jpg', '/Content/uploads/recruiter/avatar_1_20251124181802686.jpg', 95, 'jpg', GETDATE());

IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'avatar_rec2.png')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat, UploadedAt)
    VALUES ('avatar_rec2.png', '/Content/uploads/recruiter/avatar_2_20251109134341518.png', 110, 'png', GETDATE());

IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'avatar_rec4.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat, UploadedAt)
    VALUES ('avatar_rec4.jpg', '/Content/uploads/recruiter/avatar_4_20251125151831284.jpg', 88, 'jpg', GETDATE());

-- Avatar ứng viên
IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'avatar_cand1.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat, UploadedAt)
    VALUES ('avatar_cand1.jpg', '/Content/uploads/candidate/avatar_1_20251105182624655.jpg', 75, 'jpg', GETDATE());

IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'avatar_cand2.jpg')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat, UploadedAt)
    VALUES ('avatar_cand2.jpg', '/Content/uploads/candidate/avatar_1_20251109134122689.jpg', 82, 'jpg', GETDATE());

IF NOT EXISTS (SELECT 1 FROM ProfilePhotos WHERE FileName = 'avatar_cand3.png')
    INSERT INTO ProfilePhotos (FileName, FilePath, FileSizeKB, FileFormat, UploadedAt)
    VALUES ('avatar_cand3.png', '/Content/uploads/candidate/avatar_34_20251106170910515.png', 65, 'png', GETDATE());

PRINT '=== Bước 3: Lấy PhotoID ===';

DECLARE @AppleLogo INT, @AmazonLogo INT, @CocaColaLogo INT;
DECLARE @RecPhoto1 INT, @RecPhoto2 INT, @RecPhoto4 INT;
DECLARE @CandPhoto1 INT, @CandPhoto2 INT, @CandPhoto3 INT;

SELECT @AppleLogo = PhotoID FROM ProfilePhotos WHERE FileName = 'apple.jpg';
SELECT @AmazonLogo = PhotoID FROM ProfilePhotos WHERE FileName = 'amazon.jpg';
SELECT @CocaColaLogo = PhotoID FROM ProfilePhotos WHERE FileName = 'cocacola.jpg';
SELECT @RecPhoto1 = PhotoID FROM ProfilePhotos WHERE FileName = 'avatar_rec1.jpg';
SELECT @RecPhoto2 = PhotoID FROM ProfilePhotos WHERE FileName = 'avatar_rec2.png';
SELECT @RecPhoto4 = PhotoID FROM ProfilePhotos WHERE FileName = 'avatar_rec4.jpg';
SELECT @CandPhoto1 = PhotoID FROM ProfilePhotos WHERE FileName = 'avatar_cand1.jpg';
SELECT @CandPhoto2 = PhotoID FROM ProfilePhotos WHERE FileName = 'avatar_cand2.jpg';
SELECT @CandPhoto3 = PhotoID FROM ProfilePhotos WHERE FileName = 'avatar_cand3.png';

PRINT '=== Bước 4: Chèn Công ty (Companies) ===';

DECLARE @AppleCo INT, @AmazonCo INT, @CocaCo INT;

IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyName = N'Công ty TNHH Apple Việt Nam')
BEGIN
    INSERT INTO Companies (CompanyName, TaxCode, Industry, Address, Phone, CompanyEmail, Website, Fax, Description, PhotoID, ActiveFlag, CreatedAt)
    VALUES (N'Công ty TNHH Apple Việt Nam', '0123456789', N'Công nghệ điện tử', 
        N'Keangnam, Phạm Hùng, Cầu Giấy, Hà Nội', '0243838xxxx', 'contact@apple.vn', 'https://www.apple.com/vn', NULL,
        N'<p>Apple là tập đoàn công nghệ hàng đầu thế giới, nổi tiếng với các sản phẩm iPhone, iPad, Mac.</p>', @AppleLogo, 1, DATEADD(DAY, -100, GETDATE()));
END

IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyName = 'Amazon Web Services Vietnam')
BEGIN
    INSERT INTO Companies (CompanyName, TaxCode, Industry, Address, Phone, CompanyEmail, Website, Fax, Description, PhotoID, ActiveFlag, CreatedAt)
    VALUES ('Amazon Web Services Vietnam', '0234567890', N'Điện toán đám mây', 
        N'Tòa nhà Landmark 72, Nam Từ Liêm, Hà Nội', '0243939xxxx', 'aws@amazon.com', 'https://aws.amazon.com', NULL,
        N'<p>AWS cung cấp nền tảng điện toán đám mây toàn diện và phổ biến nhất thế giới.</p>', @AmazonLogo, 1, DATEADD(DAY, -95, GETDATE()));
END

IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyName = N'Coca-Cola Việt Nam')
BEGIN
    INSERT INTO Companies (CompanyName, TaxCode, Industry, Address, Phone, CompanyEmail, Website, Fax, Description, PhotoID, ActiveFlag, CreatedAt)
    VALUES (N'Coca-Cola Việt Nam', '0345678901', N'F&B (Đồ uống)', 
        N'Nguyễn Huy Tưởng, Thanh Xuân, Hà Nội', '0243737xxxx', 'hr@coca-cola.vn', 'https://www.coca-cola.com.vn', NULL,
        N'<p>Coca-Cola là thương hiệu nước giải khát lớn nhất thế giới với lịch sử lâu đời.</p>', @CocaColaLogo, 1, DATEADD(DAY, -90, GETDATE()));
END

SELECT @AppleCo = CompanyID FROM Companies WHERE CompanyName = N'Công ty TNHH Apple Việt Nam';
SELECT @AmazonCo = CompanyID FROM Companies WHERE CompanyName = 'Amazon Web Services Vietnam';
SELECT @CocaCo = CompanyID FROM Companies WHERE CompanyName = N'Coca-Cola Việt Nam';

PRINT '=== Bước 5: Chèn Tài khoản Nhà tuyển dụng (Recruiter Accounts) ===';

DECLARE @PwdHash NVARCHAR(255) = 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A='; -- Password123!
DECLARE @AppleAcc INT, @AwsAcc INT, @CocaAcc INT;

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'appleHR')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, PhotoID, ActiveFlag, CreatedAt)
    VALUES ('appleHR', @PwdHash, 'hr@apple.vn', '0901111111', 'Recruiter', @RecPhoto1, 1, DATEADD(DAY, -95, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'awsHR')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, PhotoID, ActiveFlag, CreatedAt)
    VALUES ('awsHR', @PwdHash, 'hr@aws.com', '0902222222', 'Recruiter', @RecPhoto2, 1, DATEADD(DAY, -90, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'cocaHR')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, PhotoID, ActiveFlag, CreatedAt)
    VALUES ('cocaHR', @PwdHash, 'hr@coca.vn', '0903333333', 'Recruiter', @RecPhoto4, 1, DATEADD(DAY, -85, GETDATE()));

SELECT @AppleAcc = AccountID FROM Accounts WHERE Username = 'appleHR';
SELECT @AwsAcc = AccountID FROM Accounts WHERE Username = 'awsHR';
SELECT @CocaAcc = AccountID FROM Accounts WHERE Username = 'cocaHR';

PRINT '=== Bước 6: Chèn Thông tin Nhà tuyển dụng (Recruiters) ===';

DECLARE @AppleRec INT, @AwsRec INT, @CocaRec INT;

IF NOT EXISTS (SELECT 1 FROM Recruiters WHERE AccountID = @AppleAcc)
    INSERT INTO Recruiters (AccountID, CompanyID, FullName, PositionTitle, CompanyEmail, Phone, PhotoID, ActiveFlag, CreatedAt)
    VALUES (@AppleAcc, @AppleCo, N'Nguyễn Thị Minh Anh', N'Trưởng phòng Nhân sự', 'minhanh@apple.vn', '0901111111', @RecPhoto1, 1, DATEADD(DAY, -95, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Recruiters WHERE AccountID = @AwsAcc)
    INSERT INTO Recruiters (AccountID, CompanyID, FullName, PositionTitle, CompanyEmail, Phone, PhotoID, ActiveFlag, CreatedAt)
    VALUES (@AwsAcc, @AmazonCo, N'Trần Văn Bình', N'Chuyên viên Tuyển dụng', 'binh@aws.com', '0902222222', @RecPhoto2, 1, DATEADD(DAY, -90, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Recruiters WHERE AccountID = @CocaAcc)
    INSERT INTO Recruiters (AccountID, CompanyID, FullName, PositionTitle, CompanyEmail, Phone, PhotoID, ActiveFlag, CreatedAt)
    VALUES (@CocaAcc, @CocaCo, N'Lê Thị Cẩm Ly', N'Đối tác Nhân sự (HRBP)', 'ly@coca.vn', '0903333333', @RecPhoto4, 1, DATEADD(DAY, -85, GETDATE()));

SELECT @AppleRec = RecruiterID FROM Recruiters WHERE AccountID = @AppleAcc;
SELECT @AwsRec = RecruiterID FROM Recruiters WHERE AccountID = @AwsAcc;
SELECT @CocaRec = RecruiterID FROM Recruiters WHERE AccountID = @CocaAcc;

-- Lấy ID các nhà tuyển dụng cũ (FPT, Samsung, NVIDIA) nếu database đã có
DECLARE @FPTRec INT, @SamsungRec INT, @NvidiaRec INT;
DECLARE @FPTCompany INT, @SamsungCompany INT, @NvidiaCompany INT;

SELECT @FPTCompany = CompanyID FROM Companies WHERE CompanyName LIKE N'%FPT%';
SELECT @SamsungCompany = CompanyID FROM Companies WHERE CompanyName LIKE N'%Samsung%';
SELECT @NvidiaCompany = CompanyID FROM Companies WHERE CompanyName LIKE N'%NVIDIA%';

SELECT TOP 1 @FPTRec = RecruiterID FROM Recruiters WHERE CompanyID = @FPTCompany;
SELECT TOP 1 @SamsungRec = RecruiterID FROM Recruiters WHERE CompanyID = @SamsungCompany;
SELECT TOP 1 @NvidiaRec = RecruiterID FROM Recruiters WHERE CompanyID = @NvidiaCompany;

PRINT '=== Bước 7: Chèn Tài khoản Ứng viên (Candidate Accounts) ===';

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'cand_an')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, PhotoID, ActiveFlag, CreatedAt)
    VALUES ('cand_an', @PwdHash, 'vanan@gmail.com', '0911111111', 'Candidate', @CandPhoto1, 1, DATEADD(DAY, -60, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'cand_bich')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, PhotoID, ActiveFlag, CreatedAt)
    VALUES ('cand_bich', @PwdHash, 'tbich@gmail.com', '0912222222', 'Candidate', @CandPhoto2, 1, DATEADD(DAY, -58, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'cand_cuong')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, PhotoID, ActiveFlag, CreatedAt)
    VALUES ('cand_cuong', @PwdHash, 'vcuong@gmail.com', '0913333333', 'Candidate', @CandPhoto3, 1, DATEADD(DAY, -56, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'cand_dung')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, ActiveFlag, CreatedAt)
    VALUES ('cand_dung', @PwdHash, 'tdung@gmail.com', '0914444444', 'Candidate', 1, DATEADD(DAY, -54, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'cand_duc')
    INSERT INTO Accounts (Username, PasswordHash, Email, Phone, Role, ActiveFlag, CreatedAt)
    VALUES ('cand_duc', @PwdHash, 'vduc@gmail.com', '0915555555', 'Candidate', 1, DATEADD(DAY, -52, GETDATE()));

PRINT '=== Bước 8: Chèn Thông tin Ứng viên (Candidates) ===';

DECLARE @CandAcc1 INT, @CandAcc2 INT, @CandAcc3 INT, @CandAcc4 INT, @CandAcc5 INT;

SELECT @CandAcc1 = AccountID FROM Accounts WHERE Username = 'cand_an';
SELECT @CandAcc2 = AccountID FROM Accounts WHERE Username = 'cand_bich';
SELECT @CandAcc3 = AccountID FROM Accounts WHERE Username = 'cand_cuong';
SELECT @CandAcc4 = AccountID FROM Accounts WHERE Username = 'cand_dung';
SELECT @CandAcc5 = AccountID FROM Accounts WHERE Username = 'cand_duc';

IF NOT EXISTS (SELECT 1 FROM Candidates WHERE AccountID = @CandAcc1)
    INSERT INTO Candidates (AccountID, FullName, BirthDate, Gender, Phone, Email, ApplicationEmail, Address, Summary, PhotoID, ActiveFlag, CreatedAt)
    VALUES (@CandAcc1, N'Nguyễn Văn An', '1995-03-15', N'Nam', '0911111111', 'vanan@gmail.com', 'vanan@gmail.com',
        N'Thanh Xuân, Hà Nội', N'Lập trình viên Full Stack với 4 năm kinh nghiệm .NET và Angular.', @CandPhoto1, 1, DATEADD(DAY, -60, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Candidates WHERE AccountID = @CandAcc2)
    INSERT INTO Candidates (AccountID, FullName, BirthDate, Gender, Phone, Email, ApplicationEmail, Address, Summary, PhotoID, ActiveFlag, CreatedAt)
    VALUES (@CandAcc2, N'Trần Thị Bích', '1996-07-20', N'Nữ', '0912222222', 'tbich@gmail.com', 'tbich@gmail.com',
        N'Quận 1, TP.HCM', N'Chuyên viên Frontend Developer thành thạo VueJS và ReactJS.', @CandPhoto2, 1, DATEADD(DAY, -58, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Candidates WHERE AccountID = @CandAcc3)
    INSERT INTO Candidates (AccountID, FullName, BirthDate, Gender, Phone, Email, ApplicationEmail, Address, Summary, PhotoID, ActiveFlag, CreatedAt)
    VALUES (@CandAcc3, N'Lê Văn Cường', '1994-11-10', N'Nam', '0913333333', 'vcuong@gmail.com', 'vcuong@gmail.com',
        N'Đà Nẵng', N'Senior Backend Developer chuyên sâu về .NET Core và Microservices.', @CandPhoto3, 1, DATEADD(DAY, -56, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Candidates WHERE AccountID = @CandAcc4)
    INSERT INTO Candidates (AccountID, FullName, BirthDate, Gender, Phone, Email, ApplicationEmail, Address, Summary, ActiveFlag, CreatedAt)
    VALUES (@CandAcc4, N'Phạm Thị Dung', '1997-05-25', N'Nữ', '0914444444', 'tdung@gmail.com', 'tdung@gmail.com',
        N'Ba Đình, Hà Nội', N'Lập trình viên Mobile (Flutter/Dart) đã phát hành 5 ứng dụng lên Store.', 1, DATEADD(DAY, -54, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM Candidates WHERE AccountID = @CandAcc5)
    INSERT INTO Candidates (AccountID, FullName, BirthDate, Gender, Phone, Email, ApplicationEmail, Address, Summary, ActiveFlag, CreatedAt)
    VALUES (@CandAcc5, N'Hoàng Văn Đức', '1993-09-08', N'Nam', '0915555555', 'vduc@gmail.com', 'vduc@gmail.com',
        N'Quận 3, TP.HCM', N'Kỹ sư DevOps với chứng chỉ AWS Solution Architect.', 1, DATEADD(DAY, -52, GETDATE()));

PRINT '=== Bước 9: Chèn Hồ sơ năng lực (Resumes) ===';

DECLARE @Cand1 INT, @Cand2 INT, @Cand3 INT, @Cand4 INT;

SELECT @Cand1 = CandidateID FROM Candidates WHERE FullName = N'Nguyễn Văn An';
SELECT @Cand2 = CandidateID FROM Candidates WHERE FullName = N'Trần Thị Bích';
SELECT @Cand3 = CandidateID FROM Candidates WHERE FullName = N'Lê Văn Cường';
SELECT @Cand4 = CandidateID FROM Candidates WHERE FullName = N'Phạm Thị Dung';

IF NOT EXISTS (SELECT 1 FROM ResumeFiles WHERE CandidateID = @Cand1)
    INSERT INTO ResumeFiles (CandidateID, FileName, FilePath, UploadedAt)
    VALUES (@Cand1, 'CV_NguyenVanAn.pdf', '/Content/uploads/resumes/cv1.jpg', DATEADD(DAY, -60, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM ResumeFiles WHERE CandidateID = @Cand2)
    INSERT INTO ResumeFiles (CandidateID, FileName, FilePath, UploadedAt)
    VALUES (@Cand2, 'CV_TranThiBich.pdf', '/Content/uploads/resumes/cv2.webp', DATEADD(DAY, -58, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM ResumeFiles WHERE CandidateID = @Cand3)
    INSERT INTO ResumeFiles (CandidateID, FileName, FilePath, UploadedAt)
    VALUES (@Cand3, 'CV_LeVanCuong.pdf', '/Content/uploads/resumes/cv3.jpg', DATEADD(DAY, -56, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM ResumeFiles WHERE CandidateID = @Cand4)
    INSERT INTO ResumeFiles (CandidateID, FileName, FilePath, UploadedAt)
    VALUES (@Cand4, 'CV_PhamThiDung.pdf', '/Content/uploads/resumes/cv4.webp', DATEADD(DAY, -54, GETDATE()));

PRINT '=== Bước 10: Chèn Tin tuyển dụng (JobPosts) ===';

-- FPT Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'FPT001')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('FPT001', @FPTRec, @FPTCompany, N'Lập trình viên Java (Senior)', N'<p>Tham gia phát triển dự án Spring Boot cho khách hàng Nhật Bản.</p>', 
        N'<ul><li>Trên 3 năm kinh nghiệm Java</li><li>Tiếng Nhật N3 là lợi thế</li></ul>', 25000000, 35000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 30, GETDATE()), 'Published', DATEADD(DAY, -15, GETDATE()), GETDATE(), 327);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'FPT002')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('FPT002', @FPTRec, @FPTCompany, N'Lập trình viên ReactJS', N'<p>Xây dựng giao diện Frontend cho ứng dụng Web quy mô lớn.</p>', 
        N'<ul><li>Tối thiểu 2 năm kinh nghiệm ReactJS</li><li>Thành thạo Redux, Hooks</li></ul>', 18000000, 25000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 25, GETDATE()), 'Published', DATEADD(DAY, -10, GETDATE()), GETDATE(), 245);

-- Samsung Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG001')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('SSG001', @SamsungRec, @SamsungCompany, N'Chuyên viên phát triển Android', N'<p>Phát triển ứng dụng Android cho các dòng sản phẩm Flagship.</p>', 
        N'<ul><li>4+ năm kinh nghiệm Android Native</li><li>Kotlin/Java</li></ul>', 30000000, 45000000, 'VND', N'TP.HCM', 'Full-time', 
        DATEADD(DAY, 35, GETDATE()), 'Published', DATEADD(DAY, -20, GETDATE()), GETDATE(), 412);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG002')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('SSG002', @SamsungRec, @SamsungCompany, N'Kỹ sư Lập trình Nhúng', N'<p>Phát triển Firmware cho các thiết bị IoT.</p>', 
        N'<ul><li>Thành thạo C/C++</li><li>Hiểu biết về RTOS</li></ul>', 28000000, 40000000, 'VND', N'TP.HCM', 'Full-time', 
        DATEADD(DAY, 40, GETDATE()), 'Published', DATEADD(DAY, -18, GETDATE()), GETDATE(), 289);

-- NVIDIA Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV001')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('NV001', @NvidiaRec, @NvidiaCompany, N'Kỹ sư Deep Learning', N'<p>Nghiên cứu và phát triển tại phòng Lab AI (GPU computing).</p>', 
        N'<ul><li>Yêu cầu bằng Thạc sĩ/Tiến sĩ</li><li>Kinh nghiệm với PyTorch/TensorFlow</li></ul>', 35000000, 60000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 45, GETDATE()), 'Published', DATEADD(DAY, -12, GETDATE()), GETDATE(), 534);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV002')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('NV002', @NvidiaRec, @NvidiaCompany, N'Kỹ sư CUDA', N'<p>Tối ưu hóa hiệu năng tính toán trên GPU.</p>', 
        N'<ul><li>3+ năm kinh nghiệm lập trình CUDA C++</li></ul>', 32000000, 50000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 38, GETDATE()), 'Published', DATEADD(DAY, -8, GETDATE()), GETDATE(), 378);

-- Apple Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL001')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('APL001', @AppleRec, @AppleCo, N'Kỹ sư iOS (SwiftUI)', N'<p>Phát triển ứng dụng iOS mượt mà, tối ưu trải nghiệm người dùng.</p>', 
        N'<ul><li>3+ năm kinh nghiệm Swift</li><li>Có sản phẩm trên App Store</li></ul>', 35000000, 55000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 42, GETDATE()), 'Published', DATEADD(DAY, -14, GETDATE()), GETDATE(), 498);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL002')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('APL002', @AppleRec, @AppleCo, N'Kỹ sư hệ thống macOS', N'<p>Xây dựng ứng dụng nền tảng cho hệ điều hành macOS.</p>', 
        N'<ul><li>Thành thạo Objective-C và Swift</li><li>4 năm kinh nghiệm</li></ul>', 38000000, 60000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 50, GETDATE()), 'Published', DATEADD(DAY, -7, GETDATE()), GETDATE(), 321);

-- AWS Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS001')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('AWS001', @AwsRec, @AmazonCo, N'Kiến trúc sư giải pháp Cloud', N'<p>Tư vấn giải pháp đám mây cho khách hàng doanh nghiệp.</p>', 
        N'<ul><li>Có chứng chỉ AWS Solution Architect Professional</li></ul>', 40000000, 65000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 55, GETDATE()), 'Published', DATEADD(DAY, -16, GETDATE()), GETDATE(), 567);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS002')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('AWS002', @AwsRec, @AmazonCo, N'Kỹ sư DevOps', N'<p>Vận hành hệ thống CI/CD, Kubernetes, Terraform.</p>', 
        N'<ul><li>3+ năm kinh nghiệm DevOps</li></ul>', 30000000, 45000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 48, GETDATE()), 'Published', DATEADD(DAY, -9, GETDATE()), GETDATE(), 423);

-- Coca-Cola Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC001')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('CC001', @CocaRec, @CocaCo, N'Chuyên viên phân tích nghiệp vụ (BA)', N'<p>Phân tích quy trình chuỗi cung ứng (Supply Chain).</p>', 
        N'<ul><li>3+ năm kinh nghiệm BA</li><li>Kỹ năng giao tiếp tốt</li></ul>', 22000000, 32000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 32, GETDATE()), 'Published', DATEADD(DAY, -11, GETDATE()), GETDATE(), 267);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC002')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('CC002', @CocaRec, @CocaCo, N'Kỹ sư dữ liệu (Data Engineer)', N'<p>Xây dựng Data Warehouse và báo cáo Power BI.</p>', 
        N'<ul><li>Thành thạo SQL, ETL</li></ul>', 20000000, 28000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 28, GETDATE()), 'Published', DATEADD(DAY, -6, GETDATE()), GETDATE(), 198);

-- More Apple Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL003')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('APL003', @AppleRec, @AppleCo, N'Kỹ sư phần mềm cấp cao', N'<p>Phát triển hệ sinh thái phần mềm Apple.</p>', 
        N'<ul><li>5+ năm kinh nghiệm</li><li>Thành thạo Swift/Objective-C</li></ul>', 45000000, 70000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 45, GETDATE()), 'Published', DATEADD(DAY, -20, GETDATE()), GETDATE(), 612);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL004')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('APL004', @AppleRec, @AppleCo, N'Nhà thiết kế UI/UX', N'<p>Thiết kế giao diện người dùng thân thiện, hiện đại.</p>', 
        N'<ul><li>3+ năm kinh nghiệm UI/UX</li><li>Thành thạo Figma, Sketch</li></ul>', 28000000, 42000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 35, GETDATE()), 'Published', DATEADD(DAY, -5, GETDATE()), GETDATE(), 289);

-- More AWS Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS003')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('AWS003', @AwsRec, @AmazonCo, N'Lập trình viên Backend (Senior)', N'<p>Kiến trúc Microservices trên nền tảng AWS.</p>', 
        N'<ul><li>4+ năm Backend</li><li>Node.js hoặc Python</li></ul>', 38000000, 58000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 52, GETDATE()), 'Published', DATEADD(DAY, -18, GETDATE()), GETDATE(), 534);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS004')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('AWS004', @AwsRec, @AmazonCo, N'Kỹ sư Bảo mật (Security)', N'<p>Đảm bảo an toàn thông tin và tuân thủ chuẩn AWS.</p>', 
        N'<ul><li>Có chứng chỉ AWS Security Specialty</li></ul>', 42000000, 65000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 60, GETDATE()), 'Published', DATEADD(DAY, -12, GETDATE()), GETDATE(), 387);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS005')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('AWS005', @AwsRec, @AmazonCo, N'Chuyên gia Khoa học dữ liệu (Data Scientist)', N'<p>Xây dựng mô hình Machine Learning trên AWS SageMaker.</p>', 
        N'<ul><li>Thành thạo Python, TensorFlow</li><li>Tư duy toán học tốt</li></ul>', 35000000, 55000000, 'VND', N'TP.HCM', 'Full-time', 
        DATEADD(DAY, 40, GETDATE()), 'Published', DATEADD(DAY, -8, GETDATE()), GETDATE(), 445);

-- More Coca-Cola Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC003')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('CC003', @CocaRec, @CocaCo, N'Lập trình viên Full Stack', N'<p>Phát triển Web App nội bộ.</p>', 
        N'<ul><li>Thành thạo .NET Core và React</li></ul>', 30000000, 45000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 38, GETDATE()), 'Published', DATEADD(DAY, -15, GETDATE()), GETDATE(), 356);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC004')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('CC004', @CocaRec, @CocaCo, N'Kỹ sư kiểm thử tự động (Automation QC)', N'<p>Viết script test tự động cho hệ thống ERP.</p>', 
        N'<ul><li>Sử dụng Selenium, Cypress</li></ul>', 22000000, 35000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 30, GETDATE()), 'Published', DATEADD(DAY, -10, GETDATE()), GETDATE(), 278);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC005')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('CC005', @CocaRec, @CocaCo, N'Scrum Master', N'<p>Quản lý quy trình Agile cho đội dự án IT.</p>', 
        N'<ul><li>Có chứng chỉ Scrum Master</li><li>Kỹ năng giải quyết vấn đề</li></ul>', 28000000, 40000000, 'VND', N'TP.HCM', 'Full-time', 
        DATEADD(DAY, 25, GETDATE()), 'Published', DATEADD(DAY, -7, GETDATE()), GETDATE(), 312);

-- More FPT Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'FPT003')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('FPT003', @FPTRec, @FPTCompany, N'Lập trình viên Python', N'<p>Phát triển Backend sử dụng Django/Flask.</p>', 
        N'<ul><li>2+ năm kinh nghiệm Python</li></ul>', 20000000, 32000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 35, GETDATE()), 'Published', DATEADD(DAY, -12, GETDATE()), GETDATE(), 298);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'FPT004')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('FPT004', @FPTRec, @FPTCompany, N'Lập trình viên Angular', N'<p>Phát triển Frontend cho dự án ngân hàng.</p>', 
        N'<ul><li>Thành thạo TypeScript, RxJS</li></ul>', 19000000, 28000000, 'VND', N'Đà Nẵng', 'Full-time', 
        DATEADD(DAY, 28, GETDATE()), 'Published', DATEADD(DAY, -8, GETDATE()), GETDATE(), 234);

-- More Samsung Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG003')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('SSG003', @SamsungRec, @SamsungCompany, N'Lập trình viên Flutter', N'<p>Phát triển Mobile App cho Samsung SmartThings.</p>', 
        N'<ul><li>Thành thạo Dart, Mobile UI/UX</li></ul>', 25000000, 38000000, 'VND', N'TP.HCM', 'Full-time', 
        DATEADD(DAY, 42, GETDATE()), 'Published', DATEADD(DAY, -16, GETDATE()), GETDATE(), 401);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG004')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('SSG004', @SamsungRec, @SamsungCompany, N'Kỹ sư IoT', N'<p>Nghiên cứu phát triển thiết bị nhà thông minh (Smart Home).</p>', 
        N'<ul><li>Hiểu biết về MQTT, Zigbee, Bluetooth LE</li></ul>', 32000000, 48000000, 'VND', N'TP.HCM', 'Full-time', 
        DATEADD(DAY, 50, GETDATE()), 'Published', DATEADD(DAY, -14, GETDATE()), GETDATE(), 367);

-- More NVIDIA Jobs
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV003')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('NV003', @NvidiaRec, @NvidiaCompany, N'Kỹ sư Thị giác máy tính (Computer Vision)', N'<p>Phát triển thuật toán AI cho xe tự hành.</p>', 
        N'<ul><li>Thành thạo OpenCV, Deep Learning</li></ul>', 40000000, 65000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 48, GETDATE()), 'Published', DATEADD(DAY, -10, GETDATE()), GETDATE(), 489);

IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV004')
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
        SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('NV004', @NvidiaRec, @NvidiaCompany, N'Kỹ sư Hiệu năng GPU', N'<p>Tối ưu hóa các ứng dụng tính toán song song.</p>', 
        N'<ul><li>Am hiểu kiến trúc GPU, lập trình C++</li></ul>', 45000000, 70000000, 'VND', N'Hà Nội', 'Full-time', 
        DATEADD(DAY, 55, GETDATE()), 'Published', DATEADD(DAY, -6, GETDATE()), GETDATE(), 423);

PRINT '=== Bước 11: Chèn Ứng tuyển (Applications) ===';

DECLARE @Job1 INT, @Job2 INT, @Job3 INT, @Job4 INT, @Job5 INT;

SELECT @Job1 = JobPostID FROM JobPosts WHERE JobCode = 'FPT001';
SELECT @Job2 = JobPostID FROM JobPosts WHERE JobCode = 'SSG001';
SELECT @Job3 = JobPostID FROM JobPosts WHERE JobCode = 'NV001';
SELECT @Job4 = JobPostID FROM JobPosts WHERE JobCode = 'APL001';
SELECT @Job5 = JobPostID FROM JobPosts WHERE JobCode = 'AWS001';

-- Insert Applications
INSERT INTO Applications (CandidateID, JobPostID, AppliedAt, Status, ResumeFilePath, Note, UpdatedAt)
SELECT @Cand1, @Job1, DATEADD(DAY, -14, GETDATE()), 'Under review', '/Content/uploads/resumes/cv1.jpg', NULL, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Applications WHERE CandidateID = @Cand1 AND JobPostID = @Job1);

INSERT INTO Applications (CandidateID, JobPostID, AppliedAt, Status, ResumeFilePath, Note, UpdatedAt)
SELECT @Cand1, @Job4, DATEADD(DAY, -10, GETDATE()), 'Shortlisted', '/Content/uploads/resumes/cv1.jpg', N'Đã phỏng vấn kỹ thuật', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Applications WHERE CandidateID = @Cand1 AND JobPostID = @Job4);

INSERT INTO Applications (CandidateID, JobPostID, AppliedAt, Status, ResumeFilePath, Note, UpdatedAt)
SELECT @Cand2, @Job1, DATEADD(DAY, -13, GETDATE()), 'Rejected', '/Content/uploads/resumes/cv2.webp', NULL, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Applications WHERE CandidateID = @Cand2 AND JobPostID = @Job1);

INSERT INTO Applications (CandidateID, JobPostID, AppliedAt, Status, ResumeFilePath, Note, UpdatedAt)
SELECT @Cand2, @Job2, DATEADD(DAY, -15, GETDATE()), 'Under review', '/Content/uploads/resumes/cv2.webp', NULL, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Applications WHERE CandidateID = @Cand2 AND JobPostID = @Job2);

INSERT INTO Applications (CandidateID, JobPostID, AppliedAt, Status, ResumeFilePath, Note, UpdatedAt)
SELECT @Cand3, @Job1, DATEADD(DAY, -12, GETDATE()), 'Interviewed', '/Content/uploads/resumes/cv3.jpg', N'Đang chờ kết quả', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Applications WHERE CandidateID = @Cand3 AND JobPostID = @Job1);

INSERT INTO Applications (CandidateID, JobPostID, AppliedAt, Status, ResumeFilePath, Note, UpdatedAt)
SELECT @Cand3, @Job5, DATEADD(DAY, -8, GETDATE()), 'Under review', '/Content/uploads/resumes/cv3.jpg', NULL, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Applications WHERE CandidateID = @Cand3 AND JobPostID = @Job5);

INSERT INTO Applications (CandidateID, JobPostID, AppliedAt, Status, ResumeFilePath, Note, UpdatedAt)
SELECT @Cand4, @Job3, DATEADD(DAY, -9, GETDATE()), 'Shortlisted', '/Content/uploads/resumes/cv4.webp', N'Portfolio ấn tượng', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Applications WHERE CandidateID = @Cand4 AND JobPostID = @Job3);

PRINT '=== Bước 12: Chèn Công việc đã lưu (SavedJobs) ===';

INSERT INTO SavedJobs (CandidateID, JobPostID, SavedAt, Note)
SELECT @Cand1, @Job3, DATEADD(DAY, -5, GETDATE()), N'Vị trí AI hấp dẫn'
WHERE NOT EXISTS (SELECT 1 FROM SavedJobs WHERE CandidateID = @Cand1 AND JobPostID = @Job3);

INSERT INTO SavedJobs (CandidateID, JobPostID, SavedAt, Note)
SELECT @Cand1, @Job5, DATEADD(DAY, -4, GETDATE()), N'Mức lương tốt'
WHERE NOT EXISTS (SELECT 1 FROM SavedJobs WHERE CandidateID = @Cand1 AND JobPostID = @Job5);

INSERT INTO SavedJobs (CandidateID, JobPostID, SavedAt, Note)
SELECT @Cand2, @Job4, DATEADD(DAY, -6, GETDATE()), NULL
WHERE NOT EXISTS (SELECT 1 FROM SavedJobs WHERE CandidateID = @Cand2 AND JobPostID = @Job4);

INSERT INTO SavedJobs (CandidateID, JobPostID, SavedAt, Note)
SELECT @Cand3, @Job2, DATEADD(DAY, -7, GETDATE()), N'Môi trường Samsung'
WHERE NOT EXISTS (SELECT 1 FROM SavedJobs WHERE CandidateID = @Cand3 AND JobPostID = @Job2);

PRINT '=== HOÀN TẤT! ===';

-- Kiểm tra lại dữ liệu
SELECT 'Summary' AS Info, 
    (SELECT COUNT(*) FROM Companies) AS Companies,
    (SELECT COUNT(*) FROM Accounts) AS Accounts,
    (SELECT COUNT(*) FROM Recruiters) AS Recruiters,
    (SELECT COUNT(*) FROM Candidates) AS Candidates,
    (SELECT COUNT(*) FROM JobPosts) AS JobPosts,
    (SELECT COUNT(*) FROM Applications) AS Applications,
    (SELECT COUNT(*) FROM SavedJobs) AS SavedJobs;

-- Hiển thị 10 bài đăng mới nhất để kiểm tra tiếng Việt
SELECT TOP 10 jp.JobCode, jp.Title, c.CompanyName, jp.ViewCount, 
    (SELECT COUNT(*) FROM Applications WHERE JobPostID = jp.JobPostID) AS Apps
FROM JobPosts jp
JOIN Companies c ON jp.CompanyID = c.CompanyID
WHERE jp.Status = 'Published'
ORDER BY jp.ViewCount DESC;

GO
-- =============================================
-- Sample Data Insert Script for JOBPORTAL_EN
-- Creates sample data for testing Analytics Dashboard
-- Generated: 2025-11-18
-- =============================================

-- Set proper collation for Vietnamese characters
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE JOBPORTAL_EN;
GO

-- Ensure proper encoding for Vietnamese characters
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'JOBPORTAL_EN' AND collation_name LIKE '%UTF8%')
BEGIN
    PRINT 'Note: Database is not using UTF8 collation. Vietnamese characters will be stored using NVARCHAR/NCHAR types with N prefix.';
END
GO

PRINT 'Starting sample data insertion...';
GO

-- =============================================
-- Insert Sample Accounts
-- =============================================
PRINT 'Inserting sample accounts...';

-- Insert 5 Recruiter Accounts
INSERT INTO Accounts (Username, PasswordHash, Email, Role, ActiveFlag, CreatedAt)
VALUES 
('recruiter1', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'recruiter1@techcorp.vn', 'Recruiter', 1, DATEADD(DAY, -90, GETDATE())),
('recruiter2', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'recruiter2@innovate.vn', 'Recruiter', 1, DATEADD(DAY, -85, GETDATE())),
('recruiter3', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'recruiter3@softdev.vn', 'Recruiter', 1, DATEADD(DAY, -80, GETDATE())),
('recruiter4', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'recruiter4@digitalhub.vn', 'Recruiter', 1, DATEADD(DAY, -75, GETDATE())),
('recruiter5', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'recruiter5@cloudtech.vn', 'Recruiter', 1, DATEADD(DAY, -70, GETDATE()));

-- Insert 15 Candidate Accounts
INSERT INTO Accounts (Username, PasswordHash, Email, Role, ActiveFlag, CreatedAt)
VALUES 
('candidate1', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'nguyenvana@gmail.com', 'Candidate', 1, DATEADD(DAY, -60, GETDATE())),
('candidate2', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'tranthib@gmail.com', 'Candidate', 1, DATEADD(DAY, -58, GETDATE())),
('candidate3', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'levanc@gmail.com', 'Candidate', 1, DATEADD(DAY, -56, GETDATE())),
('candidate4', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'phamthid@gmail.com', 'Candidate', 1, DATEADD(DAY, -54, GETDATE())),
('candidate5', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'hoangvane@gmail.com', 'Candidate', 1, DATEADD(DAY, -52, GETDATE())),
('candidate6', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'vothif@gmail.com', 'Candidate', 1, DATEADD(DAY, -50, GETDATE())),
('candidate7', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'dovang@gmail.com', 'Candidate', 1, DATEADD(DAY, -48, GETDATE())),
('candidate8', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'buithih@gmail.com', 'Candidate', 1, DATEADD(DAY, -46, GETDATE())),
('candidate9', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'dangvani@gmail.com', 'Candidate', 1, DATEADD(DAY, -44, GETDATE())),
('candidate10', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'ngothik@gmail.com', 'Candidate', 1, DATEADD(DAY, -42, GETDATE())),
('candidate11', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'lyvanl@gmail.com', 'Candidate', 1, DATEADD(DAY, -40, GETDATE())),
('candidate12', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'duongthim@gmail.com', 'Candidate', 1, DATEADD(DAY, -38, GETDATE())),
('candidate13', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'tranvann@gmail.com', 'Candidate', 1, DATEADD(DAY, -36, GETDATE())),
('candidate14', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'nguyenthio@gmail.com', 'Candidate', 1, DATEADD(DAY, -34, GETDATE())),
('candidate15', 'AQAAAAEAACcQAAAAEFvJqKzqM3h9L0YVZ5wMlvbXqP6vNXRqK9L3yT4wJ5A=', 'phanvanp@gmail.com', 'Candidate', 1, DATEADD(DAY, -32, GETDATE()));

PRINT 'Accounts inserted successfully.';
GO

-- =============================================
-- Insert Sample Companies
-- =============================================
PRINT 'Inserting sample companies...';

INSERT INTO Companies (CompanyName, Industry, Website, Address, Description, ActiveFlag, CreatedAt)
VALUES 
('TechCorp Vietnam', 'Information Technology', 'https://techcorp.vn', N'123 Láng Hạ, Ba Đình, Hà Nội', 
    '<p>TechCorp Vietnam là công ty công nghệ hàng đầu chuyên về phát triển phần mềm và giải pháp doanh nghiệp.</p>', 
    1, DATEADD(DAY, -90, GETDATE())),
    
('Innovate Solutions', 'Software Development', 'https://innovate.vn', N'456 Nguyễn Huệ, Quận 1, Hồ Chí Minh', 
    '<p>Innovate Solutions cung cấp giải pháp công nghệ sáng tạo cho doanh nghiệp Việt Nam và quốc tế.</p>', 
    1, DATEADD(DAY, -85, GETDATE())),
    
('SoftDev JSC', 'Software Development', 'https://softdev.vn', N'789 Trần Phú, Hải Châu, Đà Nẵng', 
    '<p>SoftDev JSC chuyên phát triển phần mềm outsourcing cho thị trường Nhật Bản và châu Âu.</p>', 
    1, DATEADD(DAY, -80, GETDATE())),
    
('Digital Hub', 'Digital Transformation', 'https://digitalhub.vn', N'321 Cầu Giấy, Cầu Giấy, Hà Nội', 
    '<p>Digital Hub là đơn vị tư vấn chuyển đổi số hàng đầu với nhiều dự án lớn trong nước.</p>', 
    1, DATEADD(DAY, -75, GETDATE())),
    
('CloudTech Vietnam', 'Cloud Computing', 'https://cloudtech.vn', N'654 Lê Duẩn, Quận 3, Hồ Chí Minh', 
    '<p>CloudTech Vietnam cung cấp giải pháp điện toán đám mây và hạ tầng cho doanh nghiệp.</p>', 
    1, DATEADD(DAY, -70, GETDATE()));

PRINT 'Companies inserted successfully.';
GO

-- =============================================
-- Insert Sample Recruiters
-- =============================================
PRINT 'Inserting sample recruiters...';

DECLARE @AccountIDStart INT = (SELECT MIN(AccountID) FROM Accounts WHERE Username LIKE 'recruiter%');
DECLARE @CompanyIDStart INT = (SELECT MIN(CompanyID) FROM Companies WHERE CompanyName IN ('TechCorp Vietnam', 'Innovate Solutions', 'SoftDev JSC', 'Digital Hub', 'CloudTech Vietnam'));

INSERT INTO Recruiters (AccountID, CompanyID, FullName, PositionTitle, CompanyEmail, Phone, ActiveFlag, CreatedAt)
VALUES 
(@AccountIDStart, @CompanyIDStart, N'Nguyễn Minh Tuấn', 'HR Manager', 'recruiter1@techcorp.vn', '0901234567', 1, DATEADD(DAY, -90, GETDATE())),
(@AccountIDStart + 1, @CompanyIDStart + 1, N'Trần Thu Hà', 'Talent Acquisition Lead', 'recruiter2@innovate.vn', '0902234567', 1, DATEADD(DAY, -85, GETDATE())),
(@AccountIDStart + 2, @CompanyIDStart + 2, N'Lê Văn Đức', 'Recruitment Specialist', 'recruiter3@softdev.vn', '0903234567', 1, DATEADD(DAY, -80, GETDATE())),
(@AccountIDStart + 3, @CompanyIDStart + 3, N'Phạm Thị Lan', 'Senior Recruiter', 'recruiter4@digitalhub.vn', '0904234567', 1, DATEADD(DAY, -75, GETDATE())),
(@AccountIDStart + 4, @CompanyIDStart + 4, N'Hoàng Văn Nam', 'HR Executive', 'recruiter5@cloudtech.vn', '0905234567', 1, DATEADD(DAY, -70, GETDATE()));

PRINT 'Recruiters inserted successfully.';
GO

-- =============================================
-- Insert Sample Candidates
-- =============================================
PRINT 'Inserting sample candidates...';

DECLARE @CandidateAccountStart INT = (SELECT MIN(AccountID) FROM Accounts WHERE Username LIKE 'candidate%');

INSERT INTO Candidates (AccountID, FullName, BirthDate, Gender, Phone, Email, Address, Summary, ActiveFlag, CreatedAt)
VALUES 
(@CandidateAccountStart, N'Nguyễn Văn An', '1995-03-15', 'Male', '0911234567', 'nguyenvana@gmail.com', N'123 Nguyễn Trãi, Thanh Xuân, Hà Nội', 
    N'Full Stack Developer với 3 năm kinh nghiệm', 1, DATEADD(DAY, -60, GETDATE())),
    
(@CandidateAccountStart + 1, N'Trần Thị Bích', '1996-07-20', 'Female', '0912234567', 'tranthib@gmail.com', N'456 Lê Lợi, Quận 1, Hồ Chí Minh', 
    N'Frontend Developer chuyên ReactJS', 1, DATEADD(DAY, -58, GETDATE())),
    
(@CandidateAccountStart + 2, N'Lê Văn Cường', '1994-11-10', 'Male', '0913234567', 'levanc@gmail.com', N'789 Trần Phú, Hải Châu, Đà Nẵng', 
    N'Backend Developer .NET Core', 1, DATEADD(DAY, -56, GETDATE())),
    
(@CandidateAccountStart + 3, N'Phạm Thị Dung', '1997-05-25', 'Female', '0914234567', 'phamthid@gmail.com', N'321 Hai Bà Trưng, Ba Đình, Hà Nội', 
    N'Mobile Developer Flutter/React Native', 1, DATEADD(DAY, -54, GETDATE())),
    
(@CandidateAccountStart + 4, N'Hoàng Văn Đạt', '1993-09-08', 'Male', '0915234567', 'hoangvane@gmail.com', N'654 Võ Văn Tần, Quận 3, Hồ Chí Minh', 
    N'DevOps Engineer với kinh nghiệm AWS', 1, DATEADD(DAY, -52, GETDATE())),
    
(@CandidateAccountStart + 5, N'Võ Thị Hoa', '1998-01-30', 'Female', '0916234567', 'vothif@gmail.com', N'987 Lý Thường Kiệt, Thanh Khê, Đà Nẵng', 
    N'QA Engineer automation testing', 1, DATEADD(DAY, -50, GETDATE())),
    
(@CandidateAccountStart + 6, N'Đỗ Văn Giang', '1995-12-18', 'Male', '0917234567', 'dovang@gmail.com', N'147 Nguyễn Huệ, Hoàn Kiếm, Hà Nội', 
    N'Data Analyst Python/SQL', 1, DATEADD(DAY, -48, GETDATE())),
    
(@CandidateAccountStart + 7, N'Bùi Thị Hương', '1996-06-22', 'Female', '0918234567', 'buithih@gmail.com', N'258 Điện Biên Phủ, Quận 10, Hồ Chí Minh', 
    N'UI/UX Designer Figma/Adobe XD', 1, DATEADD(DAY, -46, GETDATE())),
    
(@CandidateAccountStart + 8, N'Đặng Văn Hùng', '1994-04-14', 'Male', '0919234567', 'dangvani@gmail.com', N'369 Trưng Nữ Vương, Sơn Trà, Đà Nẵng', 
    N'Project Manager Agile/Scrum', 1, DATEADD(DAY, -44, GETDATE())),
    
(@CandidateAccountStart + 9, N'Ngô Thị Kim', '1997-08-05', 'Female', '0920234567', 'ngothik@gmail.com', N'741 Lạc Long Quân, Tây Hồ, Hà Nội', 
    N'Business Analyst IT/Banking', 1, DATEADD(DAY, -42, GETDATE())),
    
(@CandidateAccountStart + 10, N'Lý Văn Long', '1995-02-28', 'Male', '0921234567', 'lyvanl@gmail.com', N'852 Cách Mạng Tháng 8, Quận 3, Hồ Chí Minh', 
    N'Java Spring Boot Developer', 1, DATEADD(DAY, -40, GETDATE())),
    
(@CandidateAccountStart + 11, N'Dương Thị Mai', '1998-10-12', 'Female', '0922234567', 'duongthim@gmail.com', N'963 Lê Duẩn, Thanh Khê, Đà Nẵng', 
    N'Python Developer Django/FastAPI', 1, DATEADD(DAY, -38, GETDATE())),
    
(@CandidateAccountStart + 12, N'Trần Văn Nam', '1993-07-07', 'Male', '0923234567', 'tranvann@gmail.com', N'159 Giải Phóng, Hai Bà Trưng, Hà Nội', 
    N'System Administrator Linux/Windows', 1, DATEADD(DAY, -36, GETDATE())),
    
(@CandidateAccountStart + 13, N'Nguyễn Thị Oanh', '1996-11-19', 'Female', '0924234567', 'nguyenthio@gmail.com', N'357 Nguyễn Thái Học, Quận 1, Hồ Chí Minh', 
    N'Database Administrator SQL Server', 1, DATEADD(DAY, -34, GETDATE())),
    
(@CandidateAccountStart + 14, N'Phan Văn Phong', '1994-03-03', 'Male', '0925234567', 'phanvanp@gmail.com', N'753 Phan Châu Trinh, Hải Châu, Đà Nẵng', 
    N'AI/ML Engineer Computer Vision', 1, DATEADD(DAY, -32, GETDATE()));

PRINT 'Candidates inserted successfully.';
GO

-- =============================================
-- Insert Sample Job Posts with ViewCount
-- =============================================
PRINT 'Inserting sample job posts...';

-- Get IDs from newly inserted data
DECLARE @RecruiterID INT = (SELECT TOP 1 RecruiterID FROM Recruiters WHERE FullName = N'Nguyễn Minh Tuấn');
DECLARE @CompanyID INT = (SELECT TOP 1 CompanyID FROM Companies WHERE CompanyName = 'TechCorp Vietnam');

-- If sample data not found, try to get any existing recruiter/company
IF @RecruiterID IS NULL
    SET @RecruiterID = (SELECT TOP 1 RecruiterID FROM Recruiters WHERE ActiveFlag = 1);
IF @CompanyID IS NULL
    SET @CompanyID = (SELECT TOP 1 CompanyID FROM Companies WHERE ActiveFlag = 1);

IF @RecruiterID IS NULL OR @CompanyID IS NULL
BEGIN
    PRINT 'ERROR: No active Recruiter or Company found. Please run the full script to insert sample data first.';
    RETURN;
END

PRINT 'Using RecruiterID: ' + CAST(@RecruiterID AS VARCHAR) + ', CompanyID: ' + CAST(@CompanyID AS VARCHAR);

-- Job Post 1: Full Stack Developer
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2001', @RecruiterID, @CompanyID, 'Full Stack Developer (ReactJS/NodeJS)', 
    N'<p>Chúng tôi đang tìm kiếm Full Stack Developer có kinh nghiệm với ReactJS và NodeJS để tham gia vào dự án phát triển hệ thống quản lý doanh nghiệp.</p>', 
    N'<ul><li>3+ năm kinh nghiệm với ReactJS và NodeJS</li><li>Thành thạo JavaScript/TypeScript</li><li>Kinh nghiệm với SQL Server, MongoDB</li><li>Am hiểu về RESTful API, Microservices</li></ul>', 
    20000000, 35000000, 'VND', 'Hà Nội', 'Full-time', DATEADD(DAY, 30, GETDATE()), 
    'Published', DATEADD(DAY, -15, GETDATE()), GETDATE(), 245);

-- Job Post 2: Senior Backend Developer
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2002', @RecruiterID, @CompanyID, 'Senior Backend Developer (.NET Core)', 
    N'<p>Vị trí Senior Backend Developer làm việc với công nghệ .NET Core, SQL Server cho dự án banking.</p>', 
    N'<ul><li>5+ năm kinh nghiệm .NET Core/ASP.NET Core</li><li>Kinh nghiệm với Microservices architecture</li><li>Thành thạo SQL Server, Entity Framework</li><li>Ưu tiên có kinh nghiệm domain Banking/Finance</li></ul>', 
    30000000, 50000000, 'VND', 'Hà Nội', 'Full-time', DATEADD(DAY, 45, GETDATE()), 
    'Published', DATEADD(DAY, -10, GETDATE()), GETDATE(), 189);

-- Job Post 3: Frontend Developer
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2003', @RecruiterID, @CompanyID, 'Frontend Developer (VueJS/Angular)', 
    N'<p>Tham gia phát triển giao diện người dùng cho ứng dụng web E-commerce quy mô lớn.</p>', 
    N'<ul><li>2+ năm kinh nghiệm VueJS hoặc Angular</li><li>Thành thạo HTML5, CSS3, JavaScript ES6+</li><li>Am hiểu về responsive design, cross-browser compatibility</li><li>Có kinh nghiệm làm việc với REST API</li></ul>', 
    15000000, 25000000, 'VND', 'Hồ Chí Minh', 'Full-time', DATEADD(DAY, 25, GETDATE()), 
    'Published', DATEADD(DAY, -20, GETDATE()), GETDATE(), 312);

-- Job Post 4: Mobile Developer
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2004', @RecruiterID, @CompanyID, 'Mobile Developer (Flutter/React Native)', 
    N'<p>Phát triển ứng dụng mobile cho nền tảng iOS và Android sử dụng Flutter hoặc React Native.</p>', 
    N'<ul><li>2+ năm kinh nghiệm Flutter hoặc React Native</li><li>Có kinh nghiệm publish app lên AppStore/Google Play</li><li>Am hiểu về mobile UI/UX best practices</li><li>Kinh nghiệm làm việc với Firebase, REST API</li></ul>', 
    18000000, 30000000, 'VND', 'Đà Nẵng', 'Full-time', DATEADD(DAY, 35, GETDATE()), 
    'Published', DATEADD(DAY, -8, GETDATE()), GETDATE(), 156);

-- Job Post 5: DevOps Engineer
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2005', @RecruiterID, @CompanyID, 'DevOps Engineer (AWS/Docker/Kubernetes)', 
    N'<p>Xây dựng và duy trì hạ tầng cloud, CI/CD pipeline cho các dự án của công ty.</p>', 
    N'<ul><li>3+ năm kinh nghiệm DevOps</li><li>Thành thạo AWS hoặc Azure</li><li>Kinh nghiệm với Docker, Kubernetes</li><li>Am hiểu về CI/CD (Jenkins, GitLab CI, GitHub Actions)</li><li>Scripting: Bash, Python</li></ul>', 
    25000000, 45000000, 'VND', 'Hà Nội', 'Full-time', DATEADD(DAY, 40, GETDATE()), 
    'Published', DATEADD(DAY, -12, GETDATE()), GETDATE(), 178);

-- Job Post 6: QA Engineer
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2006', @RecruiterID, @CompanyID, 'QA Engineer (Manual & Automation)', 
    N'<p>Đảm bảo chất lượng sản phẩm thông qua testing manual và automation.</p>', 
    N'<ul><li>2+ năm kinh nghiệm QA/Testing</li><li>Kinh nghiệm automation testing (Selenium, Cypress, Playwright)</li><li>Am hiểu về test case design, test plan</li><li>Có kinh nghiệm API testing (Postman, RestAssured)</li></ul>', 
    12000000, 22000000, 'VND', 'Hồ Chí Minh', 'Full-time', DATEADD(DAY, 30, GETDATE()), 
    'Published', DATEADD(DAY, -5, GETDATE()), GETDATE(), 203);

-- Job Post 7: Data Analyst
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2007', @RecruiterID, @CompanyID, 'Data Analyst (Python/SQL/Power BI)', 
    N'<p>Phân tích dữ liệu kinh doanh, xây dựng dashboard và báo cáo để hỗ trợ ra quyết định.</p>', 
    N'<ul><li>2+ năm kinh nghiệm Data Analysis</li><li>Thành thạo SQL, Python (pandas, numpy)</li><li>Kinh nghiệm với Power BI, Tableau hoặc tương tự</li><li>Am hiểu về statistical analysis, data visualization</li></ul>', 
    15000000, 28000000, 'VND', 'Hà Nội', 'Full-time', DATEADD(DAY, 35, GETDATE()), 
    'Published', DATEADD(DAY, -18, GETDATE()), GETDATE(), 267);

-- Job Post 8: UI/UX Designer
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2008', @RecruiterID, @CompanyID, 'UI/UX Designer (Figma/Adobe XD)', 
    N'<p>Thiết kế giao diện và trải nghiệm người dùng cho website và mobile app.</p>', 
    N'<ul><li>2+ năm kinh nghiệm UI/UX Design</li><li>Thành thạo Figma, Adobe XD, Sketch</li><li>Portfolio thể hiện design process</li><li>Am hiểu về user research, wireframing, prototyping</li></ul>', 
    12000000, 25000000, 'VND', 'Hồ Chí Minh', 'Full-time', DATEADD(DAY, 30, GETDATE()), 
    'Published', DATEADD(DAY, -7, GETDATE()), GETDATE(), 298);

-- Job Post 9: Project Manager
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2009', @RecruiterID, @CompanyID, 'IT Project Manager (Agile/Scrum)', 
    N'<p>Quản lý các dự án phần mềm, điều phối team và đảm bảo dự án hoàn thành đúng tiến độ.</p>', 
    N'<ul><li>5+ năm kinh nghiệm quản lý dự án IT</li><li>Có chứng chỉ PMP, Scrum Master (ưu tiên)</li><li>Kinh nghiệm Agile/Scrum methodology</li><li>Kỹ năng communication và stakeholder management tốt</li></ul>', 
    25000000, 45000000, 'VND', 'Hà Nội', 'Full-time', DATEADD(DAY, 45, GETDATE()), 
    'Published', DATEADD(DAY, -14, GETDATE()), GETDATE(), 134);

-- Job Post 10: Business Analyst
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2010', @RecruiterID, @CompanyID, 'Business Analyst (IT/Banking)', 
    N'<p>Thu thập yêu cầu nghiệp vụ, phân tích và tư vấn giải pháp công nghệ cho khách hàng.</p>', 
    N'<ul><li>3+ năm kinh nghiệm Business Analyst</li><li>Kinh nghiệm domain Banking/Finance (ưu tiên)</li><li>Am hiểu về SDLC, Agile methodology</li><li>Kỹ năng documentation và presentation tốt</li></ul>', 
    18000000, 35000000, 'VND', 'Hà Nội', 'Full-time', DATEADD(DAY, 30, GETDATE()), 
    'Published', DATEADD(DAY, -9, GETDATE()), GETDATE(), 221);

-- Job Post 11: Java Developer (Closed - for testing)
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2011', @RecruiterID, @CompanyID, 'Java Spring Boot Developer', 
    N'<p>Phát triển backend services với Java Spring Boot cho hệ thống E-commerce.</p>', 
    N'<ul><li>3+ năm Java, Spring Boot</li><li>Microservices architecture</li><li>MySQL, Redis</li></ul>', 
    22000000, 40000000, 'VND', 'Hồ Chí Minh', 'Full-time', DATEADD(DAY, -5, GETDATE()), 
    'Closed', DATEADD(DAY, -45, GETDATE()), DATEADD(DAY, -3, GETDATE()), 456);

-- Job Post 12: Python Developer
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2012', @RecruiterID, @CompanyID, 'Python Developer (Django/FastAPI)', 
    N'<p>Xây dựng backend API và web services sử dụng Python Django/FastAPI.</p>', 
    N'<ul><li>2+ năm kinh nghiệm Python</li><li>Thành thạo Django hoặc FastAPI</li><li>PostgreSQL, MongoDB</li><li>Có kinh nghiệm với Celery, Redis</li></ul>', 
    18000000, 32000000, 'VND', 'Đà Nẵng', 'Full-time', DATEADD(DAY, 40, GETDATE()), 
    'Published', DATEADD(DAY, -6, GETDATE()), GETDATE(), 187);

-- Job Post 13: System Administrator
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2013', @RecruiterID, @CompanyID, 'System Administrator (Linux/Windows)', 
    N'<p>Quản trị hệ thống server, đảm bảo uptime và performance của infrastructure.</p>', 
    N'<ul><li>3+ năm kinh nghiệm System Admin</li><li>Thành thạo Linux (Ubuntu/CentOS) và Windows Server</li><li>Kinh nghiệm với VMware, Hyper-V</li><li>Am hiểu về network, security</li></ul>', 
    15000000, 28000000, 'VND', 'Hà Nội', 'Full-time', DATEADD(DAY, 35, GETDATE()), 
    'Published', DATEADD(DAY, -11, GETDATE()), GETDATE(), 142);

-- Job Post 14: Security Engineer (Expired - for testing)
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2014', @RecruiterID, @CompanyID, 'Security Engineer (Penetration Testing)', 
    N'<p>Thực hiện penetration testing, vulnerability assessment cho hệ thống.</p>', 
    N'<ul><li>3+ năm kinh nghiệm Security</li><li>CEH, OSCP (ưu tiên)</li><li>Penetration testing tools</li></ul>', 
    25000000, 45000000, 'VND', 'Hà Nội', 'Full-time', DATEADD(DAY, -2, GETDATE()), 
    'Expired', DATEADD(DAY, -35, GETDATE()), DATEADD(DAY, -1, GETDATE()), 289);

-- Job Post 15: Content Writer
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2015', @RecruiterID, @CompanyID, 'Technical Content Writer', 
    N'<p>Viết content kỹ thuật, tài liệu sản phẩm, blog posts về công nghệ.</p>', 
    N'<ul><li>2+ năm kinh nghiệm Technical Writing</li><li>Tiếng Anh tốt</li><li>Am hiểu về software development</li><li>SEO writing là lợi thế</li></ul>', 
    10000000, 18000000, 'VND', 'Remote', 'Full-time', DATEADD(DAY, 25, GETDATE()), 
    'Published', DATEADD(DAY, -4, GETDATE()), GETDATE(), 176);

-- Job Post 16: Database Administrator
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2016', @RecruiterID, @CompanyID, 'Database Administrator (SQL Server/PostgreSQL)', 
    N'<p>Quản trị database, performance tuning, backup và disaster recovery.</p>', 
    N'<ul><li>3+ năm kinh nghiệm DBA</li><li>Thành thạo SQL Server hoặc PostgreSQL</li><li>Database performance tuning</li><li>High availability, replication</li></ul>', 
    20000000, 38000000, 'VND', 'Hồ Chí Minh', 'Full-time', DATEADD(DAY, 40, GETDATE()), 
    'Published', DATEADD(DAY, -13, GETDATE()), GETDATE(), 168);

-- Job Post 17: AI/ML Engineer
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2017', @RecruiterID, @CompanyID, 'AI/ML Engineer (Computer Vision/NLP)', 
    N'<p>Nghiên cứu và phát triển các mô hình AI/ML cho sản phẩm công ty.</p>', 
    N'<ul><li>2+ năm kinh nghiệm AI/ML</li><li>Python, TensorFlow, PyTorch</li><li>Computer Vision hoặc NLP</li><li>Có paper/project thực tế</li></ul>', 
    25000000, 50000000, 'VND', 'Hà Nội', 'Full-time', DATEADD(DAY, 50, GETDATE()), 
    'Published', DATEADD(DAY, -3, GETDATE()), GETDATE(), 234);

-- Job Post 18: Sales Executive
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2018', @RecruiterID, @CompanyID, 'IT Sales Executive (B2B)', 
    N'<p>Tư vấn và bán các giải pháp công nghệ cho khách hàng doanh nghiệp.</p>', 
    N'<ul><li>2+ năm kinh nghiệm IT Sales</li><li>Am hiểu về IT solutions, cloud services</li><li>Kỹ năng negotiation, presentation tốt</li><li>Có network khách hàng là lợi thế</li></ul>', 
    12000000, 30000000, 'VND', 'Hồ Chí Minh', 'Full-time', DATEADD(DAY, 30, GETDATE()), 
    'Published', DATEADD(DAY, -16, GETDATE()), GETDATE(), 195);

-- Job Post 19: Product Manager
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2019', @RecruiterID, @CompanyID, 'Product Manager (SaaS/Mobile App)', 
    N'<p>Quản lý sản phẩm từ ý tưởng đến launch, làm việc với engineering và design team.</p>', 
    N'<ul><li>3+ năm kinh nghiệm Product Management</li><li>Kinh nghiệm với SaaS hoặc Mobile product</li><li>Am hiểu về product roadmap, user research</li><li>Data-driven decision making</li></ul>', 
    28000000, 50000000, 'VND', 'Hà Nội', 'Full-time', DATEADD(DAY, 45, GETDATE()), 
    'Published', DATEADD(DAY, -8, GETDATE()), GETDATE(), 159);

-- Job Post 20: HR Executive
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, 
    SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, 
    Status, PostedAt, UpdatedAt, ViewCount)
VALUES 
('JOB2020', @RecruiterID, @CompanyID, 'HR Executive (IT Recruitment)', 
    N'<p>Tuyển dụng nhân sự IT, xây dựng employer branding và employee engagement.</p>', 
    N'<ul><li>2+ năm kinh nghiệm HR/Recruitment</li><li>Kinh nghiệm tuyển dụng IT là lợi thế</li><li>Kỹ năng sourcing, interviewing tốt</li><li>Am hiểu về các role IT</li></ul>', 
    10000000, 18000000, 'VND', 'Hà Nội', 'Full-time', DATEADD(DAY, 30, GETDATE()), 
    'Published', DATEADD(DAY, -5, GETDATE()), GETDATE(), 213);

PRINT 'Job posts inserted successfully.';
GO

-- =============================================
-- Insert JobPostDetails for all jobs
-- =============================================
PRINT 'Inserting job post details...';

INSERT INTO JobPostDetails (JobPostID, Industry, Major, YearsExperience, DegreeRequired, 
    Skills, Headcount, GenderRequirement, AgeFrom, AgeTo, Status)
SELECT 
    jp.JobPostID,
    CASE 
        WHEN jp.Title LIKE '%Full Stack%' OR jp.Title LIKE '%Backend%' OR jp.Title LIKE '%Frontend%' THEN 'Information Technology'
        WHEN jp.Title LIKE '%Mobile%' THEN 'Mobile Development'
        WHEN jp.Title LIKE '%DevOps%' THEN 'DevOps'
        WHEN jp.Title LIKE '%QA%' OR jp.Title LIKE '%Test%' THEN 'Quality Assurance'
        WHEN jp.Title LIKE '%Data%' THEN 'Data Science'
        WHEN jp.Title LIKE '%Designer%' THEN 'Design'
        WHEN jp.Title LIKE '%Manager%' THEN 'Management'
        WHEN jp.Title LIKE '%Analyst%' THEN 'Business Analysis'
        WHEN jp.Title LIKE '%Java%' OR jp.Title LIKE '%Python%' THEN 'Software Development'
        WHEN jp.Title LIKE '%System%' OR jp.Title LIKE '%Database%' THEN 'System Administration'
        WHEN jp.Title LIKE '%Security%' THEN 'Information Security'
        WHEN jp.Title LIKE '%AI%' OR jp.Title LIKE '%ML%' THEN 'Artificial Intelligence'
        ELSE 'Other'
    END AS Industry,
    'Computer Science' AS Major,
    CASE 
        WHEN jp.Title LIKE '%Senior%' THEN 5
        WHEN jp.SalaryFrom > 20000000 THEN 3
        ELSE 2
    END AS YearsExperience,
    'Bachelor' AS DegreeRequired,
    CASE 
        WHEN jp.Title LIKE '%Full Stack%' THEN 'ReactJS, NodeJS, TypeScript, SQL Server, MongoDB'
        WHEN jp.Title LIKE '%Backend%' THEN '.NET Core, C#, SQL Server, Microservices, API Design'
        WHEN jp.Title LIKE '%Frontend%' THEN 'VueJS, Angular, HTML5, CSS3, JavaScript, Responsive Design'
        WHEN jp.Title LIKE '%Mobile%' THEN 'Flutter, React Native, iOS, Android, Firebase'
        WHEN jp.Title LIKE '%DevOps%' THEN 'AWS, Docker, Kubernetes, CI/CD, Jenkins, Terraform'
        WHEN jp.Title LIKE '%QA%' THEN 'Selenium, Cypress, Postman, Test Automation, Manual Testing'
        WHEN jp.Title LIKE '%Data%' THEN 'Python, SQL, Power BI, Tableau, Data Analysis'
        WHEN jp.Title LIKE '%Designer%' THEN 'Figma, Adobe XD, Sketch, UI Design, UX Research'
        WHEN jp.Title LIKE '%Java%' THEN 'Java, Spring Boot, Hibernate, MySQL, Microservices'
        WHEN jp.Title LIKE '%Python%' THEN 'Python, Django, FastAPI, PostgreSQL, Redis'
        ELSE 'Communication, Teamwork, Problem Solving'
    END AS Skills,
    CASE 
        WHEN jp.Title LIKE '%Senior%' OR jp.Title LIKE '%Manager%' THEN 1
        ELSE FLOOR(RAND() * 3) + 2
    END AS Headcount,
    'Not required' AS GenderRequirement,
    22 AS AgeFrom,
    40 AS AgeTo,
    jp.Status
FROM JobPosts jp
WHERE jp.JobCode LIKE 'JOB20%' 
AND NOT EXISTS (SELECT 1 FROM JobPostDetails WHERE JobPostID = jp.JobPostID);

PRINT 'Job post details inserted successfully.';
GO

-- =============================================
-- Insert Sample Applications
-- =============================================
PRINT 'Inserting sample applications...';

-- Get sample candidate IDs (prioritize newly inserted candidates)
DECLARE @CandidateIDs TABLE (CandidateID INT, RowNum INT);
INSERT INTO @CandidateIDs
SELECT TOP 15 CandidateID, ROW_NUMBER() OVER (ORDER BY CandidateID) AS RowNum
FROM Candidates 
WHERE ActiveFlag = 1
ORDER BY CreatedAt DESC;

-- Insert applications for each job with varying numbers
DECLARE @JobPostID INT, @CandidateID INT;
DECLARE @Counter INT = 1;
DECLARE @JobCounter INT;
DECLARE @ApplicationCount INT;

DECLARE job_cursor CURSOR FOR
SELECT JobPostID FROM JobPosts WHERE JobCode LIKE 'JOB20%';

OPEN job_cursor;
FETCH NEXT FROM job_cursor INTO @JobPostID;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Random number of applications per job (0-15)
    SET @ApplicationCount = FLOOR(RAND() * 16);
    SET @JobCounter = 0;
    
    WHILE @JobCounter < @ApplicationCount
    BEGIN
        -- Get random candidate
        SELECT TOP 1 @CandidateID = CandidateID 
        FROM @CandidateIDs 
        ORDER BY NEWID();
        
        -- Check if application already exists
        IF NOT EXISTS (
            SELECT 1 FROM Applications 
            WHERE JobPostID = @JobPostID AND CandidateID = @CandidateID
        )
        BEGIN
            INSERT INTO Applications (JobPostID, CandidateID, AppliedAt, Status, 
                ResumeFilePath, CertificateFilePath, Note, UpdatedAt)
            VALUES (
                @JobPostID,
                @CandidateID,
                DATEADD(DAY, -FLOOR(RAND() * 20), GETDATE()),
                CASE FLOOR(RAND() * 5)
                    WHEN 0 THEN 'Submitted'
                    WHEN 1 THEN 'Under review'
                    WHEN 2 THEN 'Interview'
                    WHEN 3 THEN 'Offered'
                    ELSE 'Rejected'
                END,
                '~/Uploads/Resumes/resume_' + CAST(@CandidateID AS VARCHAR) + '.pdf',
                '~/Uploads/Certificates/cert_' + CAST(@CandidateID AS VARCHAR) + '.pdf',
                N'Tôi rất quan tâm đến vị trí này và tin rằng kinh nghiệm của tôi phù hợp với yêu cầu công việc.',
                GETDATE()
            );
        END
        
        SET @JobCounter = @JobCounter + 1;
    END
    
    FETCH NEXT FROM job_cursor INTO @JobPostID;
END

CLOSE job_cursor;
DEALLOCATE job_cursor;

PRINT 'Applications inserted successfully.';
GO

-- =============================================
-- Summary Report
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'Sample Data Insertion Summary';
PRINT '========================================';
PRINT 'Total Job Posts: ' + CAST((SELECT COUNT(*) FROM JobPosts WHERE JobCode LIKE 'JOB20%') AS VARCHAR);
PRINT 'Total Job Post Details: ' + CAST((SELECT COUNT(*) FROM JobPostDetails jpd 
    INNER JOIN JobPosts jp ON jpd.JobPostID = jp.JobPostID 
    WHERE jp.JobCode LIKE 'JOB20%') AS VARCHAR);
PRINT 'Total Applications: ' + CAST((SELECT COUNT(*) FROM Applications a 
    INNER JOIN JobPosts jp ON a.JobPostID = jp.JobPostID 
    WHERE jp.JobCode LIKE 'JOB20%') AS VARCHAR);
PRINT '';
PRINT 'Job Status Distribution:';
PRINT 'Published: ' + CAST((SELECT COUNT(*) FROM JobPosts WHERE JobCode LIKE 'JOB20%' AND Status = 'Published') AS VARCHAR);
PRINT 'Closed: ' + CAST((SELECT COUNT(*) FROM JobPosts WHERE JobCode LIKE 'JOB20%' AND Status = 'Closed') AS VARCHAR);
PRINT 'Expired: ' + CAST((SELECT COUNT(*) FROM JobPosts WHERE JobCode LIKE 'JOB20%' AND Status = 'Expired') AS VARCHAR);
PRINT '';
PRINT 'Analytics Summary:';
SELECT 
    'Total Views' AS Metric,
    SUM(ViewCount) AS Value
FROM JobPosts 
WHERE JobCode LIKE 'JOB20%'
UNION ALL
SELECT 
    'Avg Views per Job' AS Metric,
    AVG(ViewCount) AS Value
FROM JobPosts 
WHERE JobCode LIKE 'JOB20%'
UNION ALL
SELECT 
    'Total Applications' AS Metric,
    COUNT(*) AS Value
FROM Applications a
INNER JOIN JobPosts jp ON a.JobPostID = jp.JobPostID
WHERE jp.JobCode LIKE 'JOB20%';

PRINT '';
PRINT 'Complete Data Summary:';
PRINT 'Total Accounts: ' + CAST((SELECT COUNT(*) FROM Accounts WHERE Username LIKE 'recruiter%' OR Username LIKE 'candidate%') AS VARCHAR);
PRINT 'Total Companies: ' + CAST((SELECT COUNT(*) FROM Companies WHERE CompanyName IN ('TechCorp Vietnam', 'Innovate Solutions', 'SoftDev JSC', 'Digital Hub', 'CloudTech Vietnam')) AS VARCHAR);
PRINT 'Total Recruiters: ' + CAST((SELECT COUNT(*) FROM Recruiters WHERE FullName IN (N'Nguyễn Minh Tuấn', N'Trần Thu Hà', N'Lê Văn Đức', N'Phạm Thị Lan', N'Hoàng Văn Nam')) AS VARCHAR);
PRINT 'Total Candidates: ' + CAST((SELECT COUNT(*) FROM Candidates WHERE FullName LIKE N'%Văn%' OR FullName LIKE N'%Thị%') AS VARCHAR);
PRINT '';
PRINT 'Sample data insertion completed successfully!';
PRINT '========================================';
GO

-- =============================================
-- Note: WorkExperiences, Education, and Skills tables are not included
-- These tables do not exist in the current database schema
-- =============================================

-- ==========================================
-- File: AdditionalJobPostsAndApplications.sql
-- Mục đích: Thêm 15 JobPosts cho mỗi công ty và 12 đơn ứng tuyển cho mỗi JobPost
-- Hướng dẫn: Chạy file này SAU KHI chạy CleanAndInsertSampleData.sql
-- ==========================================

USE JOBPORTAL_EN;
GO

PRINT '=== Bắt đầu chèn JobPosts bổ sung ===';

-- Lấy các ID công ty và recruiter
DECLARE @AppleCo INT, @AmazonCo INT, @CocaCo INT, @NvidiaCo INT, @SamsungCo INT;
DECLARE @AppleRec INT, @AwsRec INT, @CocaRec INT, @NvidiaRec INT, @SamsungRec INT;

SELECT @AppleCo = CompanyID FROM Companies WHERE CompanyName = N'Công ty TNHH Apple Việt Nam';
SELECT @AmazonCo = CompanyID FROM Companies WHERE CompanyName = 'Amazon Web Services Vietnam';
SELECT @CocaCo = CompanyID FROM Companies WHERE CompanyName = N'Coca-Cola Việt Nam';
SELECT @NvidiaCo = CompanyID FROM Companies WHERE CompanyName = 'NVIDIA Vietnam';
SELECT @SamsungCo = CompanyID FROM Companies WHERE CompanyName = N'Samsung Electronics Việt Nam';

SELECT @AppleRec = RecruiterID FROM Recruiters r JOIN Accounts a ON r.AccountID = a.AccountID WHERE a.Username = 'appleHR';
SELECT @AwsRec = RecruiterID FROM Recruiters r JOIN Accounts a ON r.AccountID = a.AccountID WHERE a.Username = 'awsHR';
SELECT @CocaRec = RecruiterID FROM Recruiters r JOIN Accounts a ON r.AccountID = a.AccountID WHERE a.Username = 'cocaHR';
SELECT @NvidiaRec = RecruiterID FROM Recruiters r JOIN Accounts a ON r.AccountID = a.AccountID WHERE a.Username = 'nvidiaHR';
SELECT @SamsungRec = RecruiterID FROM Recruiters r JOIN Accounts a ON r.AccountID = a.AccountID WHERE a.Username = 'samsungHR';

-- Lấy danh sách CandidateID
DECLARE @Cand1 INT, @Cand2 INT, @Cand3 INT, @Cand4 INT, @Cand5 INT;
DECLARE @Cand6 INT, @Cand7 INT, @Cand8 INT, @Cand9 INT, @Cand10 INT;
DECLARE @Cand11 INT, @Cand12 INT, @Cand13 INT, @Cand14 INT, @Cand15 INT;

SELECT @Cand1 = CandidateID FROM Candidates WHERE FullName = N'Nguyễn Văn An';
SELECT @Cand2 = CandidateID FROM Candidates WHERE FullName = N'Trần Thị Bích';
SELECT @Cand3 = CandidateID FROM Candidates WHERE FullName = N'Lê Văn Cường';
SELECT @Cand4 = CandidateID FROM Candidates WHERE FullName = N'Phạm Thị Dung';
SELECT @Cand5 = CandidateID FROM Candidates WHERE FullName = N'Hoàng Văn Đức';
SELECT @Cand6 = CandidateID FROM Candidates WHERE FullName = N'Trần Thị Hoa';
SELECT @Cand7 = CandidateID FROM Candidates WHERE FullName = N'Nguyễn Thị Giang';
SELECT @Cand8 = CandidateID FROM Candidates WHERE FullName = N'Trần Văn Khánh';
SELECT @Cand9 = CandidateID FROM Candidates WHERE FullName = N'Võ Minh Long';
SELECT @Cand10 = CandidateID FROM Candidates WHERE FullName = N'Nguyễn Văn Minh';
SELECT @Cand11 = CandidateID FROM Candidates WHERE FullName = N'Trần Thị Nga';
SELECT @Cand12 = CandidateID FROM Candidates WHERE FullName = N'Lê Thị Phương';
SELECT @Cand13 = CandidateID FROM Candidates WHERE FullName = N'Vũ Đức Quân';
SELECT @Cand14 = CandidateID FROM Candidates WHERE FullName = N'Nguyễn Thanh Sơn';
SELECT @Cand15 = CandidateID FROM Candidates WHERE FullName = N'Trương Thu Hà';

PRINT '=== Thêm JobPosts cho Apple (11 jobs còn lại) ===';

-- Apple Job 5
IF NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL005')
BEGIN
    INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
    VALUES ('APL005', @AppleRec, @AppleCo, N'Kỹ sư Machine Learning', N'<p>Phát triển các mô hình ML cho Siri và Apple Intelligence.</p>', N'<ul><li>4+ năm ML</li><li>Python, TensorFlow</li></ul>', 40000000, 65000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 55, GETDATE()), 'Published', DATEADD(DAY, -22, GETDATE()), GETDATE(), 487);
END

-- Apple Job 6-15
INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'APL006', @AppleRec, @AppleCo, N'Product Manager - Apple Watch', N'<p>Quản lý sản phẩm Apple Watch tại thị trường Việt Nam.</p>', N'<ul><li>5+ năm Product Management</li></ul>', 50000000, 80000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 60, GETDATE()), 'Published', DATEADD(DAY, -19, GETDATE()), GETDATE(), 542
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL006');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'APL007', @AppleRec, @AppleCo, N'Technical Writer (iOS)', N'<p>Viết tài liệu kỹ thuật cho developer iOS.</p>', N'<ul><li>Có kinh nghiệm technical writing</li></ul>', 25000000, 38000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 45, GETDATE()), 'Published', DATEADD(DAY, -10, GETDATE()), GETDATE(), 234
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL007');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'APL008', @AppleRec, @AppleCo, N'Senior QA Engineer', N'<p>Kiểm thử chất lượng ứng dụng iOS/macOS.</p>', N'<ul><li>4+ năm QA</li><li>Automation Testing</li></ul>', 32000000, 48000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 50, GETDATE()), 'Published', DATEADD(DAY, -15, GETDATE()), GETDATE(), 356
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL008');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'APL009', @AppleRec, @AppleCo, N'Cloud Engineer - iCloud', N'<p>Vận hành hạ tầng iCloud Services.</p>', N'<ul><li>Kinh nghiệm AWS/Azure</li></ul>', 38000000, 58000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 48, GETDATE()), 'Published', DATEADD(DAY, -12, GETDATE()), GETDATE(), 401
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL009');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'APL010', @AppleRec, @AppleCo, N'iOS Security Engineer', N'<p>Bảo mật ứng dụng iOS, phát hiện lỗ hổng.</p>', N'<ul><li>Kinh nghiệm iOS Security</li></ul>', 45000000, 70000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 52, GETDATE()), 'Published', DATEADD(DAY, -17, GETDATE()), GETDATE(), 478
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL010');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'APL011', @AppleRec, @AppleCo, N'Data Scientist - Apple Services', N'<p>Phân tích dữ liệu người dùng Apple Services.</p>', N'<ul><li>Python, R, SQL</li></ul>', 38000000, 60000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 47, GETDATE()), 'Published', DATEADD(DAY, -13, GETDATE()), GETDATE(), 389
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL011');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'APL012', @AppleRec, @AppleCo, N'Retail Operations Manager', N'<p>Quản lý vận hành chuỗi cửa hàng Apple Store.</p>', N'<ul><li>5+ năm quản lý bán lẻ</li></ul>', 40000000, 60000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 43, GETDATE()), 'Published', DATEADD(DAY, -9, GETDATE()), GETDATE(), 298
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL012');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'APL013', @AppleRec, @AppleCo, N'DevOps Engineer - CI/CD', N'<p>Xây dựng pipeline CI/CD cho ứng dụng Apple.</p>', N'<ul><li>Jenkins, GitLab CI</li></ul>', 35000000, 52000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 49, GETDATE()), 'Published', DATEADD(DAY, -11, GETDATE()), GETDATE(), 412
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL013');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'APL014', @AppleRec, @AppleCo, N'Marketing Analyst', N'<p>Phân tích hiệu quả chiến dịch marketing Apple.</p>', N'<ul><li>Google Analytics, SQL</li></ul>', 25000000, 38000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 42, GETDATE()), 'Published', DATEADD(DAY, -8, GETDATE()), GETDATE(), 267
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL014');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'APL015', @AppleRec, @AppleCo, N'Supply Chain Analyst', N'<p>Tối ưu hóa chuỗi cung ứng sản phẩm Apple.</p>', N'<ul><li>Excel, ERP systems</li></ul>', 28000000, 42000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 44, GETDATE()), 'Published', DATEADD(DAY, -7, GETDATE()), GETDATE(), 321
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'APL015');

PRINT '=== Thêm JobPosts cho AWS (10 jobs còn lại) ===';

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'AWS006', @AwsRec, @AmazonCo, N'Solutions Engineer - Containers', N'<p>Hỗ trợ khách hàng triển khai container trên AWS ECS/EKS.</p>', N'<ul><li>Docker, Kubernetes</li></ul>', 40000000, 62000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 54, GETDATE()), 'Published', DATEADD(DAY, -20, GETDATE()), GETDATE(), 498
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS006');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'AWS007', @AwsRec, @AmazonCo, N'Technical Account Manager', N'<p>Quản lý tài khoản khách hàng doanh nghiệp sử dụng AWS.</p>', N'<ul><li>Kỹ năng giao tiếp, AWS knowledge</li></ul>', 45000000, 68000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 58, GETDATE()), 'Published', DATEADD(DAY, -18, GETDATE()), GETDATE(), 542
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS007');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'AWS008', @AwsRec, @AmazonCo, N'Network Engineer - AWS', N'<p>Quản lý và tối ưu hạ tầng mạng AWS VPC, Transit Gateway.</p>', N'<ul><li>Networking, AWS VPC</li></ul>', 38000000, 58000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 50, GETDATE()), 'Published', DATEADD(DAY, -14, GETDATE()), GETDATE(), 423
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS008');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'AWS009', @AwsRec, @AmazonCo, N'Database Administrator - RDS/Aurora', N'<p>Quản trị cơ sở dữ liệu AWS RDS và Aurora.</p>', N'<ul><li>PostgreSQL, MySQL, Aurora</li></ul>', 35000000, 52000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 46, GETDATE()), 'Published', DATEADD(DAY, -12, GETDATE()), GETDATE(), 378
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS009');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'AWS010', @AwsRec, @AmazonCo, N'Cloud Cost Optimization Specialist', N'<p>Tối ưu chi phí AWS cho khách hàng doanh nghiệp.</p>', N'<ul><li>AWS Billing, Cost Explorer</li></ul>', 32000000, 48000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 44, GETDATE()), 'Published', DATEADD(DAY, -10, GETDATE()), GETDATE(), 334
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS010');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'AWS011', @AwsRec, @AmazonCo, N'Technical Trainer - AWS Certification', N'<p>Đào tạo kỹ thuật và chứng chỉ AWS.</p>', N'<ul><li>Có chứng chỉ AWS Professional</li></ul>', 30000000, 45000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 42, GETDATE()), 'Published', DATEADD(DAY, -9, GETDATE()), GETDATE(), 289
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS011');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'AWS012', @AwsRec, @AmazonCo, N'Serverless Engineer', N'<p>Xây dựng ứng dụng serverless trên AWS Lambda.</p>', N'<ul><li>AWS Lambda, API Gateway, DynamoDB</li></ul>', 36000000, 54000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 48, GETDATE()), 'Published', DATEADD(DAY, -11, GETDATE()), GETDATE(), 401
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS012');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'AWS013', @AwsRec, @AmazonCo, N'Big Data Engineer - EMR/Redshift', N'<p>Xây dựng data pipeline với AWS EMR và Redshift.</p>', N'<ul><li>Spark, Hadoop, Redshift</li></ul>', 40000000, 62000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 52, GETDATE()), 'Published', DATEADD(DAY, -13, GETDATE()), GETDATE(), 456
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS013');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'AWS014', @AwsRec, @AmazonCo, N'IoT Solutions Architect', N'<p>Thiết kế giải pháp IoT trên AWS IoT Core.</p>', N'<ul><li>IoT, MQTT, AWS IoT</li></ul>', 42000000, 65000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 56, GETDATE()), 'Published', DATEADD(DAY, -15, GETDATE()), GETDATE(), 487
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS014');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'AWS015', @AwsRec, @AmazonCo, N'Cloud Migration Engineer', N'<p>Di chuyển ứng dụng on-premise lên AWS Cloud.</p>', N'<ul><li>AWS Migration Services</li></ul>', 38000000, 58000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 50, GETDATE()), 'Published', DATEADD(DAY, -12, GETDATE()), GETDATE(), 412
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'AWS015');

PRINT '=== Thêm JobPosts cho Coca-Cola (10 jobs còn lại) ===';

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'CC006', @CocaRec, @CocaCo, N'ERP Developer (SAP/Oracle)', N'<p>Phát triển và tùy chỉnh hệ thống ERP.</p>', N'<ul><li>SAP hoặc Oracle ERP</li></ul>', 35000000, 52000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 46, GETDATE()), 'Published', DATEADD(DAY, -16, GETDATE()), GETDATE(), 367
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC006');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'CC007', @CocaRec, @CocaCo, N'IT Support Specialist', N'<p>Hỗ trợ kỹ thuật IT cho nhân viên nội bộ.</p>', N'<ul><li>Windows, Office 365</li></ul>', 18000000, 28000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 35, GETDATE()), 'Published', DATEADD(DAY, -8, GETDATE()), GETDATE(), 234
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC007');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'CC008', @CocaRec, @CocaCo, N'Mobile App Developer (React Native)', N'<p>Phát triển ứng dụng Mobile cho nhân viên bán hàng.</p>', N'<ul><li>React Native, JavaScript</li></ul>', 28000000, 42000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 40, GETDATE()), 'Published', DATEADD(DAY, -12, GETDATE()), GETDATE(), 312
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC008');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'CC009', @CocaRec, @CocaCo, N'Network Administrator', N'<p>Quản trị mạng nội bộ và VPN.</p>', N'<ul><li>Cisco, Networking</li></ul>', 25000000, 38000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 38, GETDATE()), 'Published', DATEADD(DAY, -10, GETDATE()), GETDATE(), 289
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC009');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'CC010', @CocaRec, @CocaCo, N'Cybersecurity Analyst', N'<p>Giám sát và phản ứng sự cố an ninh mạng.</p>', N'<ul><li>Security monitoring, SIEM</li></ul>', 32000000, 48000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 42, GETDATE()), 'Published', DATEADD(DAY, -14, GETDATE()), GETDATE(), 356
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC010');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'CC011', @CocaRec, @CocaCo, N'SharePoint Developer', N'<p>Phát triển và quản lý SharePoint nội bộ.</p>', N'<ul><li>SharePoint, PowerApps</li></ul>', 30000000, 45000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 39, GETDATE()), 'Published', DATEADD(DAY, -11, GETDATE()), GETDATE(), 298
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC011');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'CC012', @CocaRec, @CocaCo, N'IT Project Manager', N'<p>Quản lý dự án triển khai hệ thống IT.</p>', N'<ul><li>PMP, ITIL</li></ul>', 40000000, 60000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 44, GETDATE()), 'Published', DATEADD(DAY, -13, GETDATE()), GETDATE(), 378
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC012');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'CC013', @CocaRec, @CocaCo, N'Data Warehouse Developer', N'<p>Xây dựng Data Warehouse cho phân tích kinh doanh.</p>', N'<ul><li>SQL Server, SSIS, SSRS</li></ul>', 32000000, 48000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 41, GETDATE()), 'Published', DATEADD(DAY, -9, GETDATE()), GETDATE(), 323
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC013');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'CC014', @CocaRec, @CocaCo, N'RPA Developer (UiPath/Automation Anywhere)', N'<p>Phát triển bot tự động hóa quy trình.</p>', N'<ul><li>UiPath, Automation Anywhere</li></ul>', 30000000, 45000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 37, GETDATE()), 'Published', DATEADD(DAY, -7, GETDATE()), GETDATE(), 267
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC014');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'CC015', @CocaRec, @CocaCo, N'UX Researcher', N'<p>Nghiên cứu trải nghiệm người dùng cho ứng dụng nội bộ.</p>', N'<ul><li>UX Research, User Testing</li></ul>', 28000000, 42000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 36, GETDATE()), 'Published', DATEADD(DAY, -6, GETDATE()), GETDATE(), 245
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'CC015');

PRINT '=== Thêm JobPosts cho NVIDIA (11 jobs còn lại) ===';

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'NV005', @NvidiaRec, @NvidiaCo, N'Graphics Software Engineer', N'<p>Phát triển driver đồ họa cho GPU NVIDIA.</p>', N'<ul><li>C++, Graphics API (Vulkan/DirectX)</li></ul>', 45000000, 72000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 57, GETDATE()), 'Published', DATEADD(DAY, -19, GETDATE()), GETDATE(), 512
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV005');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'NV006', @NvidiaRec, @NvidiaCo, N'AI Research Scientist - NLP', N'<p>Nghiên cứu Natural Language Processing.</p>', N'<ul><li>PhD preferred, NLP, Transformers</li></ul>', 50000000, 80000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 60, GETDATE()), 'Published', DATEADD(DAY, -21, GETDATE()), GETDATE(), 589
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV006');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'NV007', @NvidiaRec, @NvidiaCo, N'Robotics Engineer', N'<p>Phát triển hệ thống robot tự hành với NVIDIA Jetson.</p>', N'<ul><li>ROS, Computer Vision, Embedded</li></ul>', 42000000, 68000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 54, GETDATE()), 'Published', DATEADD(DAY, -17, GETDATE()), GETDATE(), 478
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV007');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'NV008', @NvidiaRec, @NvidiaCo, N'Technical Marketing Engineer', N'<p>Quảng bá kỹ thuật sản phẩm GPU NVIDIA.</p>', N'<ul><li>Background kỹ thuật + Marketing</li></ul>', 38000000, 58000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 50, GETDATE()), 'Published', DATEADD(DAY, -14, GETDATE()), GETDATE(), 398
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV008');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'NV009', @NvidiaRec, @NvidiaCo, N'Autonomous Driving Engineer', N'<p>Phát triển phần mềm lái xe tự động sử dụng NVIDIA DRIVE.</p>', N'<ul><li>C++, Sensor Fusion, Deep Learning</li></ul>', 48000000, 75000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 58, GETDATE()), 'Published', DATEADD(DAY, -20, GETDATE()), GETDATE(), 534
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV009');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'NV010', @NvidiaRec, @NvidiaCo, N'Game Developer - Ray Tracing', N'<p>Phát triển game engine với công nghệ Ray Tracing.</p>', N'<ul><li>C++, Unreal/Unity, RTX</li></ul>', 40000000, 65000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 52, GETDATE()), 'Published', DATEADD(DAY, -16, GETDATE()), GETDATE(), 456
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV010');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'NV011', @NvidiaRec, @NvidiaCo, N'Data Center Engineer', N'<p>Vận hành GPU cluster cho AI training.</p>', N'<ul><li>Linux, Kubernetes, GPU Infrastructure</li></ul>', 42000000, 65000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 55, GETDATE()), 'Published', DATEADD(DAY, -18, GETDATE()), GETDATE(), 487
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV011');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'NV012', @NvidiaRec, @NvidiaCo, N'Product Manager - AI Platforms', N'<p>Quản lý sản phẩm nền tảng AI.</p>', N'<ul><li>5+ năm Product Management, AI background</li></ul>', 55000000, 85000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 60, GETDATE()), 'Published', DATEADD(DAY, -22, GETDATE()), GETDATE(), 598
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV012');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'NV013', @NvidiaRec, @NvidiaCo, N'Developer Relations Engineer', N'<p>Hỗ trợ developer sử dụng NVIDIA SDKs.</p>', N'<ul><li>CUDA, Deep Learning frameworks</li></ul>', 38000000, 58000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 51, GETDATE()), 'Published', DATEADD(DAY, -15, GETDATE()), GETDATE(), 423
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV013');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'NV014', @NvidiaRec, @NvidiaCo, N'Medical Imaging AI Engineer', N'<p>Phát triển AI cho hình ảnh y tế (NVIDIA Clara).</p>', N'<ul><li>Medical Imaging, Deep Learning</li></ul>', 45000000, 70000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 56, GETDATE()), 'Published', DATEADD(DAY, -19, GETDATE()), GETDATE(), 498
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV014');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'NV015', @NvidiaRec, @NvidiaCo, N'High-Performance Computing Specialist', N'<p>Tối ưu hóa ứng dụng HPC trên GPU.</p>', N'<ul><li>HPC, MPI, CUDA</li></ul>', 48000000, 75000000, 'VND', N'Hà Nội', 'Full-time', DATEADD(DAY, 59, GETDATE()), 'Published', DATEADD(DAY, -21, GETDATE()), GETDATE(), 545
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'NV015');

PRINT '=== Thêm JobPosts cho Samsung (11 jobs còn lại) ===';

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'SSG005', @SamsungRec, @SamsungCo, N'Firmware Engineer - Smart TV', N'<p>Phát triển firmware cho Smart TV Samsung.</p>', N'<ul><li>C/C++, Embedded Linux</li></ul>', 35000000, 52000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 48, GETDATE()), 'Published', DATEADD(DAY, -17, GETDATE()), GETDATE(), 423
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG005');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'SSG006', @SamsungRec, @SamsungCo, N'Hardware Engineer - Mobile', N'<p>Thiết kế mạch điện tử cho điện thoại Samsung Galaxy.</p>', N'<ul><li>Circuit Design, PCB Layout</li></ul>', 38000000, 58000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 52, GETDATE()), 'Published', DATEADD(DAY, -19, GETDATE()), GETDATE(), 467
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG006');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'SSG007', @SamsungRec, @SamsungCo, N'5G Network Engineer', N'<p>Phát triển giải pháp kết nối 5G cho thiết bị Samsung.</p>', N'<ul><li>5G, Telecom protocols</li></ul>', 40000000, 62000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 55, GETDATE()), 'Published', DATEADD(DAY, -20, GETDATE()), GETDATE(), 512
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG007');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'SSG008', @SamsungRec, @SamsungCo, N'AI/ML Engineer - Bixby', N'<p>Phát triển trợ lý ảo Bixby với AI/ML.</p>', N'<ul><li>NLP, TensorFlow, PyTorch</li></ul>', 42000000, 65000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 57, GETDATE()), 'Published', DATEADD(DAY, -21, GETDATE()), GETDATE(), 534
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG008');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'SSG009', @SamsungRec, @SamsungCo, N'Battery Research Engineer', N'<p>Nghiên cứu công nghệ pin cho thiết bị di động.</p>', N'<ul><li>Battery Chemistry, R&D</li></ul>', 40000000, 62000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 54, GETDATE()), 'Published', DATEADD(DAY, -18, GETDATE()), GETDATE(), 489
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG009');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'SSG010', @SamsungRec, @SamsungCo, N'Cloud Service Backend Developer', N'<p>Phát triển backend cho Samsung Cloud.</p>', N'<ul><li>Java/Kotlin, Spring Boot, Microservices</li></ul>', 35000000, 52000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 50, GETDATE()), 'Published', DATEADD(DAY, -15, GETDATE()), GETDATE(), 445
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG010');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'SSG011', @SamsungRec, @SamsungCo, N'AR/VR Software Engineer', N'<p>Phát triển ứng dụng AR/VR cho Samsung Gear.</p>', N'<ul><li>Unity, Unreal, AR/VR SDKs</li></ul>', 38000000, 58000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 51, GETDATE()), 'Published', DATEADD(DAY, -16, GETDATE()), GETDATE(), 456
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG011');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'SSG012', @SamsungRec, @SamsungCo, N'Product Quality Engineer', N'<p>Đảm bảo chất lượng sản phẩm điện tử Samsung.</p>', N'<ul><li>Quality Assurance, Six Sigma</li></ul>', 30000000, 45000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 46, GETDATE()), 'Published', DATEADD(DAY, -13, GETDATE()), GETDATE(), 389
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG012');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'SSG013', @SamsungRec, @SamsungCo, N'Camera Software Engineer', N'<p>Phát triển phần mềm xử lý ảnh camera điện thoại.</p>', N'<ul><li>Image Processing, C++, Camera ISP</li></ul>', 40000000, 62000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 53, GETDATE()), 'Published', DATEADD(DAY, -17, GETDATE()), GETDATE(), 478
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG013');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'SSG014', @SamsungRec, @SamsungCo, N'Display Technology Engineer', N'<p>Nghiên cứu công nghệ màn hình OLED/AMOLED.</p>', N'<ul><li>Display Technology, Electronics</li></ul>', 42000000, 65000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 56, GETDATE()), 'Published', DATEADD(DAY, -19, GETDATE()), GETDATE(), 501
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG014');

INSERT INTO JobPosts (JobCode, RecruiterID, CompanyID, Title, Description, Requirements, SalaryFrom, SalaryTo, SalaryCurrency, Location, EmploymentType, ApplicationDeadline, Status, PostedAt, UpdatedAt, ViewCount)
SELECT 'SSG015', @SamsungRec, @SamsungCo, N'Supply Chain Data Analyst', N'<p>Phân tích dữ liệu chuỗi cung ứng sản phẩm Samsung.</p>', N'<ul><li>SQL, Excel, Power BI</li></ul>', 28000000, 42000000, 'VND', N'TP.HCM', 'Full-time', DATEADD(DAY, 45, GETDATE()), 'Published', DATEADD(DAY, -12, GETDATE()), GETDATE(), 367
WHERE NOT EXISTS (SELECT 1 FROM JobPosts WHERE JobCode = 'SSG015');

PRINT '=== Hoàn thành thêm JobPosts! ===';
PRINT '=== Bắt đầu thêm Applications (12 ứng viên cho mỗi JobPost) ===';

-- Tạo stored procedure để thêm 12 applications cho mỗi JobPost
DECLARE @JobPostID INT;
DECLARE @CandList TABLE (CandID INT, RowNum INT);
DECLARE @Status NVARCHAR(50);
DECLARE @Note NVARCHAR(500);
DECLARE @DaysAgo INT;
DECLARE @CandID INT;
DECLARE @ResumeIndex INT;
DECLARE @ResumePath NVARCHAR(255);

-- Danh sách trạng thái applications
DECLARE @StatusList TABLE (Status NVARCHAR(50));
INSERT INTO @StatusList VALUES ('Submitted'), ('Under review'), ('Shortlisted'), ('Interviewed'), ('Offered'), ('Rejected');

-- Cursor qua tất cả JobPosts mới
DECLARE job_cursor CURSOR FOR
SELECT JobPostID FROM JobPosts WHERE JobCode LIKE 'APL%' OR JobCode LIKE 'AWS%' OR JobCode LIKE 'CC0%' OR JobCode LIKE 'NV0%' OR JobCode LIKE 'SSG%';

OPEN job_cursor;
FETCH NEXT FROM job_cursor INTO @JobPostID;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Lấy danh sách 12 candidates ngẫu nhiên
    DELETE FROM @CandList;
    INSERT INTO @CandList (CandID, RowNum)
    SELECT TOP 12 CandidateID, ROW_NUMBER() OVER (ORDER BY NEWID()) AS RowNum
    FROM (VALUES (@Cand1), (@Cand2), (@Cand3), (@Cand4), (@Cand5), (@Cand6), (@Cand7), (@Cand8), (@Cand9), (@Cand10), (@Cand11), (@Cand12), (@Cand13), (@Cand14), (@Cand15)) AS C(CandidateID)
    ORDER BY NEWID();

    -- Thêm 12 applications
    DECLARE @RowNum INT = 1;
    WHILE @RowNum <= 12
    BEGIN
        SELECT @CandID = CandID FROM @CandList WHERE RowNum = @RowNum;
        
        -- Chọn trạng thái theo phân bổ: 40% Under review, 25% Shortlisted, 20% Interviewed, 10% Offered, 5% Rejected
        SET @Status = CASE 
            WHEN @RowNum <= 5 THEN 'Under review'
            WHEN @RowNum <= 8 THEN 'Shortlisted'
            WHEN @RowNum <= 10 THEN 'Interviewed'
            WHEN @RowNum = 11 THEN 'Offered'
            ELSE 'Rejected'
        END;
        
        SET @Note = CASE @Status
            WHEN 'Shortlisted' THEN N'Hồ sơ phù hợp yêu cầu'
            WHEN 'Interviewed' THEN N'Phỏng vấn tốt'
            WHEN 'Offered' THEN N'Offer đã gửi'
            WHEN 'Rejected' THEN N'Kinh nghiệm chưa đủ'
            ELSE NULL
        END;
        
        SET @DaysAgo = 1 + (@RowNum * 2); -- Spread applications over time
        
        -- Lấy resume path (cycling through 5 resume paths)
        SET @ResumeIndex = (@RowNum % 5) + 1;
        SET @ResumePath = CASE @ResumeIndex
            WHEN 1 THEN '/Content/uploads/resumes/cv1.jpg'
            WHEN 2 THEN '/Content/uploads/resumes/cv2.webp'
            WHEN 3 THEN '/Content/uploads/resumes/cv3.jpg'
            WHEN 4 THEN '/Content/uploads/resumes/cv4.webp'
            ELSE '/Content/uploads/resumes/cv5.jpg'
        END;
        
        -- Insert application
        IF NOT EXISTS (SELECT 1 FROM Applications WHERE CandidateID = @CandID AND JobPostID = @JobPostID)
        BEGIN
            INSERT INTO Applications (CandidateID, JobPostID, AppliedAt, Status, ResumeFilePath, Note, UpdatedAt)
            VALUES (@CandID, @JobPostID, DATEADD(DAY, -@DaysAgo, GETDATE()), @Status, @ResumePath, @Note, GETDATE());
        END
        
        SET @RowNum = @RowNum + 1;
    END

    FETCH NEXT FROM job_cursor INTO @JobPostID;
END

CLOSE job_cursor;
DEALLOCATE job_cursor;

PRINT '=== Hoàn thành thêm Applications! ===';

-- Hiển thị thống kê
SELECT 'Thống kê sau khi thêm dữ liệu' AS Info,
    (SELECT COUNT(*) FROM JobPosts) AS TotalJobPosts,
    (SELECT COUNT(*) FROM Applications) AS TotalApplications,
    (SELECT COUNT(*) FROM Candidates) AS TotalCandidates;

SELECT TOP 10 
    jp.JobCode, 
    jp.Title, 
    c.CompanyName,
    (SELECT COUNT(*) FROM Applications WHERE JobPostID = jp.JobPostID) AS ApplicationCount
FROM JobPosts jp
JOIN Companies c ON jp.CompanyID = c.CompanyID
ORDER BY jp.PostedAt DESC;

PRINT '=== HOÀN TẤT! ===';
GO

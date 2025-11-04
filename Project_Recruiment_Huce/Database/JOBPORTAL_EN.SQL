-- File: WEBVER2_English.sql
-- Full database schema translated to English + Vietnamese annotations
-- (SQL Server)


-- (Tuỳ chọn) Tạo database & chuyển ngữ cảnh
-- (Optional) Create DB & USE it
SET NOCOUNT ON;
IF DB_ID(N'JOBPORTAL_EN') IS NULL
BEGIN
    CREATE DATABASE JOBPORTAL_EN;
END
GO
USE JOBPORTAL_EN;
GO


------------------------------------------------------------
-- TABLE: ProfilePhotos (ảnh đại diện / avatar)
-- Lưu thông tin file ảnh đại diện cho tài khoản/người dùng.
------------------------------------------------------------
CREATE TABLE ProfilePhotos (
    PhotoID        INT IDENTITY(1,1) PRIMARY KEY,     -- (Khoá chính) Mã ảnh
    FileName       NVARCHAR(255) NOT NULL,            -- Tên file ảnh (VD: avatar.png)
    FilePath       NVARCHAR(500) NOT NULL,            -- Đường dẫn/URL tới file ảnh
    FileSizeKB     INT NULL,                          -- Kích thước ảnh (KB)
    FileFormat     NVARCHAR(50) NULL,                 -- Định dạng ảnh (png/jpg/...)
    UploadedAt     DATETIME DEFAULT GETDATE()         -- Thời điểm upload
);
GO

------------------------------------------------------------
-- TABLE: Accounts (tài khoản đăng nhập)
-- Quản lý thông tin đăng nhập, vai trò và ảnh đại diện liên kết.
------------------------------------------------------------
CREATE TABLE Accounts (
    AccountID      INT IDENTITY(1,1) PRIMARY KEY,     -- (Khoá chính) Mã tài khoản
    Username       NVARCHAR(100) NOT NULL UNIQUE,     -- Tên đăng nhập (duy nhất)
    PasswordHash   NVARCHAR(255),                     -- Mật khẩu băm
    Salt           NVARCHAR(255),                     -- Salt dùng khi băm mật khẩu
    Email          NVARCHAR(150) NOT NULL UNIQUE,     -- Email (duy nhất)
    Phone          NVARCHAR(20) NULL,                 -- Số điện thoại
    Role           NVARCHAR(30)  NOT NULL             -- Vai trò người dùng
                     CHECK (Role IN (N'Admin', N'Company', N'Recruiter', N'Candidate')),
    CreatedAt      DATETIME DEFAULT GETDATE(),        -- Ngày tạo tài khoản
    ActiveFlag     TINYINT  DEFAULT 1,                -- Trạng thái (1=hoạt động, 0=khóa)
    PhotoID        INT NULL                           -- FK -> ProfilePhotos
);
GO

------------------------------------------------------------
-- TABLE: Admins (quản trị viên)
-- Map 1-1 với Accounts, lưu thông tin hồ sơ admin.
------------------------------------------------------------
CREATE TABLE Admins (
    AdminID        INT IDENTITY(1,1) PRIMARY KEY,     -- (Khoá chính) Mã admin
    AccountID      INT NOT NULL UNIQUE,               -- Tài khoản liên kết (duy nhất)
    FullName       NVARCHAR(100) NOT NULL,            -- Họ tên quản trị
    ContactEmail   NVARCHAR(150) NULL,                -- Email liên hệ
    Permission     NVARCHAR(100) DEFAULT N'FullAccess', -- Quyền hạn (mặc định: toàn quyền)
    CreatedAt      DATETIME DEFAULT GETDATE()         -- Ngày tạo hồ sơ admin
);
GO

------------------------------------------------------------
-- TABLE: Companies (công ty)
-- Thông tin doanh nghiệp đăng tuyển.
------------------------------------------------------------
CREATE TABLE Companies (
    CompanyID      INT IDENTITY(1,1) PRIMARY KEY,     -- (Khoá chính) Mã công ty
    CompanyName    NVARCHAR(255) NOT NULL,            -- Tên công ty
    TaxCode        NVARCHAR(50) NULL,                 -- Mã số thuế
    Industry       NVARCHAR(150) NULL,                -- Lĩnh vực hoạt động
    Address        NVARCHAR(255) NULL,                -- Địa chỉ
    Phone          NVARCHAR(20) NULL,                 -- Số điện thoại
    CompanyEmail   NVARCHAR(150) NULL,                -- Email công ty
    Website        NVARCHAR(200) NULL,                -- Website
    Description    NVARCHAR(1000) NULL,               -- Mô tả ngắn về công ty
    CreatedAt      DATETIME DEFAULT GETDATE(),        -- Ngày tạo
    ActiveFlag     TINYINT DEFAULT 1                  -- Trạng thái hoạt động
);
GO

------------------------------------------------------------
-- TABLE: Recruiters (nhà tuyển dụng)
-- Người đại diện công ty đăng tuyển; map 1-1 với Accounts.
------------------------------------------------------------
CREATE TABLE Recruiters (
    RecruiterID    INT IDENTITY(1,1) PRIMARY KEY,     -- (Khoá chính) Mã NTD
    AccountID      INT NOT NULL UNIQUE,               -- Tài khoản liên kết (duy nhất)
    CompanyID      INT NULL,                          -- Công ty trực thuộc (nếu có)
    FullName       NVARCHAR(100) NOT NULL,            -- Họ tên NTD
    PositionTitle  NVARCHAR(100) NULL,                -- Chức vụ
    CompanyEmail   NVARCHAR(150) NULL,                -- Email công ty
    Phone          NVARCHAR(20) NULL,                 -- Số điện thoại
    CreatedAt      DATETIME DEFAULT GETDATE(),        -- Ngày tạo
    ActiveFlag     TINYINT DEFAULT 1                  -- Trạng thái hoạt động
);
GO

------------------------------------------------------------
-- TABLE: BankCards (thẻ ngân hàng của nhà tuyển dụng)
-- Lưu thông tin thẻ để thanh toán/phí dịch vụ.
------------------------------------------------------------
CREATE TABLE BankCards (
    CardID         INT IDENTITY(1,1) PRIMARY KEY,     -- (Khoá chính) Mã thẻ
    RecruiterID    INT NOT NULL,                      -- FK -> Recruiters
    CardNumber     NVARCHAR(50) NOT NULL,             -- Số thẻ
    BankName       NVARCHAR(150) NULL,                -- Tên ngân hàng
    CardHolderName NVARCHAR(100) NULL,                -- Tên chủ thẻ
    ExpiryDate     DATE NULL,                         -- Ngày hết hạn
    ActiveFlag     TINYINT DEFAULT 1,                 -- Trạng thái sử dụng
    CreatedAt      DATETIME DEFAULT GETDATE()         -- Ngày tạo
);
GO

------------------------------------------------------------
-- TABLE: PaymentHistory (lịch sử thanh toán)
-- Ghi nhận các khoản đã thanh toán bởi nhà tuyển dụng.
------------------------------------------------------------
CREATE TABLE PaymentHistory (
    PaymentHistoryID INT IDENTITY(1,1) PRIMARY KEY,   -- (Khoá chính) Mã lịch sử TT
    RecruiterID      INT NOT NULL,                    -- FK -> Recruiters
    Amount           DECIMAL(18,2) NOT NULL,          -- Số tiền
    Method           NVARCHAR(50) NULL,               -- Phương thức (QR/Bank/...)
    PaidAt           DATETIME DEFAULT GETDATE(),      -- Thời điểm thanh toán
    Status           NVARCHAR(50) DEFAULT N'Completed', -- Trạng thái
    Note             NVARCHAR(255) NULL               -- Ghi chú
);
GO

------------------------------------------------------------
-- TABLE: PendingPayments (khoản chưa thanh toán)
-- Dùng cho công nợ/chờ thanh toán.
------------------------------------------------------------
CREATE TABLE PendingPayments (
    PendingPaymentID INT IDENTITY(1,1) PRIMARY KEY,   -- (Khoá chính) Mã khoản nợ
    RecruiterID      INT NOT NULL,                    -- FK -> Recruiters
    Amount           DECIMAL(18,2) NOT NULL,          -- Số tiền
    DueDate          DATE NULL,                        -- Hạn thanh toán
    Reason           NVARCHAR(255) NULL,              -- Lý do phát sinh
    Status           NVARCHAR(50) DEFAULT N'Unpaid'   -- Trạng thái
);
GO

------------------------------------------------------------
-- TABLE: PostingHistory (lịch sử đăng tin)
-- Lưu vết các tin đã đăng/đang tuyển của nhà tuyển dụng.
------------------------------------------------------------
CREATE TABLE PostingHistory (
    PostingHistoryID INT IDENTITY(1,1) PRIMARY KEY,   -- (Khoá chính) Mã lịch sử đăng
    RecruiterID      INT NOT NULL,                    -- FK -> Recruiters
    JobPostID        INT NOT NULL,                    -- FK -> JobPosts
    PostedAt         DATETIME DEFAULT GETDATE(),      -- Thời điểm đăng
    Status           NVARCHAR(50) DEFAULT N'Open',    -- Trạng thái tin (Open/Closed)
    Note             NVARCHAR(255) NULL               -- Ghi chú
);
GO

------------------------------------------------------------
-- TABLE: Candidates (người ứng tuyển)
-- Hồ sơ người tìm việc; map 1-1 với Accounts.
------------------------------------------------------------
CREATE TABLE Candidates (
    CandidateID     INT IDENTITY(1,1) PRIMARY KEY,    -- (Khoá chính) Mã ứng viên
    AccountID       INT NOT NULL UNIQUE,              -- Tài khoản liên kết (duy nhất)
    FullName        NVARCHAR(100) NOT NULL,           -- Họ tên
    BirthDate       DATE NULL,                        -- Ngày sinh
    Gender          NVARCHAR(10) NOT NULL             -- Giới tính (Male/Female)
                     CHECK (Gender IN (N'Male', N'Female')),
    Phone           NVARCHAR(15) NULL,                -- Số điện thoại
    Email           NVARCHAR(100) NULL,               -- Email
    Address         NVARCHAR(255) NULL,               -- Địa chỉ
    Summary         NVARCHAR(500) NULL,               -- Giới thiệu bản thân
    PhotoID         INT NULL,                         -- Ảnh đại diện (có thể tham chiếu ProfilePhotos)
    CreatedAt       DATETIME DEFAULT GETDATE(),       -- Ngày tạo
    ActiveFlag      TINYINT DEFAULT 1                 -- Trạng thái hoạt động
);
GO

------------------------------------------------------------
-- TABLE: ResumeFiles (tệp CV đính kèm)
-- Lưu file CV ứng viên upload (PDF/IMG/...).
------------------------------------------------------------
CREATE TABLE ResumeFiles (
    ResumeFileID    INT IDENTITY(1,1) PRIMARY KEY,    -- (Khoá chính) Mã tệp CV
    CandidateID     INT NOT NULL,                     -- FK -> Candidates
    FileName        NVARCHAR(255) NULL,               -- Tên file
    FilePath        NVARCHAR(500) NULL,               -- Đường dẫn/URL
    UploadedAt      DATETIME DEFAULT GETDATE()        -- Thời điểm upload
);
GO

------------------------------------------------------------
-- TABLE: WorkExperiences (kinh nghiệm làm việc)
-- Lịch sử việc làm trước đây của ứng viên.
------------------------------------------------------------
CREATE TABLE WorkExperiences (
    ExperienceID    INT IDENTITY(1,1) PRIMARY KEY,    -- (Khoá chính) Mã kinh nghiệm
    CandidateID     INT NOT NULL,                     -- FK -> Candidates
    PreviousCompany NVARCHAR(255) NOT NULL,           -- Tên công ty cũ
    JobTitle        NVARCHAR(150) NOT NULL,           -- Chức danh
    Industry        NVARCHAR(150) NULL,               -- Ngành nghề
    Major           NVARCHAR(150) NULL,               -- Chuyên ngành (nếu có)
    JobDescription  NVARCHAR(MAX) NULL,               -- Mô tả công việc
    StartDate       DATE NOT NULL,                    -- Ngày bắt đầu
    EndDate         DATE NULL,                        -- Ngày kết thúc
    Salary          DECIMAL(18,2) NULL,               -- Mức lương
    SalaryCurrency  NVARCHAR(20) DEFAULT N'VND',      -- Đơn vị tiền tệ
    Achievements    NVARCHAR(500) NULL,               -- Thành tựu
    CreatedAt       DATETIME DEFAULT GETDATE()        -- Ngày tạo bản ghi
);
GO

------------------------------------------------------------
-- TABLE: Educations (học vấn)
-- Học tập/bằng cấp của ứng viên.
------------------------------------------------------------
CREATE TABLE Educations (
    EducationID     INT IDENTITY(1,1) PRIMARY KEY,    -- (Khoá chính) Mã học vấn
    CandidateID     INT NOT NULL,                     -- FK -> Candidates
    School          NVARCHAR(255) NOT NULL,           -- Trường học
    Degree          NVARCHAR(100) NOT NULL,           -- Trình độ/bằng cấp
    FieldOfStudy    NVARCHAR(150) NOT NULL,           -- Ngành học
    Major           NVARCHAR(150) NULL,               -- Chuyên ngành (nếu có)
    StartYear       INT NULL,                         -- Năm bắt đầu
    EndYear         INT NULL,                         -- Năm kết thúc
    ExtraDescription NVARCHAR(500) NULL,              -- Mô tả thêm
    DegreeFilePath  NVARCHAR(500) NULL,               -- File scan/chứng chỉ
    UploadedAt      DATETIME DEFAULT GETDATE()        -- Thời điểm upload
);
GO

------------------------------------------------------------
-- TABLE: Certificates (chứng chỉ)
-- Danh mục các loại chứng chỉ có thể đính kèm.
------------------------------------------------------------
CREATE TABLE Certificates (
    CertificateID   INT IDENTITY(1,1) PRIMARY KEY,    -- (Khoá chính) Mã chứng chỉ
    CertificateName NVARCHAR(100) NOT NULL,           -- Tên chứng chỉ
    Issuer          NVARCHAR(100) NULL,               -- Cơ quan cấp
    Industry        NVARCHAR(100) NOT NULL,           -- Ngành nghề
    Major           NVARCHAR(150) NULL,               -- Chuyên ngành
    Description     NVARCHAR(255) NULL                -- Mô tả ngắn
);
GO

------------------------------------------------------------
-- TABLE: CandidateCertificates (ứng viên - chứng chỉ)
-- Chứng chỉ cụ thể mà ứng viên sở hữu.
------------------------------------------------------------
CREATE TABLE CandidateCertificates (
    CandidateCertificateID INT IDENTITY(1,1) PRIMARY KEY, -- (Khoá chính) Mã chi tiết CC
    CandidateID            INT NOT NULL,                  -- FK -> Candidates
    CertificateID          INT NOT NULL,                  -- FK -> Certificates
    IssueDate              DATE NULL,                     -- Ngày cấp
    ExpiryDate             DATE NULL,                     -- Ngày hết hạn
    FilePath               NVARCHAR(500) NULL,            -- File đính kèm
    Score                  NVARCHAR(50) NULL,             -- Điểm/số hiệu
    Note                   NVARCHAR(255) NULL,            -- Ghi chú
    UploadedAt             DATETIME DEFAULT GETDATE()     -- Thời điểm upload
);
GO

------------------------------------------------------------
-- TABLE: CertificateImages (ảnh minh hoạ chứng chỉ)
-- Ảnh chụp/scan nhiều trang cho một chứng chỉ của ứng viên.
------------------------------------------------------------
CREATE TABLE CertificateImages (
    CertificateImageID INT IDENTITY(1,1) PRIMARY KEY, -- (Khoá chính) Mã ảnh CC
    CandidateCertificateID INT NOT NULL,              -- FK -> CandidateCertificates
    FileName          NVARCHAR(255) NULL,             -- Tên file
    FilePath          NVARCHAR(500) NULL,             -- Đường dẫn/URL
    UploadedAt        DATETIME DEFAULT GETDATE()      -- Thời điểm upload
);
GO

------------------------------------------------------------
-- TABLE: JobPosts (tin tuyển dụng)
-- Thông tin tin tuyển dụng do nhà tuyển dụng đăng.
------------------------------------------------------------
CREATE TABLE JobPosts (
    JobPostID       INT IDENTITY(1,1) PRIMARY KEY,    -- (Khoá chính) Mã tin
    JobCode         NVARCHAR(20) UNIQUE NULL,         -- Mã tin (tuỳ chọn)
    RecruiterID     INT NOT NULL,                     -- FK -> Recruiters
    CompanyID       INT NULL,                         -- FK -> Companies (nếu có)
    Title           NVARCHAR(200) NOT NULL,           -- Tiêu đề tin
    Description     NVARCHAR(MAX) NULL,               -- Mô tả công việc
    Requirements    NVARCHAR(MAX) NULL,               -- Yêu cầu ứng viên
    SalaryFrom      DECIMAL(18,2) NULL,               -- Lương từ
    SalaryTo        DECIMAL(18,2) NULL,               -- Lương đến
    SalaryCurrency  NVARCHAR(50) DEFAULT N'VND',      -- Đơn vị tiền tệ
    Location        NVARCHAR(255) NULL,               -- Địa điểm làm việc
    EmploymentType  NVARCHAR(100) NULL,               -- Hình thức (Full-time/Part-time/...)
    ApplicationDeadline DATE NULL,                    -- Hạn nộp hồ sơ
    Status          NVARCHAR(50) DEFAULT N'Published',-- Trạng thái hiển thị
    PostedAt        DATETIME DEFAULT GETDATE(),       -- Ngày đăng
    UpdatedAt       DATETIME DEFAULT GETDATE()        -- Ngày cập nhật gần nhất
);
GO

------------------------------------------------------------
-- TABLE: JobPostDetails (chi tiết yêu cầu tin)
-- Ràng buộc bổ sung: ngành, bằng cấp, số lượng, độ tuổi, giới tính...
------------------------------------------------------------
CREATE TABLE JobPostDetails (
    DetailID        INT IDENTITY(1,1) PRIMARY KEY,    -- (Khoá chính) Mã chi tiết
    JobPostID       INT NOT NULL,                     -- FK -> JobPosts
    Industry        NVARCHAR(150) NOT NULL,           -- Ngành nghề
    Major           NVARCHAR(150) NULL,               -- Chuyên ngành
    YearsExperience INT DEFAULT 0,                    -- Số năm kinh nghiệm
    DegreeRequired  NVARCHAR(100) NULL,               -- Bằng cấp yêu cầu
    Skills          NVARCHAR(MAX) NULL,               -- Kỹ năng
    Headcount       INT DEFAULT 1,                    -- Số lượng cần tuyển
    GenderRequirement NVARCHAR(20) DEFAULT N'Not required', -- Yêu cầu giới tính
    AgeFrom         INT NULL,                         -- Độ tuổi từ
    AgeTo           INT NULL,                         -- Độ tuổi đến
    Status          NVARCHAR(50) DEFAULT N'Published' -- Trạng thái hiển thị
);
GO

------------------------------------------------------------
-- TABLE: Applications (lịch sử ứng tuyển)
-- Hồ sơ ứng tuyển của ứng viên vào một tin cụ thể.
------------------------------------------------------------
CREATE TABLE Applications (
    ApplicationID   INT IDENTITY(1,1) PRIMARY KEY,    -- (Khoá chính) Mã ứng tuyển
    CandidateID     INT NOT NULL,                     -- FK -> Candidates
    JobPostID       INT NOT NULL,                     -- FK -> JobPosts
    AppliedAt       DATETIME DEFAULT GETDATE(),       -- Thời điểm ứng tuyển
    Status          NVARCHAR(50) DEFAULT N'Under review', -- Trạng thái xử lý
    ResumeFilePath  NVARCHAR(500) NULL,               -- Tệp CV nộp kèm
    CertificateFilePath NVARCHAR(500) NULL,           -- Tệp chứng chỉ kèm
    Note            NVARCHAR(500) NULL,               -- Ghi chú
    UpdatedAt       DATETIME DEFAULT GETDATE()        -- Ngày cập nhật
);
GO

------------------------------------------------------------
-- TABLE: Transactions (lịch sử giao dịch)
-- Giao dịch gắn với tài khoản (nạp tiền, thanh toán dịch vụ, ...).
------------------------------------------------------------
CREATE TABLE Transactions (
    TransactionID   INT IDENTITY(1,1) PRIMARY KEY,    -- (Khoá chính) Mã giao dịch
    TransactionCode NVARCHAR(20) UNIQUE NULL,         -- Mã GD hiển thị
    AccountID       INT NOT NULL,                     -- FK -> Accounts
    TransactionType NVARCHAR(100) NOT NULL,           -- Loại GD (Top-up/Fee/Refund/...)
    Amount          DECIMAL(18,2) NOT NULL,           -- Số tiền
    Method          NVARCHAR(50) NOT NULL,            -- Phương thức (QR/Bank/...)
    Status          NVARCHAR(50) DEFAULT N'Processing', -- Trạng thái
    TransactionDate DATETIME DEFAULT GETDATE(),       -- Ngày giao dịch
    Description     NVARCHAR(500) NULL,               -- Mô tả
    UpdatedAt       DATETIME DEFAULT GETDATE()        -- Ngày cập nhật
);
GO


/* =============================
   FOREIGN KEYS (ràng buộc khoá ngoại)
   ============================= */

-- Accounts.PhotoID -> ProfilePhotos.PhotoID
-- (Ảnh đại diện của tài khoản; xóa ảnh thì set NULL)
ALTER TABLE Accounts
ADD CONSTRAINT FK_Accounts_ProfilePhotos FOREIGN KEY (PhotoID)
REFERENCES ProfilePhotos(PhotoID)
ON DELETE SET NULL;
GO

-- Admins.AccountID -> Accounts.AccountID (1-1)
-- (Xoá tài khoản sẽ xoá hồ sơ admin)
ALTER TABLE Admins
ADD CONSTRAINT FK_Admins_Accounts FOREIGN KEY (AccountID)
REFERENCES Accounts(AccountID)
ON DELETE CASCADE;
GO

-- Recruiters.AccountID -> Accounts.AccountID (1-1)
-- (Xoá tài khoản sẽ xoá hồ sơ NTD)
ALTER TABLE Recruiters
ADD CONSTRAINT FK_Recruiters_Accounts FOREIGN KEY (AccountID)
REFERENCES Accounts(AccountID)
ON DELETE CASCADE;
GO

-- Recruiters.CompanyID -> Companies.CompanyID
-- (Không xoá dây chuyền khi xoá công ty)
ALTER TABLE Recruiters
ADD CONSTRAINT FK_Recruiters_Companies FOREIGN KEY (CompanyID)
REFERENCES Companies(CompanyID)
ON DELETE NO ACTION;
GO

-- BankCards.RecruiterID -> Recruiters.RecruiterID
-- (Xoá NTD sẽ xoá thẻ)
ALTER TABLE BankCards
ADD CONSTRAINT FK_BankCards_Recruiters FOREIGN KEY (RecruiterID)
REFERENCES Recruiters(RecruiterID)
ON DELETE CASCADE;
GO

-- PaymentHistory.RecruiterID -> Recruiters.RecruiterID
-- (Xoá NTD sẽ xoá lịch sử thanh toán của họ)
ALTER TABLE PaymentHistory
ADD CONSTRAINT FK_PaymentHistory_Recruiters FOREIGN KEY (RecruiterID)
REFERENCES Recruiters(RecruiterID)
ON DELETE CASCADE;
GO

-- PendingPayments.RecruiterID -> Recruiters.RecruiterID
-- (Xoá NTD sẽ xoá các khoản nợ chờ của họ)
ALTER TABLE PendingPayments
ADD CONSTRAINT FK_PendingPayments_Recruiters FOREIGN KEY (RecruiterID)
REFERENCES Recruiters(RecruiterID)
ON DELETE CASCADE;
GO

-- PostingHistory.RecruiterID -> Recruiters.RecruiterID
ALTER TABLE PostingHistory
ADD CONSTRAINT FK_PostingHistory_Recruiters FOREIGN KEY (RecruiterID)
REFERENCES Recruiters(RecruiterID)
ON DELETE CASCADE;
GO

-- PostingHistory.JobPostID -> JobPosts.JobPostID
ALTER TABLE PostingHistory
ADD CONSTRAINT FK_PostingHistory_JobPosts FOREIGN KEY (JobPostID)
REFERENCES JobPosts(JobPostID)
ON DELETE CASCADE;
GO

-- Candidates.AccountID -> Accounts.AccountID (1-1)
ALTER TABLE Candidates
ADD CONSTRAINT FK_Candidates_Accounts FOREIGN KEY (AccountID)
REFERENCES Accounts(AccountID)
ON DELETE CASCADE;
GO

-- ResumeFiles.CandidateID -> Candidates.CandidateID
ALTER TABLE ResumeFiles
ADD CONSTRAINT FK_ResumeFiles_Candidates FOREIGN KEY (CandidateID)
REFERENCES Candidates(CandidateID)
ON DELETE CASCADE;
GO

-- WorkExperiences.CandidateID -> Candidates.CandidateID
ALTER TABLE WorkExperiences
ADD CONSTRAINT FK_WorkExperiences_Candidates FOREIGN KEY (CandidateID)
REFERENCES Candidates(CandidateID)
ON DELETE CASCADE;
GO

-- Educations.CandidateID -> Candidates.CandidateID
ALTER TABLE Educations
ADD CONSTRAINT FK_Educations_Candidates FOREIGN KEY (CandidateID)
REFERENCES Candidates(CandidateID)
ON DELETE CASCADE;
GO

-- CandidateCertificates.CandidateID -> Candidates.CandidateID
ALTER TABLE CandidateCertificates
ADD CONSTRAINT FK_CandidateCertificates_Candidates FOREIGN KEY (CandidateID)
REFERENCES Candidates(CandidateID)
ON DELETE CASCADE;
GO

-- CandidateCertificates.CertificateID -> Certificates.CertificateID
ALTER TABLE CandidateCertificates
ADD CONSTRAINT FK_CandidateCertificates_Certificates FOREIGN KEY (CertificateID)
REFERENCES Certificates(CertificateID)
ON DELETE CASCADE;
GO

-- CertificateImages.CandidateCertificateID -> CandidateCertificates.CandidateCertificateID
ALTER TABLE CertificateImages
ADD CONSTRAINT FK_CertificateImages_CandidateCertificates FOREIGN KEY (CandidateCertificateID)
REFERENCES CandidateCertificates(CandidateCertificateID)
ON DELETE CASCADE;
GO

-- JobPosts.RecruiterID -> Recruiters.RecruiterID
-- (Không xoá dây chuyền để tránh mất tin đã đăng)
ALTER TABLE JobPosts
ADD CONSTRAINT FK_JobPosts_Recruiters FOREIGN KEY (RecruiterID)
REFERENCES Recruiters(RecruiterID)
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- JobPosts.CompanyID -> Companies.CompanyID
ALTER TABLE JobPosts
ADD CONSTRAINT FK_JobPosts_Companies FOREIGN KEY (CompanyID)
REFERENCES Companies(CompanyID)
ON DELETE NO ACTION;
GO

-- JobPostDetails.JobPostID -> JobPosts.JobPostID
ALTER TABLE JobPostDetails
ADD CONSTRAINT FK_JobPostDetails_JobPosts FOREIGN KEY (JobPostID)
REFERENCES JobPosts(JobPostID)
ON DELETE CASCADE;
GO

-- Applications.CandidateID -> Candidates.CandidateID
ALTER TABLE Applications
ADD CONSTRAINT FK_Applications_Candidates FOREIGN KEY (CandidateID)
REFERENCES Candidates(CandidateID)
ON DELETE CASCADE;
GO

-- Applications.JobPostID -> JobPosts.JobPostID
ALTER TABLE Applications
ADD CONSTRAINT FK_Applications_JobPosts FOREIGN KEY (JobPostID)
REFERENCES JobPosts(JobPostID)
ON DELETE CASCADE;
GO

-- Transactions.AccountID -> Accounts.AccountID
ALTER TABLE Transactions
ADD CONSTRAINT FK_Transactions_Accounts FOREIGN KEY (AccountID)
REFERENCES Accounts(AccountID)
ON DELETE CASCADE;
GO


/* =============================
   INDEXES (chỉ mục gợi ý)
   ============================= */

-- Tăng tốc truy vấn theo ngày đăng tin
CREATE INDEX IX_JobPosts_PostedAt ON JobPosts(PostedAt);
GO


-- Kết thúc: In thông báo
PRINT 'WEBVER2 (English) schema created successfully.';

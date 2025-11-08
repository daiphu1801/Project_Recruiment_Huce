-- =====================================================================
-- File: JOBPORTAL_EN_full_with_company_photo.sql
-- Target: Microsoft SQL Server
-- Notes:
--   - Idempotent: chạy lại nhiều lần không lỗi
--   - Dùng IF OBJECT_ID ... để tạo bảng nếu chưa có
--   - JobPosts.JobCode & Transactions.TransactionCode: dùng unique index có filter (cho phép nhiều NULL)
--   - Thêm FK PhotoID cho Accounts/Companies/Recruiters/Candidates -> ProfilePhotos (ON DELETE SET NULL)
--   - Trigger tự xóa SavedJobs khi JobPosts chuyển trạng thái Closed/Expired/Archived
-- =====================================================================

SET NOCOUNT ON;
GO

-- 0) Ensure database exists
IF DB_ID(N'JOBPORTAL_EN') IS NULL
BEGIN
    PRINT 'Creating database JOBPORTAL_EN...';
    CREATE DATABASE JOBPORTAL_EN;
END;
GO
USE JOBPORTAL_EN;
GO

/* =====================================================
   1) TABLES
   ===================================================== */

-- ProfilePhotos
IF OBJECT_ID(N'dbo.ProfilePhotos','U') IS NULL
BEGIN
    CREATE TABLE dbo.ProfilePhotos (
        PhotoID     INT IDENTITY(1,1) PRIMARY KEY,
        FileName    NVARCHAR(255) NOT NULL,
        FilePath    NVARCHAR(500) NOT NULL,
        FileSizeKB  INT NULL,
        FileFormat  NVARCHAR(50) NULL,
        UploadedAt  DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- Accounts
IF OBJECT_ID(N'dbo.Accounts','U') IS NULL
BEGIN
    CREATE TABLE dbo.Accounts (
        AccountID     INT IDENTITY(1,1) PRIMARY KEY,
        Username      NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash  NVARCHAR(255) NULL,
        Salt          NVARCHAR(255) NULL,
        Email         NVARCHAR(150) NOT NULL UNIQUE,
        Phone         NVARCHAR(20) NULL,
        Role          NVARCHAR(30) NOT NULL CHECK (Role IN (N'Admin', N'Company', N'Recruiter', N'Candidate')),
        CreatedAt     DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        ActiveFlag    TINYINT NOT NULL DEFAULT 1,
        PhotoID       INT NULL
    );
END
GO

-- Admins
IF OBJECT_ID(N'dbo.Admins','U') IS NULL
BEGIN
    CREATE TABLE dbo.Admins (
        AdminID       INT IDENTITY(1,1) PRIMARY KEY,
        AccountID     INT NOT NULL UNIQUE,
        FullName      NVARCHAR(100) NOT NULL,
        ContactEmail  NVARCHAR(150) NULL,
        Permission    NVARCHAR(100) NOT NULL DEFAULT N'FullAccess',
        CreatedAt     DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- Companies
IF OBJECT_ID(N'dbo.Companies','U') IS NULL
BEGIN
    CREATE TABLE dbo.Companies (
        CompanyID     INT IDENTITY(1,1) PRIMARY KEY,
        CompanyName   NVARCHAR(255) NOT NULL,
        TaxCode       NVARCHAR(50) NULL,
        Industry      NVARCHAR(150) NULL,
        Address       NVARCHAR(255) NULL,
        Phone         NVARCHAR(20) NULL,
        CompanyEmail  NVARCHAR(150) NULL,
        Website       NVARCHAR(200) NULL,
        Description   NVARCHAR(1000) NULL,
        CreatedAt     DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        ActiveFlag    TINYINT NOT NULL DEFAULT 1,
        PhotoID       INT NULL            -- logo/ảnh công ty
    );
END
GO

-- Recruiters
IF OBJECT_ID(N'dbo.Recruiters','U') IS NULL
BEGIN
    CREATE TABLE dbo.Recruiters (
        RecruiterID    INT IDENTITY(1,1) PRIMARY KEY,
        AccountID      INT NOT NULL UNIQUE,
        CompanyID      INT NULL,
        FullName       NVARCHAR(100) NOT NULL,
        PositionTitle  NVARCHAR(100) NULL,
        CompanyEmail   NVARCHAR(150) NULL,
        Phone          NVARCHAR(20) NULL,
        CreatedAt      DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        ActiveFlag     TINYINT NOT NULL DEFAULT 1,
        PhotoID        INT NULL          -- ảnh đại diện recruiter
    );
END
GO

-- BankCards
IF OBJECT_ID(N'dbo.BankCards','U') IS NULL
BEGIN
    CREATE TABLE dbo.BankCards (
        CardID          INT IDENTITY(1,1) PRIMARY KEY,
        RecruiterID     INT NOT NULL,
        CardNumber      NVARCHAR(50) NOT NULL,
        BankName        NVARCHAR(150) NULL,
        CardHolderName  NVARCHAR(100) NULL,
        ExpiryDate      DATE NULL,
        ActiveFlag      TINYINT NOT NULL DEFAULT 1,
        CreatedAt       DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- PaymentHistory
IF OBJECT_ID(N'dbo.PaymentHistory','U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentHistory (
        PaymentHistoryID INT IDENTITY(1,1) PRIMARY KEY,
        RecruiterID      INT NOT NULL,
        Amount           DECIMAL(18,2) NOT NULL,
        Method           NVARCHAR(50) NULL,
        PaidAt           DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        Status           NVARCHAR(50) NOT NULL DEFAULT N'Completed',
        Note             NVARCHAR(255) NULL
    );
END
GO

-- PendingPayments
IF OBJECT_ID(N'dbo.PendingPayments','U') IS NULL
BEGIN
    CREATE TABLE dbo.PendingPayments (
        PendingPaymentID INT IDENTITY(1,1) PRIMARY KEY,
        RecruiterID      INT NOT NULL,
        Amount           DECIMAL(18,2) NOT NULL,
        DueDate          DATE NULL,
        Reason           NVARCHAR(255) NULL,
        Status           NVARCHAR(50) NOT NULL DEFAULT N'Unpaid'
    );
END
GO

-- PostingHistory
IF OBJECT_ID(N'dbo.PostingHistory','U') IS NULL
BEGIN
    CREATE TABLE dbo.PostingHistory (
        PostingHistoryID INT IDENTITY(1,1) PRIMARY KEY,
        RecruiterID      INT NOT NULL,
        JobPostID        INT NOT NULL,
        PostedAt         DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        Status           NVARCHAR(50) NOT NULL DEFAULT N'Open',
        Note             NVARCHAR(255) NULL
    );
END
GO

-- Candidates
IF OBJECT_ID(N'dbo.Candidates','U') IS NULL
BEGIN
    CREATE TABLE dbo.Candidates (
        CandidateID  INT IDENTITY(1,1) PRIMARY KEY,
        AccountID    INT NOT NULL UNIQUE,
        FullName     NVARCHAR(100) NOT NULL,
        BirthDate    DATE NULL,
        Gender       NVARCHAR(10) NOT NULL CHECK (Gender IN (N'Nam', N'Nữ')),
        Phone        NVARCHAR(15) NULL,
        Email        NVARCHAR(100) NULL,
        Address      NVARCHAR(255) NULL,
        Summary      NVARCHAR(500) NULL,
        PhotoID      INT NULL,
        CreatedAt    DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        ActiveFlag   TINYINT NOT NULL DEFAULT 1
    );
END
GO

-- ResumeFiles
IF OBJECT_ID(N'dbo.ResumeFiles','U') IS NULL
BEGIN
    CREATE TABLE dbo.ResumeFiles (
        ResumeFileID INT IDENTITY(1,1) PRIMARY KEY,
        CandidateID  INT NOT NULL,
        FileName     NVARCHAR(255) NULL,
        FilePath     NVARCHAR(500) NULL,
        UploadedAt   DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- WorkExperiences
IF OBJECT_ID(N'dbo.WorkExperiences','U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkExperiences (
        ExperienceID     INT IDENTITY(1,1) PRIMARY KEY,
        CandidateID      INT NOT NULL,
        PreviousCompany  NVARCHAR(255) NOT NULL,
        JobTitle         NVARCHAR(150) NOT NULL,
        Industry         NVARCHAR(150) NULL,
        Major            NVARCHAR(150) NULL,
        JobDescription   NVARCHAR(MAX) NULL,
        StartDate        DATE NOT NULL,
        EndDate          DATE NULL,
        Salary           DECIMAL(18,2) NULL,
        SalaryCurrency   NVARCHAR(20) NOT NULL DEFAULT N'VND',
        Achievements     NVARCHAR(500) NULL,
        CreatedAt        DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- Educations
IF OBJECT_ID(N'dbo.Educations','U') IS NULL
BEGIN
    CREATE TABLE dbo.Educations (
        EducationID      INT IDENTITY(1,1) PRIMARY KEY,
        CandidateID      INT NOT NULL,
        School           NVARCHAR(255) NOT NULL,
        Degree           NVARCHAR(100) NOT NULL,
        FieldOfStudy     NVARCHAR(150) NOT NULL,
        Major            NVARCHAR(150) NULL,
        StartYear        INT NULL,
        EndYear          INT NULL,
        ExtraDescription NVARCHAR(500) NULL,
        DegreeFilePath   NVARCHAR(500) NULL,
        UploadedAt       DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- Certificates
IF OBJECT_ID(N'dbo.Certificates','U') IS NULL
BEGIN
    CREATE TABLE dbo.Certificates (
        CertificateID    INT IDENTITY(1,1) PRIMARY KEY,
        CertificateName  NVARCHAR(100) NOT NULL,
        Issuer           NVARCHAR(100) NULL,
        Industry         NVARCHAR(100) NOT NULL,
        Major            NVARCHAR(150) NULL,
        Description      NVARCHAR(255) NULL
    );
END
GO

-- CandidateCertificates
IF OBJECT_ID(N'dbo.CandidateCertificates','U') IS NULL
BEGIN
    CREATE TABLE dbo.CandidateCertificates (
        CandidateCertificateID INT IDENTITY(1,1) PRIMARY KEY,
        CandidateID            INT NOT NULL,
        CertificateID          INT NOT NULL,
        IssueDate              DATE NULL,
        ExpiryDate             DATE NULL,
        FilePath               NVARCHAR(500) NULL,
        Score                  NVARCHAR(50) NULL,
        Note                   NVARCHAR(255) NULL,
        UploadedAt             DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- CertificateImages
IF OBJECT_ID(N'dbo.CertificateImages','U') IS NULL
BEGIN
    CREATE TABLE dbo.CertificateImages (
        CertificateImageID     INT IDENTITY(1,1) PRIMARY KEY,
        CandidateCertificateID INT NOT NULL,
        FileName               NVARCHAR(255) NULL,
        FilePath               NVARCHAR(500) NULL,
        UploadedAt             DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- JobPosts  (KHÔNG khai báo UNIQUE trực tiếp trên JobCode để cho phép nhiều NULL)
IF OBJECT_ID(N'dbo.JobPosts','U') IS NULL
BEGIN
    CREATE TABLE dbo.JobPosts (
        JobPostID        INT IDENTITY(1,1) PRIMARY KEY,
        JobCode          NVARCHAR(20) NULL,
        RecruiterID      INT NOT NULL,
        CompanyID        INT NULL,
        Title            NVARCHAR(200) NOT NULL,
        Description      NVARCHAR(MAX) NULL,
        Requirements     NVARCHAR(MAX) NULL,
        SalaryFrom       DECIMAL(18,2) NULL,
        SalaryTo         DECIMAL(18,2) NULL,
        SalaryCurrency   NVARCHAR(50) NOT NULL DEFAULT N'VND',
        Location         NVARCHAR(255) NULL,
        EmploymentType   NVARCHAR(100) NULL,
        ApplicationDeadline DATE NULL,
        Status           NVARCHAR(50) NOT NULL DEFAULT N'Published',
        PostedAt         DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        UpdatedAt        DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- JobPostDetails
IF OBJECT_ID(N'dbo.JobPostDetails','U') IS NULL
BEGIN
    CREATE TABLE dbo.JobPostDetails (
        DetailID         INT IDENTITY(1,1) PRIMARY KEY,
        JobPostID        INT NOT NULL,
        Industry         NVARCHAR(150) NOT NULL,
        Major            NVARCHAR(150) NULL,
        YearsExperience  INT NOT NULL DEFAULT 0,
        DegreeRequired   NVARCHAR(100) NULL,
        Skills           NVARCHAR(MAX) NULL,
        Headcount        INT NOT NULL DEFAULT 1,
        GenderRequirement NVARCHAR(20) NOT NULL DEFAULT N'Not required',
        AgeFrom          INT NULL,
        AgeTo            INT NULL,
        Status           NVARCHAR(50) NOT NULL DEFAULT N'Published'
    );
END
GO

-- Applications
IF OBJECT_ID(N'dbo.Applications','U') IS NULL
BEGIN
    CREATE TABLE dbo.Applications (
        ApplicationID       INT IDENTITY(1,1) PRIMARY KEY,
        CandidateID         INT NOT NULL,
        JobPostID           INT NOT NULL,
        AppliedAt           DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        Status              NVARCHAR(50) NOT NULL DEFAULT N'Under review',
        ResumeFilePath      NVARCHAR(500) NULL,
        CertificateFilePath NVARCHAR(500) NULL,
        Note                NVARCHAR(500) NULL,
        UpdatedAt           DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- Transactions  (KHÔNG khai báo UNIQUE trực tiếp trên TransactionCode để cho phép nhiều NULL)
IF OBJECT_ID(N'dbo.Transactions','U') IS NULL
BEGIN
    CREATE TABLE dbo.Transactions (
        TransactionID    INT IDENTITY(1,1) PRIMARY KEY,
        TransactionCode  NVARCHAR(20) NULL,
        AccountID        INT NOT NULL,
        TransactionType  NVARCHAR(100) NOT NULL,
        Amount           DECIMAL(18,2) NOT NULL,
        Method           NVARCHAR(50) NOT NULL,
        Status           NVARCHAR(50) NOT NULL DEFAULT N'Processing',
        TransactionDate  DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        Description      NVARCHAR(500) NULL,
        UpdatedAt        DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- SavedJobs (tạo bảng + ràng buộc trong lần tạo đầu)
IF OBJECT_ID(N'dbo.SavedJobs','U') IS NULL
BEGIN
    CREATE TABLE dbo.SavedJobs (
        SavedJobID  INT IDENTITY(1,1) PRIMARY KEY,
        CandidateID INT NOT NULL,
        JobPostID   INT NOT NULL,
        SavedAt     DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        Note        NVARCHAR(500) NULL,
        CONSTRAINT UQ_SavedJobs_Candidate_Job UNIQUE (CandidateID, JobPostID)
    );
END
GO


/* =====================================================
   2) FOREIGN KEYS  (create if not exists)
   ===================================================== */

-- Helper: add FK if not exists (pattern used repeatedly)
-- Accounts.PhotoID -> ProfilePhotos
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Accounts_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Accounts
    ADD CONSTRAINT FK_Accounts_ProfilePhotos
        FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID)
        ON DELETE SET NULL;
END
GO

-- Companies.PhotoID -> ProfilePhotos
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Companies_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Companies
    ADD CONSTRAINT FK_Companies_ProfilePhotos
        FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID)
        ON DELETE SET NULL;
END
GO

-- Recruiters.PhotoID -> ProfilePhotos
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Recruiters_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_ProfilePhotos
        FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID)
        ON DELETE SET NULL;
END
GO

-- Candidates.PhotoID -> ProfilePhotos
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Candidates_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Candidates
    ADD CONSTRAINT FK_Candidates_ProfilePhotos
        FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID)
        ON DELETE SET NULL;
END
GO

-- Admins.AccountID -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Admins_Accounts')
BEGIN
    ALTER TABLE dbo.Admins
    ADD CONSTRAINT FK_Admins_Accounts
        FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
        ON DELETE CASCADE;
END
GO

-- Recruiters.AccountID -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Recruiters_Accounts')
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_Accounts
        FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
        ON DELETE CASCADE;
END
GO

-- Recruiters.CompanyID -> Companies
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Recruiters_Companies')
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_Companies
        FOREIGN KEY (CompanyID) REFERENCES dbo.Companies(CompanyID)
        ON DELETE NO ACTION;
END
GO

-- BankCards.RecruiterID -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BankCards_Recruiters')
BEGIN
    ALTER TABLE dbo.BankCards
    ADD CONSTRAINT FK_BankCards_Recruiters
        FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
        ON DELETE CASCADE;
END
GO

-- PaymentHistory.RecruiterID -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PaymentHistory_Recruiters')
BEGIN
    ALTER TABLE dbo.PaymentHistory
    ADD CONSTRAINT FK_PaymentHistory_Recruiters
        FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
        ON DELETE CASCADE;
END
GO

-- PendingPayments.RecruiterID -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PendingPayments_Recruiters')
BEGIN
    ALTER TABLE dbo.PendingPayments
    ADD CONSTRAINT FK_PendingPayments_Recruiters
        FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
        ON DELETE CASCADE;
END
GO

-- PostingHistory.RecruiterID -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PostingHistory_Recruiters')
BEGIN
    ALTER TABLE dbo.PostingHistory
    ADD CONSTRAINT FK_PostingHistory_Recruiters
        FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
        ON DELETE CASCADE;
END
GO

-- PostingHistory.JobPostID -> JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PostingHistory_JobPosts')
BEGIN
    ALTER TABLE dbo.PostingHistory
    ADD CONSTRAINT FK_PostingHistory_JobPosts
        FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID)
        ON DELETE CASCADE;
END
GO

-- Candidates.AccountID -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Candidates_Accounts')
BEGIN
    ALTER TABLE dbo.Candidates
    ADD CONSTRAINT FK_Candidates_Accounts
        FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
        ON DELETE CASCADE;
END
GO

-- ResumeFiles.CandidateID -> Candidates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ResumeFiles_Candidates')
BEGIN
    ALTER TABLE dbo.ResumeFiles
    ADD CONSTRAINT FK_ResumeFiles_Candidates
        FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
        ON DELETE CASCADE;
END
GO

-- WorkExperiences.CandidateID -> Candidates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_WorkExperiences_Candidates')
BEGIN
    ALTER TABLE dbo.WorkExperiences
    ADD CONSTRAINT FK_WorkExperiences_Candidates
        FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
        ON DELETE CASCADE;
END
GO

-- Educations.CandidateID -> Candidates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Educations_Candidates')
BEGIN
    ALTER TABLE dbo.Educations
    ADD CONSTRAINT FK_Educations_Candidates
        FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
        ON DELETE CASCADE;
END
GO

-- CandidateCertificates.CandidateID -> Candidates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CandidateCertificates_Candidates')
BEGIN
    ALTER TABLE dbo.CandidateCertificates
    ADD CONSTRAINT FK_CandidateCertificates_Candidates
        FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
        ON DELETE CASCADE;
END
GO

-- CandidateCertificates.CertificateID -> Certificates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CandidateCertificates_Certificates')
BEGIN
    ALTER TABLE dbo.CandidateCertificates
    ADD CONSTRAINT FK_CandidateCertificates_Certificates
        FOREIGN KEY (CertificateID) REFERENCES dbo.Certificates(CertificateID)
        ON DELETE CASCADE;
END
GO

-- CertificateImages.CandidateCertificateID -> CandidateCertificates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CertificateImages_CandidateCertificates')
BEGIN
    ALTER TABLE dbo.CertificateImages
    ADD CONSTRAINT FK_CertificateImages_CandidateCertificates
        FOREIGN KEY (CandidateCertificateID) REFERENCES dbo.CandidateCertificates(CandidateCertificateID)
        ON DELETE CASCADE;
END
GO

-- JobPosts.RecruiterID -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_JobPosts_Recruiters')
BEGIN
    ALTER TABLE dbo.JobPosts
    ADD CONSTRAINT FK_JobPosts_Recruiters
        FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
        ON DELETE NO ACTION;
END
GO

-- JobPosts.CompanyID -> Companies
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_JobPosts_Companies')
BEGIN
    ALTER TABLE dbo.JobPosts
    ADD CONSTRAINT FK_JobPosts_Companies
        FOREIGN KEY (CompanyID) REFERENCES dbo.Companies(CompanyID)
        ON DELETE NO ACTION;
END
GO

-- JobPostDetails.JobPostID -> JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_JobPostDetails_JobPosts')
BEGIN
    ALTER TABLE dbo.JobPostDetails
    ADD CONSTRAINT FK_JobPostDetails_JobPosts
        FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID)
        ON DELETE CASCADE;
END
GO

-- Applications.CandidateID -> Candidates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Applications_Candidates')
BEGIN
    ALTER TABLE dbo.Applications
    ADD CONSTRAINT FK_Applications_Candidates
        FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
        ON DELETE CASCADE;
END
GO

-- Applications.JobPostID -> JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Applications_JobPosts')
BEGIN
    ALTER TABLE dbo.Applications
    ADD CONSTRAINT FK_Applications_JobPosts
        FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID)
        ON DELETE CASCADE;
END
GO

-- Transactions.AccountID -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Transactions_Accounts')
BEGIN
    ALTER TABLE dbo.Transactions
    ADD CONSTRAINT FK_Transactions_Accounts
        FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
        ON DELETE CASCADE;
END
GO

-- SavedJobs FKs (nếu bảng đã tồn tại trước đó mà chưa có FK)
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SavedJobs_Candidates')
BEGIN
    ALTER TABLE dbo.SavedJobs
    ADD CONSTRAINT FK_SavedJobs_Candidates
        FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
        ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SavedJobs_JobPosts')
BEGIN
    ALTER TABLE dbo.SavedJobs
    ADD CONSTRAINT FK_SavedJobs_JobPosts
        FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID)
        ON DELETE CASCADE;
END
GO


/* =====================================================
   3) INDEXES (create if not exists)
   ===================================================== */

-- JobPosts.PostedAt
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JobPosts_PostedAt' AND object_id = OBJECT_ID('dbo.JobPosts'))
    CREATE INDEX IX_JobPosts_PostedAt ON dbo.JobPosts(PostedAt);
GO

-- Companies.PhotoID
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Companies_PhotoID' AND object_id = OBJECT_ID('dbo.Companies'))
    CREATE INDEX IX_Companies_PhotoID ON dbo.Companies(PhotoID);
GO

-- Applications(CandidateID), Applications(JobPostID)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Applications_Candidate' AND object_id = OBJECT_ID('dbo.Applications'))
    CREATE INDEX IX_Applications_Candidate ON dbo.Applications(CandidateID);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Applications_JobPost' AND object_id = OBJECT_ID('dbo.Applications'))
    CREATE INDEX IX_Applications_JobPost ON dbo.Applications(JobPostID);
GO

-- JobPostDetails(JobPostID)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JobPostDetails_JobPost' AND object_id = OBJECT_ID('dbo.JobPostDetails'))
    CREATE INDEX IX_JobPostDetails_JobPost ON dbo.JobPostDetails(JobPostID);
GO

-- Recruiters(CompanyID)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Recruiters_Company' AND object_id = OBJECT_ID('dbo.Recruiters'))
    CREATE INDEX IX_Recruiters_Company ON dbo.Recruiters(CompanyID);
GO

-- SavedJobs(CandidateID, SavedAt DESC)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SavedJobs_Candidate_SavedAt' AND object_id = OBJECT_ID('dbo.SavedJobs'))
    CREATE INDEX IX_SavedJobs_Candidate_SavedAt ON dbo.SavedJobs(CandidateID, SavedAt DESC);
GO

-- SavedJobs(JobPostID)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SavedJobs_JobPost' AND object_id = OBJECT_ID('dbo.SavedJobs'))
    CREATE INDEX IX_SavedJobs_JobPost ON dbo.SavedJobs(JobPostID);
GO


/* =====================================================
   4) FILTERED UNIQUE INDEXES (allow multiple NULLs)
   - Drop any legacy unique constraints/indexes on JobCode/TransactionCode
   ===================================================== */

-- (4.1) JobPosts.JobCode: drop legacy unique constraint/index if present (non-filtered)
DECLARE @objJP INT = OBJECT_ID('dbo.JobPosts');
IF @objJP IS NOT NULL
BEGIN
    -- Drop UNIQUE CONSTRAINT on JobCode (single-column) if exists
    DECLARE @uqJP TABLE (name SYSNAME);
    INSERT INTO @uqJP(name)
    SELECT kc.name
    FROM sys.key_constraints kc
    JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
    JOIN sys.columns c ON c.object_id = kc.parent_object_id AND c.column_id = ic.column_id
    WHERE kc.parent_object_id=@objJP AND kc.type='UQ'
    GROUP BY kc.name
    HAVING COUNT(*)=1 AND MAX(c.name)='JobCode';

    WHILE EXISTS(SELECT 1 FROM @uqJP)
    BEGIN
        DECLARE @n SYSNAME = (SELECT TOP 1 name FROM @uqJP);
        EXEC('ALTER TABLE dbo.JobPosts DROP CONSTRAINT [' + @n + '];');
        DELETE FROM @uqJP WHERE name=@n;
    END

    -- Drop UNIQUE INDEX (non-filtered) on JobCode if exists
    DECLARE @idxJP TABLE (name SYSNAME);
    INSERT INTO @idxJP(name)
    SELECT i.name
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
    JOIN sys.columns c ON c.object_id=i.object_id AND c.column_id=ic.column_id
    WHERE i.object_id=@objJP AND i.is_unique=1 AND i.filter_definition IS NULL
    GROUP BY i.name
    HAVING COUNT(*)=1 AND MAX(c.name)='JobCode';

    WHILE EXISTS(SELECT 1 FROM @idxJP)
    BEGIN
        DECLARE @m SYSNAME = (SELECT TOP 1 name FROM @idxJP);
        EXEC('DROP INDEX [' + @m + '] ON dbo.JobPosts;');
        DELETE FROM @idxJP WHERE name=@m;
    END
END
GO

-- Create filtered unique index on JobCode (non-NULL)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_JobPosts_JobCode_NN' AND object_id = OBJECT_ID('dbo.JobPosts'))
BEGIN
    CREATE UNIQUE INDEX UX_JobPosts_JobCode_NN ON dbo.JobPosts(JobCode) WHERE JobCode IS NOT NULL;
END
GO

-- (4.2) Transactions.TransactionCode: drop legacy unique constraint/index if present (non-filtered)
DECLARE @objTX INT = OBJECT_ID('dbo.Transactions');
IF @objTX IS NOT NULL
BEGIN
    -- Drop UNIQUE CONSTRAINT on TransactionCode
    DECLARE @uqTX TABLE (name SYSNAME);
    INSERT INTO @uqTX(name)
    SELECT kc.name
    FROM sys.key_constraints kc
    JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
    JOIN sys.columns c ON c.object_id = kc.parent_object_id AND c.column_id = ic.column_id
    WHERE kc.parent_object_id=@objTX AND kc.type='UQ'
    GROUP BY kc.name
    HAVING COUNT(*)=1 AND MAX(c.name)='TransactionCode';

    WHILE EXISTS(SELECT 1 FROM @uqTX)
    BEGIN
        DECLARE @n2 SYSNAME = (SELECT TOP 1 name FROM @uqTX);
        EXEC('ALTER TABLE dbo.Transactions DROP CONSTRAINT [' + @n2 + '];');
        DELETE FROM @uqTX WHERE name=@n2;
    END

    -- Drop UNIQUE INDEX (non-filtered) on TransactionCode if exists
    DECLARE @idxTX TABLE (name SYSNAME);
    INSERT INTO @idxTX(name)
    SELECT i.name
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
    JOIN sys.columns c ON c.object_id=i.object_id AND c.column_id=ic.column_id
    WHERE i.object_id=@objTX AND i.is_unique=1 AND i.filter_definition IS NULL
    GROUP BY i.name
    HAVING COUNT(*)=1 AND MAX(c.name)='TransactionCode';

    WHILE EXISTS(SELECT 1 FROM @idxTX)
    BEGIN
        DECLARE @m2 SYSNAME = (SELECT TOP 1 name FROM @idxTX);
        EXEC('DROP INDEX [' + @m2 + '] ON dbo.Transactions;');
        DELETE FROM @idxTX WHERE name=@m2;
    END
END
GO

-- Create filtered unique index on TransactionCode (non-NULL)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Transactions_Code_NN' AND object_id = OBJECT_ID('dbo.Transactions'))
BEGIN
    CREATE UNIQUE INDEX UX_Transactions_Code_NN ON dbo.Transactions(TransactionCode) WHERE TransactionCode IS NOT NULL;
END
GO


/* =====================================================
   5) TRIGGER: Delete SavedJobs when JobPosts closes
   ===================================================== */

DROP TRIGGER IF EXISTS dbo.trg_JobPosts_DeleteSavedWhenClosed;
GO
CREATE TRIGGER dbo.trg_JobPosts_DeleteSavedWhenClosed
ON dbo.JobPosts
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(Status)
    BEGIN
        DELETE sj
        FROM dbo.SavedJobs sj
        INNER JOIN inserted i ON sj.JobPostID = i.JobPostID
        INNER JOIN deleted  d ON d.JobPostID = i.JobPostID
        WHERE (d.Status IS NULL OR d.Status NOT IN (N'Closed', N'Expired', N'Archived'))
          AND  i.Status IN (N'Closed', N'Expired', N'Archived');
    END
END;
GO


/* =====================================================
   6) DONE
   ===================================================== */
PRINT 'JOBPORTAL_EN schema ready. All tables, FKs, indexes & trigger are in place.';

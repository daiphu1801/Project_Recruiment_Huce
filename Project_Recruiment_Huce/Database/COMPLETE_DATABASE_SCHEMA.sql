-- =====================================================================
-- File: COMPLETE_DATABASE_SCHEMA.sql
-- Target: Microsoft SQL Server 2016+
-- Created: 2025-11-25
-- Description: 
--   Script hoàn chỉnh để tạo database JOBPORTAL_EN từ đầu
--   Bao gồm: Database, Tables, Foreign Keys, Indexes, Stored Procedures, Triggers
--   KHÔNG bao gồm: Dữ liệu mẫu (sample data)
--   
-- Notes:
--   - Idempotent: An toàn để chạy nhiều lần
--   - Đã loại bỏ cột Salt (migration sang PBKDF2)
--   - Bao gồm các bảng: ProfilePhotos, Accounts, Admins, Companies, 
--     Recruiters, BankCards, PaymentHistory, PendingPayments, 
--     PostingHistory, Candidates, ResumeFiles, JobPosts, JobPostDetails,
--     Applications, Transactions, SavedJobs, PasswordResetTokens,
--     SchemaMigrations
--   - Bao gồm stored procedures cho analytics và view counting
--   - Bao gồm trigger tự động xóa SavedJobs khi JobPost đóng
-- =====================================================================

SET NOCOUNT ON;
GO

-- =====================================================================
-- 0) TẠO DATABASE (nếu chưa tồn tại)
-- =====================================================================
IF DB_ID(N'JOBPORTAL_EN') IS NULL
BEGIN
    PRINT 'Creating database JOBPORTAL_EN...';
    CREATE DATABASE JOBPORTAL_EN;
    PRINT '[✓] Database JOBPORTAL_EN created successfully.';
END
ELSE
BEGIN
    PRINT '[i] Database JOBPORTAL_EN already exists.';
END
GO

USE JOBPORTAL_EN;
GO

PRINT '';
PRINT '=====================================================================';
PRINT 'JOBPORTAL_EN - Complete Database Schema Creation';
PRINT '=====================================================================';
PRINT '';

-- =====================================================================
-- A) CLEANUP - XÓA CÁC BẢNG KHÔNG SỬ DỤNG (nếu tồn tại)
-- =====================================================================
PRINT '-- Cleaning up unused tables...';

IF OBJECT_ID(N'dbo.CertificateImages','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.CertificateImages;
    PRINT '[✓] Dropped table dbo.CertificateImages';
END

IF OBJECT_ID(N'dbo.CandidateCertificates','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.CandidateCertificates;
    PRINT '[✓] Dropped table dbo.CandidateCertificates';
END

IF OBJECT_ID(N'dbo.Certificates','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Certificates;
    PRINT '[✓] Dropped table dbo.Certificates';
END

IF OBJECT_ID(N'dbo.Educations','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Educations;
    PRINT '[✓] Dropped table dbo.Educations';
END

IF OBJECT_ID(N'dbo.WorkExperiences','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.WorkExperiences;
    PRINT '[✓] Dropped table dbo.WorkExperiences';
END

PRINT '[i] Cleanup completed.';
PRINT '';

-- =====================================================================
-- 1) TẠO CÁC BẢNG (TABLES)
-- =====================================================================
PRINT '-- Creating tables...';
PRINT '';

-- ---------------------------------------------------------------------
-- 1.1) ProfilePhotos - Lưu trữ ảnh đại diện/logo
-- ---------------------------------------------------------------------
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
    PRINT '[✓] Created table dbo.ProfilePhotos';
END

-- ---------------------------------------------------------------------
-- 1.2) Accounts - Tài khoản người dùng (KHÔNG CÒN CỘT SALT)
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Accounts','U') IS NULL
BEGIN
    CREATE TABLE dbo.Accounts (
        AccountID     INT IDENTITY(1,1) PRIMARY KEY,
        Username      NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash  NVARCHAR(255) NULL,  -- PBKDF2 hash (ASP.NET Identity format)
        Email         NVARCHAR(150) NOT NULL UNIQUE,
        Phone         NVARCHAR(20) NULL,
        Role          NVARCHAR(30) NOT NULL CHECK (Role IN (N'Admin', N'Company', N'Recruiter', N'Candidate')),
        CreatedAt     DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        ActiveFlag    TINYINT NOT NULL DEFAULT 1,
        PhotoID       INT NULL
    );
    PRINT '[✓] Created table dbo.Accounts (without Salt column - using PBKDF2)';
END
ELSE
BEGIN
    PRINT '[i] Table dbo.Accounts already exists';
END

-- ---------------------------------------------------------------------
-- 1.3) Admins - Quản trị viên
-- ---------------------------------------------------------------------
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
    PRINT '[✓] Created table dbo.Admins';
END

-- ---------------------------------------------------------------------
-- 1.4) Companies - Công ty
-- ---------------------------------------------------------------------
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
        Fax           NVARCHAR(20) NULL,
        Description   NVARCHAR(1000) NULL,
        CreatedAt     DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        ActiveFlag    TINYINT NOT NULL DEFAULT 1,
        PhotoID       INT NULL  -- Logo công ty
    );
    PRINT '[✓] Created table dbo.Companies';
END
ELSE
BEGIN
    -- Thêm cột Fax nếu chưa có (migration)
    IF COL_LENGTH('dbo.Companies', 'Fax') IS NULL
    BEGIN
        ALTER TABLE dbo.Companies ADD Fax NVARCHAR(20) NULL;
        PRINT '[✓] Added Fax column to dbo.Companies';
    END
END

-- ---------------------------------------------------------------------
-- 1.5) Recruiters - Nhà tuyển dụng
-- ---------------------------------------------------------------------
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
        PhotoID        INT NULL,
        RowVer         ROWVERSION NOT NULL  -- Concurrency token
    );
    PRINT '[✓] Created table dbo.Recruiters';
END
ELSE
BEGIN
    -- Thêm RowVer nếu chưa có
    IF COL_LENGTH('dbo.Recruiters', 'RowVer') IS NULL
    BEGIN
        ALTER TABLE dbo.Recruiters ADD RowVer ROWVERSION NOT NULL;
        PRINT '[✓] Added RowVer column to dbo.Recruiters';
    END
END

-- ---------------------------------------------------------------------
-- 1.6) BankCards - Thẻ ngân hàng
-- ---------------------------------------------------------------------
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
    PRINT '[✓] Created table dbo.BankCards';
END

-- ---------------------------------------------------------------------
-- 1.7) PaymentHistory - Lịch sử thanh toán
-- ---------------------------------------------------------------------
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
    PRINT '[✓] Created table dbo.PaymentHistory';
END

-- ---------------------------------------------------------------------
-- 1.8) PendingPayments - Thanh toán chờ xử lý
-- ---------------------------------------------------------------------
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
    PRINT '[✓] Created table dbo.PendingPayments';
END

-- ---------------------------------------------------------------------
-- 1.9) PostingHistory - Lịch sử đăng tin
-- ---------------------------------------------------------------------
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
    PRINT '[✓] Created table dbo.PostingHistory';
END

-- ---------------------------------------------------------------------
-- 1.10) Candidates - Ứng viên
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Candidates','U') IS NULL
BEGIN
    CREATE TABLE dbo.Candidates (
        CandidateID         INT IDENTITY(1,1) PRIMARY KEY,
        AccountID           INT NOT NULL UNIQUE,
        FullName            NVARCHAR(100) NOT NULL,
        BirthDate           DATE NULL,
        Gender              NVARCHAR(10) NOT NULL CHECK (Gender IN (N'Nam', N'Nữ')),
        Phone               NVARCHAR(15) NULL,
        Email               NVARCHAR(100) NULL,
        ApplicationEmail    NVARCHAR(100) NULL,
        Address             NVARCHAR(255) NULL,
        Summary             NVARCHAR(500) NULL,
        PhotoID             INT NULL,
        CreatedAt           DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        ActiveFlag          TINYINT NOT NULL DEFAULT 1
    );
    PRINT '[✓] Created table dbo.Candidates';
END

-- ---------------------------------------------------------------------
-- 1.11) ResumeFiles - File CV
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.ResumeFiles','U') IS NULL
BEGIN
    CREATE TABLE dbo.ResumeFiles (
        ResumeFileID INT IDENTITY(1,1) PRIMARY KEY,
        CandidateID  INT NOT NULL,
        FileName     NVARCHAR(255) NULL,
        FilePath     NVARCHAR(500) NULL,
        UploadedAt   DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
    PRINT '[✓] Created table dbo.ResumeFiles';
END

-- ---------------------------------------------------------------------
-- 1.12) JobPosts - Tin tuyển dụng (bao gồm ViewCount)
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.JobPosts','U') IS NULL
BEGIN
    CREATE TABLE dbo.JobPosts (
        JobPostID           INT IDENTITY(1,1) PRIMARY KEY,
        JobCode             NVARCHAR(20) NULL,
        RecruiterID         INT NOT NULL,
        CompanyID           INT NULL,
        Title               NVARCHAR(200) NOT NULL,
        Description         NVARCHAR(MAX) NULL,
        Requirements        NVARCHAR(MAX) NULL,
        SalaryFrom          DECIMAL(18,2) NULL,
        SalaryTo            DECIMAL(18,2) NULL,
        SalaryCurrency      NVARCHAR(50) NOT NULL DEFAULT N'VND',
        Location            NVARCHAR(255) NULL,
        EmploymentType      NVARCHAR(100) NULL,
        ApplicationDeadline DATE NULL,
        Status              NVARCHAR(50) NOT NULL DEFAULT N'Published',
        PostedAt            DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        UpdatedAt           DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        ViewCount           INT NOT NULL DEFAULT 0  -- Số lượt xem
    );
    PRINT '[✓] Created table dbo.JobPosts';
END
ELSE
BEGIN
    -- Thêm ViewCount nếu chưa có (migration)
    IF COL_LENGTH('dbo.JobPosts', 'ViewCount') IS NULL
    BEGIN
        ALTER TABLE dbo.JobPosts ADD ViewCount INT NOT NULL CONSTRAINT DF_JobPosts_ViewCount DEFAULT(0);
        PRINT '[✓] Added ViewCount column to dbo.JobPosts';
    END
    
    -- Chuẩn hóa giá trị Status cũ
    IF EXISTS (SELECT 1 FROM dbo.JobPosts WHERE Status = 'Visible')
    BEGIN
        UPDATE dbo.JobPosts SET Status = 'Published' WHERE Status = 'Visible';
        PRINT '[✓] Normalized legacy Status values in dbo.JobPosts';
    END
END

-- ---------------------------------------------------------------------
-- 1.13) JobPostDetails - Chi tiết tin tuyển dụng
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.JobPostDetails','U') IS NULL
BEGIN
    CREATE TABLE dbo.JobPostDetails (
        DetailID            INT IDENTITY(1,1) PRIMARY KEY,
        JobPostID           INT NOT NULL,
        Industry            NVARCHAR(150) NOT NULL,
        Major               NVARCHAR(150) NULL,
        YearsExperience     INT NOT NULL DEFAULT 0,
        DegreeRequired      NVARCHAR(100) NULL,
        Skills              NVARCHAR(MAX) NULL,
        Headcount           INT NOT NULL DEFAULT 1,
        GenderRequirement   NVARCHAR(20) NOT NULL DEFAULT N'Not required',
        AgeFrom             INT NULL,
        AgeTo               INT NULL,
        Status              NVARCHAR(50) NOT NULL DEFAULT N'Published'
    );
    PRINT '[✓] Created table dbo.JobPostDetails';
END

-- ---------------------------------------------------------------------
-- 1.14) Applications - Hồ sơ ứng tuyển
-- ---------------------------------------------------------------------
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
    PRINT '[✓] Created table dbo.Applications';
END

-- ---------------------------------------------------------------------
-- 1.15) Transactions - Giao dịch
-- ---------------------------------------------------------------------
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
    PRINT '[✓] Created table dbo.Transactions';
END

-- ---------------------------------------------------------------------
-- 1.16) SavedJobs - Việc làm đã lưu
-- ---------------------------------------------------------------------
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
    PRINT '[✓] Created table dbo.SavedJobs';
END

-- ---------------------------------------------------------------------
-- 1.17) PasswordResetTokens - Token reset mật khẩu
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.PasswordResetTokens','U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens (
        TokenID      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
        AccountID    INT NOT NULL,
        ResetCode    NVARCHAR(6) NOT NULL,
        Email        NVARCHAR(150) NOT NULL,
        CreatedAt    DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
        ExpiresAt    DATETIME2(7) NOT NULL,
        UsedFlag     TINYINT NOT NULL CONSTRAINT DF_PasswordResetTokens_UsedFlag DEFAULT (0),
        AttemptCount INT NOT NULL CONSTRAINT DF_PasswordResetTokens_AttemptCount DEFAULT (0),
        CONSTRAINT CK_PasswordResetTokens_ExpiresAfterCreated CHECK (ExpiresAt > CreatedAt)
    );
    PRINT '[✓] Created table dbo.PasswordResetTokens';
END

-- ---------------------------------------------------------------------
-- 1.18) SchemaMigrations - Theo dõi migrations
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SchemaMigrations','U') IS NULL
BEGIN
    CREATE TABLE dbo.SchemaMigrations (
        MigrationId INT IDENTITY(1,1) PRIMARY KEY,
        ScriptName  NVARCHAR(200) NOT NULL UNIQUE,
        AppliedAt   DATETIME NOT NULL DEFAULT (GETDATE())
    );
    PRINT '[✓] Created table dbo.SchemaMigrations';
END

PRINT '';
PRINT '[✓] All tables created successfully.';
PRINT '';

-- =====================================================================
-- 2) TẠO FOREIGN KEYS
-- =====================================================================
PRINT '-- Creating foreign keys...';
PRINT '';

-- Accounts.PhotoID -> ProfilePhotos
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Accounts_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Accounts
    ADD CONSTRAINT FK_Accounts_ProfilePhotos
        FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID)
        ON DELETE SET NULL;
    PRINT '[✓] FK_Accounts_ProfilePhotos';
END

-- Companies.PhotoID -> ProfilePhotos
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Companies_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Companies
    ADD CONSTRAINT FK_Companies_ProfilePhotos
        FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID)
        ON DELETE SET NULL;
    PRINT '[✓] FK_Companies_ProfilePhotos';
END

-- Recruiters.PhotoID -> ProfilePhotos
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Recruiters_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_ProfilePhotos
        FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID)
        ON DELETE SET NULL;
    PRINT '[✓] FK_Recruiters_ProfilePhotos';
END

-- Candidates.PhotoID -> ProfilePhotos
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Candidates_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Candidates
    ADD CONSTRAINT FK_Candidates_ProfilePhotos
        FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID)
        ON DELETE SET NULL;
    PRINT '[✓] FK_Candidates_ProfilePhotos';
END

-- Admins.AccountID -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Admins_Accounts')
BEGIN
    ALTER TABLE dbo.Admins
    ADD CONSTRAINT FK_Admins_Accounts
        FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_Admins_Accounts';
END

-- Recruiters.AccountID -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Recruiters_Accounts')
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_Accounts
        FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_Recruiters_Accounts';
END

-- Recruiters.CompanyID -> Companies
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Recruiters_Companies')
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_Companies
        FOREIGN KEY (CompanyID) REFERENCES dbo.Companies(CompanyID)
        ON DELETE NO ACTION;
    PRINT '[✓] FK_Recruiters_Companies';
END

-- BankCards.RecruiterID -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BankCards_Recruiters')
BEGIN
    ALTER TABLE dbo.BankCards
    ADD CONSTRAINT FK_BankCards_Recruiters
        FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_BankCards_Recruiters';
END

-- PaymentHistory.RecruiterID -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PaymentHistory_Recruiters')
BEGIN
    ALTER TABLE dbo.PaymentHistory
    ADD CONSTRAINT FK_PaymentHistory_Recruiters
        FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_PaymentHistory_Recruiters';
END

-- PendingPayments.RecruiterID -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PendingPayments_Recruiters')
BEGIN
    ALTER TABLE dbo.PendingPayments
    ADD CONSTRAINT FK_PendingPayments_Recruiters
        FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_PendingPayments_Recruiters';
END

-- PostingHistory.RecruiterID -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PostingHistory_Recruiters')
BEGIN
    ALTER TABLE dbo.PostingHistory
    ADD CONSTRAINT FK_PostingHistory_Recruiters
        FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_PostingHistory_Recruiters';
END

-- PostingHistory.JobPostID -> JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PostingHistory_JobPosts')
BEGIN
    ALTER TABLE dbo.PostingHistory
    ADD CONSTRAINT FK_PostingHistory_JobPosts
        FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_PostingHistory_JobPosts';
END

-- Candidates.AccountID -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Candidates_Accounts')
BEGIN
    ALTER TABLE dbo.Candidates
    ADD CONSTRAINT FK_Candidates_Accounts
        FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_Candidates_Accounts';
END

-- ResumeFiles.CandidateID -> Candidates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ResumeFiles_Candidates')
BEGIN
    ALTER TABLE dbo.ResumeFiles
    ADD CONSTRAINT FK_ResumeFiles_Candidates
        FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_ResumeFiles_Candidates';
END

-- JobPosts.RecruiterID -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_JobPosts_Recruiters')
BEGIN
    ALTER TABLE dbo.JobPosts
    ADD CONSTRAINT FK_JobPosts_Recruiters
        FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
        ON DELETE NO ACTION;
    PRINT '[✓] FK_JobPosts_Recruiters';
END

-- JobPosts.CompanyID -> Companies
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_JobPosts_Companies')
BEGIN
    ALTER TABLE dbo.JobPosts
    ADD CONSTRAINT FK_JobPosts_Companies
        FOREIGN KEY (CompanyID) REFERENCES dbo.Companies(CompanyID)
        ON DELETE NO ACTION;
    PRINT '[✓] FK_JobPosts_Companies';
END

-- JobPostDetails.JobPostID -> JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_JobPostDetails_JobPosts')
BEGIN
    ALTER TABLE dbo.JobPostDetails
    ADD CONSTRAINT FK_JobPostDetails_JobPosts
        FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_JobPostDetails_JobPosts';
END

-- Applications.CandidateID -> Candidates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Applications_Candidates')
BEGIN
    ALTER TABLE dbo.Applications
    ADD CONSTRAINT FK_Applications_Candidates
        FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_Applications_Candidates';
END

-- Applications.JobPostID -> JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Applications_JobPosts')
BEGIN
    ALTER TABLE dbo.Applications
    ADD CONSTRAINT FK_Applications_JobPosts
        FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_Applications_JobPosts';
END

-- Transactions.AccountID -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Transactions_Accounts')
BEGIN
    ALTER TABLE dbo.Transactions
    ADD CONSTRAINT FK_Transactions_Accounts
        FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_Transactions_Accounts';
END

-- SavedJobs.CandidateID -> Candidates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SavedJobs_Candidates')
BEGIN
    ALTER TABLE dbo.SavedJobs
    ADD CONSTRAINT FK_SavedJobs_Candidates
        FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_SavedJobs_Candidates';
END

-- SavedJobs.JobPostID -> JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SavedJobs_JobPosts')
BEGIN
    ALTER TABLE dbo.SavedJobs
    ADD CONSTRAINT FK_SavedJobs_JobPosts
        FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID)
        ON DELETE CASCADE;
    PRINT '[✓] FK_SavedJobs_JobPosts';
END

-- PasswordResetTokens.AccountID -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PasswordResetTokens_Accounts')
BEGIN
    ALTER TABLE dbo.PasswordResetTokens
    ADD CONSTRAINT FK_PasswordResetTokens_Accounts
        FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID);
    PRINT '[✓] FK_PasswordResetTokens_Accounts';
END

PRINT '';
PRINT '[✓] All foreign keys created successfully.';
PRINT '';

-- =====================================================================
-- 3) TẠO INDEXES
-- =====================================================================
PRINT '-- Creating indexes...';
PRINT '';

-- JobPosts.PostedAt
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JobPosts_PostedAt' AND object_id = OBJECT_ID('dbo.JobPosts'))
BEGIN
    CREATE INDEX IX_JobPosts_PostedAt ON dbo.JobPosts(PostedAt);
    PRINT '[✓] IX_JobPosts_PostedAt';
END

-- JobPosts.RecruiterID (cho analytics)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JobPosts_RecruiterID' AND object_id = OBJECT_ID('dbo.JobPosts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_JobPosts_RecruiterID ON dbo.JobPosts (RecruiterID);
    PRINT '[✓] IX_JobPosts_RecruiterID';
END

-- Companies.PhotoID
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Companies_PhotoID' AND object_id = OBJECT_ID('dbo.Companies'))
BEGIN
    CREATE INDEX IX_Companies_PhotoID ON dbo.Companies(PhotoID);
    PRINT '[✓] IX_Companies_PhotoID';
END

-- Applications.CandidateID
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Applications_Candidate' AND object_id = OBJECT_ID('dbo.Applications'))
BEGIN
    CREATE INDEX IX_Applications_Candidate ON dbo.Applications(CandidateID);
    PRINT '[✓] IX_Applications_Candidate';
END

-- Applications.JobPostID
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Applications_JobPost' AND object_id = OBJECT_ID('dbo.Applications'))
BEGIN
    CREATE INDEX IX_Applications_JobPost ON dbo.Applications(JobPostID);
    PRINT '[✓] IX_Applications_JobPost';
END

-- JobPostDetails.JobPostID
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JobPostDetails_JobPost' AND object_id = OBJECT_ID('dbo.JobPostDetails'))
BEGIN
    CREATE INDEX IX_JobPostDetails_JobPost ON dbo.JobPostDetails(JobPostID);
    PRINT '[✓] IX_JobPostDetails_JobPost';
END

-- Recruiters.CompanyID
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Recruiters_Company' AND object_id = OBJECT_ID('dbo.Recruiters'))
BEGIN
    CREATE INDEX IX_Recruiters_Company ON dbo.Recruiters(CompanyID);
    PRINT '[✓] IX_Recruiters_Company';
END

-- SavedJobs.CandidateID, SavedAt DESC
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SavedJobs_Candidate_SavedAt' AND object_id = OBJECT_ID('dbo.SavedJobs'))
BEGIN
    CREATE INDEX IX_SavedJobs_Candidate_SavedAt ON dbo.SavedJobs(CandidateID, SavedAt DESC);
    PRINT '[✓] IX_SavedJobs_Candidate_SavedAt';
END

-- SavedJobs.JobPostID
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SavedJobs_JobPost' AND object_id = OBJECT_ID('dbo.SavedJobs'))
BEGIN
    CREATE INDEX IX_SavedJobs_JobPost ON dbo.SavedJobs(JobPostID);
    PRINT '[✓] IX_SavedJobs_JobPost';
END

-- PasswordResetTokens indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PasswordResetTokens_AccountID' AND object_id = OBJECT_ID('dbo.PasswordResetTokens'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_PasswordResetTokens_AccountID ON dbo.PasswordResetTokens (AccountID);
    PRINT '[✓] IX_PasswordResetTokens_AccountID';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PasswordResetTokens_ExpiresAt' AND object_id = OBJECT_ID('dbo.PasswordResetTokens'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_PasswordResetTokens_ExpiresAt ON dbo.PasswordResetTokens (ExpiresAt) WHERE UsedFlag = 0;
    PRINT '[✓] IX_PasswordResetTokens_ExpiresAt';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PasswordResetTokens_ResetCode' AND object_id = OBJECT_ID('dbo.PasswordResetTokens'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_PasswordResetTokens_ResetCode ON dbo.PasswordResetTokens (ResetCode);
    PRINT '[✓] IX_PasswordResetTokens_ResetCode';
END

PRINT '';
PRINT '[✓] All indexes created successfully.';
PRINT '';

-- =====================================================================
-- 4) TẠO FILTERED UNIQUE INDEXES (cho phép nhiều NULL)
-- =====================================================================
PRINT '-- Creating filtered unique indexes...';
PRINT '';

-- JobPosts.JobCode - cho phép nhiều NULL, nhưng không trùng khi NOT NULL
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_JobPosts_JobCode_NN' AND object_id = OBJECT_ID('dbo.JobPosts'))
BEGIN
    CREATE UNIQUE INDEX UX_JobPosts_JobCode_NN ON dbo.JobPosts(JobCode) WHERE JobCode IS NOT NULL;
    PRINT '[✓] UX_JobPosts_JobCode_NN (filtered)';
END

-- Transactions.TransactionCode - cho phép nhiều NULL, nhưng không trùng khi NOT NULL
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Transactions_Code_NN' AND object_id = OBJECT_ID('dbo.Transactions'))
BEGIN
    CREATE UNIQUE INDEX UX_Transactions_Code_NN ON dbo.Transactions(TransactionCode) WHERE TransactionCode IS NOT NULL;
    PRINT '[✓] UX_Transactions_Code_NN (filtered)';
END

PRINT '';
PRINT '[✓] All filtered unique indexes created successfully.';
PRINT '';

-- =====================================================================
-- 5) TẠO STORED PROCEDURES
-- =====================================================================
PRINT '-- Creating stored procedures...';
PRINT '';

-- ---------------------------------------------------------------------
-- 5.1) sp_IncrementJobViewCount - Tăng số lượt xem tin tuyển dụng
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_IncrementJobViewCount','P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_IncrementJobViewCount;
    PRINT '[i] Dropped existing procedure dbo.sp_IncrementJobViewCount';
END
GO

CREATE PROCEDURE dbo.sp_IncrementJobViewCount
    @JobPostID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Tăng view count atomically
    UPDATE dbo.JobPosts
    SET ViewCount = ViewCount + 1
    WHERE JobPostID = @JobPostID;
    
    -- Trả về số lượt xem hiện tại
    SELECT ViewCount 
    FROM dbo.JobPosts 
    WHERE JobPostID = @JobPostID;
END
GO

PRINT '[✓] Created procedure dbo.sp_IncrementJobViewCount';

-- ---------------------------------------------------------------------
-- 5.2) sp_GetRecruiterAnalytics - Lấy thống kê cho recruiter
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_GetRecruiterAnalytics','P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_GetRecruiterAnalytics;
    PRINT '[i] Dropped existing procedure dbo.sp_GetRecruiterAnalytics';
END
GO

CREATE PROCEDURE dbo.sp_GetRecruiterAnalytics
    @RecruiterID INT,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default date range nếu không được cung cấp
    IF @FromDate IS NULL
        SET @FromDate = DATEADD(MONTH, -3, GETDATE());
    IF @ToDate IS NULL
        SET @ToDate = GETDATE();
    
    -- Result Set 1: Summary Metrics (tổng hợp)
    SELECT 
        ISNULL(SUM(jp.ViewCount), 0) AS TotalViews,
        ISNULL(COUNT(DISTINCT a.ApplicationID), 0) AS TotalApplications,
        COUNT(DISTINCT jp.JobPostID) AS TotalJobs,
        CASE 
            WHEN SUM(jp.ViewCount) > 0 
            THEN CAST(COUNT(DISTINCT a.ApplicationID) AS FLOAT) / SUM(jp.ViewCount) * 100 
            ELSE 0 
        END AS ConversionRatePercent
    FROM dbo.JobPosts jp
    LEFT JOIN dbo.Applications a ON a.JobPostID = jp.JobPostID
    WHERE jp.RecruiterID = @RecruiterID
        AND jp.PostedAt BETWEEN @FromDate AND @ToDate;
    
    -- Result Set 2: Job Breakdown (chi tiết từng công việc)
    SELECT 
        jp.JobPostID,
        jp.Title AS JobTitle,
        jp.PostedAt,
        jp.Status AS JobStatus,
        jp.ViewCount AS Views,
        COUNT(DISTINCT a.ApplicationID) AS Applications,
        CASE 
            WHEN jp.ViewCount > 0 
            THEN CAST(COUNT(DISTINCT a.ApplicationID) AS FLOAT) / jp.ViewCount * 100 
            ELSE 0 
        END AS ConversionRatePercent
    FROM dbo.JobPosts jp
    LEFT JOIN dbo.Applications a ON a.JobPostID = jp.JobPostID
    WHERE jp.RecruiterID = @RecruiterID
        AND jp.PostedAt BETWEEN @FromDate AND @ToDate
    GROUP BY 
        jp.JobPostID, 
        jp.Title, 
        jp.PostedAt, 
        jp.Status, 
        jp.ViewCount
    ORDER BY jp.PostedAt DESC;
END
GO

PRINT '[✓] Created procedure dbo.sp_GetRecruiterAnalytics';

PRINT '';
PRINT '[✓] All stored procedures created successfully.';
PRINT '';

-- =====================================================================
-- 6) TẠO TRIGGERS
-- =====================================================================
PRINT '-- Creating triggers...';
PRINT '';

-- ---------------------------------------------------------------------
-- 6.1) trg_JobPosts_DeleteSavedWhenClosed - Xóa SavedJobs khi JobPost đóng
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.trg_JobPosts_DeleteSavedWhenClosed','TR') IS NOT NULL
BEGIN
    DROP TRIGGER dbo.trg_JobPosts_DeleteSavedWhenClosed;
    PRINT '[i] Dropped existing trigger dbo.trg_JobPosts_DeleteSavedWhenClosed';
END
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

PRINT '[✓] Created trigger dbo.trg_JobPosts_DeleteSavedWhenClosed';

PRINT '';
PRINT '[✓] All triggers created successfully.';
PRINT '';

-- =====================================================================
-- 7) GHI NHẬN MIGRATIONS ĐÃ ÁP DỤNG
-- =====================================================================
PRINT '-- Recording applied migrations...';
PRINT '';

-- Ghi nhận các migrations đã được tích hợp vào script này
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'AddViewCount_20251118')
BEGIN
    INSERT INTO dbo.SchemaMigrations (ScriptName) VALUES (N'AddViewCount_20251118');
    PRINT '[✓] Recorded migration: AddViewCount_20251118';
END

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'AddFaxToCompanies_20251124')
BEGIN
    INSERT INTO dbo.SchemaMigrations (ScriptName) VALUES (N'AddFaxToCompanies_20251124');
    PRINT '[✓] Recorded migration: AddFaxToCompanies_20251124';
END

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'PasswordHelper_PBKDF2_20251124')
BEGIN
    INSERT INTO dbo.SchemaMigrations (ScriptName) VALUES (N'PasswordHelper_PBKDF2_20251124');
    PRINT '[✓] Recorded migration: PasswordHelper_PBKDF2_20251124';
END

PRINT '';
PRINT '[✓] All migrations recorded successfully.';
PRINT '';

-- =====================================================================
-- 8) HOÀN TẤT
-- =====================================================================
PRINT '=====================================================================';
PRINT 'DATABASE SCHEMA CREATION COMPLETED SUCCESSFULLY!';
PRINT '=====================================================================';
PRINT '';
PRINT 'Summary:';
PRINT '  - Database: JOBPORTAL_EN';
PRINT '  - Tables: 18 tables created';
PRINT '  - Foreign Keys: All relationships established';
PRINT '  - Indexes: Performance indexes created';
PRINT '  - Stored Procedures: 2 procedures for analytics and view counting';
PRINT '  - Triggers: 1 trigger for automatic SavedJobs cleanup';
PRINT '  - Password Security: PBKDF2 (ASP.NET Identity) - Salt column removed';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Run SampleData_Insert.sql to populate test data (optional)';
PRINT '  2. Configure connection string in Web.config';
PRINT '  3. Test application connectivity';
PRINT '';
PRINT 'Notes:';
PRINT '  - This script is idempotent (safe to run multiple times)';
PRINT '  - No sample data included - structure only';
PRINT '  - All migrations have been applied and recorded';
PRINT '';
PRINT '=====================================================================';
GO

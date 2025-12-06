-- =====================================================================
-- File: COMPLETE_DATABASE_SCHEMA_SEPAY.sql
-- Target: Microsoft SQL Server 2016+
-- Created: 2025-12-06
-- Description: 
--   Script hoàn chỉnh để tạo database JOBPORTAL_EN với SePay Integration
--   Bao gồm: Database, Tables, Foreign Keys, Indexes, Stored Procedures
--   
-- Payment System: SePay (VietQR + Bank API)
-- Subscription Plans: Free, Monthly (25,000 VND), Lifetime (250,000 VND)
--   
-- Notes:
--   - Idempotent: An toàn để chạy nhiều lần
--   - ĐÃ LOẠI BỎ: BankCards, PaymentHistory, PendingPayments, Transactions (legacy)
--   - ĐÃ THÊM: SePayTransactions, Subscription fields trong Recruiters
--   - Sử dụng PBKDF2 để hash password (không còn Salt column)
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
PRINT 'JOBPORTAL_EN - Complete Database Schema with SePay Integration';
PRINT '=====================================================================';
PRINT '';

-- =====================================================================
-- A) CLEANUP - XÓA CÁC BẢNG LEGACY PAYMENT (nếu tồn tại)
-- =====================================================================
PRINT '-- Cleaning up legacy payment tables...';

IF OBJECT_ID(N'dbo.BankCards','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.BankCards;
    PRINT '[✓] Dropped table dbo.BankCards (legacy)';
END

IF OBJECT_ID(N'dbo.PaymentHistory','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.PaymentHistory;
    PRINT '[✓] Dropped table dbo.PaymentHistory (legacy)';
END

IF OBJECT_ID(N'dbo.PendingPayments','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.PendingPayments;
    PRINT '[✓] Dropped table dbo.PendingPayments (legacy)';
END

IF OBJECT_ID(N'dbo.Transactions','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Transactions;
    PRINT '[✓] Dropped table dbo.Transactions (legacy)';
END

-- Drop unused tables
IF OBJECT_ID(N'dbo.CertificateImages','U') IS NOT NULL DROP TABLE dbo.CertificateImages;
IF OBJECT_ID(N'dbo.CandidateCertificates','U') IS NOT NULL DROP TABLE dbo.CandidateCertificates;
IF OBJECT_ID(N'dbo.Certificates','U') IS NOT NULL DROP TABLE dbo.Certificates;
IF OBJECT_ID(N'dbo.Educations','U') IS NOT NULL DROP TABLE dbo.Educations;
IF OBJECT_ID(N'dbo.WorkExperiences','U') IS NOT NULL DROP TABLE dbo.WorkExperiences;

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
        FileSize    BIGINT NULL,
        UploadedAt  DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
    PRINT '[✓] Created table dbo.ProfilePhotos';
END

-- ---------------------------------------------------------------------
-- 1.2) Accounts - Tài khoản (Login)
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Accounts','U') IS NULL
BEGIN
    CREATE TABLE dbo.Accounts (
        AccountID   INT IDENTITY(1,1) PRIMARY KEY,
        Username    NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        Email       NVARCHAR(100) NOT NULL UNIQUE,
        Phone       NVARCHAR(20) NULL,
        Role        NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin','Recruiter','Candidate')),
        ActiveFlag  TINYINT NOT NULL DEFAULT 1,
        CreatedAt   DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        LastLoginAt DATETIME2(7) NULL
    );
    PRINT '[✓] Created table dbo.Accounts';
END

-- ---------------------------------------------------------------------
-- 1.3) Admins - Quản trị viên
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Admins','U') IS NULL
BEGIN
    CREATE TABLE dbo.Admins (
        AdminID    INT IDENTITY(1,1) PRIMARY KEY,
        AccountID  INT NOT NULL UNIQUE,
        FullName   NVARCHAR(100) NULL,
        CreatedAt  DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
    PRINT '[✓] Created table dbo.Admins';
END

-- ---------------------------------------------------------------------
-- 1.4) Companies - Công ty
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Companies','U') IS NULL
BEGIN
    CREATE TABLE dbo.Companies (
        CompanyID      INT IDENTITY(1,1) PRIMARY KEY,
        CompanyName    NVARCHAR(200) NOT NULL,
        Address        NVARCHAR(500) NULL,
        Website        NVARCHAR(255) NULL,
        Industry       NVARCHAR(100) NULL,
        Description    NVARCHAR(MAX) NULL,
        Email          NVARCHAR(100) NULL,
        Phone          NVARCHAR(20) NULL,
        Fax            NVARCHAR(20) NULL,
        PhotoID        INT NULL,
        CreatedAt      DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
    PRINT '[✓] Created table dbo.Companies';
END

-- ---------------------------------------------------------------------
-- 1.5) Recruiters - Nhà tuyển dụng (với Subscription)
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Recruiters','U') IS NULL
BEGIN
    CREATE TABLE dbo.Recruiters (
        RecruiterID             INT IDENTITY(1,1) PRIMARY KEY,
        AccountID               INT NOT NULL UNIQUE,
        FullName                NVARCHAR(100) NULL,
        Position                NVARCHAR(100) NULL,
        CompanyID               INT NULL,
        PhotoID                 INT NULL,
        ActiveFlag              TINYINT NOT NULL DEFAULT 1,
        
        -- Subscription Fields (SePay Integration)
        SubscriptionType        NVARCHAR(20) NOT NULL DEFAULT N'Free' CHECK (SubscriptionType IN ('Free','Monthly','Lifetime')),
        SubscriptionExpiryDate  DATETIME NULL,
        FreeJobPostCount        INT NOT NULL DEFAULT 0,
        
        -- Tracking columns for monthly limits
        MonthlyJobPostCount     INT NOT NULL DEFAULT 0,
        MonthlyCVViewCount      INT NOT NULL DEFAULT 0,
        MonthlyEmailInviteCount INT NOT NULL DEFAULT 0,
        LastResetDate           DATETIME NOT NULL DEFAULT GETDATE(),
        
        CreatedAt               DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        RowVer                  ROWVERSION NOT NULL
    );
    PRINT '[✓] Created table dbo.Recruiters with Subscription fields';
END
ELSE
BEGIN
    -- Add subscription columns if not exists
    IF COL_LENGTH('dbo.Recruiters', 'SubscriptionType') IS NULL
    BEGIN
        ALTER TABLE dbo.Recruiters ADD SubscriptionType NVARCHAR(20) NOT NULL DEFAULT N'Free';
        PRINT '[✓] Added SubscriptionType column to dbo.Recruiters';
    END
    
    IF COL_LENGTH('dbo.Recruiters', 'SubscriptionExpiryDate') IS NULL
    BEGIN
        ALTER TABLE dbo.Recruiters ADD SubscriptionExpiryDate DATETIME NULL;
        PRINT '[✓] Added SubscriptionExpiryDate column to dbo.Recruiters';
    END
    
    IF COL_LENGTH('dbo.Recruiters', 'FreeJobPostCount') IS NULL
    BEGIN
        ALTER TABLE dbo.Recruiters ADD FreeJobPostCount INT NOT NULL DEFAULT 0;
        PRINT '[✓] Added FreeJobPostCount column to dbo.Recruiters';
    END
    
    IF COL_LENGTH('dbo.Recruiters', 'MonthlyJobPostCount') IS NULL
    BEGIN
        ALTER TABLE dbo.Recruiters ADD MonthlyJobPostCount INT NOT NULL DEFAULT 0;
        PRINT '[✓] Added MonthlyJobPostCount column to dbo.Recruiters';
    END
    
    IF COL_LENGTH('dbo.Recruiters', 'MonthlyCVViewCount') IS NULL
    BEGIN
        ALTER TABLE dbo.Recruiters ADD MonthlyCVViewCount INT NOT NULL DEFAULT 0;
        PRINT '[✓] Added MonthlyCVViewCount column to dbo.Recruiters';
    END
    
    IF COL_LENGTH('dbo.Recruiters', 'MonthlyEmailInviteCount') IS NULL
    BEGIN
        ALTER TABLE dbo.Recruiters ADD MonthlyEmailInviteCount INT NOT NULL DEFAULT 0;
        PRINT '[✓] Added MonthlyEmailInviteCount column to dbo.Recruiters';
    END
    
    IF COL_LENGTH('dbo.Recruiters', 'LastResetDate') IS NULL
    BEGIN
        ALTER TABLE dbo.Recruiters ADD LastResetDate DATETIME NOT NULL DEFAULT GETDATE();
        PRINT '[✓] Added LastResetDate column to dbo.Recruiters';
    END
    
    IF COL_LENGTH('dbo.Recruiters', 'RowVer') IS NULL
    BEGIN
        ALTER TABLE dbo.Recruiters ADD RowVer ROWVERSION NOT NULL;
        PRINT '[✓] Added RowVer column to dbo.Recruiters';
    END
END

-- ---------------------------------------------------------------------
-- 1.6) SePayTransactions - Lịch sử giao dịch SePay
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SePayTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SePayTransactions (
        Id                   INT IDENTITY(1,1) PRIMARY KEY,
        Gateway              NVARCHAR(50) NULL,
        TransactionDate      DATETIME NOT NULL,
        AccountNumber        NVARCHAR(50) NULL,
        SubAccount           NVARCHAR(50) NULL,
        AmountIn             DECIMAL(18, 2) NOT NULL DEFAULT 0,
        AmountOut            DECIMAL(18, 2) NOT NULL DEFAULT 0,
        Accumulated          DECIMAL(18, 2) NOT NULL DEFAULT 0,
        Code                 NVARCHAR(50) NULL,
        TransactionContent   NVARCHAR(MAX) NULL,
        ReferenceCode        NVARCHAR(50) NULL UNIQUE,
        Description          NVARCHAR(MAX) NULL,
        CreatedAt            DATETIME NOT NULL DEFAULT GETDATE(),
        
        INDEX IX_SePayTransactions_ReferenceCode (ReferenceCode),
        INDEX IX_SePayTransactions_TransactionDate (TransactionDate DESC),
        INDEX IX_SePayTransactions_AccountNumber (AccountNumber)
    );
    PRINT '[✓] Created table dbo.SePayTransactions';
END

-- ---------------------------------------------------------------------
-- 1.7) PostingHistory - Lịch sử đăng tin
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.PostingHistory','U') IS NULL
BEGIN
    CREATE TABLE dbo.PostingHistory (
        PostingHistoryID INT IDENTITY(1,1) PRIMARY KEY,
        RecruiterID      INT NOT NULL,
        PostedAt         DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        JobPostID        INT NULL
    );
    PRINT '[✓] Created table dbo.PostingHistory';
END

-- ---------------------------------------------------------------------
-- 1.8) Candidates - Ứng viên
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Candidates','U') IS NULL
BEGIN
    CREATE TABLE dbo.Candidates (
        CandidateID INT IDENTITY(1,1) PRIMARY KEY,
        AccountID   INT NOT NULL UNIQUE,
        FullName    NVARCHAR(100) NULL,
        DateOfBirth DATE NULL,
        Address     NVARCHAR(500) NULL,
        Email       NVARCHAR(100) NULL,
        Phone       NVARCHAR(20) NULL,
        PhotoID     INT NULL,
        ActiveFlag  TINYINT NOT NULL DEFAULT 1,
        CreatedAt   DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
    PRINT '[✓] Created table dbo.Candidates';
END

-- ---------------------------------------------------------------------
-- 1.9) ResumeFiles - File CV
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.ResumeFiles','U') IS NULL
BEGIN
    CREATE TABLE dbo.ResumeFiles (
        ResumeFileID INT IDENTITY(1,1) PRIMARY KEY,
        CandidateID  INT NOT NULL,
        FileName     NVARCHAR(255) NOT NULL,
        FilePath     NVARCHAR(500) NOT NULL,
        FileSize     BIGINT NULL,
        UploadedAt   DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
    PRINT '[✓] Created table dbo.ResumeFiles';
END

-- ---------------------------------------------------------------------
-- 1.10) JobPosts - Tin tuyển dụng
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.JobPosts','U') IS NULL
BEGIN
    CREATE TABLE dbo.JobPosts (
        JobPostID      INT IDENTITY(1,1) PRIMARY KEY,
        Title          NVARCHAR(255) NOT NULL,
        Location       NVARCHAR(255) NULL,
        Salary         NVARCHAR(100) NULL,
        Description    NVARCHAR(MAX) NULL,
        PostedDate     DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        ExpiryDate     DATETIME2(7) NULL,
        RecruiterID    INT NOT NULL,
        ViewCount      INT NOT NULL DEFAULT 0,
        Status         NVARCHAR(50) NOT NULL DEFAULT N'Published',
        
        -- Subscription tracking
        RefreshCount   INT NOT NULL DEFAULT 0,
        DisplayDays    INT NOT NULL DEFAULT 7,
        LastRefreshDate DATETIME NULL,
        
        CreatedAt      DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
    PRINT '[✓] Created table dbo.JobPosts';
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.JobPosts', 'RefreshCount') IS NULL
    BEGIN
        ALTER TABLE dbo.JobPosts ADD RefreshCount INT NOT NULL DEFAULT 0;
        PRINT '[✓] Added RefreshCount column to dbo.JobPosts';
    END
    
    IF COL_LENGTH('dbo.JobPosts', 'DisplayDays') IS NULL
    BEGIN
        ALTER TABLE dbo.JobPosts ADD DisplayDays INT NOT NULL DEFAULT 7;
        PRINT '[✓] Added DisplayDays column to dbo.JobPosts';
    END
    
    IF COL_LENGTH('dbo.JobPosts', 'LastRefreshDate') IS NULL
    BEGIN
        ALTER TABLE dbo.JobPosts ADD LastRefreshDate DATETIME NULL;
        PRINT '[✓] Added LastRefreshDate column to dbo.JobPosts';
    END
END

-- ---------------------------------------------------------------------
-- 1.11) JobPostDetails - Chi tiết tin tuyển dụng
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.JobPostDetails','U') IS NULL
BEGIN
    CREATE TABLE dbo.JobPostDetails (
        JobPostDetailID  INT IDENTITY(1,1) PRIMARY KEY,
        JobPostID        INT NOT NULL UNIQUE,
        Industry         NVARCHAR(100) NULL,
        Major            NVARCHAR(100) NULL,
        YearsExperience  NVARCHAR(50) NULL,
        DegreeRequired   NVARCHAR(50) NULL,
        Skills           NVARCHAR(500) NULL,
        Headcount        INT NULL,
        WorkType         NVARCHAR(50) NULL,
        Gender           NVARCHAR(20) NULL,
        Benefits         NVARCHAR(MAX) NULL,
        Requirements     NVARCHAR(MAX) NULL,
        OtherInfo        NVARCHAR(MAX) NULL
    );
    PRINT '[✓] Created table dbo.JobPostDetails';
END

-- ---------------------------------------------------------------------
-- 1.12) Applications - Đơn ứng tuyển
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Applications','U') IS NULL
BEGIN
    CREATE TABLE dbo.Applications (
        ApplicationID  INT IDENTITY(1,1) PRIMARY KEY,
        JobPostID      INT NOT NULL,
        CandidateID    INT NOT NULL,
        AppliedDate    DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        Status         NVARCHAR(50) NOT NULL DEFAULT N'Pending',
        ResumeFileID   INT NULL,
        CoverLetter    NVARCHAR(MAX) NULL
    );
    PRINT '[✓] Created table dbo.Applications';
END

-- ---------------------------------------------------------------------
-- 1.13) SavedJobs - Tin đã lưu
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SavedJobs','U') IS NULL
BEGIN
    CREATE TABLE dbo.SavedJobs (
        SavedJobID  INT IDENTITY(1,1) PRIMARY KEY,
        CandidateID INT NOT NULL,
        JobPostID   INT NOT NULL,
        SavedDate   DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_SavedJobs UNIQUE (CandidateID, JobPostID)
    );
    PRINT '[✓] Created table dbo.SavedJobs';
END

-- ---------------------------------------------------------------------
-- 1.14) PasswordResetTokens - Token đặt lại mật khẩu
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.PasswordResetTokens','U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens (
        TokenID     INT IDENTITY(1,1) PRIMARY KEY,
        AccountID   INT NOT NULL,
        Token       NVARCHAR(255) NOT NULL UNIQUE,
        ExpiryDate  DATETIME2(7) NOT NULL,
        IsUsed      BIT NOT NULL DEFAULT 0,
        CreatedAt   DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
    PRINT '[✓] Created table dbo.PasswordResetTokens';
END

-- ---------------------------------------------------------------------
-- 1.15) SchemaMigrations - Theo dõi migration
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SchemaMigrations','U') IS NULL
BEGIN
    CREATE TABLE dbo.SchemaMigrations (
        MigrationID   INT IDENTITY(1,1) PRIMARY KEY,
        MigrationName NVARCHAR(255) NOT NULL UNIQUE,
        AppliedAt     DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
    );
    PRINT '[✓] Created table dbo.SchemaMigrations';
    
    -- Insert initial migration
    INSERT INTO dbo.SchemaMigrations (MigrationName) 
    VALUES ('Initial_Schema_SePay_Integration');
END

-- ---------------------------------------------------------------------
-- 1.16) ContactMessages - Tin nhắn liên hệ từ người dùng
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.ContactMessages','U') IS NULL
BEGIN
    CREATE TABLE dbo.ContactMessages (
        ContactMessageID INT IDENTITY(1,1) PRIMARY KEY,
        FirstName        NVARCHAR(50) NOT NULL,
        LastName         NVARCHAR(50) NOT NULL,
        Email            NVARCHAR(100) NOT NULL,
        Subject          NVARCHAR(200) NOT NULL,
        Message          NVARCHAR(MAX) NOT NULL,
        Status           NVARCHAR(20) NOT NULL DEFAULT N'Pending' CHECK (Status IN ('Pending','Read','Replied','Closed')),
        AdminNotes       NVARCHAR(MAX) NULL,
        CreatedAt        DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
        ReadAt           DATETIME2(7) NULL,
        RepliedAt        DATETIME2(7) NULL,
        
        INDEX IX_ContactMessages_Email (Email),
        INDEX IX_ContactMessages_Status (Status),
        INDEX IX_ContactMessages_CreatedAt (CreatedAt DESC)
    );
    PRINT '[✓] Created table dbo.ContactMessages';
END

PRINT '';
PRINT '[i] All tables created successfully.';
PRINT '';

-- =====================================================================
-- 2) TẠO FOREIGN KEYS
-- =====================================================================
PRINT '-- Creating foreign keys...';

-- Admins -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Admins_Accounts')
BEGIN
    ALTER TABLE dbo.Admins
    ADD CONSTRAINT FK_Admins_Accounts FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID) ON DELETE CASCADE;
    PRINT '[✓] FK_Admins_Accounts';
END

-- Companies -> ProfilePhotos
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Companies_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Companies
    ADD CONSTRAINT FK_Companies_ProfilePhotos FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID) ON DELETE SET NULL;
    PRINT '[✓] FK_Companies_ProfilePhotos';
END

-- Recruiters -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Recruiters_Accounts')
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_Accounts FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID) ON DELETE CASCADE;
    PRINT '[✓] FK_Recruiters_Accounts';
END

-- Recruiters -> Companies
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Recruiters_Companies')
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_Companies FOREIGN KEY (CompanyID) REFERENCES dbo.Companies(CompanyID) ON DELETE SET NULL;
    PRINT '[✓] FK_Recruiters_Companies';
END

-- Recruiters -> ProfilePhotos
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Recruiters_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_ProfilePhotos FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID) ON DELETE SET NULL;
    PRINT '[✓] FK_Recruiters_ProfilePhotos';
END

-- PostingHistory -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PostingHistory_Recruiters')
BEGIN
    ALTER TABLE dbo.PostingHistory
    ADD CONSTRAINT FK_PostingHistory_Recruiters FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID) ON DELETE CASCADE;
    PRINT '[✓] FK_PostingHistory_Recruiters';
END

-- PostingHistory -> JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PostingHistory_JobPosts')
BEGIN
    ALTER TABLE dbo.PostingHistory
    ADD CONSTRAINT FK_PostingHistory_JobPosts FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID) ON DELETE SET NULL;
    PRINT '[✓] FK_PostingHistory_JobPosts';
END

-- Candidates -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Candidates_Accounts')
BEGIN
    ALTER TABLE dbo.Candidates
    ADD CONSTRAINT FK_Candidates_Accounts FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID) ON DELETE CASCADE;
    PRINT '[✓] FK_Candidates_Accounts';
END

-- Candidates -> ProfilePhotos
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Candidates_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Candidates
    ADD CONSTRAINT FK_Candidates_ProfilePhotos FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID) ON DELETE SET NULL;
    PRINT '[✓] FK_Candidates_ProfilePhotos';
END

-- ResumeFiles -> Candidates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ResumeFiles_Candidates')
BEGIN
    ALTER TABLE dbo.ResumeFiles
    ADD CONSTRAINT FK_ResumeFiles_Candidates FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID) ON DELETE CASCADE;
    PRINT '[✓] FK_ResumeFiles_Candidates';
END

-- JobPosts -> Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_JobPosts_Recruiters')
BEGIN
    ALTER TABLE dbo.JobPosts
    ADD CONSTRAINT FK_JobPosts_Recruiters FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID) ON DELETE CASCADE;
    PRINT '[✓] FK_JobPosts_Recruiters';
END

-- JobPostDetails -> JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_JobPostDetails_JobPosts')
BEGIN
    ALTER TABLE dbo.JobPostDetails
    ADD CONSTRAINT FK_JobPostDetails_JobPosts FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID) ON DELETE CASCADE;
    PRINT '[✓] FK_JobPostDetails_JobPosts';
END

-- Applications -> JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Applications_JobPosts')
BEGIN
    ALTER TABLE dbo.Applications
    ADD CONSTRAINT FK_Applications_JobPosts FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID) ON DELETE CASCADE;
    PRINT '[✓] FK_Applications_JobPosts';
END

-- Applications -> Candidates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Applications_Candidates')
BEGIN
    ALTER TABLE dbo.Applications
    ADD CONSTRAINT FK_Applications_Candidates FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID) ON DELETE NO ACTION;
    PRINT '[✓] FK_Applications_Candidates';
END

-- Applications -> ResumeFiles
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Applications_ResumeFiles')
BEGIN
    ALTER TABLE dbo.Applications
    ADD CONSTRAINT FK_Applications_ResumeFiles FOREIGN KEY (ResumeFileID) REFERENCES dbo.ResumeFiles(ResumeFileID) ON DELETE SET NULL;
    PRINT '[✓] FK_Applications_ResumeFiles';
END

-- SavedJobs -> Candidates
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SavedJobs_Candidates')
BEGIN
    ALTER TABLE dbo.SavedJobs
    ADD CONSTRAINT FK_SavedJobs_Candidates FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID) ON DELETE CASCADE;
    PRINT '[✓] FK_SavedJobs_Candidates';
END

-- SavedJobs -> JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SavedJobs_JobPosts')
BEGIN
    ALTER TABLE dbo.SavedJobs
    ADD CONSTRAINT FK_SavedJobs_JobPosts FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID) ON DELETE NO ACTION;
    PRINT '[✓] FK_SavedJobs_JobPosts';
END

-- PasswordResetTokens -> Accounts
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PasswordResetTokens_Accounts')
BEGIN
    ALTER TABLE dbo.PasswordResetTokens
    ADD CONSTRAINT FK_PasswordResetTokens_Accounts FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID) ON DELETE CASCADE;
    PRINT '[✓] FK_PasswordResetTokens_Accounts';
END

PRINT '';
PRINT '[i] All foreign keys created successfully.';
PRINT '';

-- =====================================================================
-- 3) TẠO INDEXES (Performance optimization)
-- =====================================================================
PRINT '-- Creating indexes...';

-- Accounts
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_Email')
    CREATE INDEX IX_Accounts_Email ON dbo.Accounts(Email);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_Role')
    CREATE INDEX IX_Accounts_Role ON dbo.Accounts(Role);

-- Recruiters
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Recruiters_CompanyID')
    CREATE INDEX IX_Recruiters_CompanyID ON dbo.Recruiters(CompanyID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Recruiters_SubscriptionType')
    CREATE INDEX IX_Recruiters_SubscriptionType ON dbo.Recruiters(SubscriptionType);

-- JobPosts
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JobPosts_RecruiterID')
    CREATE INDEX IX_JobPosts_RecruiterID ON dbo.JobPosts(RecruiterID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JobPosts_Status')
    CREATE INDEX IX_JobPosts_Status ON dbo.JobPosts(Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JobPosts_PostedDate')
    CREATE INDEX IX_JobPosts_PostedDate ON dbo.JobPosts(PostedDate DESC);

-- Applications
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Applications_JobPostID')
    CREATE INDEX IX_Applications_JobPostID ON dbo.Applications(JobPostID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Applications_CandidateID')
    CREATE INDEX IX_Applications_CandidateID ON dbo.Applications(CandidateID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Applications_Status')
    CREATE INDEX IX_Applications_Status ON dbo.Applications(Status);

PRINT '[✓] All indexes created successfully.';
PRINT '';

-- =====================================================================
-- 4) STORED PROCEDURES
-- =====================================================================
PRINT '-- Creating stored procedures...';

-- SP: Reset monthly counters for Monthly subscription
IF OBJECT_ID(N'dbo.sp_ResetMonthlyCounters', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ResetMonthlyCounters;
GO

CREATE PROCEDURE dbo.sp_ResetMonthlyCounters
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Recruiters 
    SET MonthlyJobPostCount = 0,
        MonthlyCVViewCount = 0,
        MonthlyEmailInviteCount = 0,
        LastResetDate = GETDATE()
    WHERE SubscriptionType = 'Monthly' 
      AND DATEDIFF(DAY, LastResetDate, GETDATE()) >= 30;
      
    RETURN @@ROWCOUNT;
END
GO

PRINT '[✓] Created stored procedure sp_ResetMonthlyCounters';

-- SP: Get subscription statistics
IF OBJECT_ID(N'dbo.sp_GetSubscriptionStats', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetSubscriptionStats;
GO

CREATE PROCEDURE dbo.sp_GetSubscriptionStats
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        SubscriptionType,
        COUNT(*) AS TotalUsers,
        SUM(CASE WHEN SubscriptionExpiryDate IS NULL OR SubscriptionExpiryDate > GETDATE() THEN 1 ELSE 0 END) AS ActiveUsers,
        SUM(CASE WHEN SubscriptionExpiryDate < GETDATE() THEN 1 ELSE 0 END) AS ExpiredUsers
    FROM dbo.Recruiters
    GROUP BY SubscriptionType
    ORDER BY 
        CASE SubscriptionType 
            WHEN 'Lifetime' THEN 1 
            WHEN 'Monthly' THEN 2 
            WHEN 'Free' THEN 3 
            ELSE 4 
        END;
END
GO

PRINT '[✓] Created stored procedure sp_GetSubscriptionStats';

-- SP: Increment view count
IF OBJECT_ID(N'dbo.sp_IncrementJobPostViewCount', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_IncrementJobPostViewCount;
GO

CREATE PROCEDURE dbo.sp_IncrementJobPostViewCount
    @JobPostID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.JobPosts 
    SET ViewCount = ViewCount + 1
    WHERE JobPostID = @JobPostID;
END
GO

PRINT '[✓] Created stored procedure sp_IncrementJobPostViewCount';

-- SP: Get recruiter analytics with summary metrics
IF OBJECT_ID(N'dbo.sp_GetRecruiterAnalytics', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetRecruiterAnalytics;
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
        AND jp.PostedDate BETWEEN @FromDate AND @ToDate;
    
    -- Result Set 2: Job Breakdown (chi tiết từng công việc)
    SELECT 
        jp.JobPostID,
        jp.Title AS JobTitle,
        jp.PostedDate,
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
        AND jp.PostedDate BETWEEN @FromDate AND @ToDate
    GROUP BY 
        jp.JobPostID, 
        jp.Title, 
        jp.PostedDate, 
        jp.Status, 
        jp.ViewCount
    ORDER BY jp.PostedDate DESC;
END
GO

PRINT '[✓] Created stored procedure sp_GetRecruiterAnalytics';

PRINT '';
PRINT '[i] All stored procedures created successfully.';
PRINT '';

-- =====================================================================
-- 5) TRIGGERS
-- =====================================================================
PRINT '-- Creating triggers...';

-- Trigger: Auto delete SavedJobs when JobPost is closed
IF OBJECT_ID(N'dbo.trg_JobPosts_StatusUpdate', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_JobPosts_StatusUpdate;
GO

CREATE TRIGGER dbo.trg_JobPosts_StatusUpdate
ON dbo.JobPosts
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF UPDATE(Status)
    BEGIN
        DELETE FROM dbo.SavedJobs
        WHERE JobPostID IN (
            SELECT i.JobPostID 
            FROM inserted i 
            WHERE i.Status IN ('Closed', 'Expired', 'Deleted')
        );
    END
END
GO

PRINT '[✓] Created trigger trg_JobPosts_StatusUpdate';
PRINT '';

-- =====================================================================
-- FINAL MESSAGE
-- =====================================================================
PRINT '';
PRINT '=====================================================================';
PRINT '✓ Database schema created successfully!';
PRINT '=====================================================================';
PRINT '';
PRINT 'Summary:';
PRINT '  - Tables: Accounts, Admins, Companies, Recruiters (with Subscription),';
PRINT '    Candidates, JobPosts, Applications, SePayTransactions, etc.';
PRINT '  - Payment System: SePay (VietQR + Bank API)';
PRINT '  - Subscription Plans: Free, Monthly (25K VND), Lifetime (250K VND)';
PRINT '  - Foreign Keys: ✓';
PRINT '  - Indexes: ✓';
PRINT '  - Stored Procedures: ✓';
PRINT '  - Triggers: ✓';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Configure Web.config with SePay credentials';
PRINT '  2. Setup Ngrok for webhook testing';
PRINT '  3. Configure SePay Dashboard webhook URL';
PRINT '  4. Test payment flow with QR codes';
PRINT '';
PRINT '=====================================================================';
GO

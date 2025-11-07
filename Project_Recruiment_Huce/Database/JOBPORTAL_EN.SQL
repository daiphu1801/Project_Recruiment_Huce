-- File: JOBPORTAL_EN_full_with_company_photo.sql
-- Clean schema with Companies.PhotoID referencing ProfilePhotos (SQL Server)

SET NOCOUNT ON;

IF DB_ID(N'JOBPORTAL_EN') IS NULL
BEGIN
    CREATE DATABASE JOBPORTAL_EN;
END;
GO
USE JOBPORTAL_EN;
GO

/* =========================
   TABLES
   ========================= */

-- ProfilePhotos (ảnh đại diện / avatar)
CREATE TABLE IF NOT EXISTS ProfilePhotos (
    PhotoID     INT IDENTITY(1,1) PRIMARY KEY,
    FileName    NVARCHAR(255) NOT NULL,
    FilePath    NVARCHAR(500) NOT NULL,
    FileSizeKB  INT NULL,
    FileFormat  NVARCHAR(50) NULL,
    UploadedAt  DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
);
GO

-- Accounts (tài khoản)
CREATE TABLE IF NOT EXISTS Accounts (
    AccountID   INT IDENTITY(1,1) PRIMARY KEY,
    Username    NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NULL,
    Salt        NVARCHAR(255) NULL,
    Email       NVARCHAR(150) NOT NULL UNIQUE,
    Phone       NVARCHAR(20) NULL,
    Role        NVARCHAR(30) NOT NULL
        CHECK (Role IN (N'Admin', N'Company', N'Recruiter', N'Candidate')),
    CreatedAt   DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
    ActiveFlag  TINYINT NOT NULL DEFAULT 1,
    PhotoID     INT NULL
);
GO

-- Admins
CREATE TABLE IF NOT EXISTS Admins (
    AdminID      INT IDENTITY(1,1) PRIMARY KEY,
    AccountID    INT NOT NULL UNIQUE,
    FullName     NVARCHAR(100) NOT NULL,
    ContactEmail NVARCHAR(150) NULL,
    Permission   NVARCHAR(100) NOT NULL DEFAULT N'FullAccess',
    CreatedAt    DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
);
GO

-- Companies (thêm PhotoID)
CREATE TABLE IF NOT EXISTS Companies (
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
    PhotoID       INT NULL              -- NEW: logo/ảnh công ty
);
GO

-- Recruiters
CREATE TABLE IF NOT EXISTS Recruiters (
    RecruiterID   INT IDENTITY(1,1) PRIMARY KEY,
    AccountID     INT NOT NULL UNIQUE,
    CompanyID     INT NULL,
    FullName      NVARCHAR(100) NOT NULL,
    PositionTitle NVARCHAR(100) NULL,
    CompanyEmail  NVARCHAR(150) NULL,
    Phone         NVARCHAR(20) NULL,
    CreatedAt     DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
    ActiveFlag    TINYINT NOT NULL DEFAULT 1,
    PhotoID       INT NULL              -- NEW: ảnh đại diện nhà tuyển dụng
);
GO

-- BankCards
CREATE TABLE IF NOT EXISTS BankCards (
    CardID         INT IDENTITY(1,1) PRIMARY KEY,
    RecruiterID    INT NOT NULL,
    CardNumber     NVARCHAR(50) NOT NULL,
    BankName       NVARCHAR(150) NULL,
    CardHolderName NVARCHAR(100) NULL,
    ExpiryDate     DATE NULL,
    ActiveFlag     TINYINT NOT NULL DEFAULT 1,
    CreatedAt      DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
);
GO

-- PaymentHistory
CREATE TABLE IF NOT EXISTS PaymentHistory (
    PaymentHistoryID INT IDENTITY(1,1) PRIMARY KEY,
    RecruiterID      INT NOT NULL,
    Amount           DECIMAL(18,2) NOT NULL,
    Method           NVARCHAR(50) NULL,
    PaidAt           DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
    Status           NVARCHAR(50) NOT NULL DEFAULT N'Completed',
    Note             NVARCHAR(255) NULL
);
GO

-- PendingPayments
CREATE TABLE IF NOT EXISTS PendingPayments (
    PendingPaymentID INT IDENTITY(1,1) PRIMARY KEY,
    RecruiterID      INT NOT NULL,
    Amount           DECIMAL(18,2) NOT NULL,
    DueDate          DATE NULL,
    Reason           NVARCHAR(255) NULL,
    Status           NVARCHAR(50) NOT NULL DEFAULT N'Unpaid'
);
GO

-- PostingHistory
CREATE TABLE IF NOT EXISTS PostingHistory (
    PostingHistoryID INT IDENTITY(1,1) PRIMARY KEY,
    RecruiterID      INT NOT NULL,
    JobPostID        INT NOT NULL,
    PostedAt         DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
    Status           NVARCHAR(50) NOT NULL DEFAULT N'Open',
    Note             NVARCHAR(255) NULL
);
GO

-- Candidates
CREATE TABLE IF NOT EXISTS Candidates (
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
GO

-- ResumeFiles
CREATE TABLE IF NOT EXISTS ResumeFiles (
    ResumeFileID INT IDENTITY(1,1) PRIMARY KEY,
    CandidateID  INT NOT NULL,
    FileName     NVARCHAR(255) NULL,
    FilePath     NVARCHAR(500) NULL,
    UploadedAt   DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
);
GO

-- WorkExperiences
CREATE TABLE IF NOT EXISTS WorkExperiences (
    ExperienceID   INT IDENTITY(1,1) PRIMARY KEY,
    CandidateID    INT NOT NULL,
    PreviousCompany NVARCHAR(255) NOT NULL,
    JobTitle       NVARCHAR(150) NOT NULL,
    Industry       NVARCHAR(150) NULL,
    Major          NVARCHAR(150) NULL,
    JobDescription NVARCHAR(MAX) NULL,
    StartDate      DATE NOT NULL,
    EndDate        DATE NULL,
    Salary         DECIMAL(18,2) NULL,
    SalaryCurrency NVARCHAR(20) NOT NULL DEFAULT N'VND',
    Achievements   NVARCHAR(500) NULL,
    CreatedAt      DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
);
GO

-- Educations
CREATE TABLE IF NOT EXISTS Educations (
    EducationID     INT IDENTITY(1,1) PRIMARY KEY,
    CandidateID     INT NOT NULL,
    School          NVARCHAR(255) NOT NULL,
    Degree          NVARCHAR(100) NOT NULL,
    FieldOfStudy    NVARCHAR(150) NOT NULL,
    Major           NVARCHAR(150) NULL,
    StartYear       INT NULL,
    EndYear         INT NULL,
    ExtraDescription NVARCHAR(500) NULL,
    DegreeFilePath  NVARCHAR(500) NULL,
    UploadedAt      DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
);
GO

-- Certificates
CREATE TABLE IF NOT EXISTS Certificates (
    CertificateID   INT IDENTITY(1,1) PRIMARY KEY,
    CertificateName NVARCHAR(100) NOT NULL,
    Issuer          NVARCHAR(100) NULL,
    Industry        NVARCHAR(100) NOT NULL,
    Major           NVARCHAR(150) NULL,
    Description     NVARCHAR(255) NULL
);
GO

-- CandidateCertificates
CREATE TABLE IF NOT EXISTS CandidateCertificates (
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
GO

-- CertificateImages
CREATE TABLE IF NOT EXISTS CertificateImages (
    CertificateImageID   INT IDENTITY(1,1) PRIMARY KEY,
    CandidateCertificateID INT NOT NULL,
    FileName             NVARCHAR(255) NULL,
    FilePath             NVARCHAR(500) NULL,
    UploadedAt           DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
);
GO

-- JobPosts
CREATE TABLE IF NOT EXISTS JobPosts (
    JobPostID       INT IDENTITY(1,1) PRIMARY KEY,
    JobCode         NVARCHAR(20) UNIQUE NULL,
    RecruiterID     INT NOT NULL,
    CompanyID       INT NULL,
    Title           NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(MAX) NULL,
    Requirements    NVARCHAR(MAX) NULL,
    SalaryFrom      DECIMAL(18,2) NULL,
    SalaryTo        DECIMAL(18,2) NULL,
    SalaryCurrency  NVARCHAR(50) NOT NULL DEFAULT N'VND',
    Location        NVARCHAR(255) NULL,
    EmploymentType  NVARCHAR(100) NULL,
    ApplicationDeadline DATE NULL,
    Status          NVARCHAR(50) NOT NULL DEFAULT N'Published',
    PostedAt        DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt       DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
);
GO

-- JobPostDetails
CREATE TABLE IF NOT EXISTS JobPostDetails (
    DetailID        INT IDENTITY(1,1) PRIMARY KEY,
    JobPostID       INT NOT NULL,
    Industry        NVARCHAR(150) NOT NULL,
    Major           NVARCHAR(150) NULL,
    YearsExperience INT NOT NULL DEFAULT 0,
    DegreeRequired  NVARCHAR(100) NULL,
    Skills          NVARCHAR(MAX) NULL,
    Headcount       INT NOT NULL DEFAULT 1,
    GenderRequirement NVARCHAR(20) NOT NULL DEFAULT N'Not required',
    AgeFrom         INT NULL,
    AgeTo           INT NULL,
    Status          NVARCHAR(50) NOT NULL DEFAULT N'Published'
);
GO

-- Applications
CREATE TABLE IF NOT EXISTS Applications (
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
GO

-- Transactions
CREATE TABLE IF NOT EXISTS Transactions (
    TransactionID   INT IDENTITY(1,1) PRIMARY KEY,
    TransactionCode NVARCHAR(20) UNIQUE NULL,
    AccountID       INT NOT NULL,
    TransactionType NVARCHAR(100) NOT NULL,
    Amount          DECIMAL(18,2) NOT NULL,
    Method          NVARCHAR(50) NOT NULL,
    Status          NVARCHAR(50) NOT NULL DEFAULT N'Processing',
    TransactionDate DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
    Description     NVARCHAR(500) NULL,
    UpdatedAt       DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
);
GO

/* =========================
   FOREIGN KEYS
   ========================= */

-- Accounts.PhotoID -> ProfilePhotos
ALTER TABLE dbo.Accounts
    ADD CONSTRAINT FK_Accounts_ProfilePhotos
    FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID)
    ON DELETE SET NULL;
GO

-- Companies.PhotoID -> ProfilePhotos  (NEW)
ALTER TABLE dbo.Companies
    ADD CONSTRAINT FK_Companies_ProfilePhotos
    FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID)
    ON DELETE SET NULL;
GO

-- Admins.AccountID -> Accounts
ALTER TABLE dbo.Admins
    ADD CONSTRAINT FK_Admins_Accounts
    FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
    ON DELETE CASCADE;
GO

-- Recruiters.AccountID -> Accounts
ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_Accounts
    FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
    ON DELETE CASCADE;
GO

-- Recruiters.CompanyID -> Companies
ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_Companies
    FOREIGN KEY (CompanyID) REFERENCES dbo.Companies(CompanyID)
    ON DELETE NO ACTION;
GO

-- BankCards.RecruiterID -> Recruiters
ALTER TABLE dbo.BankCards
    ADD CONSTRAINT FK_BankCards_Recruiters
    FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
    ON DELETE CASCADE;
GO

-- PaymentHistory.RecruiterID -> Recruiters
ALTER TABLE dbo.PaymentHistory
    ADD CONSTRAINT FK_PaymentHistory_Recruiters
    FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
    ON DELETE CASCADE;
GO

-- PendingPayments.RecruiterID -> Recruiters
ALTER TABLE dbo.PendingPayments
    ADD CONSTRAINT FK_PendingPayments_Recruiters
    FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
    ON DELETE CASCADE;
GO

-- PostingHistory.RecruiterID -> Recruiters
ALTER TABLE dbo.PostingHistory
    ADD CONSTRAINT FK_PostingHistory_Recruiters
    FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
    ON DELETE CASCADE;
GO

-- PostingHistory.JobPostID -> JobPosts
ALTER TABLE dbo.PostingHistory
    ADD CONSTRAINT FK_PostingHistory_JobPosts
    FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID)
    ON DELETE CASCADE;
GO

-- Candidates.AccountID -> Accounts
ALTER TABLE dbo.Candidates
    ADD CONSTRAINT FK_Candidates_Accounts
    FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
    ON DELETE CASCADE;
GO

-- ResumeFiles.CandidateID -> Candidates
ALTER TABLE dbo.ResumeFiles
    ADD CONSTRAINT FK_ResumeFiles_Candidates
    FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
    ON DELETE CASCADE;
GO

-- WorkExperiences.CandidateID -> Candidates
ALTER TABLE dbo.WorkExperiences
    ADD CONSTRAINT FK_WorkExperiences_Candidates
    FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
    ON DELETE CASCADE;
GO

-- Educations.CandidateID -> Candidates
ALTER TABLE dbo.Educations
    ADD CONSTRAINT FK_Educations_Candidates
    FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
    ON DELETE CASCADE;
GO

-- CandidateCertificates.CandidateID -> Candidates
ALTER TABLE dbo.CandidateCertificates
    ADD CONSTRAINT FK_CandidateCertificates_Candidates
    FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
    ON DELETE CASCADE;
GO

-- CandidateCertificates.CertificateID -> Certificates
ALTER TABLE dbo.CandidateCertificates
    ADD CONSTRAINT FK_CandidateCertificates_Certificates
    FOREIGN KEY (CertificateID) REFERENCES dbo.Certificates(CertificateID)
    ON DELETE CASCADE;
GO

-- CertificateImages.CandidateCertificateID -> CandidateCertificates
ALTER TABLE dbo.CertificateImages
    ADD CONSTRAINT FK_CertificateImages_CandidateCertificates
    FOREIGN KEY (CandidateCertificateID) REFERENCES dbo.CandidateCertificates(CandidateCertificateID)
    ON DELETE CASCADE;
GO

-- JobPosts.RecruiterID -> Recruiters
ALTER TABLE dbo.JobPosts
    ADD CONSTRAINT FK_JobPosts_Recruiters
    FOREIGN KEY (RecruiterID) REFERENCES dbo.Recruiters(RecruiterID)
    ON DELETE NO ACTION;
GO

-- JobPosts.CompanyID -> Companies
ALTER TABLE dbo.JobPosts
    ADD CONSTRAINT FK_JobPosts_Companies
    FOREIGN KEY (CompanyID) REFERENCES dbo.Companies(CompanyID)
    ON DELETE NO ACTION;
GO

-- JobPostDetails.JobPostID -> JobPosts
ALTER TABLE dbo.JobPostDetails
    ADD CONSTRAINT FK_JobPostDetails_JobPosts
    FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID)
    ON DELETE CASCADE;
GO

-- Applications.CandidateID -> Candidates
ALTER TABLE dbo.Applications
    ADD CONSTRAINT FK_Applications_Candidates
    FOREIGN KEY (CandidateID) REFERENCES dbo.Candidates(CandidateID)
    ON DELETE CASCADE;
GO

-- Applications.JobPostID -> JobPosts
ALTER TABLE dbo.Applications
    ADD CONSTRAINT FK_Applications_JobPosts
    FOREIGN KEY (JobPostID) REFERENCES dbo.JobPosts(JobPostID)
    ON DELETE CASCADE;
GO

-- Transactions.AccountID -> Accounts
ALTER TABLE dbo.Transactions
    ADD CONSTRAINT FK_Transactions_Accounts
    FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID)
    ON DELETE CASCADE;
GO

/* =========================
   INDEXES
   ========================= */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JobPosts_PostedAt' AND object_id = OBJECT_ID('dbo.JobPosts'))
    CREATE INDEX IX_JobPosts_PostedAt ON dbo.JobPosts(PostedAt);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Companies_PhotoID' AND object_id = OBJECT_ID('dbo.Companies'))
    CREATE INDEX IX_Companies_PhotoID ON dbo.Companies(PhotoID);
GO

-- SavedJobs
CREATE TABLE dbo.SavedJobs (
    SavedJobID  INT IDENTITY(1,1) PRIMARY KEY,
    CandidateID INT NOT NULL,
    JobPostID   INT NOT NULL,
    SavedAt     DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    Note        NVARCHAR(500) NULL,
    CONSTRAINT UQ_SavedJobs_Candidate_Job UNIQUE (CandidateID, JobPostID),
    CONSTRAINT FK_SavedJobs_Candidates FOREIGN KEY (CandidateID)
        REFERENCES dbo.Candidates(CandidateID)
        ON DELETE CASCADE,
    CONSTRAINT FK_SavedJobs_JobPosts FOREIGN KEY (JobPostID)
        REFERENCES dbo.JobPosts(JobPostID)
        ON DELETE CASCADE
);
GO

CREATE INDEX IX_SavedJobs_Candidate_SavedAt
    ON dbo.SavedJobs(CandidateID, SavedAt DESC);
GO

CREATE INDEX IX_SavedJobs_JobPost
    ON dbo.SavedJobs(JobPostID);
GO

-- TRIGGER: Delete SavedJobs when JobPosts is closed
CREATE TRIGGER dbo.trg_JobPosts_DeleteSavedWhenClosed
ON dbo.JobPosts
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Chỉ xử lý khi Status thực sự thay đổi
    IF UPDATE(Status)
    BEGIN
        DELETE sj
        FROM dbo.SavedJobs sj
        INNER JOIN inserted i ON sj.JobPostID = i.JobPostID
        INNER JOIN deleted d ON d.JobPostID = i.JobPostID
        WHERE (d.Status IS NULL OR d.Status NOT IN (N'Closed', N'Expired', N'Archived'))
          AND i.Status IN (N'Closed', N'Expired', N'Archived');
    END
END;
GO


PRINT 'JOBPORTAL_EN schema created with Companies.PhotoID (FK -> ProfilePhotos). Done.';

CREATE DATABASE JOBPORTAL_EN;
GO
USE JOBPORTAL_EN;
GO

/* =================== MASTER TABLES =================== */
CREATE TABLE Accounts (
    AccountId    INT IDENTITY(1,1) PRIMARY KEY,
    Username     VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email        VARCHAR(150) NOT NULL UNIQUE,
    Phone        VARCHAR(20),
    Role         NVARCHAR(30) NOT NULL CHECK (Role IN (N'Admin', N'Company', N'Recruiter', N'Candidate')),
    CreatedAt    DATETIME DEFAULT GETDATE(),
    ActiveFlag   TINYINT  DEFAULT 1,
    PhotoId      INT NULL
);
GO

CREATE TABLE Photos (
    PhotoId     INT IDENTITY(1,1) PRIMARY KEY,
    FileName    VARCHAR(255) NOT NULL,
    FilePath    VARCHAR(500) NOT NULL,
    SizeKB      INT,
    MimeType    VARCHAR(50),
    UploadedAt  DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Admins (
    AdminId     INT IDENTITY(1,1) PRIMARY KEY,
    AccountId   INT NOT NULL UNIQUE,
    FullName    NVARCHAR(100) NOT NULL,
    ContactEmail VARCHAR(150),
    Permission  NVARCHAR(100) DEFAULT N'Full',
    CreatedAt   DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Companies (
    CompanyId      INT IDENTITY(1,1) PRIMARY KEY,
    AccountId      INT NOT NULL UNIQUE,
    CompanyName    NVARCHAR(255) NOT NULL,
    TaxCode        VARCHAR(20),
    Industry       NVARCHAR(150),
    Address        NVARCHAR(255),
    Phone          VARCHAR(20),
    CompanyEmail   VARCHAR(150),
    Website        VARCHAR(200),
    Description    NVARCHAR(1000),
    CreatedAt      DATETIME DEFAULT GETDATE(),
    ActiveFlag     TINYINT DEFAULT 1
);
GO

CREATE TABLE Recruiters (
    RecruiterId   INT IDENTITY(1,1) PRIMARY KEY,
    AccountId     INT NOT NULL UNIQUE,
    CompanyId     INT NULL,
    FullName      NVARCHAR(100) NOT NULL,
    PositionTitle NVARCHAR(100),
    WorkEmail     VARCHAR(150),
    Phone         VARCHAR(20),
    CreatedAt     DATETIME DEFAULT GETDATE(),
    ActiveFlag    TINYINT DEFAULT 1
);
GO

CREATE TABLE Candidates (
    CandidateId  INT IDENTITY(1,1) PRIMARY KEY,
    AccountId    INT NOT NULL UNIQUE,
    FullName     NVARCHAR(100) NOT NULL,
    BirthDate    DATE,
    Gender       NVARCHAR(5) NOT NULL CHECK (Gender IN (N'Nam', N'Nữ')), -- giữ nguyên nhãn VN nếu bạn muốn
    Phone        VARCHAR(15),
    Email        VARCHAR(100),
    Address      NVARCHAR(255),
    About        NVARCHAR(500),
    PhotoId      INT NULL,
    CreatedAt    DATETIME DEFAULT GETDATE(),
    ActiveFlag   TINYINT DEFAULT 1
);
GO

/* =================== CANDIDATE DOMAIN =================== */
CREATE TABLE WorkExperiences (
    ExperienceId    INT IDENTITY(1,1) PRIMARY KEY,
    CandidateId     INT NOT NULL,
    CompanyName     NVARCHAR(255) NOT NULL,
    JobTitle        NVARCHAR(150) NOT NULL,
    Industry        NVARCHAR(150),
    Major           NVARCHAR(150),
    JobDescription  NVARCHAR(MAX),
    StartDate       DATE NOT NULL,
    EndDate         DATE NULL,
    SalaryAmount    DECIMAL(18,2) NULL,
    SalaryCurrency  NVARCHAR(20) DEFAULT N'VNĐ',
    Achievements    NVARCHAR(500),
    CreatedAt       DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Certificates (
    CertificateId   INT IDENTITY(1,1) PRIMARY KEY,
    CertificateName NVARCHAR(100) NOT NULL,
    Issuer          NVARCHAR(100),
    Industry        NVARCHAR(100) NOT NULL,
    Major           NVARCHAR(150),
    Summary         NVARCHAR(255)
);
GO

CREATE TABLE CandidateCertificates (
    CandidateCertificateId INT IDENTITY(1,1) PRIMARY KEY,
    CandidateId            INT NOT NULL,
    CertificateId          INT NOT NULL,
    IssuedDate             DATE,
    ExpiredDate            DATE,
    FilePath               VARCHAR(500),
    ScoreText              NVARCHAR(50),
    Note                   NVARCHAR(255),
    UploadedAt             DATETIME DEFAULT GETDATE()
);
GO

/* =================== JOB POSTS =================== */
CREATE TABLE JobPosts (
    JobId         INT IDENTITY(1,1) PRIMARY KEY,
    JobCode       NVARCHAR(20) UNIQUE,
    RecruiterId   INT NOT NULL,
    CompanyId     INT NULL,
    Title         NVARCHAR(200) NOT NULL,
    Description   NVARCHAR(MAX),
    Requirements  NVARCHAR(MAX),
    SalaryMin     DECIMAL(18,2),
    SalaryMax     DECIMAL(18,2),
    SalaryUnit    NVARCHAR(50) DEFAULT N'VNĐ',
    LocationText  NVARCHAR(255),
    Employment    NVARCHAR(100),   -- Full-time / Part-time / Remote
    Deadline      DATE,
    Status        NVARCHAR(50) DEFAULT N'Visible',
    PostedAt      DATETIME DEFAULT GETDATE(),
    UpdatedAt     DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_JobPosts_Salary CHECK (
        (SalaryMin IS NULL AND SalaryMax IS NULL) OR
        (SalaryMin IS NOT NULL AND SalaryMax IS NOT NULL AND SalaryMin <= SalaryMax)
    )
);
GO

CREATE TABLE JobPostDetails (
    DetailId        INT IDENTITY(1,1) PRIMARY KEY,
    JobId           INT NOT NULL,
    Industry        NVARCHAR(150) NOT NULL,
    Major           NVARCHAR(150),
    YearsExperience INT  DEFAULT 0,
    EducationLevel  NVARCHAR(100),
    Skills          NVARCHAR(MAX),
    Headcount       INT DEFAULT 1,
    Gender          NVARCHAR(10) CHECK (Gender IN (N'Nam', N'Nữ', N'Không yêu cầu')),
    AgeFrom         INT NULL,
    AgeTo           INT NULL,
    Status          NVARCHAR(50) DEFAULT N'Visible',
    CONSTRAINT CHK_JobPostDetails_Age CHECK (
        (AgeFrom IS NULL AND AgeTo IS NULL) OR
        (AgeFrom IS NOT NULL AND AgeTo IS NOT NULL AND AgeFrom <= AgeTo)
    )
);
GO

/* =================== APPLICATIONS & TRANSACTIONS =================== */
CREATE TABLE Applications (
    ApplicationId  INT IDENTITY(1,1) PRIMARY KEY,
    CandidateId    INT NOT NULL,
    JobId          INT NOT NULL,
    AppliedAt      DATETIME DEFAULT GETDATE(),
    AppStatus      NVARCHAR(50) DEFAULT N'Under review',
    CvPath         NVARCHAR(500),
    CertificatePath NVARCHAR(500),
    Note           NVARCHAR(500),
    UpdatedAt      DATETIME DEFAULT GETDATE(),
    CONSTRAINT UQ_Applications UNIQUE (CandidateId, JobId)
);
GO

CREATE TABLE Transactions (
    TransactionId  INT IDENTITY(1,1) PRIMARY KEY,
    TransactionNo  NVARCHAR(20) UNIQUE,
    AccountId      INT NOT NULL,
    Type           NVARCHAR(100) NOT NULL, -- Topup/Payment/Package/Refund...
    Amount         DECIMAL(18,2) NOT NULL,
    Method         NVARCHAR(50)  NOT NULL, -- Bank, Momo, Card...
    Status         NVARCHAR(50) DEFAULT N'Processing',
    TransactedAt   DATETIME DEFAULT GETDATE(),
    Description    NVARCHAR(500),
    UpdatedAt      DATETIME DEFAULT GETDATE()
);
GO

/* =================== FOREIGN KEYS =================== */
-- Accounts ↔ Photos
ALTER TABLE dbo.Accounts
  ADD CONSTRAINT FK_Accounts_Photos
  FOREIGN KEY (PhotoId) REFERENCES dbo.Photos(PhotoId);

-- Admins ↔ Accounts
ALTER TABLE dbo.Admins
  ADD CONSTRAINT FK_Admins_Accounts
  FOREIGN KEY (AccountId) REFERENCES dbo.Accounts(AccountId);

-- Companies ↔ Accounts
ALTER TABLE dbo.Companies
  ADD CONSTRAINT FK_Companies_Accounts
  FOREIGN KEY (AccountId) REFERENCES dbo.Accounts(AccountId);

-- Recruiters ↔ (Accounts, Companies)
ALTER TABLE dbo.Recruiters
  ADD CONSTRAINT FK_Recruiters_Accounts
  FOREIGN KEY (AccountId) REFERENCES dbo.Accounts(AccountId);
ALTER TABLE dbo.Recruiters
  ADD CONSTRAINT FK_Recruiters_Companies
  FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(CompanyId);

-- Candidates ↔ (Accounts, Photos)
ALTER TABLE dbo.Candidates
  ADD CONSTRAINT FK_Candidates_Accounts
  FOREIGN KEY (AccountId) REFERENCES dbo.Accounts(AccountId);
ALTER TABLE dbo.Candidates
  ADD CONSTRAINT FK_Candidates_Photos
  FOREIGN KEY (PhotoId) REFERENCES dbo.Photos(PhotoId);

-- WorkExperiences ↔ Candidates
ALTER TABLE dbo.WorkExperiences
  ADD CONSTRAINT FK_WorkExperiences_Candidates
  FOREIGN KEY (CandidateId) REFERENCES dbo.Candidates(CandidateId)
  ON DELETE CASCADE;

-- CandidateCertificates ↔ (Candidates, Certificates)
ALTER TABLE dbo.CandidateCertificates
  ADD CONSTRAINT FK_CandidateCertificates_Candidates
  FOREIGN KEY (CandidateId) REFERENCES dbo.Candidates(CandidateId)
  ON DELETE CASCADE;
ALTER TABLE dbo.CandidateCertificates
  ADD CONSTRAINT FK_CandidateCertificates_Certificates
  FOREIGN KEY (CertificateId) REFERENCES dbo.Certificates(CertificateId);

-- JobPosts ↔ (Recruiters, Companies)
ALTER TABLE dbo.JobPosts
  ADD CONSTRAINT FK_JobPosts_Recruiters
  FOREIGN KEY (RecruiterId) REFERENCES dbo.Recruiters(RecruiterId);
ALTER TABLE dbo.JobPosts
  ADD CONSTRAINT FK_JobPosts_Companies
  FOREIGN KEY (CompanyId)  REFERENCES dbo.Companies(CompanyId);

-- JobPostDetails ↔ JobPosts
ALTER TABLE dbo.JobPostDetails
  ADD CONSTRAINT FK_JobPostDetails_JobPosts
  FOREIGN KEY (JobId) REFERENCES dbo.JobPosts(JobId)
  ON DELETE CASCADE;

-- Applications ↔ (Candidates, JobPosts)
ALTER TABLE dbo.Applications
  ADD CONSTRAINT FK_Applications_Candidates
  FOREIGN KEY (CandidateId) REFERENCES dbo.Candidates(CandidateId)
  ON DELETE CASCADE;
ALTER TABLE dbo.Applications
  ADD CONSTRAINT FK_Applications_JobPosts
  FOREIGN KEY (JobId) REFERENCES dbo.JobPosts(JobId)
  ON DELETE CASCADE;

-- Transactions ↔ Accounts
ALTER TABLE dbo.Transactions
  ADD CONSTRAINT FK_Transactions_Accounts
  FOREIGN KEY (AccountId) REFERENCES dbo.Accounts(AccountId);
GO

/* =================== INDEXES =================== */
-- Login
CREATE UNIQUE INDEX UQ_Accounts_Email ON dbo.Accounts(LOWER(Email));
CREATE INDEX IX_Accounts_Role ON dbo.Accounts(Role);

-- Recruiters/Companies
CREATE INDEX IX_Recruiters_CompanyId ON dbo.Recruiters(CompanyId);
CREATE INDEX IX_Companies_Name ON dbo.Companies(CompanyName);

-- Job listing
CREATE INDEX IX_JobPosts_Status_Date ON dbo.JobPosts(Status, PostedAt DESC) INCLUDE (Title, CompanyId, RecruiterId);
CREATE INDEX IX_JobPostDetails_JobId  ON dbo.JobPostDetails(JobId);
CREATE INDEX IX_JobPostDetails_IndustryMajor ON dbo.JobPostDetails(Industry, Major);

-- Applications
CREATE INDEX IX_Applications_Candidate ON dbo.Applications(CandidateId, AppliedAt DESC);
CREATE INDEX IX_Applications_Job       ON dbo.Applications(JobId);

-- Transactions
CREATE INDEX IX_Transactions_Account ON dbo.Transactions(AccountId, TransactedAt DESC);
GO

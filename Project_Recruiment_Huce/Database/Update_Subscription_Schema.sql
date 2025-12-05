-- =====================================================================
-- Migration Script: Subscription Model & SePay Integration
-- Created: 2025-12-01
-- Description: 
--   1. Drops legacy payment tables (BankCards, PaymentHistory, PendingPayments, Transactions).
--   2. Updates Recruiters table with subscription fields.
--   3. Creates SePayTransactions table for payment logging.
-- =====================================================================

USE JOBPORTAL_EN;
GO

PRINT 'Starting migration...';

-- 1. Drop legacy tables
-- =====================================================================
PRINT '-- Dropping legacy tables...';

IF OBJECT_ID(N'dbo.BankCards', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.BankCards;
    PRINT '[✓] Dropped table dbo.BankCards';
END

IF OBJECT_ID(N'dbo.PaymentHistory', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.PaymentHistory;
    PRINT '[✓] Dropped table dbo.PaymentHistory';
END

IF OBJECT_ID(N'dbo.PendingPayments', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.PendingPayments;
    PRINT '[✓] Dropped table dbo.PendingPayments';
END

IF OBJECT_ID(N'dbo.Transactions', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Transactions;
    PRINT '[✓] Dropped table dbo.Transactions';
END

-- 2. Update Recruiters table
-- =====================================================================
PRINT '-- Updating Recruiters table...';

IF COL_LENGTH('dbo.Recruiters', 'SubscriptionType') IS NULL
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD SubscriptionType NVARCHAR(20) NOT NULL DEFAULT N'Free';
    PRINT '[✓] Added SubscriptionType column';
END

IF COL_LENGTH('dbo.Recruiters', 'SubscriptionExpiryDate') IS NULL
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD SubscriptionExpiryDate DATETIME NULL;
    PRINT '[✓] Added SubscriptionExpiryDate column';
END

IF COL_LENGTH('dbo.Recruiters', 'FreeJobPostCount') IS NULL
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD FreeJobPostCount INT NOT NULL DEFAULT 0;
    PRINT '[✓] Added FreeJobPostCount column';
END

-- 3. Create SePayTransactions table
-- =====================================================================
PRINT '-- Creating SePayTransactions table...';

IF OBJECT_ID(N'dbo.SePayTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SePayTransactions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Gateway NVARCHAR(50) NULL,
        TransactionDate DATETIME NOT NULL,
        AccountNumber NVARCHAR(50) NULL,
        SubAccount NVARCHAR(50) NULL,
        AmountIn DECIMAL(18, 2) NOT NULL DEFAULT 0,
        AmountOut DECIMAL(18, 2) NOT NULL DEFAULT 0,
        Accumulated DECIMAL(18, 2) NOT NULL DEFAULT 0,
        Code NVARCHAR(50) NULL,
        TransactionContent NVARCHAR(MAX) NULL,
        ReferenceCode NVARCHAR(50) NULL,
        Description NVARCHAR(MAX) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT '[✓] Created table dbo.SePayTransactions';
END

PRINT 'Migration completed successfully.';
GO

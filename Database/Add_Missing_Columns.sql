-- =============================================
-- Script: Thêm các trường còn thiếu cho Google Login
-- Date: 2025-12-13
-- Description: Thêm FullName, GoogleId, IsGoogleAccount vào Accounts
--              Thêm Avatar vào Candidates và Recruiters cho Google login
-- LƯU Ý: Avatar trong Candidates/Recruiters dùng để lưu URL từ Google
--        PhotoID vẫn dùng cho avatar upload thủ công
-- =============================================

USE JOBPORTAL_EN;
GO

PRINT '========================================';
PRINT 'STARTING DATABASE SCHEMA UPDATE';
PRINT '========================================';
GO

-- =============================================
-- 1. BẢNG ACCOUNTS - THÊM CÁC TRƯỜNG CẦN THIẾT
-- =============================================
PRINT '';
PRINT '1. Updating Accounts table...';

-- Thêm FullName
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Accounts' AND COLUMN_NAME = 'FullName'
)
BEGIN
    ALTER TABLE dbo.Accounts
    ADD FullName NVARCHAR(255) NOT NULL DEFAULT '';
    PRINT '   ✓ Added FullName column';
END
ELSE
    PRINT '   - FullName column already exists';

-- Thêm GoogleId
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Accounts' AND COLUMN_NAME = 'GoogleId'
)
BEGIN
    ALTER TABLE dbo.Accounts
    ADD GoogleId NVARCHAR(255) NULL;
    PRINT '   ✓ Added GoogleId column';
END
ELSE
    PRINT '   - GoogleId column already exists';

-- Thêm IsGoogleAccount
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Accounts' AND COLUMN_NAME = 'IsGoogleAccount'
)
BEGIN
    ALTER TABLE dbo.Accounts
    ADD IsGoogleAccount BIT NOT NULL DEFAULT 0;
    PRINT '   ✓ Added IsGoogleAccount column';
END
ELSE
    PRINT '   - IsGoogleAccount column already exists';
GO

-- =============================================
-- 2. BẢNG CANDIDATES - THÊM AVATAR CHO GOOGLE
-- =============================================
PRINT '';
PRINT '2. Updating Candidates table...';

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Candidates' AND COLUMN_NAME = 'Avatar'
)
BEGIN
    ALTER TABLE dbo.Candidates
    ADD Avatar NVARCHAR(500) NULL;
    PRINT '   ✓ Added Avatar column (for Google avatar URL)';
END
ELSE
    PRINT '   - Avatar column already exists';
GO

-- =============================================
-- 3. BẢNG RECRUITERS - THÊM AVATAR CHO GOOGLE
-- =============================================
PRINT '';
PRINT '3. Updating Recruiters table...';

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Recruiters' AND COLUMN_NAME = 'Avatar'
)
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD Avatar NVARCHAR(500) NULL;
    PRINT '   ✓ Added Avatar column (for Google avatar URL)';
END
ELSE
    PRINT '   - Avatar column already exists';
GO

-- =============================================
-- 4. TẠO INDEXES ĐỂ TỐI ƯU HIỆU SUẤT
-- =============================================
PRINT '';
PRINT '4. Creating indexes...';

-- Index cho GoogleId trong Accounts
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Accounts_GoogleId' 
    AND object_id = OBJECT_ID('dbo.Accounts')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Accounts_GoogleId
    ON dbo.Accounts(GoogleId)
    WHERE GoogleId IS NOT NULL;
    PRINT '   ✓ Created index IX_Accounts_GoogleId';
END
ELSE
    PRINT '   - Index IX_Accounts_GoogleId already exists';

-- Index cho Email trong Accounts (nếu chưa có)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Accounts_Email' 
    AND object_id = OBJECT_ID('dbo.Accounts')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Accounts_Email
    ON dbo.Accounts(Email);
    PRINT '   ✓ Created index IX_Accounts_Email';
END
ELSE
    PRINT '   - Index IX_Accounts_Email already exists';
GO

-- =============================================
-- 5. CẬP NHẬT DỮ LIỆU HIỆN CÓ
-- =============================================
PRINT '';
PRINT '5. Updating existing data...';

-- Cập nhật FullName từ Username nếu FullName rỗng
UPDATE dbo.Accounts
SET FullName = Username
WHERE FullName = '' OR FullName IS NULL;
PRINT '   ✓ Updated FullName from Username';

-- Đánh dấu các tài khoản không có password là Google account
UPDATE dbo.Accounts
SET IsGoogleAccount = 1
WHERE (PasswordHash IS NULL OR PasswordHash = '')
  AND Email IS NOT NULL
  AND IsGoogleAccount = 0;
PRINT '   ✓ Marked accounts without password as Google accounts';
GO

-- =============================================
-- 6. HIỂN THỊ KẾT QUẢ
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'DATABASE SCHEMA UPDATE COMPLETED';
PRINT '========================================';
PRINT '';

-- Hiển thị cấu trúc bảng Accounts
PRINT 'Accounts Table Structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Accounts'
ORDER BY ORDINAL_POSITION;

-- Thống kê
PRINT '';
PRINT 'Statistics:';
SELECT 
    'Total Accounts' AS Metric,
    COUNT(*) AS Count
FROM dbo.Accounts
UNION ALL
SELECT 
    'Google Accounts',
    COUNT(*)
FROM dbo.Accounts
WHERE IsGoogleAccount = 1
UNION ALL
SELECT 
    'Regular Accounts',
    COUNT(*)
FROM dbo.Accounts
WHERE IsGoogleAccount = 0;

PRINT '';
PRINT '========================================';
PRINT 'SUMMARY:';
PRINT '✓ Added FullName, GoogleId, IsGoogleAccount to Accounts';
PRINT '✓ Added Avatar to Candidates and Recruiters';
PRINT '✓ Created indexes for better performance';
PRINT '✓ Updated existing data';
PRINT '';
PRINT 'HOW AVATAR WORKS:';
PRINT '- Google Login: Avatar URL stored in Candidates/Recruiters.Avatar';
PRINT '- Manual Upload: PhotoID -> ProfilePhotos table';
PRINT '- Display Priority: PhotoID first, then Avatar URL';
PRINT '========================================';
GO

-- =============================================
-- Script: Thêm các trường cần thiết cho Google Login
-- Date: 2025-12-13
-- Description: Thêm GoogleId và IsGoogleAccount vào bảng Accounts
-- =============================================

USE JOBPORTAL_EN;
GO

-- Kiểm tra và thêm cột GoogleId nếu chưa tồn tại
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Accounts' 
    AND COLUMN_NAME = 'GoogleId'
)
BEGIN
    ALTER TABLE dbo.Accounts
    ADD GoogleId NVARCHAR(255) NULL;
    
    PRINT 'Added column GoogleId to Accounts table';
END
ELSE
BEGIN
    PRINT 'Column GoogleId already exists in Accounts table';
END
GO

-- Kiểm tra và thêm cột IsGoogleAccount nếu chưa tồn tại
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Accounts' 
    AND COLUMN_NAME = 'IsGoogleAccount'
)
BEGIN
    ALTER TABLE dbo.Accounts
    ADD IsGoogleAccount BIT NOT NULL DEFAULT 0;
    
    PRINT 'Added column IsGoogleAccount to Accounts table';
END
ELSE
BEGIN
    PRINT 'Column IsGoogleAccount already exists in Accounts table';
END
GO

-- Tạo index cho GoogleId để tăng tốc độ tìm kiếm
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Accounts_GoogleId' 
    AND object_id = OBJECT_ID('dbo.Accounts')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Accounts_GoogleId
    ON dbo.Accounts(GoogleId)
    WHERE GoogleId IS NOT NULL;
    
    PRINT 'Created index IX_Accounts_GoogleId';
END
ELSE
BEGIN
    PRINT 'Index IX_Accounts_GoogleId already exists';
END
GO

-- Cập nhật các tài khoản hiện tại không có password thành Google account
UPDATE dbo.Accounts
SET IsGoogleAccount = 1
WHERE (PasswordHash IS NULL OR PasswordHash = '')
  AND Email IS NOT NULL
  AND IsGoogleAccount = 0;

PRINT 'Updated existing Google accounts';
GO

-- Hiển thị thông tin các cột của bảng Accounts
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Accounts'
ORDER BY ORDINAL_POSITION;
GO

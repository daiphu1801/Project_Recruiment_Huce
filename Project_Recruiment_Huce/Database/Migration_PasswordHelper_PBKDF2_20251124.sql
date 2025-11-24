-- ============================================================================
-- Migration: Password Hashing Upgrade to PBKDF2 (Microsoft.Identity)
-- Date: 2024-11-24
-- Description: 
--   - Ensures Salt column is nullable for new PBKDF2 hashes
--   - Provides monitoring queries to track migration progress
--   - Safe removal of Salt column after full migration (optional)
-- ============================================================================

USE JOBPORTAL_EN;
GO

PRINT '========================================';
PRINT 'Password Migration to PBKDF2';
PRINT '========================================';
PRINT '';

-- ============================================================================
-- STEP 1: Verify Salt column is nullable (should already be NULL)
-- ============================================================================
PRINT '-- STEP 1: Checking Salt column...';

IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Accounts' 
    AND COLUMN_NAME = 'Salt' 
    AND IS_NULLABLE = 'YES'
)
BEGIN
    PRINT '✓ Salt column is already nullable. No action needed.';
END
ELSE
BEGIN
    PRINT '! Salt column is NOT nullable. Altering now...';
    ALTER TABLE dbo.Accounts ALTER COLUMN Salt NVARCHAR(255) NULL;
    PRINT '✓ Salt column is now nullable.';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Migration Status Queries';
PRINT '========================================';
PRINT '';

-- ============================================================================
-- STEP 2: Check migration progress
-- ============================================================================
PRINT '-- STEP 2: Checking migration progress...';
PRINT '';

-- Count accounts by hash type
SELECT 
    CASE 
        WHEN Salt IS NULL AND PasswordHash IS NOT NULL THEN 'New Format (PBKDF2)'
        WHEN Salt IS NOT NULL AND PasswordHash IS NOT NULL THEN 'Legacy Format (SHA256+Salt)'
        ELSE 'Invalid/Empty'
    END AS HashType,
    COUNT(*) AS AccountCount,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM dbo.Accounts) AS DECIMAL(5,2)) AS Percentage
FROM dbo.Accounts
GROUP BY 
    CASE 
        WHEN Salt IS NULL AND PasswordHash IS NOT NULL THEN 'New Format (PBKDF2)'
        WHEN Salt IS NOT NULL AND PasswordHash IS NOT NULL THEN 'Legacy Format (SHA256+Salt)'
        ELSE 'Invalid/Empty'
    END
ORDER BY AccountCount DESC;

PRINT '';
PRINT '-- Accounts still using legacy format:';

SELECT 
    AccountID,
    Username,
    Email,
    Role,
    CreatedAt,
    DATEDIFF(DAY, CreatedAt, GETDATE()) AS DaysSinceCreation
FROM dbo.Accounts
WHERE Salt IS NOT NULL
ORDER BY CreatedAt DESC;

PRINT '';
PRINT '========================================';
PRINT 'Migration Complete - Next Steps';
PRINT '========================================';
PRINT '';
PRINT '1. Monitor legacy account count over time (30-90 days recommended)';
PRINT '2. Legacy accounts will auto-upgrade on next login';
PRINT '3. Consider sending password reset emails for inactive accounts';
PRINT '4. After sufficient time, optionally drop Salt column (see STEP 3 below)';
PRINT '';

-- ============================================================================
-- STEP 3: [OPTIONAL] Drop Salt column after full migration
-- ============================================================================
-- ⚠️ WARNING: Only execute this after ALL accounts have migrated!
-- Uncomment and run manually when ready (not part of automatic migration)
/*
PRINT '';
PRINT '========================================';
PRINT '[OPTIONAL] Dropping Salt Column';
PRINT '========================================';
PRINT '';

-- Check if any accounts still have Salt
DECLARE @LegacyCount INT;
SELECT @LegacyCount = COUNT(*) FROM dbo.Accounts WHERE Salt IS NOT NULL;

IF @LegacyCount > 0
BEGIN
    PRINT '❌ CANNOT DROP Salt column!';
    PRINT CAST(@LegacyCount AS NVARCHAR(10)) + ' accounts still have Salt values.';
    PRINT 'Wait for more users to login or send password reset emails.';
END
ELSE
BEGIN
    PRINT '✓ All accounts migrated. Safe to drop Salt column.';
    
    -- Backup recommendation
    PRINT '';
    PRINT '⚠️  BACKUP DATABASE FIRST!';
    PRINT 'Recommended command:';
    PRINT 'BACKUP DATABASE JOBPORTAL_EN TO DISK = ''C:\Backups\JOBPORTAL_EN_BeforeSaltDrop.bak'';';
    PRINT '';
    
    -- Drop column
    ALTER TABLE dbo.Accounts DROP COLUMN Salt;
    PRINT '✓ Salt column dropped successfully.';
END
GO
*/

PRINT '';
PRINT '========================================';
PRINT 'Migration Script Complete';
PRINT '========================================';
PRINT '';
PRINT 'Review the output above to check migration progress.';
PRINT 'Keep this script for future reference and monitoring.';
GO

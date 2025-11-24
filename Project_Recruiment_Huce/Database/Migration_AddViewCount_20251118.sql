/*
=====================================================================
Migration: Add ViewCount to JobPosts and Analytics Support
Created: 2025-11-18
Target: JOBPORTAL_EN database (SQL Server)
=====================================================================
Description:
  - Adds ViewCount column to dbo.JobPosts (INT NOT NULL DEFAULT 0)
  - Creates index on RecruiterID for analytics queries
  - Creates stored procedure sp_IncrementJobViewCount to safely increment view counts
  - Creates stored procedure sp_GetRecruiterAnalytics to fetch recruiter dashboard metrics
  - Uses SchemaMigrations table to track applied migrations (idempotent - safe to run multiple times)

Usage:
  1) Open this file in SQL Server Management Studio (SSMS)
  2) Connect to your JOBPORTAL_EN database
  3) Execute the entire script (F5)
  4) Verify output messages for success
  
  Or use sqlcmd:
  sqlcmd -S <server> -d JOBPORTAL_EN -E -i "Migration_AddViewCount_20251118.sql"
=====================================================================
*/

USE JOBPORTAL_EN;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- =====================================================
    -- Step 1: Create SchemaMigrations table if not exists
    -- =====================================================
    IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'SchemaMigrations' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        CREATE TABLE dbo.SchemaMigrations (
            MigrationId INT IDENTITY(1,1) PRIMARY KEY,
            ScriptName NVARCHAR(200) NOT NULL UNIQUE,
            AppliedAt DATETIME NOT NULL DEFAULT (GETDATE())
        );
        PRINT '[✓] Created dbo.SchemaMigrations table';
    END
    ELSE
    BEGIN
        PRINT '[i] dbo.SchemaMigrations table already exists';
    END

    -- =====================================================
    -- Step 2: Check if this migration has already been applied
    -- =====================================================
    DECLARE @scriptName NVARCHAR(200) = N'AddViewCount_20251118';

    IF EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = @scriptName)
    BEGIN
        PRINT '[i] Migration "' + @scriptName + '" already applied. Skipping execution.';
        ROLLBACK TRANSACTION;
        RETURN;
    END
    ELSE
    BEGIN
        PRINT '[→] Applying migration: ' + @scriptName;
    END

    -- =====================================================
    -- Step 3: Add ViewCount column to JobPosts
    -- =====================================================
    IF NOT EXISTS(
        SELECT 1 FROM sys.columns c
        WHERE c.name = N'ViewCount' 
        AND c.object_id = OBJECT_ID(N'dbo.JobPosts')
    )
    BEGIN
        ALTER TABLE dbo.JobPosts 
        ADD ViewCount INT NOT NULL CONSTRAINT DF_JobPosts_ViewCount DEFAULT(0);
        
        PRINT '[✓] Added column dbo.JobPosts.ViewCount (INT NOT NULL DEFAULT 0)';
    END
    ELSE
    BEGIN
        PRINT '[i] Column dbo.JobPosts.ViewCount already exists';
    END

    -- =====================================================
    -- Step 4: Create index on RecruiterID for analytics queries
    -- =====================================================
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        JOIN sys.objects o ON i.object_id = o.object_id
        WHERE o.name = N'JobPosts' 
        AND i.name = N'IX_JobPosts_RecruiterID'
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_JobPosts_RecruiterID 
        ON dbo.JobPosts (RecruiterID);
        
        PRINT '[✓] Created index IX_JobPosts_RecruiterID on dbo.JobPosts(RecruiterID)';
    END
    ELSE
    BEGIN
        PRINT '[i] Index IX_JobPosts_RecruiterID already exists';
    END

    -- =====================================================
    -- Step 5: Create/replace stored procedure to increment view count
    -- =====================================================
    IF OBJECT_ID('dbo.sp_IncrementJobViewCount','P') IS NOT NULL
    BEGIN
        DROP PROCEDURE dbo.sp_IncrementJobViewCount;
        PRINT '[i] Dropped existing procedure dbo.sp_IncrementJobViewCount';
    END

    EXEC('
    CREATE PROCEDURE dbo.sp_IncrementJobViewCount
        @JobPostID INT
    AS
    BEGIN
        SET NOCOUNT ON;
        
        -- Increment view count atomically
        UPDATE dbo.JobPosts
        SET ViewCount = ViewCount + 1
        WHERE JobPostID = @JobPostID;
        
        -- Return updated count
        SELECT ViewCount 
        FROM dbo.JobPosts 
        WHERE JobPostID = @JobPostID;
    END
    ');

    PRINT '[✓] Created procedure dbo.sp_IncrementJobViewCount';

    -- =====================================================
    -- Step 6: Create/replace stored procedure for recruiter analytics
    -- =====================================================
    IF OBJECT_ID('dbo.sp_GetRecruiterAnalytics','P') IS NOT NULL
    BEGIN
        DROP PROCEDURE dbo.sp_GetRecruiterAnalytics;
        PRINT '[i] Dropped existing procedure dbo.sp_GetRecruiterAnalytics';
    END

    EXEC('
    CREATE PROCEDURE dbo.sp_GetRecruiterAnalytics
        @RecruiterID INT,
        @FromDate DATETIME = NULL,
        @ToDate DATETIME = NULL
    AS
    BEGIN
        SET NOCOUNT ON;

        -- ================================================
        -- Result Set 1: Summary metrics for the recruiter
        -- ================================================
        SELECT
            ISNULL(SUM(j.ViewCount), 0) AS TotalViews,
            COUNT(DISTINCT a.ApplicationID) AS TotalApplications,
            COUNT(DISTINCT j.JobPostID) AS TotalJobs,
            CASE 
                WHEN ISNULL(SUM(j.ViewCount), 0) = 0 THEN CAST(0 AS DECIMAL(10,2))
                ELSE CAST(COUNT(DISTINCT a.ApplicationID) * 100.0 / SUM(j.ViewCount) AS DECIMAL(10,2))
            END AS ConversionRatePercent
        FROM dbo.JobPosts j
        LEFT JOIN dbo.Applications a ON a.JobPostID = j.JobPostID
            AND (@FromDate IS NULL OR a.AppliedAt >= @FromDate)
            AND (@ToDate IS NULL OR a.AppliedAt <= @ToDate)
        WHERE j.RecruiterID = @RecruiterID
          AND (@FromDate IS NULL OR j.PostedAt >= @FromDate)
          AND (@ToDate IS NULL OR j.PostedAt <= @ToDate);

        -- ================================================
        -- Result Set 2: Per-job breakdown with metrics
        -- ================================================
        SELECT
            j.JobPostID,
            j.Title AS JobTitle,
            j.Status AS JobStatus,
            j.PostedAt,
            j.ViewCount AS Views,
            COUNT(a.ApplicationID) AS Applications,
            CASE 
                WHEN j.ViewCount = 0 THEN CAST(0 AS DECIMAL(10,2))
                ELSE CAST(COUNT(a.ApplicationID) * 100.0 / j.ViewCount AS DECIMAL(10,2))
            END AS ConversionRatePercent
        FROM dbo.JobPosts j
        LEFT JOIN dbo.Applications a ON a.JobPostID = j.JobPostID
            AND (@FromDate IS NULL OR a.AppliedAt >= @FromDate)
            AND (@ToDate IS NULL OR a.AppliedAt <= @ToDate)
        WHERE j.RecruiterID = @RecruiterID
          AND (@FromDate IS NULL OR j.PostedAt >= @FromDate)
          AND (@ToDate IS NULL OR j.PostedAt <= @ToDate)
        GROUP BY j.JobPostID, j.Title, j.Status, j.PostedAt, j.ViewCount
        ORDER BY Applications DESC, Views DESC;
    END
    ');

    PRINT '[✓] Created procedure dbo.sp_GetRecruiterAnalytics';

    -- =====================================================
    -- Step 7: Record migration as applied
    -- =====================================================
    INSERT INTO dbo.SchemaMigrations (ScriptName, AppliedAt) 
    VALUES (@scriptName, GETDATE());

    COMMIT TRANSACTION;

    PRINT '';
    PRINT '=====================================================';
    PRINT '[✓] Migration "' + @scriptName + '" applied successfully!';
    PRINT '=====================================================';
    PRINT '';
    PRINT 'Usage Examples:';
    PRINT '  -- Increment view count:';
    PRINT '  EXEC dbo.sp_IncrementJobViewCount @JobPostID = 123;';
    PRINT '';
    PRINT '  -- Get recruiter analytics (all time):';
    PRINT '  EXEC dbo.sp_GetRecruiterAnalytics @RecruiterID = 45;';
    PRINT '';
    PRINT '  -- Get recruiter analytics (date range):';
    PRINT '  EXEC dbo.sp_GetRecruiterAnalytics @RecruiterID = 45, @FromDate = ''2025-01-01'', @ToDate = ''2025-12-31'';';
    PRINT '';

END TRY
BEGIN CATCH
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrNum INT = ERROR_NUMBER();
    DECLARE @ErrLine INT = ERROR_LINE();
    
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    PRINT '';
    PRINT '=====================================================';
    PRINT '[✗] Migration failed!';
    PRINT '=====================================================';
    PRINT 'Error ' + CAST(@ErrNum AS NVARCHAR(20)) + ' at line ' + CAST(@ErrLine AS NVARCHAR(20));
    PRINT @ErrMsg;
    PRINT '';
    
    THROW;
END CATCH
GO

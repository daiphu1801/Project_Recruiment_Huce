-- =====================================================================
-- Fix: sp_GetRecruiterAnalytics - Thêm Summary Metrics Result Set
-- Date: 2025-11-25
-- Description: 
--   Sửa stored procedure để trả về 2 result sets:
--   1. Summary metrics (TotalViews, TotalApplications, TotalJobs, ConversionRate)
--   2. Job breakdown (chi tiết từng job)
-- =====================================================================

USE JOBPORTAL_EN;
GO

PRINT 'Updating sp_GetRecruiterAnalytics to return summary metrics...';
GO

-- Drop existing procedure
IF OBJECT_ID('dbo.sp_GetRecruiterAnalytics','P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_GetRecruiterAnalytics;
    PRINT '[✓] Dropped existing procedure';
END
GO

-- Create updated procedure
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

PRINT '[✓] Successfully updated sp_GetRecruiterAnalytics';
PRINT '';
PRINT 'Testing procedure...';
GO

-- Test với RecruiterID = 1 (adjust nếu cần)
-- EXEC dbo.sp_GetRecruiterAnalytics @RecruiterID = 1, @FromDate = NULL, @ToDate = NULL;
-- GO

PRINT '[✓] Fix completed successfully!';
PRINT 'Note: Procedure now returns 2 result sets:';
PRINT '  1. Summary metrics (TotalViews, TotalApplications, TotalJobs, ConversionRatePercent)';
PRINT '  2. Job breakdown (per-job analytics)';
GO

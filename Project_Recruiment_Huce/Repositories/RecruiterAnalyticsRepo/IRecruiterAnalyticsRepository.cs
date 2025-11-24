using System;
using System.Collections.Generic;
using Project_Recruiment_Huce.Models.Recruiters;

namespace Project_Recruiment_Huce.Repositories.RecruiterAnalyticsRepo
{
    /// <summary>
    /// Interface định nghĩa các phương thức truy xuất dữ liệu cho analytics của recruiter
    /// Áp dụng Repository Pattern cho analytics operations
    /// </summary>
    public interface IRecruiterAnalyticsRepository
    {
        // Recruiter operations
        int? GetRecruiterIdByAccountId(int accountId);
        bool IsJobOwnedByRecruiter(int jobPostId, int recruiterId);
        
        // Analytics data retrieval (using stored procedures)
        RecruiterAnalyticsSummaryViewModel GetSummaryMetrics(int recruiterId, DateTime? fromDate, DateTime? toDate);
        List<JobAnalyticsItemViewModel> GetJobBreakdown(int recruiterId, DateTime? fromDate, DateTime? toDate);
        JobAnalyticsItemViewModel GetJobAnalytics(int jobPostId, int recruiterId);
        
        // View count operation
        void IncrementViewCount(int jobPostId);
    }
}

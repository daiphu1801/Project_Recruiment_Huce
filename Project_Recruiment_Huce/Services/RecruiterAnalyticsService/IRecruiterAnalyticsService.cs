using System;
using Project_Recruiment_Huce.Models.Recruiters;

namespace Project_Recruiment_Huce.Services.RecruiterAnalyticsService
{
    /// <summary>
    /// Interface định nghĩa business logic cho analytics của recruiter
    /// Xử lý pagination, date filtering và validation
    /// </summary>
    public interface IRecruiterAnalyticsService
    {
        // Dashboard operations
        RecruiterAnalyticsDashboardViewModel GetDashboardData(int recruiterId, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
        
        // Job analytics operations
        JobAnalyticsResult GetJobAnalytics(int jobPostId, int recruiterId);
    }

    /// <summary>
    /// Result object cho job analytics operation
    /// </summary>
    public class JobAnalyticsResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public JobAnalyticsItemViewModel Analytics { get; set; }
    }
}

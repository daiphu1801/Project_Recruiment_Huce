using System;
using System.Linq;
using Project_Recruiment_Huce.Models.Recruiters;
using Project_Recruiment_Huce.Repositories.RecruiterAnalyticsRepo;

namespace Project_Recruiment_Huce.Services.RecruiterAnalyticsService
{
    /// <summary>
    /// Service triển khai business logic cho analytics của recruiter
    /// Xử lý pagination, date filtering và validation
    /// </summary>
    public class RecruiterAnalyticsService : IRecruiterAnalyticsService
    {
        private readonly IRecruiterAnalyticsRepository _repository;

        public RecruiterAnalyticsService(IRecruiterAnalyticsRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public RecruiterAnalyticsDashboardViewModel GetDashboardData(int recruiterId, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
        {
            // Validate and adjust page number
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max 100 items per page

            // Get summary metrics
            var summary = _repository.GetSummaryMetrics(recruiterId, fromDate, toDate);
            
            // Get job breakdown (all items)
            var allBreakdown = _repository.GetJobBreakdown(recruiterId, fromDate, toDate);
            
            // Calculate pagination
            var totalItems = allBreakdown.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure page is within valid range
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            // Get paginated items
            var pagedBreakdown = allBreakdown
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new RecruiterAnalyticsDashboardViewModel
            {
                Summary = summary,
                JobBreakdown = pagedBreakdown,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public JobAnalyticsResult GetJobAnalytics(int jobPostId, int recruiterId)
        {
            var result = new JobAnalyticsResult();

            // Verify job ownership
            if (!_repository.IsJobOwnedByRecruiter(jobPostId, recruiterId))
            {
                result.ErrorMessage = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền xem.";
                return result;
            }

            // Get analytics
            var analytics = _repository.GetJobAnalytics(jobPostId, recruiterId);
            if (analytics == null)
            {
                result.ErrorMessage = "Không thể tải dữ liệu analytics.";
                return result;
            }

            result.Success = true;
            result.Analytics = analytics;
            return result;
        }
    }
}

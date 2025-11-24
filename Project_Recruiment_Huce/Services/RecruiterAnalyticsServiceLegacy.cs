using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Recruiters;
using Project_Recruiment_Huce.Infrastructure;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// LEGACY: Service layer cho analytics của recruiter
    /// Kept for backward compatibility with existing code (e.g., JobsController for IncrementViewCount)
    /// New code should use Project_Recruiment_Huce.Services.RecruiterAnalyticsService.RecruiterAnalyticsService
    /// </summary>
    public class LegacyRecruiterAnalyticsService
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public LegacyRecruiterAnalyticsService(JOBPORTAL_ENDataContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Lấy dữ liệu dashboard analytics đầy đủ cho recruiter với phân trang
        /// Bao gồm: summary metrics và breakdown theo từng tin tuyển dụng
        /// </summary>
        /// <param name="recruiterId">ID của recruiter</param>
        /// <param name="fromDate">Ngày bắt đầu lọc</param>
        /// <param name="toDate">Ngày kết thúc lọc</param>
        /// <param name="page">Trang hiện tại</param>
        /// <param name="pageSize">Số items mỗi trang</param>
        /// <returns>ViewModel chứa summary và breakdown có phân trang</returns>
        public RecruiterAnalyticsDashboardViewModel GetDashboardData(int recruiterId, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 10)
        {
            var summary = GetSummaryMetrics(recruiterId, fromDate, toDate);
            var allBreakdown = GetJobBreakdown(recruiterId, fromDate, toDate);
            
            // Calculate pagination
            var totalItems = allBreakdown.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure page is within valid range
            if (page < 1) page = 1;
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

        /// <summary>
        /// Lấy summary metrics cho recruiter sử dụng stored procedure
        /// Gồm: TotalViews, TotalApplications, TotalJobs, ConversionRate
        /// </summary>
        /// <param name="recruiterId">ID của recruiter</param>
        /// <param name="fromDate">Ngày bắt đầu lọc</param>
        /// <param name="toDate">Ngày kết thúc lọc</param>
        /// <returns>ViewModel chứa các chỉ số tổng hợp</returns>
        public RecruiterAnalyticsSummaryViewModel GetSummaryMetrics(int recruiterId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var connectionString = _db.Connection.ConnectionString;
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("dbo.sp_GetRecruiterAnalytics", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@RecruiterID", recruiterId);
                        cmd.Parameters.AddWithValue("@FromDate", fromDate.HasValue ? (object)fromDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToDate", toDate.HasValue ? (object)toDate.Value : DBNull.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            // Result set đầu tiên: summary
                            if (reader.Read())
                            {
                                return new RecruiterAnalyticsSummaryViewModel
                                {
                                    TotalViews = reader.GetInt32(reader.GetOrdinal("TotalViews")),
                                    TotalApplications = reader.GetInt32(reader.GetOrdinal("TotalApplications")),
                                    TotalJobs = reader.GetInt32(reader.GetOrdinal("TotalJobs")),
                                    ConversionRatePercent = reader.GetDecimal(reader.GetOrdinal("ConversionRatePercent")),
                                    FromDate = fromDate,
                                    ToDate = toDate
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting summary metrics: {ex.Message}");
            }

            // Fallback to default
            return new RecruiterAnalyticsSummaryViewModel
            {
                TotalViews = 0,
                TotalApplications = 0,
                TotalJobs = 0,
                ConversionRatePercent = 0,
                FromDate = fromDate,
                ToDate = toDate
            };
        }

        /// <summary>
        /// Lấy breakdown theo từng tin tuyển dụng sử dụng stored procedure
        /// Mỗi tin có: JobTitle, Status, PostedAt, Views, Applications, ConversionRate
        /// </summary>
        /// <param name="recruiterId">ID của recruiter</param>
        /// <param name="fromDate">Ngày bắt đầu lọc</param>
        /// <param name="toDate">Ngày kết thúc lọc</param>
        /// <returns>Danh sách analytics của từng tin tuyển dụng</returns>
        public List<JobAnalyticsItemViewModel> GetJobBreakdown(int recruiterId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var breakdown = new List<JobAnalyticsItemViewModel>();

            try
            {
                var connectionString = _db.Connection.ConnectionString;
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("dbo.sp_GetRecruiterAnalytics", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@RecruiterID", recruiterId);
                        cmd.Parameters.AddWithValue("@FromDate", fromDate.HasValue ? (object)fromDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToDate", toDate.HasValue ? (object)toDate.Value : DBNull.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            // Bỏ qua result set đầu tiên (summary)
                            reader.NextResult();

                            // Result set thứ hai: breakdown theo từng job
                            while (reader.Read())
                            {
                                breakdown.Add(new JobAnalyticsItemViewModel
                                {
                                    JobPostID = reader.GetInt32(reader.GetOrdinal("JobPostID")),
                                    JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
                                    JobStatus = reader.GetString(reader.GetOrdinal("JobStatus")),
                                    PostedAt = reader.GetDateTime(reader.GetOrdinal("PostedAt")),
                                    Views = reader.GetInt32(reader.GetOrdinal("Views")),
                                    Applications = reader.GetInt32(reader.GetOrdinal("Applications")),
                                    ConversionRatePercent = reader.GetDecimal(reader.GetOrdinal("ConversionRatePercent"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting job breakdown: {ex.Message}");
            }

            return breakdown;
        }

        /// <summary>
        /// Increment view count for a job post (called when candidate views job details)
        /// Uses direct SQL UPDATE for better performance and concurrency
        /// </summary>
        public void IncrementViewCount(int jobPostId)
        {
            try
            {
                // Use ExecuteCommand for direct SQL - more efficient than loading entity
                _db.ExecuteCommand("UPDATE dbo.JobPosts SET ViewCount = ViewCount + 1 WHERE JobPostID = {0}", jobPostId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error incrementing view count: {ex.Message}");
                // Don't throw - view counting is not critical to user experience
            }
        }

        /// <summary>
        /// Get analytics for a specific job
        /// </summary>
        public JobAnalyticsItemViewModel GetJobAnalytics(int jobPostId, int recruiterId)
        {
            try
            {
                var job = _db.JobPosts.FirstOrDefault(j => j.JobPostID == jobPostId && j.RecruiterID == recruiterId);
                if (job == null) return null;

                var applicationsCount = _db.Applications.Count(a => a.JobPostID == jobPostId);
                var conversionRate = job.ViewCount == 0 ? 0 : (decimal)applicationsCount * 100.0m / job.ViewCount;

                return new JobAnalyticsItemViewModel
                {
                    JobPostID = job.JobPostID,
                    JobTitle = job.Title,
                    JobStatus = job.Status,
                    PostedAt = job.PostedAt,
                    Views = job.ViewCount,
                    Applications = applicationsCount,
                    ConversionRatePercent = Math.Round(conversionRate, 2)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting job analytics: {ex.Message}");
                return null;
            }
        }
    }
}

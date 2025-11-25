using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Recruiters;

namespace Project_Recruiment_Huce.Repositories.RecruiterAnalyticsRepo
{
    /// <summary>
    /// Repository triển khai truy xuất dữ liệu cho analytics của recruiter
    /// Sử dụng stored procedures để tối ưu hiệu suất
    /// </summary>
    public class RecruiterAnalyticsRepository : IRecruiterAnalyticsRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public RecruiterAnalyticsRepository(bool readOnly = false)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            _db = new JOBPORTAL_ENDataContext(connectionString);

            if (readOnly)
            {
                _db.ObjectTrackingEnabled = false;
            }
        }

        public int? GetRecruiterIdByAccountId(int accountId)
        {
            var recruiter = _db.Recruiters.FirstOrDefault(r => r.AccountID == accountId);
            return recruiter?.RecruiterID;
        }

        public bool IsJobOwnedByRecruiter(int jobPostId, int recruiterId)
        {
            return _db.JobPosts.Any(j => j.JobPostID == jobPostId && j.RecruiterID == recruiterId);
        }

        public RecruiterAnalyticsSummaryViewModel GetSummaryMetrics(int recruiterId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
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
                                var summary = new RecruiterAnalyticsSummaryViewModel
                                {
                                    TotalViews = reader.GetInt32(reader.GetOrdinal("TotalViews")),
                                    TotalApplications = reader.GetInt32(reader.GetOrdinal("TotalApplications")),
                                    TotalJobs = reader.GetInt32(reader.GetOrdinal("TotalJobs")),
                                    ConversionRatePercent = Convert.ToDecimal(reader.GetDouble(reader.GetOrdinal("ConversionRatePercent"))),
                                    FromDate = fromDate,
                                    ToDate = toDate
                                };
                                
                                return summary;
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

        public List<JobAnalyticsItemViewModel> GetJobBreakdown(int recruiterId, DateTime? fromDate, DateTime? toDate)
        {
            var breakdown = new List<JobAnalyticsItemViewModel>();

            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
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
                                    ConversionRatePercent = Convert.ToDecimal(reader.GetDouble(reader.GetOrdinal("ConversionRatePercent")))
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
    }
}

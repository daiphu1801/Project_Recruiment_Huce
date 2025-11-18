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
    /// Service layer for recruiter analytics
    /// Encapsulates business logic for analytics calculations and keeps controllers thin
    /// </summary>
    public class RecruiterAnalyticsService
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public RecruiterAnalyticsService(JOBPORTAL_ENDataContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Get complete analytics dashboard data for a recruiter
        /// </summary>
        public RecruiterAnalyticsDashboardViewModel GetDashboardData(int recruiterId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var summary = GetSummaryMetrics(recruiterId, fromDate, toDate);
            var breakdown = GetJobBreakdown(recruiterId, fromDate, toDate);

            return new RecruiterAnalyticsDashboardViewModel
            {
                Summary = summary,
                JobBreakdown = breakdown
            };
        }

        /// <summary>
        /// Get summary metrics for a recruiter using stored procedure
        /// </summary>
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
                            // First result set: summary
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
        /// Get per-job breakdown for a recruiter using stored procedure
        /// </summary>
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
                            // Skip first result set (summary)
                            reader.NextResult();

                            // Second result set: per-job breakdown
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

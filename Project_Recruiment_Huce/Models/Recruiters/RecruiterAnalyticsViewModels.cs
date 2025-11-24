using System;
using System.Collections.Generic;

namespace Project_Recruiment_Huce.Models.Recruiters
{
    /// <summary>
    /// ViewModel for recruiter analytics dashboard - summary metrics
    /// </summary>
    public class RecruiterAnalyticsSummaryViewModel
    {
        public int TotalViews { get; set; }
        public int TotalApplications { get; set; }
        public int TotalJobs { get; set; }
        public decimal ConversionRatePercent { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    /// <summary>
    /// ViewModel for per-job analytics breakdown
    /// </summary>
    public class JobAnalyticsItemViewModel
    {
        public int JobPostID { get; set; }
        public string JobTitle { get; set; }
        public string JobStatus { get; set; }
        public DateTime PostedAt { get; set; }
        public int Views { get; set; }
        public int Applications { get; set; }
        public decimal ConversionRatePercent { get; set; }
    }

    /// <summary>
    /// Combined ViewModel for the analytics dashboard Index page
    /// </summary>
    public class RecruiterAnalyticsDashboardViewModel
    {
        public RecruiterAnalyticsSummaryViewModel Summary { get; set; }
        public List<JobAnalyticsItemViewModel> JobBreakdown { get; set; }
        
        // Pagination properties
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}

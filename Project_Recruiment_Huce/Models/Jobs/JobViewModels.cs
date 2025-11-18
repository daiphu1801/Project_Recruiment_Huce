using System;
using System.Collections.Generic;

namespace Project_Recruiment_Huce.Models.Jobs
{
    /// <summary>
    /// ViewModel cho mỗi job item trong danh sách
    /// </summary>
    public class JobListingItemViewModel
    {
        public int JobPostID { get; set; }
        public string JobCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string EmploymentType { get; set; }
        public string EmploymentTypeDisplay { get; set; }
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public string SalaryCurrency { get; set; }
        public string SalaryRange { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public string Status { get; set; }
        public string LogoUrl { get; set; }
        public int PendingApplicationsCount { get; set; }
        public int ViewCount { get; set; }
    }

    /// <summary>
    /// ViewModel cho chi tiết job
    /// </summary>
    public class JobDetailsViewModel
    {
        public int JobPostID { get; set; }
        public string JobCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string EmploymentType { get; set; }
        public string EmploymentTypeDisplay { get; set; }
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public string SalaryCurrency { get; set; }
        public string SalaryRange { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public string Status { get; set; }
        public string LogoUrl { get; set; }
        public string Industry { get; set; }
        public string Major { get; set; }
        public int? YearsExperience { get; set; }
        public string DegreeRequired { get; set; }
        public string Skills { get; set; }
        public int? Headcount { get; set; }
        public string GenderRequirement { get; set; }
        public int? AgeFrom { get; set; }
        public int? AgeTo { get; set; }
        public string DetailStatus { get; set; }
        public int ViewCount { get; set; }

        public bool HasJobDetail =>
            !string.IsNullOrWhiteSpace(Industry) ||
            !string.IsNullOrWhiteSpace(Major) ||
            YearsExperience.HasValue ||
            !string.IsNullOrWhiteSpace(DegreeRequired) ||
            !string.IsNullOrWhiteSpace(Skills) ||
            Headcount.HasValue ||
            !string.IsNullOrWhiteSpace(GenderRequirement) ||
            AgeFrom.HasValue ||
            AgeTo.HasValue;
    }

    /// <summary>
    /// ViewModel cho danh sách related jobs
    /// </summary>
    public class RelatedJobViewModel
    {
        public int JobPostID { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string EmploymentType { get; set; }
        public string EmploymentTypeDisplay { get; set; }
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public string SalaryCurrency { get; set; }
        public string SalaryRange { get; set; }
        public DateTime? PostedAt { get; set; }
        public string LogoUrl { get; set; }
    }
}


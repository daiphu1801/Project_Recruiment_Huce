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
        public string LogoUrl { get; set; }
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
        public string LogoUrl { get; set; }
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


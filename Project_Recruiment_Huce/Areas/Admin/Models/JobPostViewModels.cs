using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách tin tuyển dụng
    /// Maps to JobPost table in database
    /// </summary>
    public class JobPostListVm
    {
        public int JobPostID { get; set; }
        public string JobCode { get; set; }
        public int RecruiterID { get; set; }
        public int? CompanyID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public string SalaryCurrency { get; set; }
        public string Location { get; set; }
        public string EmploymentType { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public string Status { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Helper properties for display (from related tables)
        public string CompanyName { get; set; }
        public string RecruiterName { get; set; }
        
        // Legacy properties for backward compatibility (mapped from new properties)
        public int JobId { get { return JobPostID; } set { JobPostID = value; } }
        public int RecruiterId { get { return RecruiterID; } set { RecruiterID = value; } }
        public int? CompanyId { get { return CompanyID; } set { CompanyID = value; } }
        public decimal? SalaryMin { get { return SalaryFrom; } set { SalaryFrom = value; } }
        public decimal? SalaryMax { get { return SalaryTo; } set { SalaryTo = value; } }
        public string SalaryUnit { get { return SalaryCurrency; } set { SalaryCurrency = value; } }
        public string LocationText { get { return Location; } set { Location = value; } }
        public string Employment { get { return EmploymentType; } set { EmploymentType = value; } }
        public DateTime? Deadline { get { return ApplicationDeadline; } set { ApplicationDeadline = value; } }
    }

    /// <summary>
    /// ViewModel cho chi tiết tin tuyển dụng
    /// Maps to JobPostDetail table in database
    /// </summary>
    public class JobPostDetailVm
    {
        public int DetailID { get; set; }
        public int JobPostID { get; set; }
        public string Industry { get; set; }
        public string Major { get; set; }
        public int? YearsExperience { get; set; }
        public string DegreeRequired { get; set; }
        public string Skills { get; set; }
        public int? Headcount { get; set; }
        public string GenderRequirement { get; set; }
        public int? AgeFrom { get; set; }
        public int? AgeTo { get; set; }
        public string Status { get; set; }
        
        // Legacy properties for backward compatibility
        public int DetailId { get { return DetailID; } set { DetailID = value; } }
        public int JobId { get { return JobPostID; } set { JobPostID = value; } }
        public string EducationLevel { get { return DegreeRequired; } set { DegreeRequired = value; } }
        public string Gender { get { return GenderRequirement; } set { GenderRequirement = value; } }
    }
}


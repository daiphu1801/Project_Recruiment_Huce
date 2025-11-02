using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách tin tuyển dụng
    /// </summary>
    public class JobPostListVm
    {
        public int JobId { get; set; }
        public string JobCode { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string RecruiterName { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string SalaryUnit { get; set; }
        public string Employment { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; }
        public DateTime PostedAt { get; set; }
    }

    /// <summary>
    /// ViewModel cho chi tiết tin tuyển dụng
    /// </summary>
    public class JobPostDetailVm
    {
        public int DetailId { get; set; }
        public int JobId { get; set; }
        public string Industry { get; set; }
        public string Major { get; set; }
        public int YearsExperience { get; set; }
        public string EducationLevel { get; set; }
        public string Gender { get; set; }
        public int? AgeFrom { get; set; }
        public int? AgeTo { get; set; }
    }
}


using System;

namespace Project_Recruiment_Huce.Models.Candidates
{
    /// <summary>
    /// ViewModel cho công việc đã lưu
    /// </summary>
    public class SavedJobViewModel
    {
        public int SavedJobID { get; set; }
        public int JobPostID { get; set; }
        public string JobCode { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string EmploymentTypeDisplay { get; set; }
        public string SalaryRange { get; set; }
        public DateTime SavedAt { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public string LogoUrl { get; set; }
        public bool IsExpired => ApplicationDeadline.HasValue && ApplicationDeadline.Value < DateTime.Now;
    }
}


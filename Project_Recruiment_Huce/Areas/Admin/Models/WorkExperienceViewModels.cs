using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho kinh nghiệm làm việc
    /// </summary>
    public class WorkExperienceVm
    {
        public int ExperienceId { get; set; }
        public string CandidateName { get; set; }
        public string CompanyName { get; set; }
        public string JobTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}


using System;
using System.ComponentModel.DataAnnotations;

namespace Project_Recruiment_Huce.Models.Candidates
{
    public class ApplicationListItemViewModel
    {
        public int ApplicationID { get; set; }
        public int JobPostID { get; set; }
        
        [Display(Name = "Tiêu đề công việc")]
        public string JobTitle { get; set; }
        
        [Display(Name = "Công ty")]
        public string CompanyName { get; set; }
        
        [Display(Name = "Địa điểm")]
        public string Location { get; set; }
        
        [Display(Name = "Lương")]
        public string SalaryRange { get; set; }
        
        [Display(Name = "Ngày ứng tuyển")]
        [DataType(DataType.DateTime)]
        public DateTime? AppliedAt { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
        
        [Display(Name = "Hạn nộp hồ sơ")]
        [DataType(DataType.Date)]
        public DateTime? ApplicationDeadline { get; set; }
        
        public string ResumeFilePath { get; set; }
        public string CertificateFilePath { get; set; }
        public string Note { get; set; }
        
        public bool IsExpired => ApplicationDeadline.HasValue && ApplicationDeadline.Value < DateTime.Now;
        public bool IsRecent => AppliedAt.HasValue && (DateTime.Now - AppliedAt.Value).Days <= 7;
    }
}


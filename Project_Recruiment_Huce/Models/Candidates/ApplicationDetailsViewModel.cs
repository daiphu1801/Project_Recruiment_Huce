using System;
using System.ComponentModel.DataAnnotations;

namespace Project_Recruiment_Huce.Models.Candidates
{
    /// <summary>
    /// ViewModel cho trang chi tiết đơn ứng tuyển của candidate
    /// </summary>
    public class ApplicationDetailsViewModel
    {
        public int ApplicationID { get; set; }
        public int JobPostID { get; set; }
        
        [Display(Name = "Tiêu đề công việc")]
        public string JobTitle { get; set; }
        
        [Display(Name = "Mô tả công việc")]
        public string JobDescription { get; set; }
        
        [Display(Name = "Yêu cầu công việc")]
        public string JobRequirements { get; set; }
        
        [Display(Name = "Công ty")]
        public string CompanyName { get; set; }
        
        public string CompanyLogo { get; set; }
        
        [Display(Name = "Địa điểm")]
        public string Location { get; set; }
        
        [Display(Name = "Lương")]
        public string SalaryRange { get; set; }
        
        [Display(Name = "Loại công việc")]
        public string JobType { get; set; }
        
        [Display(Name = "Kinh nghiệm")]
        public string ExperienceLevel { get; set; }
        
        [Display(Name = "Hạn nộp hồ sơ")]
        [DataType(DataType.Date)]
        public DateTime? ApplicationDeadline { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
        
        [Display(Name = "Ngày ứng tuyển")]
        [DataType(DataType.DateTime)]
        public DateTime? AppliedAt { get; set; }
        
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
        
        [Display(Name = "File CV")]
        public string ResumeFilePath { get; set; }
        
        [Display(Name = "File chứng chỉ")]
        public string CertificateFilePath { get; set; }
        
        [Display(Name = "Tên ứng viên")]
        public string CandidateName { get; set; }
        
        [Display(Name = "Email")]
        public string CandidateEmail { get; set; }
        
        [Display(Name = "Số điện thoại")]
        public string CandidatePhone { get; set; }
        
        [Display(Name = "Cập nhật lần cuối")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsExpired => ApplicationDeadline.HasValue && ApplicationDeadline.Value < DateTime.Now;
        public bool IsRecent => AppliedAt.HasValue && (DateTime.Now - AppliedAt.Value).Days <= 7;
    }
}

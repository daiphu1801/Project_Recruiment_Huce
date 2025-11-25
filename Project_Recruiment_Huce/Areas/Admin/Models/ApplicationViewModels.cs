using System;
using System.ComponentModel.DataAnnotations; // Quan trọng: Để dùng [Display], [Required]

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    
    public class ApplicationListVm
    {
        
        [Display(Name = "Mã hồ sơ")]
        public int ApplicationId { get; set; }

        public int CandidateId { get; set; }
        public int JobPostId { get; set; }

        
        public int CompanyId { get; set; }

        
        [Display(Name = "Tên ứng viên")]
        public string CandidateName { get; set; }

        [Display(Name = "Vị trí ứng tuyển")]
        public string JobTitle { get; set; }

        [Display(Name = "Công ty")]
        public string CompanyName { get; set; }

        
        [Display(Name = "Ngày nộp")]
        public DateTime? AppliedAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Trạng thái")]
        public string AppStatus { get; set; }

        [Display(Name = "File CV")]
        public string ResumeFilePath { get; set; }

        [Display(Name = "Chứng chỉ")]
        public string CertificateFilePath { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
    }

    
    public class CreateApplicationListVm
    {
        [Display(Name = "Ứng viên")]
        [Required(ErrorMessage = "Vui lòng chọn ứng viên.")]
        public int CandidateId { get; set; }

        [Display(Name = "Vị trí tuyển dụng")]
        [Required(ErrorMessage = "Vui lòng chọn công việc.")]
        public int JobPostId { get; set; }

        [Display(Name = "Trạng thái hồ sơ")]
        public string AppStatus { get; set; } 

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }

       
        public string ResumeFilePath { get; set; }
        public string CertificateFilePath { get; set; }
    }

    
    public class EditApplicationListVm
    {
        [Required]
        public int ApplicationId { get; set; }

        [Display(Name = "Ứng viên")]
        [Required(ErrorMessage = "Vui lòng chọn ứng viên.")]
        public int CandidateId { get; set; }

        [Display(Name = "Vị trí tuyển dụng")]
        [Required(ErrorMessage = "Vui lòng chọn công việc.")]
        public int JobPostId { get; set; }

        [Display(Name = "Trạng thái hồ sơ")]
        [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        public string AppStatus { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }

        public string ResumeFilePath { get; set; }
        public string CertificateFilePath { get; set; }
    }
}
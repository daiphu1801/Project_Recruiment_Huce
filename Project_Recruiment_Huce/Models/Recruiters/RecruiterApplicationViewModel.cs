using System;
using System.ComponentModel.DataAnnotations;

namespace Project_Recruiment_Huce.Models.Recruiters
{
    /// <summary>
    /// ViewModel cho danh sách applications của Recruiter
    /// </summary>
    public class RecruiterApplicationViewModel
    {
        public int ApplicationID { get; set; }
        public int JobPostID { get; set; }
        public int CandidateID { get; set; }

        [Display(Name = "Ứng viên")]
        public string CandidateName { get; set; }

        [Display(Name = "Email")]
        public string CandidateEmail { get; set; }

        [Display(Name = "Điện thoại")]
        public string CandidatePhone { get; set; }

        [Display(Name = "Công việc")]
        public string JobTitle { get; set; }

        [Display(Name = "Mã công việc")]
        public string JobCode { get; set; }

        [Display(Name = "Công ty")]
        public string CompanyName { get; set; }

        [Display(Name = "Ngày ứng tuyển")]
        [DataType(DataType.DateTime)]
        public DateTime? AppliedAt { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "CV")]
        public string ResumeFilePath { get; set; }

        [Display(Name = "Chứng chỉ")]
        public string CertificateFilePath { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }

        [Display(Name = "Cập nhật lúc")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        // Helper properties
        public string StatusDisplay { get; set; }
        public string StatusBadgeClass { get; set; }
    }

    /// <summary>
    /// ViewModel cho cập nhật trạng thái application
    /// </summary>
    public class UpdateApplicationStatusViewModel
    {
        public int ApplicationID { get; set; }
        public int JobPostID { get; set; }
        public string CandidateName { get; set; }
        public string JobTitle { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Ghi chú")]
        [DataType(DataType.MultilineText)]
        public string Note { get; set; }
    }

    /// <summary>
    /// ViewModel cho đặt lịch phỏng vấn
    /// </summary>
    public class InterviewScheduleViewModel
    {
        public int ApplicationID { get; set; }
        
        [Display(Name = "Ứng viên")]
        public string CandidateName { get; set; }
        
        [Display(Name = "Email ứng viên")]
        public string CandidateEmail { get; set; }
        
        [Display(Name = "Vị trí ứng tuyển")]
        public string JobTitle { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày phỏng vấn")]
        [Display(Name = "Ngày phỏng vấn")]
        [DataType(DataType.Date)]
        public DateTime InterviewDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ phỏng vấn")]
        [Display(Name = "Giờ phỏng vấn")]
        [DataType(DataType.Time)]
        public TimeSpan InterviewTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa điểm phỏng vấn")]
        [Display(Name = "Địa điểm phỏng vấn")]
        [StringLength(500, ErrorMessage = "Địa điểm không được vượt quá 500 ký tự")]
        public string Location { get; set; }

        [Display(Name = "Hình thức phỏng vấn")]
        [Required(ErrorMessage = "Vui lòng chọn hình thức phỏng vấn")]
        public string InterviewType { get; set; }

        [Display(Name = "Thời gian dự kiến (phút)")]
        [Range(15, 480, ErrorMessage = "Thời gian phỏng vấn từ 15 phút đến 8 giờ")]
        public int? Duration { get; set; }

        [Display(Name = "Người phỏng vấn")]
        [StringLength(200, ErrorMessage = "Tên người phỏng vấn không được vượt quá 200 ký tự")]
        public string Interviewer { get; set; }

        [Display(Name = "Tài liệu cần mang theo")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000, ErrorMessage = "Nội dung không được vượt quá 1000 ký tự")]
        public string RequiredDocuments { get; set; }

        [Display(Name = "Lưu ý thêm")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000, ErrorMessage = "Nội dung không được vượt quá 1000 ký tự")]
        public string AdditionalNotes { get; set; }
    }
}


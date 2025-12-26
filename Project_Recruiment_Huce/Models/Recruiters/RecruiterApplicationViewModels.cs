using System;
using System.Collections.Generic;
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

    #region Service Result Classes

    /// <summary>
    /// Result cho danh sách applications phân trang
    /// </summary>
    public class ApplicationListResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<RecruiterApplicationViewModel> Applications { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int? JobId { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Result cho chi tiết application
    /// </summary>
    public class ApplicationDetailsResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public RecruiterApplicationViewModel Application { get; set; }
    }

    /// <summary>
    /// Result cho form cập nhật trạng thái
    /// </summary>
    public class ApplicationStatusFormResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public UpdateApplicationStatusViewModel ViewModel { get; set; }
    }

    /// <summary>
    /// Result cho cập nhật trạng thái
    /// </summary>
    public class UpdateStatusResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public int ApplicationId { get; set; }
    }

    #endregion

    #region Option Classes

    /// <summary>
    /// Job item cho dropdown filter
    /// </summary>
    public class JobFilterItem
    {
        public int JobPostID { get; set; }
        public string Title { get; set; }
        public string JobCode { get; set; }
    }

    /// <summary>
    /// Status option cho dropdown
    /// </summary>
    public class StatusOption
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    #endregion
}

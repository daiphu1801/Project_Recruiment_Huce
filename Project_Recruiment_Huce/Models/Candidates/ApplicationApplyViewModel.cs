using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Project_Recruiment_Huce.Models.Candidates
{
    /// <summary>
    /// ViewModel cho form ứng tuyển
    /// </summary>
    public class ApplicationApplyViewModel
    {
        // Job Information
        public int JobPostID { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string SalaryRange { get; set; }
        public DateTime? ApplicationDeadline { get; set; }

        // Candidate Information
        public int CandidateID { get; set; }
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Summary { get; set; }
        public string PhotoUrl { get; set; }

        // Resume Selection
        [Display(Name = "Chọn CV từ danh sách")]
        public int? SelectedResumeFileID { get; set; }
        
        public List<ResumeFileViewModel> AvailableResumes { get; set; }

        // Upload New Resume
        [Display(Name = "Hoặc tải CV mới lên")]
        public HttpPostedFileBase NewResumeFile { get; set; }
        
        [Display(Name = "Tên CV")]
        public string NewResumeTitle { get; set; }

        // Additional Information
        [Display(Name = "Ghi chú (tùy chọn)")]
        [DataType(DataType.MultilineText)]
        public string Note { get; set; }
    }
}


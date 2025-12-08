using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    // ViewModel for Dashboard statistics
    public class InterviewDashboardViewModel
    {
        public int TodayInterviews { get; set; }
        public int WeekInterviews { get; set; }
        public int MonthInterviews { get; set; }
        
        public int PendingInterviews { get; set; }
        public int CompletedInterviews { get; set; }
        public int CancelledInterviews { get; set; }
        
        public decimal AttendanceRate { get; set; } // Percentage (0-100)
        
        public List<TopRecruiterViewModel> TopRecruiters { get; set; }
        public List<ChartDataPoint> TrendData { get; set; } // For interview trend chart
        
        public InterviewDashboardViewModel()
        {
            TopRecruiters = new List<TopRecruiterViewModel>();
            TrendData = new List<ChartDataPoint>();
        }
    }

    // ViewModel for top recruiters in dashboard
    public class TopRecruiterViewModel
    {
        public string RecruiterName { get; set; }
        public string RecruiterEmail { get; set; }
        public int InterviewCount { get; set; }
        public decimal AttendanceRate { get; set; }
    }

    // ViewModel for chart data points
    public class ChartDataPoint
    {
        public string Label { get; set; } // Date or category label
        public int Value { get; set; }
    }

    // ViewModel for interview list page
    public class InterviewListViewModel
    {
        public List<AdminInterviewListViewModel> Interviews { get; set; }
        public InterviewFilterModel Filters { get; set; }
        
        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        
        public InterviewListViewModel()
        {
            Interviews = new List<AdminInterviewListViewModel>();
            Filters = new InterviewFilterModel();
            PageSize = 20;
            CurrentPage = 1;
        }
    }

    // ViewModel for single interview in list
    public class AdminInterviewListViewModel
    {
        public int InterviewID { get; set; }
        
        // Candidate info
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        
        // Recruiter info
        public string RecruiterName { get; set; }
        public string RecruiterEmail { get; set; }
        
        // Job info
        public string JobTitle { get; set; }
        public int JobID { get; set; }
        
        // Interview details
        public DateTime InterviewDate { get; set; }
        public string InterviewTime { get; set; }
        public string InterviewType { get; set; } // Offline, Online, Phone
        public string Location { get; set; }
        public string Status { get; set; } // Pending, Completed, Cancelled
        
        // Email info
        public bool EmailSent { get; set; }
        public DateTime? EmailSentDate { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Filter model for interview list
    public class InterviewFilterModel
    {
        [Display(Name = "Từ ngày")]
        public DateTime? FromDate { get; set; }
        
        [Display(Name = "Đến ngày")]
        public DateTime? ToDate { get; set; }
        
        [Display(Name = "Nhà tuyển dụng")]
        public string RecruiterEmail { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } // Pending, Completed, Cancelled, All
        
        [Display(Name = "Hình thức")]
        public string InterviewType { get; set; } // Offline, Online, Phone, All
        
        [Display(Name = "Tìm kiếm")]
        public string SearchTerm { get; set; } // Search by candidate name, job title, etc.
        
        [Display(Name = "Sắp xếp theo")]
        public string SortBy { get; set; } // InterviewDate, CreatedAt, CandidateName, etc.
        
        [Display(Name = "Thứ tự")]
        public string SortOrder { get; set; } // Asc, Desc
        
        public InterviewFilterModel()
        {
            Status = "All";
            InterviewType = "All";
            SortBy = "InterviewDate";
            SortOrder = "Desc";
        }
    }

    // ViewModel for interview details page
    public class InterviewDetailsViewModel
    {
        // Interview basic info
        public int InterviewID { get; set; }
        public int ApplicationID { get; set; }
        
        // Candidate information
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public string CandidatePhone { get; set; }
        
        // Recruiter information
        public string RecruiterName { get; set; }
        public string RecruiterEmail { get; set; }
        public string RecruiterPhone { get; set; }
        public string CompanyName { get; set; }
        
        // Job information
        public int JobID { get; set; }
        public string JobTitle { get; set; }
        public string JobLocation { get; set; }
        
        // Interview schedule details
        [Display(Name = "Ngày phỏng vấn")]
        public DateTime InterviewDate { get; set; }
        
        [Display(Name = "Giờ phỏng vấn")]
        public string InterviewTime { get; set; }
        
        [Display(Name = "Thời lượng")]
        public string Duration { get; set; }
        
        [Display(Name = "Hình thức")]
        public string InterviewType { get; set; } // Offline, Online, Phone
        
        [Display(Name = "Địa điểm")]
        public string Location { get; set; }
        
        [Display(Name = "Người phỏng vấn")]
        public string Interviewer { get; set; }
        
        [Display(Name = "Tài liệu yêu cầu")]
        public string RequiredDocuments { get; set; }
        
        [Display(Name = "Ghi chú")]
        public string AdditionalNotes { get; set; }
        
        // Status and tracking
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } // Pending, Completed, Cancelled
        
        [Display(Name = "Email đã gửi")]
        public bool EmailSent { get; set; }
        
        [Display(Name = "Ngày gửi email")]
        public DateTime? EmailSentDate { get; set; }
        
        [Display(Name = "Đã tham dự")]
        public bool? Attended { get; set; }
        
        [Display(Name = "Kết quả")]
        public string Result { get; set; } // Passed, Failed, Pending
        
        // Audit fields
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Người tạo")]
        public string CreatedBy { get; set; }
        
        [Display(Name = "Cập nhật lần cuối")]
        public DateTime? UpdatedAt { get; set; }
        
        [Display(Name = "Người cập nhật")]
        public string UpdatedBy { get; set; }
        
        // Timeline events (for future enhancement)
        public List<InterviewTimelineEvent> Timeline { get; set; }
        
        public InterviewDetailsViewModel()
        {
            Timeline = new List<InterviewTimelineEvent>();
        }
    }

    // Timeline event for tracking interview history
    public class InterviewTimelineEvent
    {
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } // Created, EmailSent, Completed, Cancelled, Rescheduled
        public string Description { get; set; }
        public string PerformedBy { get; set; }
    }
}

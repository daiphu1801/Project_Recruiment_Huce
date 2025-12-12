using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý lịch phỏng vấn cho Admin
    /// </summary>
    public class InterviewManagementController : AdminBaseController
    {
        // GET: Admin/InterviewManagement/Dashboard
        /// <summary>
        /// Dashboard thống kê tổng quan lịch phỏng vấn
        /// </summary>
        public ActionResult Dashboard()
        {
            ViewBag.Title = "Thống kê lịch phỏng vấn";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> 
            { 
                Tuple.Create("Thống kê lịch phỏng vấn", "") 
            };
            
            // TODO: Lấy dữ liệu thống kê từ database
            var viewModel = new InterviewDashboardViewModel
            {
                TodayInterviews = 5,
                WeekInterviews = 23,
                MonthInterviews = 87,
                PendingInterviews = 12,
                CompletedInterviews = 65,
                CancelledInterviews = 10,
                AttendanceRate = 85.5m,
                
                TopRecruiters = new List<TopRecruiterViewModel>
                {
                    new TopRecruiterViewModel { RecruiterName = "Nguyễn Văn A", RecruiterEmail = "nguyenvana@company.com", InterviewCount = 15, AttendanceRate = 90.5m },
                    new TopRecruiterViewModel { RecruiterName = "Trần Thị B", RecruiterEmail = "tranthib@company.com", InterviewCount = 12, AttendanceRate = 85.0m },
                    new TopRecruiterViewModel { RecruiterName = "Lê Văn C", RecruiterEmail = "levanc@company.com", InterviewCount = 10, AttendanceRate = 88.5m }
                },
                
                TrendData = new List<ChartDataPoint>
                {
                    new ChartDataPoint { Label = "T2", Value = 5 },
                    new ChartDataPoint { Label = "T3", Value = 8 },
                    new ChartDataPoint { Label = "T4", Value = 6 },
                    new ChartDataPoint { Label = "T5", Value = 10 },
                    new ChartDataPoint { Label = "T6", Value = 7 },
                    new ChartDataPoint { Label = "T7", Value = 3 },
                    new ChartDataPoint { Label = "CN", Value = 2 }
                }
            };

            return View(viewModel);
        }

        // GET: Admin/InterviewManagement/Index
        /// <summary>
        /// Danh sách tất cả lịch phỏng vấn
        /// </summary>
        public ActionResult Index(InterviewFilterModel filters, int page = 1, int pageSize = 20)
        {
            ViewBag.Title = "Quản lý lịch phỏng vấn";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> 
            { 
                Tuple.Create("Quản lý lịch phỏng vấn", "") 
            };
            
            // TODO: Lấy dữ liệu từ database với filters
            var viewModel = new InterviewListViewModel
            {
                Filters = filters ?? new InterviewFilterModel(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = 150, // TODO: Get from database
                
                Interviews = GetSampleInterviews().Skip((page - 1) * pageSize).Take(pageSize).ToList()
            };

            viewModel.TotalPages = (int)Math.Ceiling((double)viewModel.TotalItems / pageSize);

            // Populate dropdowns
            ViewBag.Recruiters = new SelectList(new[] 
            {
                new { Value = "", Text = "-- Tất cả Recruiters --" },
                new { Value = "nguyenvana@company.com", Text = "Nguyễn Văn A" },
                new { Value = "tranthib@company.com", Text = "Trần Thị B" }
            }, "Value", "Text", filters?.RecruiterEmail);

            ViewBag.Statuses = new SelectList(new[] 
            {
                new { Value = "", Text = "-- Tất cả trạng thái --" },
                new { Value = "Scheduled", Text = "Đã lên lịch" },
                new { Value = "Completed", Text = "Hoàn thành" },
                new { Value = "Cancelled", Text = "Đã hủy" },
                new { Value = "NoShow", Text = "Vắng mặt" }
            }, "Value", "Text", filters?.Status);

            ViewBag.InterviewTypes = new SelectList(new[] 
            {
                new { Value = "", Text = "-- Tất cả hình thức --" },
                new { Value = "Trực tiếp", Text = "Trực tiếp" },
                new { Value = "Trực tuyến", Text = "Trực tuyến" },
                new { Value = "Điện thoại", Text = "Điện thoại" }
            }, "Value", "Text", filters?.InterviewType);

            return View(viewModel);
        }

        // GET: Admin/InterviewManagement/Details/5
        /// <summary>
        /// Chi tiết lịch phỏng vấn
        /// </summary>
        public ActionResult Details(int id)
        {
            ViewBag.Title = "Chi tiết lịch phỏng vấn";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> 
            { 
                Tuple.Create("Quản lý lịch phỏng vấn", Url.Action("Index", "InterviewManagement", new { area = "Admin" })),
                Tuple.Create("Chi tiết", "")
            };
            
            // TODO: Lấy chi tiết từ database
            var viewModel = new InterviewDetailsViewModel
            {
                InterviewID = id,
                ApplicationID = 123,
                CandidateName = "Nguyễn Văn X",
                CandidateEmail = "candidate@example.com",
                CandidatePhone = "0123456789",
                RecruiterName = "Nguyễn Văn A",
                RecruiterEmail = "recruiter@company.com",
                RecruiterPhone = "0987654321",
                JobID = 456,
                JobTitle = "Senior .NET Developer",
                JobLocation = "Hà Nội",
                CompanyName = "Công ty ABC",
                InterviewDate = DateTime.Now.AddDays(3),
                InterviewTime = "14:30",
                Duration = "60 phút",
                InterviewType = "Offline",
                Location = "Phòng họp A, Tầng 5, Tòa nhà XYZ",
                Interviewer = "Mr. Trần Văn B - Giám đốc Kỹ thuật",
                RequiredDocuments = "- Bản gốc CMND/CCCD\n- Bản sao bằng cấp\n- CV cập nhật",
                AdditionalNotes = "Vui lòng đến trước 10 phút",
                Status = "Pending",
                EmailSent = true,
                EmailSentDate = DateTime.Now.AddDays(-1),
                Attended = null,
                Result = "Pending",
                CreatedAt = DateTime.Now.AddDays(-2),
                CreatedBy = "Nguyễn Văn A",
                UpdatedAt = DateTime.Now.AddDays(-1),
                UpdatedBy = "Nguyễn Văn A"
            };

            return View(viewModel);
        }

        // Helper method - TODO: Replace with real database query
        private List<AdminInterviewListViewModel> GetSampleInterviews()
        {
            var interviews = new List<AdminInterviewListViewModel>();
            for (int i = 1; i <= 150; i++)
            {
                interviews.Add(new AdminInterviewListViewModel
                {
                    InterviewID = i,
                    CandidateName = $"Ứng viên {i}",
                    CandidateEmail = $"candidate{i}@example.com",
                    RecruiterName = $"Recruiter {(i % 5) + 1}",
                    RecruiterEmail = $"recruiter{(i % 5) + 1}@company.com",
                    JobTitle = $"Vị trí {(i % 10) + 1}",
                    JobID = (i % 10) + 1,
                    InterviewDate = DateTime.Now.AddDays(i % 30),
                    InterviewTime = $"{9 + (i % 8)}:00",
                    InterviewType = new[] { "Offline", "Online", "Phone" }[i % 3],
                    Location = $"Địa điểm {i % 5 + 1}",
                    Status = new[] { "Pending", "Completed", "Cancelled" }[i % 3],
                    EmailSent = i % 2 == 0,
                    EmailSentDate = i % 2 == 0 ? (DateTime?)DateTime.Now.AddDays(-1) : null,
                    CreatedAt = DateTime.Now.AddDays(-i),
                    UpdatedAt = i % 3 == 0 ? (DateTime?)DateTime.Now.AddDays(-1) : null
                });
            }
            return interviews;
        }
    }
}

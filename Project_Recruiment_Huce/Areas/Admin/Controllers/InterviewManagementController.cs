using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý lịch phỏng vấn cho Admin
    /// Lấy dữ liệu từ Applications có Status = "Interview"
    /// </summary>
    public class InterviewManagementController : AdminBaseController
    {
        // GET: Admin/InterviewManagement/Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.Title = "Thống kê lịch phỏng vấn";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> 
            { 
                Tuple.Create("Thống kê lịch phỏng vấn", "") 
            };
            
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);
                var weekEnd = weekStart.AddDays(7);
                var monthAgo = today.AddMonths(-1);
                
                // Lấy tất cả applications có status = "Interview"
                var interviewApplications = db.Applications
                    .Where(a => a.Status == "Interview")
                    .ToList();
                
                // Thống kê - UpdatedAt và AppliedAt là DateTime NOT NULL
                var todayInterviews = interviewApplications.Count(a => 
                    a.UpdatedAt >= today && a.UpdatedAt < tomorrow);
                var weekInterviews = interviewApplications.Count(a => 
                    a.UpdatedAt >= weekStart && a.UpdatedAt < weekEnd);
                var monthInterviews = interviewApplications.Count(a => 
                    a.UpdatedAt >= monthAgo);
                
                // Top Recruiters
                var jobPostIds = interviewApplications.Select(a => a.JobPostID).Distinct().ToList();
                var jobPosts = db.JobPosts.Where(j => jobPostIds.Contains(j.JobPostID)).ToList();
                var recruiterIds = jobPosts.Select(j => j.RecruiterID).Distinct().ToList();
                var recruitersData = db.Recruiters.Where(r => recruiterIds.Contains(r.RecruiterID)).ToList();
                
                var topRecruiters = (from app in interviewApplications
                                     join job in jobPosts on app.JobPostID equals job.JobPostID
                                     join recruiter in recruitersData on job.RecruiterID equals recruiter.RecruiterID
                                     group new { app, recruiter } by new { recruiter.RecruiterID, recruiter.FullName, recruiter.CompanyEmail } into g
                                     orderby g.Count() descending
                                     select new TopRecruiterViewModel
                                     {
                                         RecruiterName = g.Key.FullName ?? "N/A",
                                         RecruiterEmail = g.Key.CompanyEmail ?? "N/A",
                                         InterviewCount = g.Count(),
                                         AttendanceRate = 0
                                     })
                                    .Take(5)
                                    .ToList();
                
                // Trend data
                var trendData = new List<ChartDataPoint>();
                var dayNames = new[] { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };
                for (int i = 0; i < 7; i++)
                {
                    var date = weekStart.AddDays(i);
                    var nextDate = date.AddDays(1);
                    var count = interviewApplications.Count(a => 
                        a.UpdatedAt >= date && a.UpdatedAt < nextDate);
                    trendData.Add(new ChartDataPoint
                    {
                        Label = dayNames[(int)date.DayOfWeek],
                        Value = count
                    });
                }
                
                var viewModel = new InterviewDashboardViewModel
                {
                    TodayInterviews = todayInterviews,
                    WeekInterviews = weekInterviews,
                    MonthInterviews = monthInterviews,
                    PendingInterviews = interviewApplications.Count,
                    CompletedInterviews = 0,
                    CancelledInterviews = 0,
                    AttendanceRate = 0,
                    TopRecruiters = topRecruiters,
                    TrendData = trendData
                };

                return View(viewModel);
            }
        }

        // GET: Admin/InterviewManagement/Index
        public ActionResult Index(InterviewFilterModel filters, int page = 1, int pageSize = 20)
        {
            ViewBag.Title = "Quản lý lịch phỏng vấn";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> 
            { 
                Tuple.Create("Quản lý lịch phỏng vấn", "") 
            };
            
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Query các Application có Status = "Interview"
                var allInterviews = (from app in db.Applications
                                     where app.Status == "Interview"
                                     join candidate in db.Candidates on app.CandidateID equals candidate.CandidateID
                                     join job in db.JobPosts on app.JobPostID equals job.JobPostID
                                     join recruiter in db.Recruiters on job.RecruiterID equals recruiter.RecruiterID
                                     select new
                                     {
                                         app.ApplicationID,
                                         app.AppliedAt,
                                         app.UpdatedAt,
                                         app.Note,
                                         CandidateName = candidate.FullName,
                                         CandidateEmail = candidate.Email,
                                         RecruiterName = recruiter.FullName,
                                         RecruiterEmail = recruiter.CompanyEmail,
                                         job.JobPostID,
                                         JobTitle = job.Title,
                                         JobLocation = job.Location
                                     }).ToList();
                
                // Apply filters in memory
                if (filters == null) filters = new InterviewFilterModel();
                
                var filteredInterviews = allInterviews.AsEnumerable();
                
                // Filter by date range - UpdatedAt là DateTime NOT NULL
                if (filters.FromDate.HasValue)
                {
                    filteredInterviews = filteredInterviews.Where(x => x.UpdatedAt >= filters.FromDate.Value);
                }
                if (filters.ToDate.HasValue)
                {
                    var toDateEnd = filters.ToDate.Value.AddDays(1);
                    filteredInterviews = filteredInterviews.Where(x => x.UpdatedAt < toDateEnd);
                }
                
                // Filter by recruiter email
                if (!string.IsNullOrWhiteSpace(filters.RecruiterEmail))
                {
                    filteredInterviews = filteredInterviews.Where(x => 
                        x.RecruiterEmail != null && x.RecruiterEmail.Equals(filters.RecruiterEmail, StringComparison.OrdinalIgnoreCase));
                }
                
                // Filter by search term
                if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
                {
                    var term = filters.SearchTerm.ToLower();
                    filteredInterviews = filteredInterviews.Where(x => 
                        (x.CandidateName != null && x.CandidateName.ToLower().Contains(term)) ||
                        (x.JobTitle != null && x.JobTitle.ToLower().Contains(term)) ||
                        (x.CandidateEmail != null && x.CandidateEmail.ToLower().Contains(term)));
                }
                
                // Apply sorting
                var sortedList = filteredInterviews.OrderByDescending(x => x.UpdatedAt).ToList();
                
                // Get total count
                var totalItems = sortedList.Count;
                
                // Apply pagination
                var items = sortedList
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                // Map to ViewModel
                var interviews = items.Select(x => new AdminInterviewListViewModel
                {
                    InterviewID = x.ApplicationID,
                    CandidateName = x.CandidateName ?? "N/A",
                    CandidateEmail = x.CandidateEmail ?? "N/A",
                    RecruiterName = x.RecruiterName ?? "N/A",
                    RecruiterEmail = x.RecruiterEmail ?? "N/A",
                    JobTitle = x.JobTitle ?? "N/A",
                    JobID = x.JobPostID,
                    InterviewDate = x.UpdatedAt,
                    InterviewTime = ParseInterviewTime(x.Note),
                    InterviewType = ParseInterviewType(x.Note),
                    Location = ParseLocation(x.Note, x.JobLocation),
                    EmailSent = false,
                    EmailSentDate = null,
                    CreatedAt = x.AppliedAt,
                    UpdatedAt = x.UpdatedAt
                }).ToList();
                
                var viewModel = new InterviewListViewModel
                {
                    Filters = filters,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                    Interviews = interviews
                };

                // Populate dropdowns
                var recruiters = db.Recruiters
                    .Select(r => new { r.CompanyEmail, r.FullName })
                    .Distinct()
                    .ToList();
                
                var recruiterList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "-- Tất cả Recruiters --" }
                };
                recruiterList.AddRange(recruiters.Select(r => new SelectListItem
                {
                    Value = r.CompanyEmail,
                    Text = r.FullName
                }));
                ViewBag.Recruiters = new SelectList(recruiterList, "Value", "Text", filters.RecruiterEmail);

                return View(viewModel);
            }
        }

        // GET: Admin/InterviewManagement/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.Title = "Chi tiết lịch phỏng vấn";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> 
            { 
                Tuple.Create("Quản lý lịch phỏng vấn", Url.Action("Index", "InterviewManagement", new { area = "Admin" })),
                Tuple.Create("Chi tiết", "")
            };
            
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var result = (from app in db.Applications
                              where app.ApplicationID == id && app.Status == "Interview"
                              join candidate in db.Candidates on app.CandidateID equals candidate.CandidateID
                              join job in db.JobPosts on app.JobPostID equals job.JobPostID
                              join recruiter in db.Recruiters on job.RecruiterID equals recruiter.RecruiterID
                              join company in db.Companies on job.CompanyID equals company.CompanyID
                              select new
                              {
                                  app.ApplicationID,
                                  app.AppliedAt,
                                  app.UpdatedAt,
                                  app.Note,
                                  CandidateName = candidate.FullName,
                                  CandidateEmail = candidate.Email,
                                  CandidatePhone = candidate.Phone,
                                  RecruiterName = recruiter.FullName,
                                  RecruiterEmail = recruiter.CompanyEmail,
                                  RecruiterPhone = recruiter.Phone,
                                  CompanyName = company.CompanyName,
                                  job.JobPostID,
                                  JobTitle = job.Title,
                                  JobLocation = job.Location
                              }).FirstOrDefault();
                
                if (result == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch phỏng vấn";
                    return RedirectToAction("Index");
                }
                
                var viewModel = new InterviewDetailsViewModel
                {
                    InterviewID = result.ApplicationID,
                    ApplicationID = result.ApplicationID,
                    CandidateName = result.CandidateName ?? "N/A",
                    CandidateEmail = result.CandidateEmail ?? "N/A",
                    CandidatePhone = result.CandidatePhone ?? "N/A",
                    RecruiterName = result.RecruiterName ?? "N/A",
                    RecruiterEmail = result.RecruiterEmail ?? "N/A",
                    RecruiterPhone = result.RecruiterPhone ?? "N/A",
                    CompanyName = result.CompanyName ?? "N/A",
                    JobID = result.JobPostID,
                    JobTitle = result.JobTitle ?? "N/A",
                    JobLocation = result.JobLocation ?? "N/A",
                    InterviewDate = result.UpdatedAt,
                    InterviewTime = ParseInterviewTime(result.Note),
                    Duration = "60 phút",
                    InterviewType = ParseInterviewType(result.Note),
                    Location = ParseLocation(result.Note, result.JobLocation),
                    AdditionalNotes = result.Note,
                    EmailSent = false,
                    CreatedAt = result.AppliedAt,
                    UpdatedAt = result.UpdatedAt
                };

                return View(viewModel);
            }
        }
        
        #region Helper Methods
        
        private string ParseInterviewTime(string note)
        {
            if (string.IsNullOrWhiteSpace(note)) return "Chưa xác định";
            
            var patterns = new[] { "Thời gian:", "Time:", "Giờ:", "Lúc:" };
            foreach (var pattern in patterns)
            {
                var index = note.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    var afterPattern = note.Substring(index + pattern.Length).Trim();
                    var parts = afterPattern.Split(new[] { '\n', '\r', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        var timePart = parts[0].Trim();
                        if (timePart.Length <= 10) return timePart;
                    }
                }
            }
            
            return "Chưa xác định";
        }
        
        private string ParseInterviewType(string note)
        {
            if (string.IsNullOrWhiteSpace(note)) return "Chưa xác định";
            
            var lowerNote = note.ToLower();
            if (lowerNote.Contains("online") || lowerNote.Contains("trực tuyến") || lowerNote.Contains("zoom") || lowerNote.Contains("meet"))
                return "Online";
            if (lowerNote.Contains("phone") || lowerNote.Contains("điện thoại"))
                return "Phone";
            if (lowerNote.Contains("offline") || lowerNote.Contains("trực tiếp") || lowerNote.Contains("văn phòng"))
                return "Offline";
            
            return "Chưa xác định";
        }
        
        private string ParseLocation(string note, string jobLocation)
        {
            if (!string.IsNullOrWhiteSpace(note))
            {
                var patterns = new[] { "Địa điểm:", "Location:", "Tại:", "Ở:" };
                foreach (var pattern in patterns)
                {
                    var index = note.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        var afterPattern = note.Substring(index + pattern.Length).Trim();
                        var parts = afterPattern.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                        {
                            return parts[0].Trim();
                        }
                    }
                }
            }
            
            return jobLocation ?? "Chưa xác định";
        }
        
        #endregion
    }
}

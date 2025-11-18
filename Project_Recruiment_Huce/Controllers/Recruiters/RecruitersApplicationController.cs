using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Recruiters;
using Project_Recruiment_Huce.Repositories;
using Project_Recruiment_Huce.Infrastructure;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller quản lý đơn ứng tuyển cho Recruiter
    /// </summary>
    [Authorize]
    public class RecruitersApplicationController : BaseController
    {
        // Using Repository Pattern for data access
        private ApplicationRepository GetApplicationRepository(JOBPORTAL_ENDataContext db)
        {
            return new ApplicationRepository(db);
        }

        /// <summary>
        /// GET: RecruitersApplication/MyApplications
        /// Xem danh sách đơn ứng tuyển cho Recruiter
        /// </summary>
        [HttpGet]
        public ActionResult MyApplications(int? jobId, string status = null, int? page = null)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để xem đơn ứng tuyển.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var applicationRepository = GetApplicationRepository(db);

                // Get applications for jobs posted by this recruiter using repository
                var applications = applicationRepository.GetApplicationsByRecruiter(recruiterId.Value);

                // Filter by job if specified
                if (jobId.HasValue)
                {
                    applications = applications.Where(a => a.JobPostID == jobId.Value);
                }

                // Filter by status if specified
                if (!string.IsNullOrEmpty(status))
                {
                    applications = applications.Where(a => a.Status == status);
                }

                var applicationsList = applications.ToList();

                // Map to ViewModels với xử lý cẩn thận để tránh hiển thị N/A
                var applicationsViewModels = applicationsList.Select(app => {
                    string companyName = "Không có thông tin";
                    if (app.JobPost?.Company?.CompanyName != null)
                    {
                        companyName = app.JobPost.Company.CompanyName;
                    }
                    else if (app.JobPost?.Recruiter?.Company?.CompanyName != null)
                    {
                        companyName = app.JobPost.Recruiter.Company.CompanyName;
                    }

                    return new RecruiterApplicationViewModel
                    {
                        ApplicationID = app.ApplicationID,
                        JobPostID = app.JobPostID,
                        CandidateID = app.CandidateID,
                        CandidateName = !string.IsNullOrEmpty(app.Candidate?.FullName) ? app.Candidate.FullName : "Ứng viên ẩn danh",
                        CandidateEmail = app.Candidate?.Email ?? "",
                        CandidatePhone = app.Candidate?.Phone ?? "",
                        JobTitle = !string.IsNullOrEmpty(app.JobPost?.Title) ? app.JobPost.Title : "Công việc không có tiêu đề",
                        JobCode = app.JobPost?.JobCode ?? "",
                        CompanyName = companyName,
                        AppliedAt = app.AppliedAt,
                        Status = app.Status ?? "Under review",
                        ResumeFilePath = app.ResumeFilePath,
                        CertificateFilePath = app.CertificateFilePath,
                        Note = app.Note,
                        UpdatedAt = app.UpdatedAt,
                        StatusDisplay = GetApplicationStatusDisplay(app.Status ?? "Under review"),
                        StatusBadgeClass = GetApplicationStatusBadgeClass(app.Status ?? "Under review")
                    };
                }).ToList();

                // Pagination
                int pageSize = 10;
                int pageNumber = page ?? 1;
                int totalItems = applicationsViewModels.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedApplications = applicationsViewModels.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                // Get list of jobs for filter dropdown
                var jobs = db.JobPosts
                    .Where(j => j.RecruiterID == recruiterId.Value)
                    .OrderByDescending(j => j.PostedAt)
                    .Select(j => new { j.JobPostID, j.Title, j.JobCode })
                    .ToList();

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.JobId = jobId;
                ViewBag.Status = status;
                ViewBag.Jobs = new SelectList(jobs, "JobPostID", "Title", jobId);
                ViewBag.StatusOptions = new SelectList(new[] {
                    new { Value = "", Text = "Tất cả trạng thái" },
                    new { Value = "Under review", Text = "Đang xem xét" },
                    new { Value = "Interview", Text = "Phỏng vấn" },
                    new { Value = "Offered", Text = "Đã đề xuất" },
                    new { Value = "Hired", Text = "Đã tuyển" },
                    new { Value = "Rejected", Text = "Đã từ chối" }
                }, "Value", "Text", status);

                return View(pagedApplications);
            }
        }

        /// <summary>
        /// GET: RecruitersApplication/ApplicationDetails
        /// Xem chi tiết đơn ứng tuyển
        /// </summary>
        [HttpGet]
        public ActionResult ApplicationDetails(int? id)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để xem đơn ứng tuyển.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn ứng tuyển.";
                return RedirectToAction("MyApplications", "RecruitersApplication");
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {

                // Load related entities
                var loadOptions = new System.Data.Linq.DataLoadOptions();
                loadOptions.LoadWith<Application>(a => a.JobPost);
                loadOptions.LoadWith<Application>(a => a.Candidate);
                loadOptions.LoadWith<JobPost>(j => j.Company);
                db.LoadOptions = loadOptions;

                // Get application and verify it belongs to this recruiter's job
                var application = db.Applications.FirstOrDefault(a => a.ApplicationID == id.Value);
                if (application == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn ứng tuyển.";
                    return RedirectToAction("MyApplications", "RecruitersApplication");
                }

                var job = application.JobPost;
                if (job == null || job.RecruiterID != recruiterId.Value)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xem đơn ứng tuyển này.";
                    return RedirectToAction("MyApplications", "RecruitersApplication");
                }

                // Map to ViewModel với xử lý cẩn thận để tránh hiển thị N/A
                string companyName = "Không có thông tin";
                if (job.Company?.CompanyName != null)
                {
                    companyName = job.Company.CompanyName;
                }
                else if (job.Recruiter?.Company?.CompanyName != null)
                {
                    companyName = job.Recruiter.Company.CompanyName;
                }

                var viewModel = new RecruiterApplicationViewModel
                {
                    ApplicationID = application.ApplicationID,
                    JobPostID = application.JobPostID,
                    CandidateID = application.CandidateID,
                    CandidateName = !string.IsNullOrEmpty(application.Candidate?.FullName) ? application.Candidate.FullName : "Ứng viên ẩn danh",
                    CandidateEmail = application.Candidate?.Email ?? "",
                    CandidatePhone = application.Candidate?.Phone ?? "",
                    JobTitle = !string.IsNullOrEmpty(job.Title) ? job.Title : "Công việc không có tiêu đề",
                    JobCode = job.JobCode ?? "",
                    CompanyName = companyName,
                    AppliedAt = application.AppliedAt,
                    Status = application.Status ?? "Under review",
                    ResumeFilePath = application.ResumeFilePath,
                    CertificateFilePath = application.CertificateFilePath,
                    Note = application.Note,
                    UpdatedAt = application.UpdatedAt,
                    StatusDisplay = GetApplicationStatusDisplay(application.Status ?? "Under review"),
                    StatusBadgeClass = GetApplicationStatusBadgeClass(application.Status ?? "Under review")
                };

                return View(viewModel);
            }
        }

        /// <summary>
        /// GET: RecruitersApplication/UpdateApplicationStatus
        /// Form cập nhật trạng thái đơn ứng tuyển
        /// </summary>
        [HttpGet]
        public ActionResult UpdateApplicationStatus(int? id)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để cập nhật trạng thái.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn ứng tuyển.";
                return RedirectToAction("MyApplications", "RecruitersApplication");
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {

                // Load related entities
                var loadOptions = new System.Data.Linq.DataLoadOptions();
                loadOptions.LoadWith<Application>(a => a.JobPost);
                loadOptions.LoadWith<Application>(a => a.Candidate);
                db.LoadOptions = loadOptions;

                // Get application and verify it belongs to this recruiter's job
                var application = db.Applications.FirstOrDefault(a => a.ApplicationID == id.Value);
                if (application == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn ứng tuyển.";
                    return RedirectToAction("MyApplications", "RecruitersApplication");
                }

                var job = application.JobPost;
                if (job == null || job.RecruiterID != recruiterId.Value)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền cập nhật đơn ứng tuyển này.";
                    return RedirectToAction("MyApplications", "RecruitersApplication");
                }

                // Map to ViewModel với xử lý cẩn thận để tránh hiển thị N/A
                var viewModel = new UpdateApplicationStatusViewModel
                {
                    ApplicationID = application.ApplicationID,
                    JobPostID = application.JobPostID,
                    CandidateName = !string.IsNullOrEmpty(application.Candidate?.FullName) ? application.Candidate.FullName : "Ứng viên ẩn danh",
                    JobTitle = !string.IsNullOrEmpty(job.Title) ? job.Title : "Công việc không có tiêu đề",
                    Status = application.Status ?? "Under review",
                    Note = application.Note
                };

                ViewBag.StatusOptions = new SelectList(new[] {
                    new { Value = "Under review", Text = "Đang xem xét" },
                    new { Value = "Interview", Text = "Phỏng vấn" },
                    new { Value = "Offered", Text = "Đã đề xuất" },
                    new { Value = "Hired", Text = "Đã tuyển" },
                    new { Value = "Rejected", Text = "Đã từ chối" }
                }, "Value", "Text", viewModel.Status);

                return View(viewModel);
            }
        }

        /// <summary>
        /// POST: RecruitersApplication/UpdateApplicationStatus
        /// Cập nhật trạng thái đơn ứng tuyển
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateApplicationStatus(UpdateApplicationStatusViewModel viewModel)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để cập nhật trạng thái.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.StatusOptions = new SelectList(new[] {
                    new { Value = "Under review", Text = "Đang xem xét" },
                    new { Value = "Interview", Text = "Phỏng vấn" },
                    new { Value = "Offered", Text = "Đã đề xuất" },
                    new { Value = "Hired", Text = "Đã tuyển" },
                    new { Value = "Rejected", Text = "Đã từ chối" }
                }, "Value", "Text", viewModel.Status);
                return View(viewModel);
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {
                // Get application and verify it belongs to this recruiter's job
                var application = db.Applications.FirstOrDefault(a => a.ApplicationID == viewModel.ApplicationID);
                if (application == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn ứng tuyển.";
                    return RedirectToAction("MyApplications", "RecruitersApplication");
                }

                var job = application.JobPost;
                if (job == null || job.RecruiterID != recruiterId.Value)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền cập nhật đơn ứng tuyển này.";
                    return RedirectToAction("MyApplications", "RecruitersApplication");
                }

                // Validate status
                var validStatuses = new[] { "Under review", "Interview", "Offered", "Hired", "Rejected" };
                if (!validStatuses.Contains(viewModel.Status))
                {
                    ModelState.AddModelError("Status", "Trạng thái không hợp lệ.");
                    ViewBag.StatusOptions = new SelectList(new[] {
                        new { Value = "Under review", Text = "Đang xem xét" },
                        new { Value = "Interview", Text = "Phỏng vấn" },
                        new { Value = "Offered", Text = "Đã đề xuất" },
                        new { Value = "Hired", Text = "Đã tuyển" },
                        new { Value = "Rejected", Text = "Đã từ chối" }
                    }, "Value", "Text", viewModel.Status);
                    return View(viewModel);
                }

                // Update application
                application.Status = viewModel.Status;
                application.Note = viewModel.Note;
                application.UpdatedAt = DateTime.Now;

                db.SubmitChanges();

                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn ứng tuyển thành '{GetApplicationStatusDisplay(viewModel.Status)}'.";
                return RedirectToAction("ApplicationDetails", "RecruitersApplication", new { id = viewModel.ApplicationID });
            }
        }

        /// <summary>
        /// Get display text for application status
        /// </summary>
        private string GetApplicationStatusDisplay(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "Đang xem xét";

            switch (status.ToLower())
            {
                case "under review":
                    return "Đang xem xét";
                case "interview":
                    return "Phỏng vấn";
                case "offered":
                    return "Đã đề xuất";
                case "hired":
                    return "Đã tuyển";
                case "rejected":
                    return "Đã từ chối";
                default:
                    return status;
            }
        }

        /// <summary>
        /// Get badge class for application status
        /// </summary>
        private string GetApplicationStatusBadgeClass(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "badge-warning";

            switch (status.ToLower())
            {
                case "under review":
                    return "badge-warning";
                case "interview":
                    return "badge-info";
                case "offered":
                    return "badge-primary";
                case "hired":
                    return "badge-success";
                case "rejected":
                    return "badge-danger";
                default:
                    return "badge-secondary";
            }
        }
    }
}


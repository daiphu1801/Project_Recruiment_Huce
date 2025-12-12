using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Mappers;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Jobs;
using Project_Recruiment_Huce.Repositories;
using Project_Recruiment_Huce.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlTypes;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using IJobService = Project_Recruiment_Huce.Services.JobService.IJobService;
using LegacyJobRepository = Project_Recruiment_Huce.Repositories.JobRepository;
using NewJobRepository = Project_Recruiment_Huce.Repositories.JobRepo.JobRepository;
using NewJobService = Project_Recruiment_Huce.Services.JobService.JobService;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller xử lý danh sách tin tuyển dụng, chi tiết, tạo tin, lưu tin, ứng tuyển
    /// Sử dụng Service Layer và Repository Pattern cho kiến trúc tốt hơn
    /// </summary>
    public class JobsController : BaseController
    {
        private readonly IJobService _jobService;

        public JobsController()
        {
            var repository = new NewJobRepository();
            _jobService = new NewJobService(repository);
        }

        /// <summary>
        /// Tạo JobService instance với database context đã cho (legacy method for existing code)
        /// </summary>
        private LegacyJobService GetJobService(JOBPORTAL_ENDataContext db)
        {
            var jobRepository = new LegacyJobRepository(db);
            return new LegacyJobService(jobRepository, db);
        }

        /// <summary>
        /// Hiển thị chi tiết tin tuyển dụng và tăng view count
        /// </summary>
        public ActionResult JobDetails(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("JobsListing");
            }

            // Increment view count (use writable context for this single operation)
            using (var writeDb = DbContextFactory.Create())
            {
                var analyticsService = new LegacyRecruiterAnalyticsService(writeDb);
                analyticsService.IncrementViewCount(id.Value);
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {
                // Set LoadOptions BEFORE creating repository/service to avoid "LoadOptions already set" error
                var loadOptions = new System.Data.Linq.DataLoadOptions();
                loadOptions.LoadWith<JobPost>(j => j.Company);
                loadOptions.LoadWith<JobPost>(j => j.Recruiter);
                loadOptions.LoadWith<JobPost>(j => j.JobPostDetails);
                db.LoadOptions = loadOptions;

                var jobService = GetJobService(db);
                var jobRepository = new JobRepository(db);

                // Get job entity first (with LoadOptions already set above)
                var job = jobRepository.GetByIdWithDetails(id.Value);
                if (job == null)
                {
                    return RedirectToAction("JobsListing");
                }

                // Get job details view model using mapper (reuse job entity)
                JobStatusHelper.NormalizeStatuses(db);
                var jobDetailsViewModel = JobMapper.MapToDetails(job);

                // Get related jobs
                var relatedJobsViewModels = jobService.GetRelatedJobs(id.Value, job.CompanyID ?? 0, job.Location);

                bool isAuthenticated = User.Identity.IsAuthenticated;
                bool isCandidate = false;
                bool isJobSaved = false;
                bool hasApplied = false;

                if (isAuthenticated)
                {
                    var identity = (ClaimsIdentity)User.Identity;
                    var roleClaim = identity.FindFirst(ClaimTypes.Role);
                    if (roleClaim != null && roleClaim.Value == "Candidate")
                    {
                        isCandidate = true;
                        var accountIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                        if (accountIdClaim != null && int.TryParse(accountIdClaim.Value, out int accountId))
                        {
                            var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId);
                            if (candidate != null)
                            {
                                isJobSaved = jobService.IsJobSavedByCandidate(id.Value, candidate.CandidateID);
                                hasApplied = jobService.HasCandidateApplied(id.Value, candidate.CandidateID);
                            }
                        }
                    }
                }

                bool isJobOpen = jobService.IsJobOpen(job);
                bool isExpired = job.ApplicationDeadline.HasValue && job.ApplicationDeadline.Value < DateTime.Now;

                ViewBag.RelatedJobs = relatedJobsViewModels;
                ViewBag.IsJobSaved = isJobSaved;
                ViewBag.HasApplied = hasApplied;
                ViewBag.IsAuthenticated = isAuthenticated;
                ViewBag.IsCandidate = isCandidate;
                ViewBag.IsJobOpen = isJobOpen;
                ViewBag.IsJobExpired = isExpired;
                ViewBag.JobStatus = job.Status;

                return View("JobsDetails", jobDetailsViewModel);
            }
        }
        /// <summary>
        /// Danh sách tin tuyển dụng với bộ lọc và phân trang
        /// </summary>
        public ActionResult JobsListing(string keyword = null, string location = null, string employmentType = null, int? page = null)
        {
            using (var db = DbContextFactory.CreateReadOnly())
            {
                var jobService = GetJobService(db);

                // Map employment type từ tiếng Việt sang tiếng Anh (database format)
                string dbEmploymentType = employmentType;
                if (!string.IsNullOrWhiteSpace(employmentType))
                {
                    dbEmploymentType = EmploymentTypeHelper.GetDatabaseValue(employmentType);
                }

                // Sử dụng service để tìm kiếm jobs với pagination
                int pageNumber = page ?? 1;
                var (jobs, totalItems, totalPages) = jobService.SearchJobs(keyword, location, dbEmploymentType, pageNumber);

                ViewBag.TotalItems = totalItems;
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.Keyword = keyword;
                ViewBag.Location = location;
                ViewBag.EmploymentType = employmentType;

                return View(jobs);
            }
        }


        /// <summary>
        /// GET: Jobs/MyJobs - Xem danh sách tin tuyển dụng đã đăng
        /// </summary>
        [Authorize(Roles = "Recruiter")]
        [HttpGet]
        public ActionResult MyJobs(int? page = null)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để xem tin tuyển dụng.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var jobService = GetJobService(db);

                // Use service to get jobs by recruiter with pagination
                int pageNumber = page ?? 1;
                var (jobs, totalItems, totalPages) = jobService.GetJobsByRecruiter(recruiterId.Value, pageNumber);

                ViewBag.TotalItems = totalItems;
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;

                return View(jobs);
            }
        }

        /// <summary>
        /// GET: Jobs/JobsCreate
        /// </summary>
        [Authorize(Roles = "Recruiter")]
        [HttpGet]
        public ActionResult JobsCreate()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để đăng tin tuyển dụng. Vui lòng cập nhật hồ sơ trước.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            // Load companies for dropdown
            using (var db = DbContextFactory.CreateReadOnly())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId.Value);

                // Subscription Check
                bool canPost = false;
                string subType = recruiter?.SubscriptionType ?? "Free";

                if (subType == "Lifetime")
                {
                    canPost = true;
                }
                else if (subType == "Monthly")
                {
                    if (recruiter.SubscriptionExpiryDate.HasValue && recruiter.SubscriptionExpiryDate > DateTime.Now)
                    {
                        canPost = true;
                    }
                }

                // Check Free limit if not allowed by subscription
                if (!canPost)
                {
                    if (recruiter.FreeJobPostCount < 1)
                    {
                        canPost = true;
                    }
                }

                if (!canPost)
                {
                    TempData["ErrorMessage"] = "Bạn đã hết lượt đăng tin miễn phí. Vui lòng nâng cấp gói dịch vụ để tiếp tục đăng tin.";
                    return RedirectToAction("Index", "Subscription");
                }

                if (recruiter != null && recruiter.CompanyID.HasValue)
                {
                    ViewBag.CompanyID = recruiter.CompanyID.Value;
                }

                var companies = db.Companies.Where(c => c.ActiveFlag == 1).ToList();
                ViewBag.Companies = companies.Select(c => new SelectListItem
                {
                    Value = c.CompanyID.ToString(),
                    Text = c.CompanyName
                }).ToList();
            }

            return View("JobsCreate", new JobCreateViewModel());
        }

        /// <summary>
        /// POST: Jobs/JobsCreate
        /// </summary>
        [Authorize(Roles = "Recruiter")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JobsCreate(JobCreateViewModel viewModel)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để đăng tin tuyển dụng.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            // Kiểm tra địa chỉ Server - side trước khi xử lý logic khác
            if (!string.IsNullOrWhiteSpace(viewModel.Location))
            {
                bool isAddressReal = IsAddressValid(viewModel.Location);
                if (!isAddressReal)
                {
                    // Thêm lỗi vào ModelState. Điều này làm ModelState.IsValid thành false
                    ModelState.AddModelError("Location", "Địa chỉ không tồn tại trên bản đồ. Vui lòng kiểm tra lại.");
                }
            }

            if (!ModelState.IsValid)
            {
                LoadCompaniesForDropdown(recruiterId.Value);
                return View("JobsCreate", viewModel);
            }

            // Check subscription and increment if needed
            bool isFreePost = false;
            using (var db = DbContextFactory.Create())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId.Value);
                if (recruiter != null)
                {
                    bool canPost = false;
                    string subType = recruiter.SubscriptionType ?? "Free";

                    if (subType == "Lifetime")
                    {
                        canPost = true;
                    }
                    else if (subType == "Monthly")
                    {
                        if (recruiter.SubscriptionExpiryDate.HasValue && recruiter.SubscriptionExpiryDate > DateTime.Now)
                        {
                            canPost = true;
                        }
                    }

                    if (!canPost)
                    {
                        if (recruiter.FreeJobPostCount < 1)
                        {
                            canPost = true;
                            isFreePost = true;
                        }
                    }

                    if (!canPost)
                    {
                        TempData["ErrorMessage"] = "Bạn đã hết lượt đăng tin miễn phí. Vui lòng nâng cấp gói dịch vụ.";
                        return RedirectToAction("Index", "Subscription");
                    }
                }
            }

            // Call service to create job
            var result = _jobService.ValidateAndCreateJob(viewModel, recruiterId.Value);

            if (!result.Success)
            {
                // Handle validation errors
                if (result.Errors.Count > 0)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }
                }
                else if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                }

                LoadCompaniesForDropdown(recruiterId.Value);
                return View("JobsCreate", viewModel);
            }

            // Increment free count if success
            if (isFreePost)
            {
                using (var db = DbContextFactory.Create())
                {
                    var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId.Value);
                    if (recruiter != null)
                    {
                        recruiter.FreeJobPostCount++;
                        db.SubmitChanges();
                    }
                }
            }

            TempData["SuccessMessage"] = "Đăng tin tuyển dụng thành công!";
            return RedirectToAction("MyJobs", "Jobs");
        }

        /// <summary>
        /// GET: Jobs/Edit
        /// Hiển thị form chỉnh sửa tin tuyển dụng
        /// </summary>
        [Authorize(Roles = "Recruiter")]
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để chỉnh sửa tin tuyển dụng.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng.";
                return RedirectToAction("MyJobs", "Jobs");
            }

            // Call service to get job for edit
            var result = _jobService.GetJobForEdit(id.Value, recruiterId.Value);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("MyJobs", "Jobs");
            }

            ViewBag.Companies = result.Companies;
            ViewBag.JobPostID = result.JobPostId;

            return View(result.ViewModel);
        }

        /// <summary>
        /// POST: Jobs/Edit
        /// Cập nhật tin tuyển dụng
        /// </summary>
        [Authorize(Roles = "Recruiter")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, JobCreateViewModel viewModel)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để chỉnh sửa tin tuyển dụng.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng.";
                return RedirectToAction("MyJobs", "Jobs");
            }

            // Kiểm tra địa chỉ Server - side
            if (!string.IsNullOrWhiteSpace(viewModel.Location))
            {
                bool isAddressReal = IsAddressValid(viewModel.Location);
                if (!isAddressReal)
                {
                    ModelState.AddModelError("Location", "Địa chỉ không tồn tại trên bản đồ. Vui lòng kiểm tra lại.");
                }
            }

            if (!ModelState.IsValid)
            {
                LoadCompaniesForDropdown(recruiterId.Value);
                ViewBag.JobPostID = id.Value;
                return View(viewModel);
            }

            // Call service to update job
            var result = _jobService.ValidateAndUpdateJob(id.Value, viewModel, recruiterId.Value);

            if (!result.Success)
            {
                // Handle validation errors
                if (result.Errors.Count > 0)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }
                }
                else if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                }

                LoadCompaniesForDropdown(recruiterId.Value);
                ViewBag.JobPostID = id.Value;
                return View(viewModel);
            }

            TempData["SuccessMessage"] = "Cập nhật tin tuyển dụng thành công!";
            return RedirectToAction("MyJobs", "Jobs");
        }

        /// <summary>
        /// POST: Jobs/CloseJob
        /// Đóng hoặc hết hạn tin tuyển dụng
        /// </summary>
        [Authorize(Roles = "Recruiter")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CloseJob(int? id, string status = "Closed")
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để thực hiện thao tác này.";
                return RedirectToAction("MyJobs", "Jobs");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng.";
                return RedirectToAction("MyJobs", "Jobs");
            }

            // Call service to close job
            var result = _jobService.CloseJob(id.Value, recruiterId.Value, status ?? "Closed");

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("MyJobs", "Jobs");
            }

            string statusText = status == "Closed" ? "đóng" : "hết hạn";
            TempData["SuccessMessage"] = $"Đã {statusText} tin tuyển dụng thành công!";
            return RedirectToAction("MyJobs", "Jobs");
        }

        /// <summary>
        /// POST: Jobs/ReopenJob
        /// Mở lại tin tuyển dụng đã đóng
        /// </summary>
        [Authorize(Roles = "Recruiter")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReopenJob(int? id)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để thực hiện thao tác này.";
                return RedirectToAction("MyJobs", "Jobs");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng.";
                return RedirectToAction("MyJobs", "Jobs");
            }

            // Call service to reopen job
            var result = _jobService.ReopenJob(id.Value, recruiterId.Value);

            if (!result.Success)
            {
                if (!string.IsNullOrEmpty(result.WarningMessage))
                {
                    TempData["WarningMessage"] = result.WarningMessage;
                    if (!string.IsNullOrEmpty(result.RedirectAction))
                    {
                        return RedirectToAction(result.RedirectAction, "Jobs", result.RedirectRouteValues);
                    }
                }
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                }
                return RedirectToAction("MyJobs", "Jobs");
            }

            TempData["SuccessMessage"] = "Đã mở lại tin tuyển dụng thành công!";
            return RedirectToAction("MyJobs", "Jobs");
        }

        #region Helper Methods

        /// <summary>
        /// Load companies for dropdown helper method
        /// </summary>
        private void LoadCompaniesForDropdown(int recruiterId)
        {
            using (var db = DbContextFactory.CreateReadOnly())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId);
                if (recruiter != null && recruiter.CompanyID.HasValue)
                {
                    ViewBag.CompanyID = recruiter.CompanyID.Value;
                }

                var companies = db.Companies.Where(c => c.ActiveFlag == 1).ToList();
                ViewBag.Companies = companies.Select(c => new SelectListItem
                {
                    Value = c.CompanyID.ToString(),
                    Text = c.CompanyName
                }).ToList();
            }
        }

        #endregion
        /// <summary>
        /// Hàm kiểm tra địa chỉ thông minh (Vòng lặp đệ quy)
        /// Chấp nhận nếu tìm thấy Phường/Quận/Thành phố trong chuỗi địa chỉ
        /// </summary>
        private bool IsAddressValid(string address)
        {
            if (string.IsNullOrWhiteSpace(address) || address.Length < 5) return false;

            bool CallNominatimApi(string query)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("JobBoardProject/1.0");
                        client.Timeout = TimeSpan.FromSeconds(3);

                        var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";
                        var response = client.GetStringAsync(url).Result;

                        return !string.IsNullOrEmpty(response) && response != "[]";
                    }
                }
                catch
                {
                    return true;
                }
            }

            if (CallNominatimApi(address)) return true;

            var parts = address.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(p => p.Trim())
                               .ToList();

            while (parts.Count >= 3)
            {
                parts.RemoveAt(0);
                string shorterAddress = string.Join(", ", parts);

                if (CallNominatimApi(shorterAddress))
                {
                    return true;
                }
            }
            return false;
        }
    }
}



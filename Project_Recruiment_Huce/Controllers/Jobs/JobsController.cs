using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Jobs;
using Project_Recruiment_Huce.Repositories;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Mappers;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Services;

namespace Project_Recruiment_Huce.Controllers
{
    public class JobsController : BaseController
    {
        // Using Service Layer and Repository Pattern for better architecture
        private JobService GetJobService(JOBPORTAL_ENDataContext db)
        {
            var jobRepository = new JobRepository(db);
            return new JobService(jobRepository, db);
        }


        public ActionResult JobDetails(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("JobsListing");
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var jobService = GetJobService(db);
                var jobRepository = new JobRepository(db);

                // Get job details using service
                var job = jobRepository.GetByIdWithDetails(id.Value);
                if (job == null)
                {
                    return RedirectToAction("JobsListing");
                }

                var jobDetailsViewModel = jobService.GetJobDetails(id.Value);
                var relatedJobsViewModels = jobService.GetRelatedJobs(id.Value, job.CompanyID ?? 0, job.Location);

                bool isAuthenticated = User.Identity.IsAuthenticated;
                bool isCandidate = false;
                bool isJobSaved = false;
                bool hasApplied = false;

                if (isAuthenticated)
                {
                    var identity = (ClaimsIdentity)User.Identity;
                    var roleClaim = identity.FindFirst("VaiTro");
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
        public ActionResult JobsListing(string keyword = null, string location = null, string employmentType = null, int? page = null)
        {
            using (var db = DbContextFactory.CreateReadOnly())
            {
                var jobService = GetJobService(db);

                // Map employment type from Vietnamese to English
                string dbEmploymentType = employmentType;
                if (!string.IsNullOrWhiteSpace(employmentType))
                {
                    dbEmploymentType = EmploymentTypeHelper.GetDatabaseValue(employmentType);
                }

                // Use service to search jobs with pagination
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
        [Authorize]
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
        [Authorize]
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

            // Load companies for dropdown if needed
            using (var db = DbContextFactory.CreateReadOnly())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId.Value);
                if (recruiter != null && recruiter.CompanyID.HasValue)
                {
                    ViewBag.CompanyID = recruiter.CompanyID.Value;
                }

                // Load list of companies for dropdown (optional)
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
        [Authorize]
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

            if (!ModelState.IsValid)
            {
                // Reload companies for dropdown
                using (var db = DbContextFactory.Create())
                {
                    var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId.Value);
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
                return View("JobsCreate", viewModel);
            }

            using (var db = DbContextFactory.Create())
            {
                // Get recruiter to get CompanyID
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId.Value);
                if (recruiter == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin Recruiter.";
                    return RedirectToAction("RecruitersManage", "Recruiters");
                }

                // Generate JobCode if not provided
                string jobCode = viewModel.JobCode;
                if (string.IsNullOrWhiteSpace(jobCode))
                {
                    var lastJob = db.JobPosts.OrderByDescending(j => j.JobPostID).FirstOrDefault();
                    int nextNumber = 1;
                    if (lastJob != null && !string.IsNullOrEmpty(lastJob.JobCode))
                    {
                        // Try to extract number from JobCode (format: JOB#### or similar)
                        string code = lastJob.JobCode.ToUpper();
                        if (code.StartsWith("JOB"))
                        {
                            string numStr = code.Substring(3);
                            if (int.TryParse(numStr, out int lastNum))
                            {
                                nextNumber = lastNum + 1;
                            }
                        }
                        else
                        {
                            // If format is different, just use JobPostID + 1
                            nextNumber = lastJob.JobPostID + 1;
                        }
                    }
                    jobCode = $"JOB{nextNumber:D4}";
                }
                
                // Validate SalaryTo >= SalaryFrom
                if (viewModel.SalaryFrom.HasValue && viewModel.SalaryTo.HasValue && viewModel.SalaryTo < viewModel.SalaryFrom)
                {
                    ModelState.AddModelError("SalaryTo", "Lương đến phải lớn hơn hoặc bằng lương từ");
                    // Reload companies for dropdown (recruiter already loaded above)
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
                    return View("JobsCreate", viewModel);
                }

                // Validate AgeTo >= AgeFrom
                if (viewModel.AgeFrom.HasValue && viewModel.AgeTo.HasValue && viewModel.AgeTo < viewModel.AgeFrom)
                {
                    ModelState.AddModelError("AgeTo", "Độ tuổi đến phải lớn hơn hoặc bằng độ tuổi từ");
                    // Reload companies for dropdown
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
                    return View("JobsCreate", viewModel);
                }

                // Sanitize HTML content
                // Debug: Check if Description is received
                string rawDescription = viewModel.Description;
                string sanitizedDescription = null;
                
                if (!string.IsNullOrWhiteSpace(rawDescription))
                {
                    // Trim whitespace first
                    rawDescription = rawDescription.Trim();
                    
                    // Only sanitize if not empty after trim
                    if (!string.IsNullOrEmpty(rawDescription))
                    {
                        sanitizedDescription = HtmlSanitizerHelper.Sanitize(rawDescription);
                        
                        // If sanitization results in empty or only whitespace, use original
                        if (string.IsNullOrWhiteSpace(sanitizedDescription))
                        {
                            sanitizedDescription = rawDescription;
                        }
                    }
                }
                
                string rawRequirements = viewModel.Requirements;
                string sanitizedRequirements = null;
                
                if (!string.IsNullOrWhiteSpace(rawRequirements))
                {
                    rawRequirements = rawRequirements.Trim();
                    if (!string.IsNullOrEmpty(rawRequirements))
                    {
                        sanitizedRequirements = HtmlSanitizerHelper.Sanitize(rawRequirements);
                        if (string.IsNullOrWhiteSpace(sanitizedRequirements))
                        {
                            sanitizedRequirements = rawRequirements;
                        }
                    }
                }

                // Convert employment type from Vietnamese to database format
                string employmentType = viewModel.EmploymentType;
                if (!string.IsNullOrWhiteSpace(employmentType))
                {
                    if (employmentType == "Bán thời gian")
                        employmentType = "Part-time";
                    else if (employmentType == "Toàn thời gian")
                        employmentType = "Full-time";
                }

                // Create new JobPost
                var jobPost = new JobPost
                {
                    JobCode = jobCode,
                    RecruiterID = recruiterId.Value,
                    CompanyID = recruiter.CompanyID,
                    Title = viewModel.Title,
                    Description = sanitizedDescription,
                    Requirements = sanitizedRequirements,
                    SalaryFrom = viewModel.SalaryFrom,
                    SalaryTo = viewModel.SalaryTo,
                    SalaryCurrency = viewModel.SalaryCurrency ?? "VND",
                    Location = viewModel.Location,
                    EmploymentType = employmentType,
                    ApplicationDeadline = viewModel.ApplicationDeadline,
                    Status = "Published",
                    PostedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.JobPosts.InsertOnSubmit(jobPost);
                db.SubmitChanges();

                // Create JobPostDetail
                string sanitizedSkills = string.IsNullOrWhiteSpace(viewModel.Skills)
                    ? null
                    : HtmlSanitizerHelper.Sanitize(viewModel.Skills);

                var jobPostDetail = new JobPostDetail
                {
                    JobPostID = jobPost.JobPostID,
                    Industry = viewModel.Industry ?? "Khác",
                    Major = viewModel.Major,
                    YearsExperience = viewModel.YearsExperience ?? 0,
                    DegreeRequired = viewModel.DegreeRequired,
                    Skills = sanitizedSkills,
                    Headcount = viewModel.Headcount ?? 1,
                    GenderRequirement = viewModel.GenderRequirement ?? "Not required",
                    AgeFrom = viewModel.AgeFrom,
                    AgeTo = viewModel.AgeTo,
                    Status = "Published"
                };

                db.JobPostDetails.InsertOnSubmit(jobPostDetail);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Đăng tin tuyển dụng thành công!";
                return RedirectToAction("MyJobs", "Jobs");
            }
        }

        /// <summary>
        /// GET: Jobs/Edit
        /// Hiển thị form chỉnh sửa tin tuyển dụng
        /// </summary>
        [Authorize]
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

            using (var db = DbContextFactory.CreateReadOnly())
            {
                db.ObjectTrackingEnabled = false;

                // Load job and verify it belongs to this recruiter
                var job = db.JobPosts.FirstOrDefault(j => j.JobPostID == id.Value && j.RecruiterID == recruiterId.Value);
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền chỉnh sửa tin này.";
                    return RedirectToAction("MyJobs", "Jobs");
                }

                // Load job details
                var jobDetail = db.JobPostDetails.FirstOrDefault(jd => jd.JobPostID == job.JobPostID);

                // Convert employment type from database format to Vietnamese
                string employmentType = job.EmploymentType;
                if (!string.IsNullOrWhiteSpace(employmentType))
                {
                    if (employmentType == "Part-time")
                        employmentType = "Bán thời gian";
                    else if (employmentType == "Full-time")
                        employmentType = "Toàn thời gian";
                }

                // Map to ViewModel
                var viewModel = new JobCreateViewModel
                {
                    Title = job.Title,
                    Description = job.Description,
                    Requirements = job.Requirements,
                    SalaryFrom = job.SalaryFrom,
                    SalaryTo = job.SalaryTo,
                    SalaryCurrency = job.SalaryCurrency ?? "VND",
                    Location = job.Location,
                    EmploymentType = employmentType,
                    ApplicationDeadline = job.ApplicationDeadline,
                    JobCode = job.JobCode,
                    CompanyID = job.CompanyID,
                    Industry = jobDetail?.Industry,
                    Major = jobDetail?.Major,
                    YearsExperience = jobDetail?.YearsExperience,
                    DegreeRequired = jobDetail?.DegreeRequired,
                    Skills = jobDetail?.Skills,
                    Headcount = jobDetail?.Headcount ?? 1,
                    GenderRequirement = jobDetail?.GenderRequirement ?? "Not required",
                    AgeFrom = jobDetail?.AgeFrom,
                    AgeTo = jobDetail?.AgeTo
                };

                // Load companies for dropdown
                var companies = db.Companies.Where(c => c.ActiveFlag == 1).ToList();
                ViewBag.Companies = companies.Select(c => new SelectListItem
                {
                    Value = c.CompanyID.ToString(),
                    Text = c.CompanyName,
                    Selected = c.CompanyID == job.CompanyID
                }).ToList();

                ViewBag.JobPostID = job.JobPostID;

                return View(viewModel);
            }
        }

        /// <summary>
        /// POST: Jobs/Edit
        /// Cập nhật tin tuyển dụng
        /// </summary>
        [Authorize]
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

            if (!ModelState.IsValid)
            {
                // Reload companies for dropdown
                using (var db = DbContextFactory.Create())
                {
                    var companies = db.Companies.Where(c => c.ActiveFlag == 1).ToList();
                    ViewBag.Companies = companies.Select(c => new SelectListItem
                    {
                        Value = c.CompanyID.ToString(),
                        Text = c.CompanyName
                    }).ToList();
                }
                ViewBag.JobPostID = id.Value;
                return View(viewModel);
            }

            using (var db = DbContextFactory.Create())
            {
                // Get job and verify it belongs to this recruiter
                var job = db.JobPosts.FirstOrDefault(j => j.JobPostID == id.Value && j.RecruiterID == recruiterId.Value);
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền chỉnh sửa tin này.";
                    return RedirectToAction("MyJobs", "Jobs");
                }

                // Validate SalaryTo >= SalaryFrom
                if (viewModel.SalaryFrom.HasValue && viewModel.SalaryTo.HasValue && viewModel.SalaryTo < viewModel.SalaryFrom)
                {
                    ModelState.AddModelError("SalaryTo", "Lương đến phải lớn hơn hoặc bằng lương từ");
                    var companies = db.Companies.Where(c => c.ActiveFlag == 1).ToList();
                    ViewBag.Companies = companies.Select(c => new SelectListItem
                    {
                        Value = c.CompanyID.ToString(),
                        Text = c.CompanyName
                    }).ToList();
                    ViewBag.JobPostID = id.Value;
                    return View(viewModel);
                }

                // Validate AgeTo >= AgeFrom
                if (viewModel.AgeFrom.HasValue && viewModel.AgeTo.HasValue && viewModel.AgeTo < viewModel.AgeFrom)
                {
                    ModelState.AddModelError("AgeTo", "Độ tuổi đến phải lớn hơn hoặc bằng độ tuổi từ");
                    var companies = db.Companies.Where(c => c.ActiveFlag == 1).ToList();
                    ViewBag.Companies = companies.Select(c => new SelectListItem
                    {
                        Value = c.CompanyID.ToString(),
                        Text = c.CompanyName
                    }).ToList();
                    ViewBag.JobPostID = id.Value;
                    return View(viewModel);
                }

                // Sanitize HTML content
                string sanitizedDescription = null;
                if (!string.IsNullOrWhiteSpace(viewModel.Description))
                {
                    string rawDescription = viewModel.Description.Trim();
                    if (!string.IsNullOrEmpty(rawDescription))
                    {
                        sanitizedDescription = HtmlSanitizerHelper.Sanitize(rawDescription);
                        if (string.IsNullOrWhiteSpace(sanitizedDescription))
                        {
                            sanitizedDescription = rawDescription;
                        }
                    }
                }

                string sanitizedRequirements = null;
                if (!string.IsNullOrWhiteSpace(viewModel.Requirements))
                {
                    string rawRequirements = viewModel.Requirements.Trim();
                    if (!string.IsNullOrEmpty(rawRequirements))
                    {
                        sanitizedRequirements = HtmlSanitizerHelper.Sanitize(rawRequirements);
                        if (string.IsNullOrWhiteSpace(sanitizedRequirements))
                        {
                            sanitizedRequirements = rawRequirements;
                        }
                    }
                }

                // Convert employment type from Vietnamese to database format
                string employmentType = viewModel.EmploymentType;
                if (!string.IsNullOrWhiteSpace(employmentType))
                {
                    if (employmentType == "Bán thời gian")
                        employmentType = "Part-time";
                    else if (employmentType == "Toàn thời gian")
                        employmentType = "Full-time";
                }

                // Update JobPost
                job.Title = viewModel.Title;
                job.Description = sanitizedDescription;
                job.Requirements = sanitizedRequirements;
                job.SalaryFrom = viewModel.SalaryFrom;
                job.SalaryTo = viewModel.SalaryTo;
                job.SalaryCurrency = viewModel.SalaryCurrency ?? "VND";
                job.Location = viewModel.Location;
                job.EmploymentType = employmentType;
                job.ApplicationDeadline = viewModel.ApplicationDeadline;
                job.UpdatedAt = DateTime.Now;
                // PostedAt giờ là non-nullable, luôn có giá trị, không cần check HasValue

                db.SubmitChanges();

                // Update or create JobPostDetail
                var jobDetail = db.JobPostDetails.FirstOrDefault(jd => jd.JobPostID == job.JobPostID);
                if (jobDetail == null)
                {
                    jobDetail = new JobPostDetail
                    {
                        JobPostID = job.JobPostID,
                        Status = job.Status ?? "Published"
                    };
                    db.JobPostDetails.InsertOnSubmit(jobDetail);
                }

                string sanitizedSkills = string.IsNullOrWhiteSpace(viewModel.Skills)
                    ? null
                    : HtmlSanitizerHelper.Sanitize(viewModel.Skills);

                jobDetail.Industry = viewModel.Industry ?? "Khác";
                jobDetail.Major = viewModel.Major;
                jobDetail.YearsExperience = viewModel.YearsExperience ?? 0;
                jobDetail.DegreeRequired = viewModel.DegreeRequired;
                jobDetail.Skills = sanitizedSkills;
                jobDetail.Headcount = viewModel.Headcount ?? 1;
                jobDetail.GenderRequirement = viewModel.GenderRequirement ?? "Not required";
                jobDetail.AgeFrom = viewModel.AgeFrom;
                jobDetail.AgeTo = viewModel.AgeTo;

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật tin tuyển dụng thành công!";
                return RedirectToAction("MyJobs", "Jobs");
            }
        }

        /// <summary>
        /// POST: Jobs/CloseJob
        /// Đóng hoặc hết hạn tin tuyển dụng
        /// </summary>
        [Authorize]
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

            // Validate status values
            if (status != "Closed" && status != "Expired")
            {
                status = "Closed";
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {
                // Get job and verify it belongs to this recruiter
                var job = db.JobPosts.FirstOrDefault(j => j.JobPostID == id.Value && j.RecruiterID == recruiterId.Value);
                
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền thực hiện thao tác này.";
                    return RedirectToAction("MyJobs", "Jobs");
                }

                // Kiểm tra xem có hồ sơ ứng viên đang trong quá trình xử lý không
                var pendingStatuses = new[] { "Under review", "Interview", "Offered" };
                var pendingApplications = db.Applications
                    .Where(a => a.JobPostID == id.Value && 
                               a.Status != null && 
                               pendingStatuses.Contains(a.Status))
                    .Count();

                if (pendingApplications > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể đóng tin tuyển dụng này. Vui lòng xử lý hết {pendingApplications} hồ sơ ứng viên đang trong quá trình xử lý (Đang xem xét, Phỏng vấn, hoặc Đã đề xuất) trước khi đóng tin.";
                    return RedirectToAction("MyJobs", "Jobs");
                }

                // Update status
                job.Status = status;
                job.UpdatedAt = DateTime.Now;
                
                db.SubmitChanges();

                string statusText = status == "Closed" ? "đóng" : "hết hạn";
                TempData["SuccessMessage"] = $"Đã {statusText} tin tuyển dụng thành công!";
                return RedirectToAction("MyJobs", "Jobs");
            }
        }

        /// <summary>
        /// POST: Jobs/ReopenJob
        /// Mở lại tin tuyển dụng đã đóng
        /// </summary>
        [Authorize]
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

            using (var db = DbContextFactory.CreateReadOnly())
            {
                // Get job and verify it belongs to this recruiter
                var job = db.JobPosts.FirstOrDefault(j => j.JobPostID == id.Value && j.RecruiterID == recruiterId.Value);
                
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền thực hiện thao tác này.";
                    return RedirectToAction("MyJobs", "Jobs");
                }

                // Chỉ cho phép mở lại tin có status "Closed", không cho phép mở lại "Expired"
                if (job.Status != "Closed")
                {
                    if (job.Status == "Expired")
                    {
                        TempData["ErrorMessage"] = "Không thể mở lại tin đã hết hạn. Vui lòng tạo tin tuyển dụng mới hoặc cập nhật hạn nộp trước khi mở lại.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Chỉ có thể mở lại tin tuyển dụng đã đóng.";
                    }
                    return RedirectToAction("MyJobs", "Jobs");
                }

                // Kiểm tra ApplicationDeadline - nếu đã quá hạn, cần cảnh báo
                bool isDeadlinePassed = job.ApplicationDeadline.HasValue && 
                                       job.ApplicationDeadline.Value < DateTime.Now.Date;

                if (isDeadlinePassed)
                {
                    // Vẫn cho phép mở lại nhưng cảnh báo cần cập nhật deadline
                    // Có thể tự động cập nhật deadline thêm 30 ngày hoặc yêu cầu user cập nhật
                    // Ở đây ta sẽ cảnh báo và yêu cầu user cập nhật deadline
                    TempData["WarningMessage"] = "Hạn nộp hồ sơ đã qua. Vui lòng cập nhật hạn nộp mới trước khi mở lại tin tuyển dụng.";
                    // Redirect đến trang edit để cập nhật deadline
                    return RedirectToAction("Edit", "JobsCreate", new { id = job.JobPostID });
                }

                // Update status to Published
                job.Status = "Published";
                job.UpdatedAt = DateTime.Now;
                
                // PostedAt giờ là non-nullable, luôn có giá trị, không cần check HasValue
                
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Đã mở lại tin tuyển dụng thành công!";
                return RedirectToAction("MyJobs", "Jobs");
            }
        }

    }
}



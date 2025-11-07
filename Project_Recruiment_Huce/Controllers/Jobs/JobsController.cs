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

namespace Project_Recruiment_Huce.Controllers
{
    public class JobsController : Controller
    {
        private int? GetCurrentAccountId()
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
                return null;

            var idClaim = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) return null;
            int accountId;
            return int.TryParse(idClaim.Value, out accountId) ? (int?)accountId : null;
        }

        private int? GetCurrentRecruiterId(int? accountId)
        {
            if (!accountId.HasValue)
                return null;

            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                return recruiter?.RecruiterID;
            }
        }

        /// <summary>
        /// Map JobPost entity to JobListingItemViewModel
        /// </summary>
        private JobListingItemViewModel MapToJobListingItem(JobPost job, JOBPORTAL_ENDataContext db = null)
        {
            string companyName = job.Company != null ? job.Company.CompanyName : 
                                (job.Recruiter != null ? job.Recruiter.FullName : "N/A");
            
            // Get company logo URL
            string logoUrl = "/Content/images/job_logo_1.jpg"; // Default logo
            if (job.Company?.PhotoID.HasValue == true)
            {
                var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
                using (var logoDb = new JOBPORTAL_ENDataContext(connectionString))
                {
                    var photo = logoDb.ProfilePhotos.FirstOrDefault(p => p.PhotoID == job.Company.PhotoID.Value);
                    if (photo != null && !string.IsNullOrEmpty(photo.FilePath))
                    {
                        logoUrl = photo.FilePath;
                    }
                }
            }
            
            // Calculate pending applications count if db context is provided
            int pendingCount = 0;
            if (db != null)
            {
                var pendingStatuses = new[] { "Under review", "Interview", "Offered" };
                pendingCount = db.Applications
                    .Where(a => a.JobPostID == job.JobPostID && 
                               a.Status != null && 
                               pendingStatuses.Contains(a.Status))
                    .Count();
            }
            
            return new JobListingItemViewModel
            {
                JobPostID = job.JobPostID,
                JobCode = job.JobCode,
                Title = job.Title,
                Description = job.Description,
                CompanyName = companyName,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                EmploymentTypeDisplay = GetEmploymentTypeDisplay(job.EmploymentType),
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                SalaryCurrency = job.SalaryCurrency,
                SalaryRange = FormatSalaryRange(job.SalaryFrom, job.SalaryTo, job.SalaryCurrency),
                PostedAt = job.PostedAt,
                ApplicationDeadline = job.ApplicationDeadline,
                Status = job.Status,
                LogoUrl = logoUrl,
                PendingApplicationsCount = pendingCount
            };
        }

        /// <summary>
        /// Get display text for employment type
        /// </summary>
        private string GetEmploymentTypeDisplay(string employmentType)
        {
            if (string.IsNullOrEmpty(employmentType))
                return "";

            string empType = employmentType.ToLower();
            if (empType == "part-time" || empType == "part time")
                return "Bán thời gian";
            else if (empType == "full-time" || empType == "full time")
                return "Toàn thời gian";
            else
                return employmentType;
        }

        /// <summary>
        /// Format salary range for display
        /// </summary>
        private string FormatSalaryRange(decimal? salaryFrom, decimal? salaryTo, string currency)
        {
            if (!salaryFrom.HasValue && !salaryTo.HasValue)
                return "Thỏa thuận";

            string currencyDisplay = currency == "VND" ? "VNĐ" : currency ?? "VNĐ";

            if (salaryFrom.HasValue && salaryTo.HasValue)
            {
                return $"{salaryFrom.Value:N0} - {salaryTo.Value:N0} {currencyDisplay}";
            }
            else if (salaryFrom.HasValue)
            {
                return $"Từ {salaryFrom.Value:N0} {currencyDisplay}";
            }
            else if (salaryTo.HasValue)
            {
                return $"Đến {salaryTo.Value:N0} {currencyDisplay}";
            }

            return "Thỏa thuận";
        }
        public ActionResult JobsListing(string keyword = null, string location = null, string employmentType = null, int? page = null)
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString))
            {
                db.ObjectTrackingEnabled = false;

                // Eager load related entities for better performance
                var loadOptions = new System.Data.Linq.DataLoadOptions();
                loadOptions.LoadWith<JobPost>(j => j.Company);
                loadOptions.LoadWith<JobPost>(j => j.Recruiter);
                db.LoadOptions = loadOptions;

                // Filter by Status: "Published"
                var query = db.JobPosts.Where(j => j.Status == "Published");

                // Search by keyword
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(j => j.Title.Contains(keyword) || 
                                           (j.Description != null && j.Description.Contains(keyword)) ||
                                           (j.Company != null && j.Company.CompanyName.Contains(keyword)));
                }

                // Filter by location
                if (!string.IsNullOrWhiteSpace(location) && location != "Bất kỳ đâu")
                {
                    query = query.Where(j => j.Location != null && j.Location.Contains(location));
                }

                // Filter by employment type
                if (!string.IsNullOrWhiteSpace(employmentType))
                {
                    string dbValue = employmentType == "Bán thời gian" ? "Part-time" : 
                                   employmentType == "Toàn thời gian" ? "Full-time" : employmentType;
                    query = query.Where(j => j.EmploymentType == dbValue);
                }

                // Pagination
                int pageSize = 10;
                int pageNumber = page ?? 1;
                
                // Get total count before pagination
                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Order and paginate - only load the page we need
                // Use SqlDateTime.MinValue.Value (1753-01-01) instead of DateTime.MinValue to avoid SqlDateTime overflow
                var pagedJobs = query
                    .OrderByDescending(j => j.PostedAt ?? j.UpdatedAt ?? (DateTime?)SqlDateTime.MinValue.Value)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Map to ViewModels
                var jobViewModels = pagedJobs.Select(j => MapToJobListingItem(j)).ToList();

                ViewBag.TotalItems = totalItems;
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.Keyword = keyword;
                ViewBag.Location = location;
                ViewBag.EmploymentType = employmentType;

                return View(jobViewModels);
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

            var recruiterId = GetCurrentRecruiterId(accountId);
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để xem tin tuyển dụng.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString))
            {
                db.ObjectTrackingEnabled = false;

                // Eager load related entities
                var loadOptions = new System.Data.Linq.DataLoadOptions();
                loadOptions.LoadWith<JobPost>(j => j.Company);
                loadOptions.LoadWith<JobPost>(j => j.Recruiter);
                db.LoadOptions = loadOptions;

                // Get all jobs posted by this recruiter
                var query = db.JobPosts.Where(j => j.RecruiterID == recruiterId.Value);

                // Pagination
                int pageSize = 10;
                int pageNumber = page ?? 1;

                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedJobs = query
                    .OrderByDescending(j => j.PostedAt ?? j.UpdatedAt ?? (DateTime?)SqlDateTime.MinValue.Value)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Map to ViewModels with db context for pending applications count
                var jobViewModels = pagedJobs.Select(j => MapToJobListingItem(j, db)).ToList();

                ViewBag.TotalItems = totalItems;
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;

                return View(jobViewModels);
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

            var recruiterId = GetCurrentRecruiterId(accountId);
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để đăng tin tuyển dụng. Vui lòng cập nhật hồ sơ trước.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            // Load companies for dropdown if needed
            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString))
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

            var recruiterId = GetCurrentRecruiterId(accountId);
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để đăng tin tuyển dụng.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            if (!ModelState.IsValid)
            {
                // Reload companies for dropdown
                var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
                using (var db = new JOBPORTAL_ENDataContext(connectionString))
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

            var connectionString2 = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString2))
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

            var recruiterId = GetCurrentRecruiterId(accountId);
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

            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString))
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

            var recruiterId = GetCurrentRecruiterId(accountId);
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
                var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
                using (var db = new JOBPORTAL_ENDataContext(connectionString))
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

            var connectionString2 = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString2))
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
                // Keep existing PostedAt if it exists, otherwise set to now
                if (!job.PostedAt.HasValue)
                {
                    job.PostedAt = DateTime.Now;
                }

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

            var recruiterId = GetCurrentRecruiterId(accountId);
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

            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString))
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

            var recruiterId = GetCurrentRecruiterId(accountId);
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

            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString))
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
                
                // Nếu PostedAt chưa có, set nó
                if (!job.PostedAt.HasValue)
                {
                    job.PostedAt = DateTime.Now;
                }
                
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Đã mở lại tin tuyển dụng thành công!";
                return RedirectToAction("MyJobs", "Jobs");
            }
        }

    }
}



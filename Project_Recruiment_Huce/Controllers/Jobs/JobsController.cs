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
        private JobListingItemViewModel MapToJobListingItem(JobPost job)
        {
            string companyName = job.Company != null ? job.Company.CompanyName : 
                                (job.Recruiter != null ? job.Recruiter.FullName : "N/A");
            
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
                LogoUrl = "/Content/images/job_logo_1.jpg"
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

                // Map to ViewModels
                var jobViewModels = pagedJobs.Select(j => MapToJobListingItem(j)).ToList();

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
    }
}



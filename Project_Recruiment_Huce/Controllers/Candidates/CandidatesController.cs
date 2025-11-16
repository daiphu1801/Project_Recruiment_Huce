using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Candidates;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Infrastructure;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class CandidatesController : BaseController
    {

        [HttpGet]
        public ActionResult CandidatesManage()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = DbContextFactory.Create())
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    candidate = new Candidate
                    {
                        AccountID = accountId.Value,
                        FullName = User.Identity.Name,
                        Gender = "Nam",
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    db.Candidates.InsertOnSubmit(candidate);
                    db.SubmitChanges();
                }

                // Map entity to ViewModel
                var viewModel = new CandidateManageViewModel
                {
                    CandidateID = candidate.CandidateID,
                    FullName = candidate.FullName,
                    BirthDate = candidate.BirthDate,
                    Gender = candidate.Gender,
                    Phone = candidate.Phone,
                    Email = candidate.Email,
                    Address = candidate.Address,
                    Summary = candidate.Summary
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CandidatesManage(CandidateManageViewModel viewModel, HttpPostedFileBase avatar)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Do not return early on invalid model; we still allow saving avatar

            using (var db = DbContextFactory.Create())
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    candidate = new Candidate { AccountID = accountId.Value, FullName = User.Identity.Name, Gender = "Nam", CreatedAt = DateTime.Now, ActiveFlag = 1 };
                    db.Candidates.InsertOnSubmit(candidate);
                }

                // Validate phone number format, uniqueness and normalize (if provided)
                var phone = (viewModel.Phone ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    // Validate phone format
                    if (!ValidationHelper.IsValidVietnamesePhone(phone))
                    {
                        ModelState.AddModelError("Phone", ValidationHelper.GetPhoneErrorMessage());
                    }
                    else
                    {
                        // Normalize phone number
                        phone = ValidationHelper.NormalizePhone(phone);

                        // Check phone uniqueness (exclude current account)
                        if (!ValidationHelper.IsAccountPhoneUnique(phone, accountId.Value))
                        {
                            ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng bởi tài khoản hoặc hồ sơ khác.");
                        }
                    }
                }
                else
                {
                    phone = null;
                }

                // Validate email uniqueness if changed (email in Candidate is contact email, different from Account.Email)
                var email = (viewModel.Email ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(email))
                {
                    // Validate email format
                    if (!ValidationHelper.IsValidEmail(email))
                    {
                        ModelState.AddModelError("Email", "Email không hợp lệ.");
                    }
                    // Note: Candidate.Email is contact email, not login email, so we don't check uniqueness against Account.Email
                    // But we can check if it's different from other candidates' contact emails if needed
                }

                // Check if there are validation errors before updating
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin. Có lỗi trong form.";
                    return View(viewModel);
                }

                // Update profile fields only when model is valid
                candidate.FullName = viewModel.FullName;
                candidate.BirthDate = viewModel.BirthDate;
                candidate.Gender = string.IsNullOrWhiteSpace(viewModel.Gender) ? "Nam" : viewModel.Gender;
                candidate.Phone = phone; // Use normalized phone
                candidate.Email = viewModel.Email;
                candidate.Address = viewModel.Address;
                
                // NOTE: Email trong Candidate.Email là email liên lạc, không đồng bộ với Account.Email
                
                // Sanitize HTML before saving to prevent XSS attacks
                // [AllowHtml] allows the HTML to be posted, but we still sanitize it
                if (!string.IsNullOrEmpty(viewModel.Summary))
                {
                    // Sanitize HTML to remove dangerous tags and attributes
                    string sanitizedHtml = HtmlSanitizerHelper.Sanitize(viewModel.Summary);
                    
                    // Limit to 500 characters (Summary field constraint)
                    if (sanitizedHtml.Length > 500)
                    {
                        candidate.Summary = sanitizedHtml.Substring(0, 500);
                    }
                    else
                    {
                        candidate.Summary = sanitizedHtml;
                    }
                }

                // Handle avatar upload
                if (avatar != null && avatar.ContentLength > 0)
                {
                    var contentType = (avatar.ContentType ?? string.Empty).ToLowerInvariant();
                    var allowed = new[] { "image/jpeg", "image/jpg", "image/pjpeg", "image/png", "image/x-png", "image/gif", "image/webp" };
                    const int maxBytes = 2 * 1024 * 1024; // 2MB
                    if (avatar.ContentLength > maxBytes)
                    {
                        ModelState.AddModelError("", "Image must be 2MB or smaller.");
                        return View(candidate);
                    }
                    if (allowed.Contains(contentType))
                    {
                        var uploadsRoot = Server.MapPath("~/Content/uploads/candidate/");
                        if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                        var ext = Path.GetExtension(avatar.FileName);
                        if (string.IsNullOrEmpty(ext))
                        {
                            // basic fallback by mime
                            ext = contentType.Contains("png") ? ".png" : contentType.Contains("gif") ? ".gif" : contentType.Contains("webp") ? ".webp" : ".jpg";
                        }
                        var safeFileName = $"avatar_{accountId.Value}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
                        var physicalPath = Path.Combine(uploadsRoot, safeFileName);
                        avatar.SaveAs(physicalPath);

                        var relativePath = $"~/Content/uploads/candidate/{safeFileName}";
                        var photo = new ProfilePhoto
                        {
                            FileName = safeFileName,
                            FilePath = relativePath,
                            FileSizeKB = (int)Math.Round(avatar.ContentLength / 1024.0),
                            FileFormat = ext.Replace(".", "").ToLower(),
                            UploadedAt = DateTime.UtcNow
                        };
                        db.ProfilePhotos.InsertOnSubmit(photo);
                        db.SubmitChanges();

                        // Link to both candidate and account
                        candidate.PhotoID = photo.PhotoID;
                        var account = db.Accounts.FirstOrDefault(a => a.AccountID == accountId.Value);
                        if (account != null)
                        {
                            account.PhotoID = photo.PhotoID;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Only JPG, PNG, GIF or WEBP images are allowed.");
                        // Map entity back to ViewModel for return
                        viewModel.CandidateID = candidate.CandidateID;
                        viewModel.FullName = candidate.FullName;
                        viewModel.BirthDate = candidate.BirthDate;
                        viewModel.Gender = candidate.Gender;
                        viewModel.Phone = candidate.Phone;
                        viewModel.Email = candidate.Email;
                        viewModel.Address = candidate.Address;
                        viewModel.Summary = candidate.Summary;
                        return View(viewModel);
                    }
                }

                db.SubmitChanges();
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công.";
                return RedirectToAction("CandidatesManage");
            }
        }

        [HttpGet]
        public ActionResult MyApplications(int? page)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = DbContextFactory.Create())
            {
                // Load related entities using DataLoad
                // Options - MUST be set BEFORE any queries
                var loadOptions = new System.Data.Linq.DataLoadOptions();
                loadOptions.LoadWith<Application>(a => a.JobPost);
                loadOptions.LoadWith<JobPost>(j => j.Company);
                loadOptions.LoadWith<JobPost>(j => j.Recruiter);
                loadOptions.LoadWith<Recruiter>(r => r.Company);
                db.LoadOptions = loadOptions;

                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng hoàn thiện hồ sơ trước khi xem đơn ứng tuyển.";
                    return RedirectToAction("CandidatesManage");
                }

                // Lấy danh sách applications với JobPost và Company info
                var applicationsQuery = from app in db.Applications
                                       where app.CandidateID == candidate.CandidateID
                                       orderby app.AppliedAt descending
                                       select app;

                // Convert to list first to ensure all related data is loaded
                var applicationsList = applicationsQuery.ToList();

                // Map to ViewModel
                var applications = applicationsList.Select(app => new ApplicationListItemViewModel
                {
                    ApplicationID = app.ApplicationID,
                    JobPostID = app.JobPostID,
                    JobTitle = app.JobPost?.Title ?? "N/A",
                    CompanyName = app.JobPost?.Company != null ? app.JobPost.Company.CompanyName :
                                 (app.JobPost?.Recruiter?.Company != null ? app.JobPost.Recruiter.Company.CompanyName : "N/A"),
                    Location = app.JobPost?.Location ?? string.Empty,
                    SalaryRange = SalaryHelper.FormatSalaryRange(app.JobPost?.SalaryFrom, app.JobPost?.SalaryTo, app.JobPost?.SalaryCurrency),
                    AppliedAt = app.AppliedAt,
                    Status = app.Status ?? "Under review",
                    ApplicationDeadline = app.JobPost?.ApplicationDeadline,
                    ResumeFilePath = app.ResumeFilePath,
                    CertificateFilePath = app.CertificateFilePath,
                    Note = app.Note
                }).ToList();

                // Pagination
                int pageSize = 10;
                int pageNumber = page ?? 1;
                var totalItems = applications.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var pagedApplications = applications.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                return View(pagedApplications);
            }
        }


        /// <summary>
        /// GET: Candidates/Apply
        /// Hiển thị form ứng tuyển
        /// </summary>
        [HttpGet]
        public ActionResult Apply(int? jobId)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!jobId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy công việc.";
                return RedirectToAction("JobsListing", "Jobs");
            }

            using (var db = DbContextFactory.Create())
            {
                // Load related entities
                var loadOptions = new System.Data.Linq.DataLoadOptions();
                loadOptions.LoadWith<JobPost>(j => j.Company);
                loadOptions.LoadWith<JobPost>(j => j.Recruiter);
                loadOptions.LoadWith<Recruiter>(r => r.Company);
                db.LoadOptions = loadOptions;

                JobStatusHelper.NormalizeStatuses(db);

                // Get job post
                var job = db.JobPosts.FirstOrDefault(j => j.JobPostID == jobId.Value);
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy công việc.";
                    return RedirectToAction("JobsListing", "Jobs");
                }

                job.Status = JobStatusHelper.NormalizeStatus(job.Status);

                // Check if job is still open
                if (!JobStatusHelper.IsPublished(job.Status))
                {
                    TempData["ErrorMessage"] = "Công việc này không còn nhận đơn ứng tuyển.";
                    return RedirectToAction("JobDetails", "Jobs", new { id = jobId.Value });
                }

                // Check deadline
                if (job.ApplicationDeadline.HasValue && job.ApplicationDeadline.Value < DateTime.Now.Date)
                {
                    TempData["ErrorMessage"] = "Hạn nộp hồ sơ đã qua.";
                    return RedirectToAction("JobDetails", "Jobs", new { id = jobId.Value });
                }

                // Get candidate
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng hoàn thiện hồ sơ trước khi ứng tuyển.";
                    return RedirectToAction("CandidatesManage");
                }

                // Check if already applied
                var existingApplication = db.Applications.FirstOrDefault(a => a.CandidateID == candidate.CandidateID && a.JobPostID == jobId.Value);
                if (existingApplication != null)
                {
                    TempData["WarningMessage"] = "Bạn đã ứng tuyển công việc này rồi.";
                    return RedirectToAction("JobDetails", "Jobs", new { id = jobId.Value });
                }

                // Get candidate photo
                string photoUrl = "/Content/images/person_1.jpg";
                if (candidate.PhotoID.HasValue)
                {
                    var photo = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == candidate.PhotoID.Value);
                    if (photo != null && !string.IsNullOrEmpty(photo.FilePath))
                    {
                        photoUrl = photo.FilePath;
                    }
                }

                // Get available resumes
                var resumeFiles = db.ResumeFiles
                    .Where(rf => rf.CandidateID == candidate.CandidateID)
                    .OrderByDescending(rf => rf.UploadedAt)
                    .ToList();

                var availableResumes = resumeFiles.Select(rf => new ResumeFileViewModel
                {
                    ResumeFileID = rf.ResumeFileID,
                    CandidateID = rf.CandidateID,
                    FileName = rf.FileName,
                    FilePath = rf.FilePath,
                    UploadedAt = rf.UploadedAt,
                    FileExtension = Path.GetExtension(rf.FileName ?? "")?.ToLower() ?? "",
                    DisplayName = string.IsNullOrEmpty(rf.FileName) ? "CV không tên" : Path.GetFileNameWithoutExtension(rf.FileName)
                }).ToList();

                // Get company name
                string companyName = job.Company != null ? job.Company.CompanyName :
                                   (job.Recruiter?.Company != null ? job.Recruiter.Company.CompanyName : "N/A");

                // Build ViewModel
                var viewModel = new ApplicationApplyViewModel
                {
                    JobPostID = job.JobPostID,
                    JobTitle = job.Title,
                    CompanyName = companyName,
                    Location = job.Location,
                    SalaryRange = SalaryHelper.FormatSalaryRange(job.SalaryFrom, job.SalaryTo, job.SalaryCurrency),
                    ApplicationDeadline = job.ApplicationDeadline,
                    CandidateID = candidate.CandidateID,
                    FullName = candidate.FullName,
                    BirthDate = candidate.BirthDate,
                    Gender = candidate.Gender,
                    Phone = candidate.Phone,
                    Email = candidate.Email,
                    Address = candidate.Address,
                    Summary = candidate.Summary,
                    PhotoUrl = photoUrl,
                    AvailableResumes = availableResumes
                };

                return View(viewModel);
            }
        }

        /// <summary>
        /// POST: Candidates/Apply
        /// Xử lý form ứng tuyển
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Apply(ApplicationApplyViewModel viewModel, HttpPostedFileBase newResumeFile)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // Reload data for view
                return Apply(viewModel.JobPostID);
            }

            using (var db = DbContextFactory.Create())
            {
                JobStatusHelper.NormalizeStatuses(db);
                // Get candidate
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng hoàn thiện hồ sơ trước khi ứng tuyển.";
                    return RedirectToAction("CandidatesManage");
                }

                // Get job post
                var job = db.JobPosts.FirstOrDefault(j => j.JobPostID == viewModel.JobPostID);
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Công việc này không còn nhận đơn ứng tuyển.";
                    return RedirectToAction("JobsListing", "Jobs");
                }

                job.Status = JobStatusHelper.NormalizeStatus(job.Status);
                if (!JobStatusHelper.IsPublished(job.Status))
                {
                    TempData["ErrorMessage"] = "Công việc này không còn nhận đơn ứng tuyển.";
                    return RedirectToAction("JobsListing", "Jobs");
                }

                // Check if already applied
                var existingApplication = db.Applications.FirstOrDefault(a => a.CandidateID == candidate.CandidateID && a.JobPostID == viewModel.JobPostID);
                if (existingApplication != null)
                {
                    TempData["WarningMessage"] = "Bạn đã ứng tuyển công việc này rồi.";
                    return RedirectToAction("Details", "JobDetails", new { id = viewModel.JobPostID });
                }

                string resumeFilePath = null;

                // Handle CV selection or upload
                if (viewModel.SelectedResumeFileID.HasValue)
                {
                    // Use existing resume
                    var selectedResume = db.ResumeFiles.FirstOrDefault(rf => rf.ResumeFileID == viewModel.SelectedResumeFileID.Value && rf.CandidateID == candidate.CandidateID);
                    if (selectedResume != null)
                    {
                        resumeFilePath = selectedResume.FilePath;
                    }
                }
                else if (newResumeFile != null && newResumeFile.ContentLength > 0)
                {
                    // Upload new resume
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg", ".gif" };
                    var fileExtension = Path.GetExtension(newResumeFile.FileName)?.ToLower();
                    if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("NewResumeFile", "Chỉ cho phép upload file PDF, DOC, DOCX, PNG, JPG, JPEG, GIF.");
                        return Apply(viewModel.JobPostID);
                    }

                    const int maxSize = 10 * 1024 * 1024; // 10MB
                    if (newResumeFile.ContentLength > maxSize)
                    {
                        ModelState.AddModelError("NewResumeFile", "File không được vượt quá 10MB.");
                        return Apply(viewModel.JobPostID);
                    }

                    try
                    {
                        // Create upload directory
                        var uploadsRoot = Server.MapPath("~/Content/uploads/resumes/");
                        if (!Directory.Exists(uploadsRoot))
                        {
                            Directory.CreateDirectory(uploadsRoot);
                        }

                        // Generate unique filename
                        var safeFileName = $"CV_{candidate.CandidateID}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{fileExtension}";
                        var physicalPath = Path.Combine(uploadsRoot, safeFileName);
                        newResumeFile.SaveAs(physicalPath);

                        // Save to database
                        var relativePath = $"/Content/uploads/resumes/{safeFileName}";
                        var resumeFileEntity = new ResumeFile
                        {
                            CandidateID = candidate.CandidateID,
                            FileName = string.IsNullOrWhiteSpace(viewModel.NewResumeTitle) ? newResumeFile.FileName : viewModel.NewResumeTitle + fileExtension,
                            FilePath = relativePath,
                            UploadedAt = DateTime.Now
                        };

                        db.ResumeFiles.InsertOnSubmit(resumeFileEntity);
                        db.SubmitChanges();

                        resumeFilePath = relativePath;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("NewResumeFile", "Lỗi khi upload CV: " + ex.Message);
                        return Apply(viewModel.JobPostID);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Vui lòng chọn CV từ danh sách hoặc tải CV mới lên.");
                    return Apply(viewModel.JobPostID);
                }

                // Create application
                var application = new Application
                {
                    CandidateID = candidate.CandidateID,
                    JobPostID = viewModel.JobPostID,
                    AppliedAt = DateTime.Now,
                    Status = "Under review",
                    ResumeFilePath = resumeFilePath,
                    Note = viewModel.Note,
                    UpdatedAt = DateTime.Now
                };

                db.Applications.InsertOnSubmit(application);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Bạn đã gửi đơn ứng tuyển thành công! Chúng tôi sẽ xem xét và liên hệ với bạn sớm nhất có thể.";
                return RedirectToAction("Details", "JobDetails", new { id = viewModel.JobPostID });
            }
        }
    }
}



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

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class CandidatesController : Controller
    {
        private int? GetCurrentAccountId()
        {
            var idClaim = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) return null;
            int accountId;
            return int.TryParse(idClaim.Value, out accountId) ? (int?)accountId : null;
        }

        [HttpGet]
        public ActionResult CandidatesManage()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
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

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    candidate = new Candidate { AccountID = accountId.Value, FullName = User.Identity.Name, Gender = "Nam", CreatedAt = DateTime.Now, ActiveFlag = 1 };
                    db.Candidates.InsertOnSubmit(candidate);
                }

                // Update profile fields only when model is valid
                if (ModelState.IsValid)
                {
                    candidate.FullName = viewModel.FullName;
                    candidate.BirthDate = viewModel.BirthDate;
                    candidate.Gender = string.IsNullOrWhiteSpace(viewModel.Gender) ? "Nam" : viewModel.Gender;
                    candidate.Phone = viewModel.Phone;
                    candidate.Email = viewModel.Email;
                    candidate.Address = viewModel.Address;
                    
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
                if (ModelState.IsValid)
                {
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công.";
                    return RedirectToAction("CandidatesManage");
                }
                // If model invalid, stay on page but keep newly uploaded avatar
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

        [HttpGet]
        public ActionResult MyApplications(int? page)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
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
                    SalaryRange = FormatSalaryRange(app.JobPost?.SalaryFrom, app.JobPost?.SalaryTo, app.JobPost?.SalaryCurrency),
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

        private string FormatSalaryRange(decimal? salaryFrom, decimal? salaryTo, string currency)
        {
            if (!salaryFrom.HasValue && !salaryTo.HasValue)
                return "Thỏa thuận";

            string currencySymbol = currency == "VND" ? "VNĐ" : currency ?? "VNĐ";

            if (salaryFrom.HasValue && salaryTo.HasValue)
            {
                if (currency == "VND")
                {
                    return $"{salaryFrom.Value:N0} - {salaryTo.Value:N0} {currencySymbol}";
                }
                return $"{salaryFrom.Value:N0} - {salaryTo.Value:N0} {currencySymbol}";
            }
            else if (salaryFrom.HasValue)
            {
                if (currency == "VND")
                {
                    return $"Từ {salaryFrom.Value:N0} {currencySymbol}";
                }
                return $"Từ {salaryFrom.Value:N0} {currencySymbol}";
            }
            else if (salaryTo.HasValue)
            {
                if (currency == "VND")
                {
                    return $"Đến {salaryTo.Value:N0} {currencySymbol}";
                }
                return $"Đến {salaryTo.Value:N0} {currencySymbol}";
            }

            return "Thỏa thuận";
        }
    }
}



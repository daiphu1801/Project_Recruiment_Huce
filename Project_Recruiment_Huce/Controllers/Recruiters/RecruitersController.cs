using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Infrastructure;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class RecruitersController : BaseController
    {

        [HttpGet]
        public ActionResult RecruitersManage()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null) return RedirectToAction("Login", "Account");

            using (var db = DbContextFactory.Create())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                if (recruiter == null)
                {
                    // Create new recruiter profile
                    recruiter = new Recruiter
                    {
                        AccountID = accountId.Value,
                        FullName = User.Identity.Name,
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    db.Recruiters.InsertOnSubmit(recruiter);
                    db.SubmitChanges();
                }

                // Note: Avatar is loaded from Account in the view (same as Candidate)
                // No need to set ViewBag.AvatarUrl here

                return View(recruiter);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecruitersManage(Recruiter recruiter, HttpPostedFileBase avatar)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate phone number format, uniqueness and normalize (if provided)
            var phone = (recruiter.Phone ?? string.Empty).Trim();
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

            // Validate company email format and uniqueness (if provided)
            var companyEmail = (recruiter.CompanyEmail ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(companyEmail))
            {
                // Validate email format
                if (!ValidationHelper.IsValidEmail(companyEmail))
                {
                    ModelState.AddModelError("CompanyEmail", "Email không hợp lệ.");
                }
                else
                {
                    // Check email uniqueness in Accounts table (exclude current account)
                    if (!ValidationHelper.IsEmailUniqueInAccounts(companyEmail, accountId.Value))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email này đã được sử dụng bởi tài khoản khác.");
                    }
                }
            }
            else
            {
                companyEmail = null;
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin. Có lỗi trong form.";
                // Reload entity for view
                using (var db = DbContextFactory.Create())
                {
                    var existingRecruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                    if (existingRecruiter == null)
                    {
                        existingRecruiter = new Recruiter
                        {
                            AccountID = accountId.Value,
                            FullName = User.Identity.Name,
                            CreatedAt = DateTime.Now,
                            ActiveFlag = 1
                        };
                        db.Recruiters.InsertOnSubmit(existingRecruiter);
                        db.SubmitChanges();
                    }
                    return View(existingRecruiter);
                }
            }

            using (var db = DbContextFactory.Create())
            {
                // Load entity from database
                var existingRecruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                if (existingRecruiter == null)
                {
                    // Create new recruiter if doesn't exist
                    existingRecruiter = new Recruiter
                    {
                        AccountID = accountId.Value,
                        FullName = recruiter.FullName ?? User.Identity.Name,
                        PositionTitle = recruiter.PositionTitle,
                        CompanyEmail = companyEmail, // Use validated email
                        Phone = phone, // Use normalized phone
                        CompanyID = recruiter.CompanyID,
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    db.Recruiters.InsertOnSubmit(existingRecruiter);
                }
                else
                {
                    // Update existing recruiter using LINQ to SQL (same approach as CandidatesController)
                    // Always update FullName (required field)
                    if (!string.IsNullOrWhiteSpace(recruiter.FullName))
                    {
                        existingRecruiter.FullName = recruiter.FullName;
                    }
                    
                    // Update nullable fields - allow setting to null/empty if user clears them
                    existingRecruiter.PositionTitle = recruiter.PositionTitle;
                    existingRecruiter.CompanyEmail = companyEmail; // Use validated email
                    existingRecruiter.Phone = phone; // Use normalized phone
                    
                    // Update CompanyID if provided (including null to clear it)
                    existingRecruiter.CompanyID = recruiter.CompanyID;
                }

                // Handle avatar upload if provided (same as Candidate - only save to Account.PhotoID)
                if (avatar != null && avatar.ContentLength > 0)
                {
                    var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var ext = Path.GetExtension(avatar.FileName)?.ToLower();
                    
                    if (ext != null && allowedExts.Contains(ext))
                    {
                        // Get account to check for old photo
                        var account = db.Accounts.FirstOrDefault(a => a.AccountID == accountId.Value);
                        
                        // Delete old photo if exists (from Account.PhotoID)
                        if (account != null && account.PhotoID.HasValue)
                        {
                            var oldPhoto = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == account.PhotoID.Value);
                            if (oldPhoto != null)
                            {
                                var oldFilePath = Server.MapPath("~" + oldPhoto.FilePath);
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    System.IO.File.Delete(oldFilePath);
                                }
                                db.ProfilePhotos.DeleteOnSubmit(oldPhoto);
                            }
                        }
                        
                        // Save new avatar
                        var uploadsRoot = Server.MapPath("~/Content/uploads/recruiter/");
                        if (!Directory.Exists(uploadsRoot))
                        {
                            Directory.CreateDirectory(uploadsRoot);
                        }
                        
                        var safeFileName = $"avatar_{accountId.Value}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
                        var physicalPath = Path.Combine(uploadsRoot, safeFileName);
                        avatar.SaveAs(physicalPath);
                        
                        var relativePath = $"~/Content/uploads/recruiter/{safeFileName}";
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
                        
                        // Link only to Account (same as Candidate)
                        if (account != null)
                        {
                            account.PhotoID = photo.PhotoID;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Chỉ chấp nhận file ảnh (JPG, PNG, GIF, WEBP).");
                    }
                }

                // Submit changes using LINQ to SQL
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("RecruitersManage");
            }
        }
    }
}


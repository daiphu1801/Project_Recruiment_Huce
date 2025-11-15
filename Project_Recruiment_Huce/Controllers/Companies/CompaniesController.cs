using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Companies;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class CompaniesController : BaseController
    {

        // Helper: Save uploaded logo
        private int? SaveLogo(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0) return null;

            try
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExt = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExt))
                {
                    TempData["ErrorMessage"] = "Chỉ cho phép upload file ảnh (jpg, jpeg, png, gif, webp)";
                    return null;
                }

                // Validate file size (max 5MB)
                if (file.ContentLength > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "File ảnh không được vượt quá 5MB";
                    return null;
                }

                // Generate unique filename
                var fileName = Guid.NewGuid().ToString() + fileExt;
                var uploadPath = Server.MapPath("~/Content/Uploads/Photos/");
                
                // Create directory if not exists
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var fullPath = Path.Combine(uploadPath, fileName);
                file.SaveAs(fullPath);

                // Save to database - ProfilePhotos table
                using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    var photo = new ProfilePhoto
                    {
                        FileName = file.FileName,
                        FilePath = "/Content/Uploads/Photos/" + fileName,
                        FileSizeKB = file.ContentLength / 1024,
                        FileFormat = fileExt.Replace(".", ""),
                        UploadedAt = DateTime.Now
                    };

                    db.ProfilePhotos.InsertOnSubmit(photo);
                    db.SubmitChanges();
                    return photo.PhotoID;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi upload logo: " + ex.Message;
                return null;
            }
        }

        // Helper: Delete photo from ProfilePhotos
        private void DeletePhoto(int photoId)
        {
            try
            {
                using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    var photo = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId);
                    if (photo == null) return;

                    // Delete physical file
                    var filePath = Server.MapPath("~" + photo.FilePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Delete database record
                    db.ProfilePhotos.DeleteOnSubmit(photo);
                    db.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa ảnh: " + ex.Message;
            }
        }

        [HttpGet]
        public ActionResult CompaniesManage()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null) return RedirectToAction("Login", "Account");

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Get recruiter and their company
                var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                if (recruiter == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng tạo hồ sơ nhà tuyển dụng trước.";
                    return RedirectToAction("RecruitersManage", "Recruiters");
                }

                Company company = null;
                if (recruiter.CompanyID.HasValue)
                {
                    company = db.Companies.FirstOrDefault(c => c.CompanyID == recruiter.CompanyID.Value);
                }

                // Map to ViewModel
                var viewModel = new CompanyManageViewModel
                {
                    CompanyID = company?.CompanyID,
                    CompanyName = company?.CompanyName,
                    TaxCode = company?.TaxCode,
                    Industry = company?.Industry,
                    Address = company?.Address,
                    Phone = company?.Phone,
                    Fax = null, // Fax field not in database yet, will be null for now
                    CompanyEmail = company?.CompanyEmail,
                    Website = company?.Website,
                    Description = company?.Description,
                    PhotoID = company?.PhotoID
                };

                // Get logo URL if exists
                if (company?.PhotoID.HasValue == true)
                {
                    var photo = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == company.PhotoID.Value);
                    if (photo != null)
                    {
                        viewModel.LogoUrl = photo.FilePath;
                    }
                }

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompaniesManage(CompanyManageViewModel viewModel)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate phone number format and uniqueness (if provided)
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

                    // Check if phone already exists in Companies (exclude current company)
                    if (!ValidationHelper.IsCompanyPhoneUnique(phone, viewModel.CompanyID))
                    {
                        ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng bởi công ty khác.");
                    }
                }
            }
            else
            {
                phone = null;
            }

            // Validate company email uniqueness (if provided)
            var companyEmail = (viewModel.CompanyEmail ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(companyEmail))
            {
                if (!ValidationHelper.IsCompanyEmailUnique(companyEmail, viewModel.CompanyID))
                {
                    ModelState.AddModelError("CompanyEmail", "Email này đã được sử dụng bởi công ty khác.");
                }
            }

            // Validate fax number format (if provided)
            var fax = (viewModel.Fax ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(fax))
            {
                if (!ValidationHelper.IsValidFax(fax))
                {
                    ModelState.AddModelError("Fax", ValidationHelper.GetFaxErrorMessage());
                }
            }
            else
            {
                fax = null;
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Get recruiter
                var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                if (recruiter == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng tạo hồ sơ nhà tuyển dụng trước.";
                    return RedirectToAction("RecruitersManage", "Recruiters");
                }

                Company company = null;
                if (recruiter.CompanyID.HasValue)
                {
                    company = db.Companies.FirstOrDefault(c => c.CompanyID == recruiter.CompanyID.Value);
                }

                // Handle logo upload
                if (viewModel.Logo != null && viewModel.Logo.ContentLength > 0)
                {
                    // Save new logo first
                    var newPhotoId = SaveLogo(viewModel.Logo);
                    if (newPhotoId.HasValue)
                    {
                        if (company == null)
                        {
                            // Create new company
                            company = new Company
                            {
                                CompanyName = viewModel.CompanyName,
                                TaxCode = viewModel.TaxCode,
                                Industry = viewModel.Industry,
                                Address = viewModel.Address,
                                Phone = phone, // Use normalized phone
                                CompanyEmail = companyEmail, // Use trimmed email
                                Website = viewModel.Website,
                                Description = !string.IsNullOrWhiteSpace(viewModel.Description) 
                                    ? HtmlSanitizerHelper.Sanitize(viewModel.Description) 
                                    : null,
                                CreatedAt = DateTime.Now,
                                ActiveFlag = 1,
                                PhotoID = newPhotoId.Value
                            };
                            db.Companies.InsertOnSubmit(company);
                            db.SubmitChanges();

                            // Link company to recruiter
                            recruiter.CompanyID = company.CompanyID;
                            db.SubmitChanges();
                        }
                        else
                        {
                            // Store old photo ID before updating
                            var oldPhotoId = company.PhotoID;
                            
                            // Reload company entity to avoid concurrency issues
                            db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, company);
                            
                            // Update existing company
                            company.CompanyName = viewModel.CompanyName;
                            company.TaxCode = viewModel.TaxCode;
                            company.Industry = viewModel.Industry;
                            company.Address = viewModel.Address;
                            company.Phone = phone; // Use normalized phone
                            company.CompanyEmail = companyEmail; // Use trimmed email
                            company.Website = viewModel.Website;
                            company.Description = !string.IsNullOrWhiteSpace(viewModel.Description)
                                ? HtmlSanitizerHelper.Sanitize(viewModel.Description)
                                : null;
                            company.PhotoID = newPhotoId.Value;

                            try
                            {
                                db.SubmitChanges();
                                
                                // Delete old logo after successful update
                                if (oldPhotoId.HasValue)
                                {
                                    DeletePhoto(oldPhotoId.Value);
                                }
                            }
                            catch (System.Data.Linq.ChangeConflictException)
                            {
                                // Handle concurrency conflict - refresh and retry
                                db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, company);
                                
                                // Retry update with fresh entity
                                company.CompanyName = viewModel.CompanyName;
                                company.TaxCode = viewModel.TaxCode;
                                company.Industry = viewModel.Industry;
                                company.Address = viewModel.Address;
                                company.Phone = viewModel.Phone;
                                company.CompanyEmail = viewModel.CompanyEmail;
                                company.Website = viewModel.Website;
                                company.Description = !string.IsNullOrWhiteSpace(viewModel.Description)
                                    ? HtmlSanitizerHelper.Sanitize(viewModel.Description)
                                    : null;
                                company.PhotoID = newPhotoId.Value;
                                
                                db.SubmitChanges();
                                
                                // Delete old logo after successful update
                                if (oldPhotoId.HasValue)
                                {
                                    DeletePhoto(oldPhotoId.Value);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // No new logo uploaded, just update other fields
                    if (company == null)
                    {
                        // Create new company
                        company = new Company
                        {
                            CompanyName = viewModel.CompanyName,
                            TaxCode = viewModel.TaxCode,
                            Industry = viewModel.Industry,
                            Address = viewModel.Address,
                            Phone = viewModel.Phone,
                            CompanyEmail = viewModel.CompanyEmail,
                            Website = viewModel.Website,
                            Description = !string.IsNullOrWhiteSpace(viewModel.Description) 
                                ? HtmlSanitizerHelper.Sanitize(viewModel.Description) 
                                : null,
                            CreatedAt = DateTime.Now,
                            ActiveFlag = 1
                        };
                        db.Companies.InsertOnSubmit(company);
                        db.SubmitChanges();

                        // Link company to recruiter
                        recruiter.CompanyID = company.CompanyID;
                        db.SubmitChanges();
                    }
                    else
                    {
                        // Update existing company
                        company.CompanyName = viewModel.CompanyName;
                        company.TaxCode = viewModel.TaxCode;
                        company.Industry = viewModel.Industry;
                        company.Address = viewModel.Address;
                        company.Phone = viewModel.Phone;
                        company.CompanyEmail = viewModel.CompanyEmail;
                        company.Website = viewModel.Website;
                        company.Description = !string.IsNullOrWhiteSpace(viewModel.Description)
                            ? HtmlSanitizerHelper.Sanitize(viewModel.Description)
                            : null;

                        db.SubmitChanges();
                    }
                }

                TempData["SuccessMessage"] = "Cập nhật thông tin công ty thành công!";
                return RedirectToAction("CompaniesManage");
            }
        }
    }
}


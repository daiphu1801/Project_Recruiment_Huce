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
using Project_Recruiment_Huce.Infrastructure;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller quản lý thông tin công ty - logo, address, industry, website, description
    /// Liên kết với Recruiter qua CompanyID
    /// </summary>
    [Authorize]
    public class CompaniesController : BaseController
    {
        /// <summary>
        /// Helper: Lưu logo đã upload vào ProfilePhotos table
        /// Validate: file type, file size (max 5MB)
        /// </summary>
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
                using (var db = DbContextFactory.Create())
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

        /// <summary>
        /// Helper: Xóa photo khỏi ProfilePhotos table và file vật lý
        /// </summary>
        private void DeletePhoto(int photoId)
        {
            try
            {
                using (var db = DbContextFactory.Create())
                {
                    var photo = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId);
                    if (photo == null) return;

                    // Xóa file vật lý
                    var filePath = Server.MapPath("~" + photo.FilePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Xóa record trong database
                    db.ProfilePhotos.DeleteOnSubmit(photo);
                    db.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa ảnh: " + ex.Message;
            }
        }

        /// <summary>
        /// Hiển thị trang quản lý thông tin công ty
        /// GET: Companies/CompaniesManage
        /// Yêu cầu phải có Recruiter profile
        /// </summary>
        [HttpGet]
        public ActionResult CompaniesManage()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null) return RedirectToAction("Login", "Account");

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var repo = new Repositories.CompanyRepository(db);

                // Lấy recruiter và company của họ qua repository
                var recruiter = repo.GetRecruiterByAccountId(accountId.Value);
                if (recruiter == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng tạo hồ sơ nhà tuyển dụng trước.";
                    return RedirectToAction("RecruitersManage", "Recruiters");
                }

                Company company = null;
                if (recruiter.CompanyID.HasValue)
                {
                    company = repo.GetCompanyById(recruiter.CompanyID.Value);
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
                    Fax = company?.Fax,
                    CompanyEmail = company?.CompanyEmail,
                    Website = company?.Website,
                    Description = company?.Description,
                    PhotoID = company?.PhotoID
                };

                // Get logo URL nếu có
                if (company?.PhotoID.HasValue == true)
                {
                    var photo = repo.GetProfilePhotoById(company.PhotoID.Value);
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

            // Validate company email format and uniqueness (if provided)
            var companyEmail = (viewModel.CompanyEmail ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(companyEmail))
            {
                // Validate email format
                if (!ValidationHelper.IsValidEmail(companyEmail))
                {
                    ModelState.AddModelError("CompanyEmail", "Email không hợp lệ.");
                }
                else
                {
                    // Check email uniqueness
                    if (!ValidationHelper.IsCompanyEmailUnique(companyEmail, viewModel.CompanyID))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email này đã được sử dụng bởi công ty khác.");
                    }
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
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin. Có lỗi trong form.";
                return View(viewModel);
            }

            using (var db = DbContextFactory.Create())
            {
                var repo = new Repositories.CompanyRepository(db);

                // Get recruiter via repository
                var recruiter = repo.GetRecruiterByAccountId(accountId.Value);
                if (recruiter == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng tạo hồ sơ nhà tuyển dụng trước.";
                    return RedirectToAction("RecruitersManage", "Recruiters");
                }

                Company company = null;
                if (recruiter.CompanyID.HasValue)
                {
                    company = repo.GetCompanyById(recruiter.CompanyID.Value);
                }

                // Handle logo upload
                if (viewModel.Logo != null && viewModel.Logo.ContentLength > 0)
                {
                    // Save file to disk and insert ProfilePhoto via repo
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
                                    Fax = fax,
                                    CompanyEmail = companyEmail, // Use trimmed email
                                    Website = viewModel.Website,
                                    Description = !string.IsNullOrWhiteSpace(viewModel.Description)
                                        ? HtmlSanitizerHelper.Sanitize(viewModel.Description)
                                        : null,
                                CreatedAt = DateTime.Now,
                                ActiveFlag = 1,
                                PhotoID = newPhotoId.Value
                            };
                            repo.InsertCompany(company);
                            repo.SaveChanges();

                            // Link company to recruiter and save
                            recruiter.CompanyID = company.CompanyID;
                            repo.SaveChanges();
                        }
                        else
                        {
                            var oldPhotoId = company.PhotoID;

                            // Refresh entity from DB is not needed because same context tracks it
                            company.CompanyName = viewModel.CompanyName;
                            company.TaxCode = viewModel.TaxCode;
                            company.Industry = viewModel.Industry;
                            company.Address = viewModel.Address;
                            company.Phone = phone;
                            company.Fax = fax;
                            company.CompanyEmail = companyEmail;
                            company.Website = viewModel.Website;
                            company.Description = !string.IsNullOrWhiteSpace(viewModel.Description)
                                ? HtmlSanitizerHelper.Sanitize(viewModel.Description)
                                : null;
                            company.PhotoID = newPhotoId.Value;

                            try
                            {
                                repo.SaveChanges();

                                if (oldPhotoId.HasValue)
                                {
                                    DeletePhoto(oldPhotoId.Value);
                                }
                            }
                            catch (System.Data.Linq.ChangeConflictException)
                            {
                                // Concurrency conflict: refresh and retry
                                db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, company);

                                company.CompanyName = viewModel.CompanyName;
                                company.TaxCode = viewModel.TaxCode;
                                company.Industry = viewModel.Industry;
                                company.Address = viewModel.Address;
                                company.Phone = viewModel.Phone;
                                company.Fax = fax;
                                company.CompanyEmail = viewModel.CompanyEmail;
                                company.Website = viewModel.Website;
                                company.Description = !string.IsNullOrWhiteSpace(viewModel.Description)
                                    ? HtmlSanitizerHelper.Sanitize(viewModel.Description)
                                    : null;
                                company.PhotoID = newPhotoId.Value;

                                repo.SaveChanges();

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
                                Fax = viewModel.Fax,
                                CompanyEmail = viewModel.CompanyEmail,
                                Website = viewModel.Website,
                                Description = !string.IsNullOrWhiteSpace(viewModel.Description)
                                    ? HtmlSanitizerHelper.Sanitize(viewModel.Description)
                                    : null,
                            CreatedAt = DateTime.Now,
                            ActiveFlag = 1
                        };
                        repo.InsertCompany(company);
                        repo.SaveChanges();

                        // Link company to recruiter
                        recruiter.CompanyID = company.CompanyID;
                        repo.SaveChanges();
                    }
                    else
                    {
                        // Update existing company
                        company.CompanyName = viewModel.CompanyName;
                        company.TaxCode = viewModel.TaxCode;
                        company.Industry = viewModel.Industry;
                        company.Address = viewModel.Address;
                        company.Phone = viewModel.Phone;
                        company.Fax = viewModel.Fax;
                        company.CompanyEmail = viewModel.CompanyEmail;
                        company.Website = viewModel.Website;
                        company.Description = !string.IsNullOrWhiteSpace(viewModel.Description)
                            ? HtmlSanitizerHelper.Sanitize(viewModel.Description)
                            : null;

                        repo.SaveChanges();
                    }
                }

                TempData["SuccessMessage"] = "Cập nhật thông tin công ty thành công!";
                return RedirectToAction("CompaniesManage");
            }
        }
    }
}


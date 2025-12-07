using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Companies;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Services;
using Project_Recruiment_Huce.Repositories;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller quản lý thông tin công ty - logo, address, industry, website, description
    /// Liên kết với Recruiter qua CompanyID
    /// </summary>
    public class CompaniesController : BaseController
    {
        private readonly ICompanyService _companyService;

        public CompaniesController()
        {
            // Initialize service with repository
            var db = DbContextFactory.Create();
            var repo = new CompanyRepository(db);
            _companyService = new CompanyService(repo);
        }
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

            var viewModel = _companyService.GetCompanyManageViewModel(accountId.Value);
            
            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "Vui lòng tạo hồ sơ nhà tuyển dụng trước.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            return View(viewModel);
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
                    viewModel.Phone = phone;

                    // Check if phone already exists in Companies (exclude current company)
                    if (!ValidationHelper.IsCompanyPhoneUnique(phone, viewModel.CompanyID))
                    {
                        ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng bởi công ty khác.");
                    }
                }
            }
            else
            {
                viewModel.Phone = null;
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

            // Validate fax number format and uniqueness (if provided)
            var fax = (viewModel.Fax ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(fax))
            {
                if (!ValidationHelper.IsValidFax(fax))
                {
                    ModelState.AddModelError("Fax", ValidationHelper.GetFaxErrorMessage());
                }
                else
                {
                    // Normalize fax number
                    fax = ValidationHelper.NormalizeFax(fax);
                    viewModel.Fax = fax;
                    if (!ValidationHelper.IsCompanyFaxUnique(fax, viewModel.CompanyID))
                    {
                        ModelState.AddModelError("Fax", "Số Fax này đã được sử dụng.");
                    }
                }
            }
            else
            {
                viewModel.Fax = null;
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Save uploaded logo (if any)
            int? newPhotoId = null;
            if (viewModel.Logo != null && viewModel.Logo.ContentLength > 0)
            {
                newPhotoId = SaveLogo(viewModel.Logo);
                if (newPhotoId == null && TempData["ErrorMessage"] != null)
                {
                    return View(viewModel);
                }
            }

            // Use service to save/update company
            var result = _companyService.SaveOrUpdateCompany(viewModel, accountId.Value, newPhotoId);

            if (!result.IsValid)
            {
                // Add errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
                return View(viewModel);
            }

            // If old photo was replaced, delete the physical file
            if (result.Data.ContainsKey("OldPhotoId"))
            {
                DeletePhoto((int)result.Data["OldPhotoId"]);
            }

            TempData["SuccessMessage"] = "Cập nhật thông tin công ty thành công!";
            return RedirectToAction("CompaniesManage");
        }

        /// <summary>
        /// Hiển thị chi tiết công ty cho ứng viên xem (không cần đăng nhập)
        /// </summary>
        [AllowAnonymous]
        public ActionResult CompanyDetails(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("JobsListing", "Jobs");
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {
                // Set LoadOptions để load eager Company và ProfilePhoto
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Company>(c => c.ProfilePhoto);
                loadOptions.LoadWith<Company>(c => c.JobPosts);
                db.LoadOptions = loadOptions;

                var company = db.Companies.FirstOrDefault(c => c.CompanyID == id.Value);
                
                if (company == null)
                {
                    return RedirectToAction("JobsListing", "Jobs");
                }

                // Tạo view model cho company details
                var viewModel = new CompanyDetailsViewModel
                {
                    CompanyID = company.CompanyID,
                    CompanyName = company.CompanyName,
                    TaxCode = company.TaxCode,
                    Industry = company.Industry,
                    Address = company.Address,
                    Phone = company.Phone,
                    Fax = company.Fax,
                    CompanyEmail = company.CompanyEmail,
                    Website = company.Website,
                    Description = company.Description,
                    LogoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : "/Content/images/job_logo_1.jpg",
                    ActiveJobCount = company.JobPosts.Count(j => j.Status == "Published" && 
                                                                (!j.ApplicationDeadline.HasValue || j.ApplicationDeadline.Value >= DateTime.Now))
                };

                return View(viewModel);
            }
        }
    }
}

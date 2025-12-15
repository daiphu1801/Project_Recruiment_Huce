using Microsoft.Owin.BuilderProperties;
using Project_Recruiment_Huce.Areas.Admin.Helpers;
using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    /// <summary>
    /// Base CRUD Controller - Template for implementing other controllers with database.
    /// This controller demonstrates full CRUD operations (Create, Read, Update, Delete) 
    /// using JOBPORTAL_ENDataContext. Other controllers should follow this pattern.
    /// </summary>
    public class CompaniesController : AdminBaseController
    {
        // GET: Admin/Accounts
        public ActionResult Index(string q, string role = null, int page = 1)
        {
            ViewBag.Title = "Quản lý công ty";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Công ty", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var query = db.Companies.AsQueryable();

                // Search
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.Where(c =>
                        (c.CompanyName != null && c.CompanyName.Contains(q)) ||
                        (c.CompanyEmail != null && c.CompanyEmail.Contains(q)) ||
                        (c.Phone != null && c.Phone.Contains(q))
                    );
                }

                int pageSize = 10;
                int totalRecords = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var companies = query.OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CompanyListVm
                    {
                        CompanyId = c.CompanyID,
                        CompanyName = c.CompanyName,
                        TaxCode = c.TaxCode,
                        Industry = c.Industry,
                        Address = c.Address,
                        Phone = c.Phone,
                        Fax = c.Fax,
                        CompanyEmail = c.CompanyEmail,
                        Website = c.Website,
                        Description = c.Description,
                        CreatedAt = c.CreatedAt,
                        ActiveFlag = c.ActiveFlag,
                        PhotoId = c.PhotoID,
                        PhotoUrl = c.ProfilePhoto != null ? c.ProfilePhoto.FilePath : null
                    }).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalRecords;
                ViewBag.PageSize = pageSize;

                return View(companies);
            }
        }

        // GET: Admin/Accounts/Details/5
        public ActionResult Details(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var company = db.Companies.FirstOrDefault(c => c.CompanyID == id);
                if (company == null) return HttpNotFound();

                var recruiter = db.Recruiters.FirstOrDefault(r => r.CompanyID == id);

                var vm = new CompanyListVm
                {
                    CompanyId = company.CompanyID,
                    CompanyName = company.CompanyName,
                    TaxCode = company.TaxCode,
                    Industry = company.Industry,
                    Address = company.Address,
                    Phone = company.Phone,
                    Fax = company.Fax,
                    CompanyEmail = company.CompanyEmail,
                    Website = company.Website,
                    Description = company.Description,
                    CreatedAt = company.CreatedAt,
                    ActiveFlag = company.ActiveFlag,
                    PhotoId = company.PhotoID,
                    PhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null,
                    RecruiterId = recruiter?.RecruiterID,
                    RecruiterName = recruiter?.FullName
                };

                ViewBag.Title = "Chi tiết công ty";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Công ty", Url.Action("Index")),
                    new Tuple<string, string>($"#{company.CompanyID}", null)
                };

                return View(vm);
            }
        }

        // GET: Admin/Accounts/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm công ty mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Công ty", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };
            return View();
        }

        // POST: Admin/Accounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateCompanyVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Check duplicate company name
                if (!string.IsNullOrWhiteSpace(model.CompanyName) && db.Companies.Any(c => c.CompanyName == model.CompanyName))
                {
                    ModelState.AddModelError("CompanyName", "Công ty đã tồn tại");
                    return View(model);
                }

                // Check duplicate tax code
                if (!string.IsNullOrWhiteSpace(model.TaxCode) && db.Companies.Any(c => c.TaxCode == model.TaxCode))
                {
                    ModelState.AddModelError("TaxCode", "Mã số thuế đã tồn tại");
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.Address) && db.Companies.Any(c => c.Address == model.Address))
                {
                    ModelState.AddModelError("Address", "Địa chỉ đã tồn tại");
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.Address))
                {
                    bool isAddressReal = IsAddressValid(model.Address);
                    if (!isAddressReal)
                    {
                        // Thêm lỗi vào ModelState. Điều này làm ModelState.IsValid thành false
                        ModelState.AddModelError("Address", "Địa chỉ không tồn tại trên bản đồ. Vui lòng kiểm tra lại.");
                    }
                }

                // Validate phone number format and uniqueness (if provided)
                var phone = (model.Phone ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    // Validate phone format
                    if (!ValidationHelper.IsValidVietnamesePhone(phone))
                    {
                        ModelState.AddModelError("Phone", ValidationHelper.GetPhoneErrorMessage());
                        return View(model);
                    }

                    // Normalize phone number
                    phone = ValidationHelper.NormalizePhone(phone);

                    // Check if phone already exists
                    if (!ValidationHelper.IsCompanyPhoneUnique(phone))
                    {
                        ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng bởi công ty khác.");
                        return View(model);
                    }
                }
                else
                {
                    phone = null;
                }

                var fax = (model.Fax ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(fax))
                {
                    if (!ValidationHelper.IsValidFax(fax))
                    {
                        ModelState.AddModelError("Fax", ValidationHelper.GetFaxErrorMessage());
                        return View(model);
                    }

                    fax = ValidationHelper.NormalizePhone(fax);

                    if (!ValidationHelper.IsCompanyFaxUnique(fax))
                    {
                        ModelState.AddModelError("Fax", "Số Fax này đã được sử dụng bởi công ty khác.");
                        return View(model);
                    }
                }
                else
                {
                    fax = null;
                }

                // Validate company email uniqueness (if provided)
                var companyEmail = (model.CompanyEmail ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(companyEmail))
                {
                    if (!ValidationHelper.IsCompanyEmailUnique(companyEmail))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email này đã được sử dụng bởi công ty khác.");
                        return View(model);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.Website))
                {
                    var websiteLower = model.Website.ToLowerInvariant();
                    if (db.Companies.Any(c => c.Website != null && c.Website.ToLower() == websiteLower))
                    {
                        ModelState.AddModelError("Website", "Website đã tồn tại");
                        return View(model);
                    }
                }

                var company = new Company
                {
                    CompanyName = model.CompanyName,
                    TaxCode = model.TaxCode,
                    Industry = model.Industry,
                    Address = model.Address,
                    Phone = phone,
                    Fax = fax,
                    CompanyEmail = companyEmail,
                    Website = model.Website,
                    Description = !string.IsNullOrWhiteSpace(model.Description) ? HtmlSanitizerHelper.Sanitize(model.Description) : null,
                    CreatedAt = DateTime.Now,
                    ActiveFlag = model.Active ? (byte)1 : (byte)0,
                };

                //  Handle photo upload
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    ProfilePhoto photo = SavePhoto(db, model.PhotoFile);
                    if (photo != null)
                    {
                        company.ProfilePhoto = photo;
                    }
                    else
                    {
                        return View(model);
                    }
                }

                db.Companies.InsertOnSubmit(company);

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Tạo công ty thành công!";
                return RedirectToAction("Index");
            }
        }

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

        // GET: Admin/Accounts/Edit/5
        public ActionResult Edit(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var company = db.Companies.FirstOrDefault(c => c.CompanyID == id);
                if (company == null) return HttpNotFound();

                var vm = new EditCompanyVm
                {
                    CompanyId = company.CompanyID,
                    CompanyName = company.CompanyName ?? string.Empty,
                    TaxCode = company.TaxCode ?? string.Empty,
                    Industry = company.Industry ?? string.Empty,
                    Address = company.Address ?? string.Empty,
                    Phone = company.Phone ?? string.Empty,
                    Fax = company.Fax ?? string.Empty,
                    CompanyEmail = company.CompanyEmail ?? string.Empty,
                    Website = company.Website ?? string.Empty,
                    Description = company.Description ?? string.Empty,
                    ActiveFlag = company.ActiveFlag,
                    CurrentPhotoId = company.PhotoID,
                    CurrentPhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null
                };

                ViewBag.Title = "Sửa công ty";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Công ty", Url.Action("Index")),
                    new Tuple<string, string>($"#{company.CompanyID}", null)
                };
                return View(vm);
            }
        }

        // POST: Admin/Accounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditCompanyVm model)
        {
            Action<Company> refreshPhotoInfo = (company) =>
            {
                if (company != null)
                {
                    model.CurrentPhotoId = company.PhotoID;
                    model.CurrentPhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null;
                }
            };

            if (!ModelState.IsValid)
            {
                using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    var companyForPhoto = db.Companies.FirstOrDefault(c => c.CompanyID == model.CompanyId);
                    refreshPhotoInfo(companyForPhoto);
                }
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var company = db.Companies.FirstOrDefault(c => c.CompanyID == model.CompanyId);
                if (company == null) return HttpNotFound();

                // Check duplicate company name (except current company)
                if (!string.IsNullOrWhiteSpace(model.CompanyName) && db.Companies.Any(c => c.CompanyName == model.CompanyName && c.CompanyID != model.CompanyId))
                {
                    ModelState.AddModelError("CompanyName", "Tên công ty đã tồn tại");
                    refreshPhotoInfo(company);
                    return View(model);
                }

                // Check duplicate tax code
                if (!string.IsNullOrWhiteSpace(model.TaxCode) && db.Companies.Any(c => c.TaxCode == model.TaxCode && c.CompanyID != model.CompanyId))
                {
                    ModelState.AddModelError("TaxCode", "Mã số thuế đã tồn tại");
                    refreshPhotoInfo(company);
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.Address) && db.Companies.Any(c => c.Address == model.Address && c.CompanyID != model.CompanyId))
                {
                    ModelState.AddModelError("Address", "Địa chỉ đã tồn tại");
                    refreshPhotoInfo(company);
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.Address))
                {
                    bool isAddressReal = IsAddressValid(model.Address);
                    if (!isAddressReal)
                    {
                        ModelState.AddModelError("Address", "Địa chỉ không tồn tại trên bản đồ. Vui lòng kiểm tra lại.");
                        refreshPhotoInfo(company);
                        return View(model);
                    }
                }

                // Validate phone number format and uniqueness (if provided)
                var phone = (model.Phone ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    // Validate phone format
                    if (!ValidationHelper.IsValidVietnamesePhone(phone))
                    {
                        ModelState.AddModelError("Phone", ValidationHelper.GetPhoneErrorMessage());
                        refreshPhotoInfo(company);
                        return View(model);
                    }

                    // Normalize phone number
                    phone = ValidationHelper.NormalizePhone(phone);

                    // Check if phone already exists (except current company)
                    if (!ValidationHelper.IsCompanyPhoneUnique(phone, model.CompanyId))
                    {
                        ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại.");
                        refreshPhotoInfo(company);
                        return View(model);
                    }
                }
                else
                {
                    phone = null;
                }

                // Check duplicate tax code
                var fax = (model.Fax ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(fax))
                {
                    if (!ValidationHelper.IsValidFax(fax))
                    {
                        ModelState.AddModelError("Fax", ValidationHelper.GetFaxErrorMessage());
                        refreshPhotoInfo(company);
                        return View(model);
                    }

                    fax = ValidationHelper.NormalizePhone(fax);

                    if (!ValidationHelper.IsCompanyFaxUnique(fax, model.CompanyId))
                    {
                        ModelState.AddModelError("Fax", "Số Fax đã tồn tại.");
                        refreshPhotoInfo(company);
                        return View(model);
                    }
                }
                else { fax = null; }

                // Validate company email uniqueness (if provided)
                var companyEmail = (model.CompanyEmail ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(companyEmail))
                {
                    if (!ValidationHelper.IsCompanyEmailUnique(companyEmail, model.CompanyId))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email đã tồn tại.");
                        refreshPhotoInfo(company);
                        return View(model);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.Website))
                {
                    var websiteLower = model.Website.ToLowerInvariant();
                    if (db.Companies.Any(c => c.Website != null && c.Website.ToLower() == websiteLower && c.CompanyID != model.CompanyId))
                    {
                        ModelState.AddModelError("Website", "Website đã tồn tại");
                        refreshPhotoInfo(company);
                        return View(model);
                    }
                }

                // Handle photo upload
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    if (company.PhotoID.HasValue)
                    {
                        DeletePhoto(db, company.PhotoID.Value);
                    }

                    ProfilePhoto newPhotoId = SavePhoto(db, model.PhotoFile);
                    if (newPhotoId != null)
                    {
                        company.ProfilePhoto = newPhotoId;
                    }
                    else
                    {
                        refreshPhotoInfo(company);
                        return View(model);
                    }
                }

                // Update account
                company.CompanyName = model.CompanyName;
                company.TaxCode = model.TaxCode;
                company.Industry = model.Industry;
                company.Address = model.Address;
                company.Phone = phone; // Use normalized phone
                company.Fax = fax;
                company.CompanyEmail = companyEmail; // Use trimmed email
                company.Website = model.Website;
                company.Description = !string.IsNullOrWhiteSpace(model.Description) ? HtmlSanitizerHelper.Sanitize(model.Description) : null;
                company.ActiveFlag = model.ActiveFlag ?? (byte)1; // Cast byte? to byte

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật công ty thành công!";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/Accounts/Delete/5
        public ActionResult Delete(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var company = db.Companies.FirstOrDefault(c => c.CompanyID == id);
                if (company == null) return HttpNotFound();

                var vm = new CompanyListVm
                {
                    CompanyId = company.CompanyID,
                    CompanyName = company.CompanyName ?? string.Empty,
                    TaxCode = company.TaxCode ?? string.Empty,
                    Industry = company.Industry ?? string.Empty,
                    Address = company.Address ?? string.Empty,
                    Phone = company.Phone ?? string.Empty,
                    Fax = company.Fax ?? string.Empty,
                    CompanyEmail = company.CompanyEmail ?? string.Empty,
                    Website = company.Website ?? string.Empty,
                    Description = company.Description ?? string.Empty,
                    ActiveFlag = company.ActiveFlag,
                    CreatedAt = company.CreatedAt,
                    PhotoId = company.PhotoID,
                    PhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null
                };

                ViewBag.Title = "Xóa công ty";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Công ty", Url.Action("Index")),
                    new Tuple<string, string>($"#{company.CompanyID}", null)
                };

                return View(vm);
            }
        }

        // POST: Admin/Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var company = db.Companies.FirstOrDefault(c => c.CompanyID == id);
                if (company == null) return HttpNotFound();

                // Kiểm tra ràng buộc với Recruiters
                var recruiterCount = db.Recruiters.Count(r => r.CompanyID == id);
                if (recruiterCount > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa công ty này vì có {recruiterCount} nhà tuyển dụng đang liên kết. Vui lòng xóa hoặc chuyển nhà tuyển dụng sang công ty khác trước.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra ràng buộc với JobPosts
                var jobPostCount = db.JobPosts.Count(j => j.CompanyID == id);
                if (jobPostCount > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa công ty này vì có {jobPostCount} tin tuyển dụng đang được liên kết. Vui lòng xóa hoặc chuyển tin tuyển dụng sang công ty khác trước.";
                    return RedirectToAction("Index");
                }

                try
                {
                    // Delete photo if exists
                    if (company.PhotoID.HasValue)
                    {
                        DeletePhoto(db, company.PhotoID.Value);
                    }

                    db.Companies.DeleteOnSubmit(company);
                    db.SubmitChanges();

                    TempData["SuccessMessage"] = "Xóa công ty thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi hệ thống khi xóa: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }

        // Helper: Save uploaded photo
        private ProfilePhoto SavePhoto(JOBPORTAL_ENDataContext db, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0) return null;

            try
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExt = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExt))
                {
                    TempData["ErrorMessage"] = "Chỉ cho phép upload file ảnh (jpg, jpeg, png, gif)";
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
                var photo = new ProfilePhoto
                {
                    FileName = file.FileName,
                    FilePath = "/Content/Uploads/Photos/" + fileName,
                    FileSizeKB = file.ContentLength / 1024,
                    FileFormat = fileExt.Replace(".", ""),
                    UploadedAt = DateTime.Now
                };

                db.ProfilePhotos.InsertOnSubmit(photo);
                return photo;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi upload ảnh: " + ex.Message;
                return null;
            }
        }

        // Helper: Delete photo from ProfilePhotos
        private void DeletePhoto(JOBPORTAL_ENDataContext db, int photoId)
        {
            try
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
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa ảnh: " + ex.Message;
            }
        }
    }
}
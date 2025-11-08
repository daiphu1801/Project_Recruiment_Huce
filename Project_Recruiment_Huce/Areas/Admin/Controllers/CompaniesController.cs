using Microsoft.Owin.BuilderProperties;
using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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

                // Convert to ViewModel
                var companies = query.Select(c => new CompanyListVm
                {
                    CompanyId = c.CompanyID,
                    CompanyName = c.CompanyName,
                    TaxCode = c.TaxCode,
                    Industry = c.Industry,
                    Address = c.Address,
                    Phone = c.Phone,
                    CompanyEmail = c.CompanyEmail,
                    Website = c.Website,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    ActiveFlag = c.ActiveFlag,
                    PhotoId = c.PhotoID,
                    PhotoUrl = c.ProfilePhoto != null ? c.ProfilePhoto.FilePath : null
                }).ToList();

                // You may want to add paging logic here if needed, using 'page' parameter

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

                var vm = new CompanyListVm
                {
                    CompanyId = company.CompanyID,
                    CompanyName = company.CompanyName,
                    TaxCode = company.TaxCode,
                    Industry = company.Industry,
                    Address = company.Address,
                    Phone = company.Phone,
                    CompanyEmail = company.CompanyEmail,
                    Website = company.Website,
                    Description = company.Description,
                    CreatedAt = company.CreatedAt,
                    ActiveFlag = company.ActiveFlag,
                    PhotoId = company.PhotoID,
                    PhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null
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
            // Validate model first to avoid unnecessary DB work and null issues
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

                if (!string.IsNullOrWhiteSpace(model.TaxCode) && db.Companies.Any(c => c.Phone == model.Phone))
                {
                    ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại");
                    return View(model);
                }

                // Check duplicate email (safe null guards)
                if (!string.IsNullOrWhiteSpace(model.CompanyEmail))
                {
                    var emailLower = model.CompanyEmail.ToLowerInvariant();
                    if (db.Companies.Any(c => c.CompanyEmail != null && c.CompanyEmail.ToLower() == emailLower))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email đã được sử dụng");
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

                // Handle photo upload
                int? photoId = null;
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    photoId = SavePhoto(model.PhotoFile);
                }

                // Create account, set ActiveFlag based on model.Active
                var company = new Company
                {
                    CompanyName = model.CompanyName,
                    TaxCode = model.TaxCode,
                    Industry = model.Industry,
                    Address = model.Address,
                    Phone = model.Phone,
                    CompanyEmail = model.CompanyEmail,
                    Website = model.Website,
                    Description = model.Description,
                    CreatedAt = DateTime.Now,
                    ActiveFlag = model.Active ? (byte)1 : (byte)0,
                    PhotoID = photoId
                };

                db.Companies.InsertOnSubmit(company);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Tạo công ty thành công!";
                return RedirectToAction("Index");
            }
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
            // Validate model first
            if (!ModelState.IsValid)
            {
                using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    var company = db.Companies.FirstOrDefault(c => c.CompanyID == model.CompanyId);
                    if (company != null)
                    {
                        model.CurrentPhotoId = company.PhotoID;
                        model.CurrentPhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null;
                    }
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
                    model.CurrentPhotoId = company.PhotoID;
                    model.CurrentPhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null;
                    return View(model);
                }

                // Check duplicate tax code
                if (!string.IsNullOrWhiteSpace(model.TaxCode) && db.Companies.Any(c => c.TaxCode == model.TaxCode && c.CompanyID != model.CompanyId))
                {
                    ModelState.AddModelError("TaxCode", "Mã số thuế đã tồn tại");
                    model.CurrentPhotoId = company.PhotoID;
                    model.CurrentPhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null;
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.Address) && db.Companies.Any(c => c.Address == model.Address && c.CompanyID != model.CompanyId))
                {
                    ModelState.AddModelError("Address", "Địa chỉ đã tồn tại");
                    model.CurrentPhotoId = company.PhotoID;
                    model.CurrentPhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null;
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.Phone) && db.Companies.Any(c => c.Phone == model.Phone && c.CompanyID != model.CompanyId))
                {
                    ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại");
                    model.CurrentPhotoId = company.PhotoID;
                    model.CurrentPhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null;
                    return View(model);
                }

                // Check duplicate email (except current company) with null guards
                if (!string.IsNullOrWhiteSpace(model.CompanyEmail))
                {
                    var emailLower = model.CompanyEmail.ToLowerInvariant();
                    if (db.Companies.Any(c => c.CompanyEmail != null && c.CompanyEmail.ToLower() == emailLower && c.CompanyID != model.CompanyId))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email đã được sử dụng");
                        model.CurrentPhotoId = company.PhotoID;
                        model.CurrentPhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null;
                        return View(model);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.Website))
                {
                    var websiteLower = model.Website.ToLowerInvariant();
                    if (db.Companies.Any(c => c.Website != null && c.Website.ToLower() == websiteLower && c.CompanyID != model.CompanyId))
                    {
                        ModelState.AddModelError("Website", "Website đã tồn tại");
                        model.CurrentPhotoId = company.PhotoID;
                        model.CurrentPhotoUrl = company.ProfilePhoto != null ? company.ProfilePhoto.FilePath : null;
                        return View(model);
                    }
                }

                // Handle photo upload
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    int? newPhotoId = SavePhoto(model.PhotoFile);
                    if (newPhotoId.HasValue)
                    {
                        // Delete old photo if exists
                        if (company.PhotoID.HasValue)
                        {
                            DeletePhoto(company.PhotoID.Value);
                        }
                        company.PhotoID = newPhotoId;
                    }
                }

                // Update account
                company.CompanyName = model.CompanyName;
                company.TaxCode = model.TaxCode;
                company.Industry = model.Industry;
                company.Address = model.Address;
                company.Phone = model.Phone;
                company.CompanyEmail = model.CompanyEmail;
                company.Website = model.Website;
                company.Description = model.Description;
                company.ActiveFlag = model.ActiveFlag;

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

                // Delete photo if exists
                if (company.PhotoID.HasValue)
                {
                    DeletePhoto(company.PhotoID.Value);
                }

                db.Companies.DeleteOnSubmit(company);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Xóa công ty thành công!";
                return RedirectToAction("Index");
            }
        }
        // Helper: Save uploaded photo
        private int? SavePhoto(HttpPostedFileBase file)
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
                TempData["ErrorMessage"] = "Lỗi khi upload ảnh: " + ex.Message;
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
    }
}
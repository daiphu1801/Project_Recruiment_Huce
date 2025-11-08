using Microsoft.Owin.BuilderProperties;
using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    /// <summary>
    /// CRUD controller for recruiters.
    /// </summary>
    public class RecruitersController : AdminBaseController
    {
        // GET: Admin/Recruiters
        public ActionResult Index(string q, string role = null, int page = 1)
        {
            ViewBag.Title = "Quản lý nhà tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Nhà tuyển dụng", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var query = db.Recruiters.AsQueryable();

                // Search
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.Where(r =>
                        (r.FullName != null && r.FullName.Contains(q)) ||
                        (r.CompanyEmail != null && r.CompanyEmail.Contains(q)) ||
                        (r.Phone != null && r.Phone.Contains(q))
                    );
                }

                // Get recruiters and manually join with photos (since PhotoID property may not exist yet)
                var recruitersList = query.ToList();
                var recruiters = recruitersList.Select(r =>
                {
                    var company = db.Companies.FirstOrDefault(c => c.CompanyID == r.CompanyID);
                    int? photoId = GetRecruiterPhotoID(r, db);
                    var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;

                    return new RecruiterListVm
                    {
                        RecruiterId = r.RecruiterID,
                        AccountId = r.AccountID,
                        CompanyId = r.CompanyID,
                        FullName = r.FullName,
                        PositionTitle = r.PositionTitle,
                        CompanyEmail = r.CompanyEmail,
                        Phone = r.Phone,
                        CreatedAt = r.CreatedAt,
                        ActiveFlag = r.ActiveFlag,
                        CompanyName = company != null ? company.CompanyName : null,
                        PhotoId = photoId,
                        PhotoUrl = photo != null ? photo.FilePath : null
                    };
                }).ToList();

                return View(recruiters);
            }
        }

        // GET: Admin/Recruiters/Details/5
        public ActionResult Details(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == id);
                if (recruiter == null) return HttpNotFound();

                var company = db.Companies.FirstOrDefault(c => c.CompanyID == recruiter.CompanyID);
                int? photoId = GetRecruiterPhotoID(recruiter, db);
                var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;

                var vm = new RecruiterListVm
                {
                    RecruiterId = recruiter.RecruiterID,
                    AccountId = recruiter.AccountID,
                    CompanyId = recruiter.CompanyID,
                    FullName = recruiter.FullName,
                    PositionTitle = recruiter.PositionTitle,
                    Phone = recruiter.Phone,
                    CompanyEmail = recruiter.CompanyEmail,
                    CreatedAt = recruiter.CreatedAt,
                    ActiveFlag = recruiter.ActiveFlag,
                    CompanyName = company != null ? company.CompanyName : null,
                    PhotoId = photoId,
                    PhotoUrl = photo != null ? photo.FilePath : null
                };

                ViewBag.Title = "Chi tiết nhà tuyển dụng";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                    new Tuple<string, string>($"#{recruiter.RecruiterID}", null)
                };

                return View(vm);
            }
        }

        // GET: Admin/Recruiters/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm nhà tuyển dụng mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Only show accounts that have role 'Recruiter' and are active
                ViewBag.AccountOptions = new SelectList(
                    db.Accounts
                      .Where(a => a.ActiveFlag == 1 && a.Role == "Recruiter")
                      .Select(a => new { a.AccountID, a.Username })
                      .ToList(),
                    "AccountID",
                    "Username"
                );
                ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName");
            }

            return View();
        }

        // POST: Admin/Recruiters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateRecruiterVm model)
        {
            if (!ModelState.IsValid)
            {
                using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && a.Role == "Recruiter")
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                }
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                if (!string.IsNullOrWhiteSpace(model.FullName) && db.Recruiters.Any(r => r.FullName == model.FullName))
                {
                    ModelState.AddModelError("FullName", "Nhà tuyển dụng đã tồn tại");
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && a.Role == "Recruiter")
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                    return View(model);
                }

                // Validate AccountId uniqueness (DB likely enforces a unique constraint on AccountID)
                if (model.AccountId > 0 && db.Recruiters.Any(r => r.AccountID == model.AccountId))
                {
                    ModelState.AddModelError("AccountId", "Tài khoản này đã được liên kết với nhà tuyển dụng khác");
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && a.Role == "Recruiter")
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.CompanyEmail))
                {
                    var emailLower = model.CompanyEmail.ToLowerInvariant();
                    if (db.Recruiters.Any(r => r.CompanyEmail != null && r.CompanyEmail.ToLower() == emailLower))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email đã được sử dụng");
                        ViewBag.AccountOptions = new SelectList(
                            db.Accounts
                              .Where(a => a.ActiveFlag == 1 && a.Role == "Recruiter")
                              .Select(a => new { a.AccountID, a.Username })
                              .ToList(),
                            "AccountID",
                            "Username",
                            model.AccountId
                        );
                        ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                        return View(model);
                    }
                }

                // Handle photo upload
                int? photoId = null;
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    photoId = SavePhoto(model.PhotoFile);
                }

                var recruiter = new Recruiter
                {
                    AccountID = model.AccountId,
                    CompanyID = model.CompanyId,
                    FullName = model.FullName,
                    PositionTitle = model.PositionTitle,
                    CompanyEmail = model.CompanyEmail,
                    Phone = model.Phone,
                    CreatedAt = DateTime.Now,
                    ActiveFlag = model.Active ? (byte?)1 : (byte?)0
                };

                // Set PhotoID using reflection or direct SQL if property doesn't exist yet
                SetRecruiterPhotoID(recruiter, photoId, db);

                db.Recruiters.InsertOnSubmit(recruiter);
                db.SubmitChanges();

                // If PhotoID property doesn't exist, update it via SQL
                if (photoId.HasValue && !HasPhotoIDProperty())
                {
                    db.ExecuteCommand("UPDATE Recruiters SET PhotoID = {0} WHERE RecruiterID = {1}", photoId.Value, recruiter.RecruiterID);
                }

                TempData["SuccessMessage"] = "Tạo nhà tuyển dụng thành công!";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/Recruiters/Edit/5
        public ActionResult Edit(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == id);
                if (recruiter == null) return HttpNotFound();

                // Only show accounts that have role 'Recruiter' and are active,
                // but include the currently assigned account so the selection isn't lost if its role differs.
                ViewBag.AccountOptions = new SelectList(
                    db.Accounts
                      .Where(a => a.ActiveFlag == 1 && (a.Role == "Recruiter" || a.AccountID == recruiter.AccountID))
                      .Select(a => new { a.AccountID, a.Username })
                      .ToList(),
                    "AccountID",
                    "Username",
                    recruiter.AccountID
                );
                ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", recruiter.CompanyID);

                int? photoId = GetRecruiterPhotoID(recruiter, db);
                var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;

                var vm = new EditRecruiterVm
                {
                    RecruiterId = recruiter.RecruiterID,
                    AccountId = recruiter.AccountID,
                    CompanyId = recruiter.CompanyID,
                    FullName = recruiter.FullName ?? string.Empty,
                    PositionTitle = recruiter.PositionTitle ?? string.Empty,
                    Phone = recruiter.Phone ?? string.Empty,
                    CompanyEmail = recruiter.CompanyEmail ?? string.Empty,
                    ActiveFlag = recruiter.ActiveFlag,
                    Active = recruiter.ActiveFlag == 1,
                    CurrentPhotoId = photoId,
                    CurrentPhotoUrl = photo != null ? photo.FilePath : null
                };

                ViewBag.Title = "Sửa nhà tuyển dụng";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                    new Tuple<string, string>($"#{recruiter.RecruiterID}", null)
                };
                return View(vm);
            }
        }

        // POST: Admin/Recruiters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditRecruiterVm model)
        {
            if (!ModelState.IsValid)
            {
                using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && (a.Role == "Recruiter" || a.AccountID == model.AccountId))
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);

                    var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == model.RecruiterId);
                    if (recruiter != null)
                    {
                        int? photoId = GetRecruiterPhotoID(recruiter, db);
                        var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;
                        model.CurrentPhotoId = photoId;
                        model.CurrentPhotoUrl = photo != null ? photo.FilePath : null;
                    }
                }
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == model.RecruiterId);
                if (recruiter == null) return HttpNotFound();

                if (!string.IsNullOrWhiteSpace(model.FullName) && db.Recruiters.Any(r => r.FullName == model.FullName && r.RecruiterID != model.RecruiterId))
                {
                    ModelState.AddModelError("FullName", "Tên nhà tuyển dụng đã tồn tại");
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && (a.Role == "Recruiter" || a.AccountID == model.AccountId))
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                    int? photoIdErr1 = GetRecruiterPhotoID(recruiter, db);
                    var photoErr1 = photoIdErr1.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoIdErr1.Value) : null;
                    model.CurrentPhotoId = photoIdErr1;
                    model.CurrentPhotoUrl = photoErr1 != null ? photoErr1.FilePath : null;
                    return View(model);
                }

                // Prevent assigning an account that is already linked to another recruiter
                if (model.AccountId > 0 && db.Recruiters.Any(r => r.AccountID == model.AccountId && r.RecruiterID != model.RecruiterId))
                {
                    ModelState.AddModelError("AccountId", "Tài khoản này đã được liên kết với nhà tuyển dụng khác");
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && (a.Role == "Recruiter" || a.AccountID == model.AccountId))
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                    int? photoIdErr2 = GetRecruiterPhotoID(recruiter, db);
                    var photoErr2 = photoIdErr2.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoIdErr2.Value) : null;
                    model.CurrentPhotoId = photoIdErr2;
                    model.CurrentPhotoUrl = photoErr2 != null ? photoErr2.FilePath : null;
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.CompanyEmail))
                {
                    var emailLower = model.CompanyEmail.ToLowerInvariant();
                    if (db.Recruiters.Any(c => c.CompanyEmail != null && c.CompanyEmail.ToLower() == emailLower && c.RecruiterID != model.RecruiterId))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email đã được sử dụng");
                        ViewBag.AccountOptions = new SelectList(
                            db.Accounts
                              .Where(a => a.ActiveFlag == 1 && (a.Role == "Recruiter" || a.AccountID == model.AccountId))
                              .Select(a => new { a.AccountID, a.Username })
                              .ToList(),
                            "AccountID",
                            "Username",
                            model.AccountId
                        );
                        ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                        int? photoIdErr3 = GetRecruiterPhotoID(recruiter, db);
                        var photo = photoIdErr3.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoIdErr3.Value) : null;
                        model.CurrentPhotoId = photoIdErr3;
                        model.CurrentPhotoUrl = photo != null ? photo.FilePath : null;
                        return View(model);
                    }
                }

                // Handle photo upload
                int? newPhotoId = null;
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    newPhotoId = SavePhoto(model.PhotoFile);
                    if (newPhotoId.HasValue)
                    {
                        // Get old photo ID before updating
                        int? oldPhotoId = GetRecruiterPhotoID(recruiter, db);

                        // Delete old photo if exists
                        if (oldPhotoId.HasValue)
                        {
                            DeletePhoto(oldPhotoId.Value);
                        }

                        // Update PhotoID
                        SetRecruiterPhotoID(recruiter, newPhotoId, db);
                    }
                }

                recruiter.AccountID = model.AccountId;
                recruiter.CompanyID = model.CompanyId;
                recruiter.FullName = model.FullName;
                recruiter.PositionTitle = model.PositionTitle;
                recruiter.CompanyEmail = model.CompanyEmail;
                recruiter.Phone = model.Phone;
                recruiter.ActiveFlag = model.Active ? (byte?)1 : (byte?)0;

                db.SubmitChanges();

                // If PhotoID property doesn't exist and we have a new photo, update via SQL
                if (newPhotoId.HasValue && !HasPhotoIDProperty())
                {
                    db.ExecuteCommand("UPDATE Recruiters SET PhotoID = {0} WHERE RecruiterID = {1}", newPhotoId.Value, recruiter.RecruiterID);
                }

                TempData["SuccessMessage"] = "Cập nhật nhà tuyển dụng thành công!";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/Recruiters/Delete/5
        public ActionResult Delete(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == id);
                if (recruiter == null) return HttpNotFound();

                var company = db.Companies.FirstOrDefault(c => c.CompanyID == recruiter.CompanyID);
                int? photoId = GetRecruiterPhotoID(recruiter, db);
                var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;

                var vm = new RecruiterListVm
                {
                    RecruiterId = recruiter.RecruiterID,
                    AccountId = recruiter.AccountID,
                    CompanyId = recruiter.CompanyID,
                    FullName = recruiter.FullName ?? string.Empty,
                    PositionTitle = recruiter.PositionTitle ?? string.Empty,
                    Phone = recruiter.Phone ?? string.Empty,
                    CompanyEmail = recruiter.CompanyEmail ?? string.Empty,
                    CreatedAt = recruiter.CreatedAt,
                    ActiveFlag = recruiter.ActiveFlag,
                    CompanyName = company != null ? company.CompanyName : null,
                    PhotoId = photoId,
                    PhotoUrl = photo != null ? photo.FilePath : null
                };

                ViewBag.Title = "Xóa nhà tuyển dụng";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                    new Tuple<string, string>($"#{recruiter.RecruiterID}", null)
                };

                return View(vm);
            }
        }

        // POST: Admin/Recruiters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == id);
                if (recruiter == null) return HttpNotFound();

                // Delete photo if exists
                int? photoIdToDelete = GetRecruiterPhotoID(recruiter, db);
                if (photoIdToDelete.HasValue)
                {
                    DeletePhoto(photoIdToDelete.Value);
                }

                db.Recruiters.DeleteOnSubmit(recruiter);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Xóa nhà tuyển dụng thành công!";
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

        // Helper: Check if PhotoID property exists on Recruiter class
        private bool HasPhotoIDProperty()
        {
            try
            {
                var property = typeof(Recruiter).GetProperty("PhotoID");
                return property != null;
            }
            catch
            {
                return false;
            }
        }

        // Helper: Get PhotoID from Recruiter (works with or without property)
        private int? GetRecruiterPhotoID(Recruiter recruiter, JOBPORTAL_ENDataContext db)
        {
            if (HasPhotoIDProperty())
            {
                var property = typeof(Recruiter).GetProperty("PhotoID");
                var value = property.GetValue(recruiter);
                return value as int?;
            }
            else
            {
                // Use SQL query to get PhotoID
                var result = db.ExecuteQuery<int?>("SELECT PhotoID FROM Recruiters WHERE RecruiterID = {0}", recruiter.RecruiterID).FirstOrDefault();
                return result;
            }
        }

        // Helper: Set PhotoID on Recruiter (works with or without property)
        private void SetRecruiterPhotoID(Recruiter recruiter, int? photoId, JOBPORTAL_ENDataContext db)
        {
            if (HasPhotoIDProperty())
            {
                var property = typeof(Recruiter).GetProperty("PhotoID");
                property.SetValue(recruiter, photoId);
            }
            else
            {
                // Will be set via SQL after SubmitChanges if property doesn't exist
                // This is handled in the calling code
            }
        }
    }
}
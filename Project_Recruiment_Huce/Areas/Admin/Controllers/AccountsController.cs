using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class AccountsController : AdminBaseController
    {
        // GET: Admin/Accounts
        public ActionResult Index(string q, string role = null, int page = 1)
        {
            ViewBag.Title = "Quản lý tài khoản";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Tài khoản", null)
            };

            using (var db = new JOBPROTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var query = db.Accounts.AsQueryable();

                // Search
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.Where(a => 
                        (a.Username != null && a.Username.Contains(q)) ||
                        (a.Email != null && a.Email.Contains(q)) ||
                        (a.Phone != null && a.Phone.Contains(q))
                    );
                }

                // Filter by role
                if (!string.IsNullOrWhiteSpace(role))
                {
                    query = query.Where(a => a.Role == role);
                }

                // Convert to ViewModel
                var accounts = query.Select(a => new AccountListVm
                {
                    AccountId = a.AccountId,
                    Username = a.Username,
                    Email = a.Email,
                    Phone = a.Phone,
                    Role = a.Role,
                    Active = (a.ActiveFlag ?? 0) == 1,
                    CreatedAt = a.CreatedAt ?? DateTime.Now,
                    PhotoUrl = a.Photo != null ? a.Photo.FilePath : null
                }).ToList();

                ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Company", "Recruiter", "Candidate" });
                return View(accounts);
            }
        }

        // GET: Admin/Accounts/Details/5
        public ActionResult Details(int id)
        {
            using (var db = new JOBPROTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountId == id);
                if (account == null) return HttpNotFound();

                var vm = new AccountListVm
                {
                    AccountId = account.AccountId,
                    Username = account.Username,
                    Email = account.Email,
                    Phone = account.Phone,
                    Role = account.Role,
                    Active = (account.ActiveFlag ?? 0) == 1,
                    CreatedAt = account.CreatedAt ?? DateTime.Now,
                    PhotoUrl = account.Photo != null ? account.Photo.FilePath : null
                };

                ViewBag.Title = "Chi tiết tài khoản";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Tài khoản", Url.Action("Index")),
                    new Tuple<string, string>($"#{account.AccountId}", null)
                };

                return View(vm);
            }
        }

        // GET: Admin/Accounts/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm tài khoản mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Tài khoản", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };

            ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Company", "Recruiter", "Candidate" });
            return View();
        }

        // POST: Admin/Accounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateAccountVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Company", "Recruiter", "Candidate" });
                return View(model);
            }

            using (var db = new JOBPROTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Check duplicate username
                if (db.Accounts.Any(a => a.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Company", "Recruiter", "Candidate" });
                    return View(model);
                }

                // Check duplicate email
                if (db.Accounts.Any(a => a.Email.ToLower() == model.Email.ToLower()))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Company", "Recruiter", "Candidate" });
                    return View(model);
                }

                // Handle photo upload
                int? photoId = null;
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    photoId = SavePhoto(model.PhotoFile);
                }

                // Create account
                var account = new Account
                {
                    Username = model.Username,
                    Email = model.Email,
                    Phone = model.Phone,
                    Role = model.Role,
                    PasswordHash = PasswordHelper.HashPassword(model.Password),
                    ActiveFlag = 1,
                    CreatedAt = DateTime.Now,
                    PhotoId = photoId
                };

                db.Accounts.InsertOnSubmit(account);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Tạo tài khoản thành công!";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/Accounts/Edit/5
        public ActionResult Edit(int id)
        {
            using (var db = new JOBPROTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountId == id);
                if (account == null) return HttpNotFound();

                var vm = new EditAccountVm
                {
                    AccountId = account.AccountId,
                    Username = account.Username,
                    Email = account.Email,
                    Phone = account.Phone,
                    Role = account.Role,
                    Active = (account.ActiveFlag ?? 0) == 1,
                    CurrentPhotoUrl = account.Photo != null ? account.Photo.FilePath : null
                };

                ViewBag.Title = "Sửa tài khoản";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Tài khoản", Url.Action("Index")),
                    new Tuple<string, string>($"#{account.AccountId}", null)
                };

                ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Company", "Recruiter", "Candidate" });
                return View(vm);
            }
        }

        // POST: Admin/Accounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditAccountVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Company", "Recruiter", "Candidate" });
                return View(model);
            }

            using (var db = new JOBPROTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountId == model.AccountId);
                if (account == null) return HttpNotFound();

                // Check duplicate username (except current account)
                if (db.Accounts.Any(a => a.Username == model.Username && a.AccountId != model.AccountId))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Company", "Recruiter", "Candidate" });
                    model.CurrentPhotoUrl = account.Photo != null ? account.Photo.FilePath : null;
                    return View(model);
                }

                // Check duplicate email (except current account)
                if (db.Accounts.Any(a => a.Email.ToLower() == model.Email.ToLower() && a.AccountId != model.AccountId))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Company", "Recruiter", "Candidate" });
                    model.CurrentPhotoUrl = account.Photo != null ? account.Photo.FilePath : null;
                    return View(model);
                }

                // Handle photo upload
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    int? newPhotoId = SavePhoto(model.PhotoFile);
                    if (newPhotoId.HasValue)
                    {
                        // Delete old photo if exists
                        if (account.PhotoId.HasValue)
                        {
                            DeletePhoto(account.PhotoId.Value);
                        }
                        account.PhotoId = newPhotoId;
                    }
                }

                // Update account
                account.Username = model.Username;
                account.Email = model.Email;
                account.Phone = model.Phone;
                account.Role = model.Role;
                account.ActiveFlag = (byte)(model.Active ? 1 : 0);

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/Accounts/Delete/5
        public ActionResult Delete(int id)
        {
            using (var db = new JOBPROTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountId == id);
                if (account == null) return HttpNotFound();

                var vm = new AccountListVm
                {
                    AccountId = account.AccountId,
                    Username = account.Username,
                    Email = account.Email,
                    Phone = account.Phone,
                    Role = account.Role,
                    Active = (account.ActiveFlag ?? 0) == 1,
                    CreatedAt = account.CreatedAt ?? DateTime.Now,
                    PhotoUrl = account.Photo != null ? account.Photo.FilePath : null
                };

                ViewBag.Title = "Xóa tài khoản";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Tài khoản", Url.Action("Index")),
                    new Tuple<string, string>($"#{account.AccountId}", null)
                };

                return View(vm);
            }
        }

        // POST: Admin/Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new JOBPROTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountId == id);
                if (account == null) return HttpNotFound();

                // Delete photo if exists
                if (account.PhotoId.HasValue)
                {
                    DeletePhoto(account.PhotoId.Value);
                }

                db.Accounts.DeleteOnSubmit(account);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Xóa tài khoản thành công!";
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

                // Save to database
                using (var db = new JOBPROTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    var photo = new Photo
                    {
                        FileName = file.FileName,
                        FilePath = "/Content/Uploads/Photos/" + fileName,
                        SizeKB = file.ContentLength / 1024,
                        MimeType = file.ContentType,
                        UploadedAt = DateTime.Now
                    };

                    db.Photos.InsertOnSubmit(photo);
                    db.SubmitChanges();
                    return photo.PhotoId;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi upload ảnh: " + ex.Message;
                return null;
            }
        }

        // Helper: Delete photo
        private void DeletePhoto(int photoId)
        {
            try
            {
                using (var db = new JOBPROTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    var photo = db.Photos.FirstOrDefault(p => p.PhotoId == photoId);
                    if (photo == null) return;

                    // Delete physical file
                    var filePath = Server.MapPath("~" + photo.FilePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Delete database record
                    db.Photos.DeleteOnSubmit(photo);
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

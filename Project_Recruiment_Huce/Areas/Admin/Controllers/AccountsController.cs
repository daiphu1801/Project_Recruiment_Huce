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
    /// <summary>
    /// Base CRUD Controller - Template for implementing other controllers with database.
    /// This controller demonstrates full CRUD operations (Create, Read, Update, Delete) 
    /// using JOBPORTAL_ENDataContext. Other controllers should follow this pattern.
    /// </summary>
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

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
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

                // Pagination
                int pageSize = 10;
                int totalRecords = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var accounts = query
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new AccountListVm
                    {
                        AccountId = a.AccountID,
                        Username = a.Username,
                        Email = a.Email,
                        Phone = a.Phone,
                        Role = a.Role,
                        ActiveFlag = a.ActiveFlag,
                        CreatedAt = a.CreatedAt,
                        PhotoId = a.PhotoID,
                        PhotoUrl = a.ProfilePhoto != null ? a.ProfilePhoto.FilePath : null
                    }).ToList();

                ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Recruiter", "Candidate" });

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalRecords;
                ViewBag.PageSize = pageSize;

                return View(accounts);
            }
        }

        // GET: Admin/Accounts/Details/5
        public ActionResult Details(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == id);
                if (account == null) return HttpNotFound();

                var vm = new AccountListVm
                {
                    AccountId = account.AccountID,
                    Username = account.Username,
                    Email = account.Email,
                    Phone = account.Phone,
                    Role = account.Role,
                    ActiveFlag = account.ActiveFlag,
                    CreatedAt = account.CreatedAt,
                    PhotoId = account.PhotoID,
                    PhotoUrl = account.ProfilePhoto != null ? account.ProfilePhoto.FilePath : null
                };

                ViewBag.Title = "Chi tiết tài khoản";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Tài khoản", Url.Action("Index")),
                    new Tuple<string, string>($"#{account.AccountID}", null)
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

            ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Recruiter", "Candidate" });
            return View();
        }

        // POST: Admin/Accounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateAccountVm model)
        {
            ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Recruiter", "Candidate" });
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Check duplicate username
                if (db.Accounts.Any(a => a.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Check duplicate email
                if (db.Accounts.Any(a => a.Email.ToLower() == model.Email.ToLower()))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
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
                    if (!ValidationHelper.IsAccountPhoneUnique(phone))
                    {
                        ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng. Mỗi số điện thoại chỉ có thể đăng ký một tài khoản.");
                        return View(model);
                    }
                }
                else
                {
                    phone = null;
                }

                // Validate password not null
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError("Password", "Mật khẩu không được để trống");
                    return View(model);
                }

                // Hash password sử dụng PBKDF2 (không cần salt riêng)
                string passwordHash = PasswordHelper.HashPassword(model.Password);

                // Create account
                var account = new Account
                {
                    Username = model.Username,
                    Email = model.Email,
                    Phone = phone, // Use normalized phone
                    Role = model.Role,
                    PasswordHash = passwordHash,
                    ActiveFlag = 1,
                    CreatedAt = DateTime.Now,
                };

                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    ProfilePhoto photo = SavePhoto(db, model.PhotoFile);
                    if (photo != null)
                    {
                        account.ProfilePhoto = photo;
                    }
                    else
                    {
                        return View(model);
                    }
                }

                db.Accounts.InsertOnSubmit(account);

                try
                {
                    db.SubmitChanges();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu tài khoản: " + ex.Message);
                    if (account.ProfilePhoto != null)
                    {
                        var filePath = Server.MapPath("~" + account.ProfilePhoto.FilePath);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    return View(model);
                }


                // Tự động tạo profile Candidate/Recruiter (không set email - email trong profile là email liên lạc riêng)
                if (model.Role == "Candidate" || model.Role == "Recruiter")
                {
                    EmailSyncHelper.CreateProfile(db, account.AccountID, model.Role);
                    db.SubmitChanges();
                }

                TempData["SuccessMessage"] = "Tạo tài khoản thành công!";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/Accounts/Edit/5
        public ActionResult Edit(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == id);
                if (account == null) return HttpNotFound();

                var vm = new EditAccountVm
                {
                    AccountId = account.AccountID,
                    Username = account.Username ?? string.Empty,
                    Email = account.Email ?? string.Empty,
                    Phone = account.Phone ?? string.Empty,
                    Role = account.Role ?? string.Empty,
                    ActiveFlag = account.ActiveFlag,
                    CurrentPhotoId = account.PhotoID ?? 0
                };

                ViewBag.Title = "Sửa tài khoản";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Tài khoản", Url.Action("Index")),
                    new Tuple<string, string>($"#{account.AccountID}", null)
                };

                ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Recruiter", "Candidate" });
                return View(vm);
            }
        }

        // POST: Admin/Accounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditAccountVm model)
        {
            ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Recruiter", "Candidate" });
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == model.AccountId);
                if (account == null) return HttpNotFound();

                model.CurrentPhotoId = account.PhotoID;

                // Check duplicate username (except current account)
                if (db.Accounts.Any(a => a.Username == model.Username && a.AccountID != model.AccountId))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Check duplicate email (except current account)
                if (db.Accounts.Any(a => a.Email.ToLower() == model.Email.ToLower() && a.AccountID != model.AccountId))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
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

                    // Check if phone already exists (except current account)
                    if (!ValidationHelper.IsAccountPhoneUnique(phone, model.AccountId))
                    {
                        ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng. Mỗi số điện thoại chỉ có thể đăng ký một tài khoản.");
                        return View(model);
                    }
                }
                else
                {
                    phone = null;
                }

                if (account.Role == "Recruiter" && model.Role != "Recruiter")
                {
                    var linkedRecruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == account.AccountID);

                    if (linkedRecruiter != null && linkedRecruiter.CompanyID.HasValue)
                    {
                        ModelState.AddModelError("Role", "Tài khoản này đang quản lý một Công ty. Không thể thay đổi vai trò trừ khi công ty đó bị xóa.");
                        return View(model);
                    }
                }

                // Handle photo upload
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    if (account.PhotoID.HasValue)
                    {
                        DeletePhoto(db, account.PhotoID.Value);
                    }

                    ProfilePhoto newPhoto = SavePhoto(db, model.PhotoFile);
                    if (newPhoto != null)
                    {
                        account.ProfilePhoto = newPhoto;
                    }
                    else
                    {
                        return View(model);
                    }
                }

                // Update account
                // NOTE: Email trong Account dùng để đăng nhập, không đồng bộ với profile
                account.Username = model.Username;
                account.Email = model.Email;
                account.Phone = phone; // Use normalized phone
                account.Role = model.Role;
                account.ActiveFlag = model.ActiveFlag ?? (byte)1; // Cast byte? to byte

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    // Hash password sử dụng PBKDF2 (không cần salt riêng)
                    account.PasswordHash = PasswordHelper.HashPassword(model.Password);
                }

                // Đồng bộ dữ liệu
                if (account.Role == "Recruiter")
                {
                    var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == account.AccountID);
                    if (recruiter != null)
                    {
                        recruiter.Phone = phone; // Use normalized phone
                        recruiter.ActiveFlag = model.ActiveFlag ?? (byte)1; // Cast byte? to byte
                    }
                }
                else if (account.Role == "Candidate")
                {
                    var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == account.AccountID);
                    if (candidate != null)
                    {
                        candidate.Phone = phone; // Use normalized phone
                        candidate.ActiveFlag = model.ActiveFlag ?? (byte)1; // Cast byte? to byte
                    }
                }

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Delete(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == id);
                if (account == null) return HttpNotFound();

                var vm = new AccountListVm
                {
                    AccountId = account.AccountID,
                    Username = account.Username,
                    Email = account.Email,
                    Phone = account.Phone,
                    Role = account.Role,
                    ActiveFlag = account.ActiveFlag,
                    CreatedAt = account.CreatedAt,
                    PhotoId = account.PhotoID,
                    PhotoUrl = account.ProfilePhoto != null ? account.ProfilePhoto.FilePath : null
                };

                ViewBag.Title = "Xóa tài khoản";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Tài khoản", Url.Action("Index")),
                    new Tuple<string, string>($"#{account.AccountID}", null)
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
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == id);
                if (account == null) return HttpNotFound();

                // Delete photo if exists
                if (account.PhotoID.HasValue)
                {
                    DeletePhoto(db, account.PhotoID.Value);
                }

                db.Accounts.DeleteOnSubmit(account);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Xóa tài khoản thành công!";
                return RedirectToAction("Index");
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
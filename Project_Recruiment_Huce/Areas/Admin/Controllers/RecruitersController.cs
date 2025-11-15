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
                        (r.Phone != null && r.Phone.Contains(q)) ||
                        (r.Account.Username != null && r.Account.Username.Contains(q)) ||
                        (r.Account.Email != null && r.Account.Email.Contains(q))
                    );
                }

                // Get recruiters and manually join with photos
                var recruitersList = query.ToList();
                var recruiters = recruitersList.Select(r =>
                {
                    var company = db.Companies.FirstOrDefault(c => c.CompanyID == r.CompanyID);
                    var account = db.Accounts.FirstOrDefault(a => a.AccountID == r.AccountID);
                    int? photoId = account?.PhotoID;
                    var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;

                    return new RecruiterListVm
                    {
                        RecruiterId = r.RecruiterID,
                        AccountId = r.AccountID,
                        Username = account.Username,
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

        // ... (Toàn bộ các hàm Details, Create, Edit, Delete, SavePhoto, DeletePhoto giữ nguyên) ...
        // GET: Admin/Recruiters/Details/5
        public ActionResult Details(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == id);
                if (recruiter == null) return HttpNotFound();

                var company = db.Companies.FirstOrDefault(c => c.CompanyID == recruiter.CompanyID);
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == recruiter.AccountID);
                int? photoId = account?.PhotoID;
                var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;

                var vm = new RecruiterListVm
                {
                    RecruiterId = recruiter.RecruiterID,
                    AccountId = recruiter.AccountID,
                    Username = account.Username,
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
                ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName");
            }

            return View(new CreateRecruiterVm { Active = true }); // Khởi tạo giá trị Active
        }

        // POST: Admin/Recruiters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateRecruiterVm model)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);

                // Validation cho các trường Account mới
                if (db.Accounts.Any(a => a.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                }
                if (db.Accounts.Any(a => a.Email.ToLower() == model.Email.ToLower()))
                {
                    ModelState.AddModelError("Email", "Email (login) đã được sử dụng");
                }

                // Validate phone number format and uniqueness (if provided)
                var phone = (model.Phone ?? string.Empty).Trim();
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

                        // Check if phone already exists in Accounts
                        if (!ValidationHelper.IsAccountPhoneUnique(phone))
                        {
                            ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng. Mỗi số điện thoại chỉ có thể đăng ký một tài khoản.");
                        }
                    }
                }
                else
                {
                    phone = null;
                }

                // Validate company email format and uniqueness (if provided)
                var companyEmail = (model.CompanyEmail ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(companyEmail))
                {
                    // Validate email format
                    if (!ValidationHelper.IsValidEmail(companyEmail))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email không hợp lệ.");
                    }
                    else
                    {
                        // Check if email already exists in Recruiters
                        var emailLower = companyEmail.ToLowerInvariant();
                        if (db.Recruiters.Any(r => r.CompanyEmail != null && r.CompanyEmail.ToLower() == emailLower))
                        {
                            ModelState.AddModelError("CompanyEmail", "Email (liên lạc) đã được sử dụng");
                        }
                    }
                }
                else
                {
                    companyEmail = null;
                }

                if (!string.IsNullOrWhiteSpace(model.FullName) && db.Recruiters.Any(r => r.FullName == model.FullName))
                {
                    ModelState.AddModelError("FullName", "Tên nhà tuyển dụng (Họ tên) đã tồn tại");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Tạo Account mới
                string salt = PasswordHelper.GenerateSalt();
                string passwordHash = PasswordHelper.HashPassword(model.Password, salt);

                var account = new Account
                {
                    Username = model.Username,
                    Email = model.Email,
                    Phone = phone, // Use normalized phone
                    Role = "Recruiter",
                    PasswordHash = passwordHash,
                    Salt = salt,
                    ActiveFlag = model.Active ? (byte)1 : (byte)0,
                    CreatedAt = DateTime.Now
                };

                // [FIX] Xử lý ảnh (lưu vào Account)
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    ProfilePhoto photo = SavePhoto(db, model.PhotoFile); // [FIX] Pass 'db'
                    if (photo != null)
                    {
                        account.ProfilePhoto = photo; // [FIX] Gán thực thể
                    }
                    else
                    {
                        return View(model);
                    }
                }

                db.Accounts.InsertOnSubmit(account);

                // Tạo Recruiter mới và liên kết với Account
                var recruiter = new Recruiter
                {
                    Account = account, // Liên kết trực tiếp
                    CompanyID = model.CompanyId,
                    FullName = model.FullName,
                    PositionTitle = model.PositionTitle,
                    CompanyEmail = companyEmail, // Use validated email
                    Phone = phone, // Use normalized phone
                    CreatedAt = DateTime.Now,
                    ActiveFlag = model.Active ? (byte)1 : (byte)0
                };

                db.Recruiters.InsertOnSubmit(recruiter);

                // [FIX] Submit 1 lần duy nhất
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Tạo nhà tuyển dụng và tài khoản thành công!";
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

                // Lấy Account liên kết
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == recruiter.AccountID);
                if (account == null)
                {
                    TempData["ErrorMessage"] = "Lỗi: Không tìm thấy tài khoản liên kết.";
                    return RedirectToAction("Index");
                }

                ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", recruiter.CompanyID);

                int? photoId = account.PhotoID;
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
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);

                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == model.RecruiterId);
                if (recruiter == null) return HttpNotFound();

                var accountRecord = db.Accounts.FirstOrDefault(a => a.AccountID == recruiter.AccountID);
                if (accountRecord == null)
                {
                    TempData["ErrorMessage"] = "Lỗi: Không tìm thấy tài khoản liên kết.";
                    return View(model);
                }

                // Gán lại ảnh phòng trường hợp validation fail
                model.CurrentPhotoId = accountRecord.PhotoID;
                model.CurrentPhotoUrl = accountRecord.ProfilePhoto?.FilePath;

                // Validate phone number format and uniqueness (if provided)
                var phone = (model.Phone ?? string.Empty).Trim();
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

                        // Check if phone already exists in Accounts (except current account)
                        if (!ValidationHelper.IsAccountPhoneUnique(phone, accountRecord.AccountID))
                        {
                            ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng. Mỗi số điện thoại chỉ có thể đăng ký một tài khoản.");
                        }
                    }
                }
                else
                {
                    phone = null;
                }

                // Validate company email format and uniqueness (if provided)
                var companyEmail = (model.CompanyEmail ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(companyEmail))
                {
                    // Validate email format
                    if (!ValidationHelper.IsValidEmail(companyEmail))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email không hợp lệ.");
                    }
                    else
                    {
                        // Check if email already exists in Recruiters (except current recruiter)
                        var emailLower = companyEmail.ToLowerInvariant();
                        if (db.Recruiters.Any(c => c.CompanyEmail != null && c.CompanyEmail.ToLower() == emailLower && c.RecruiterID != model.RecruiterId))
                        {
                            ModelState.AddModelError("CompanyEmail", "Email (liên lạc) đã được sử dụng");
                        }
                    }
                }
                else
                {
                    companyEmail = null;
                }

                // Validation
                if (db.Accounts.Any(a => a.Username == model.FullName && a.AccountID != accountRecord.AccountID))
                {
                    ModelState.AddModelError("FullName", "Tên (Username) này đã được tài khoản khác sử dụng");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // [FIX] Handle photo upload
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    // [FIX] Xóa ảnh cũ khỏi context
                    if (accountRecord.PhotoID.HasValue)
                    {
                        DeletePhoto(db, accountRecord.PhotoID.Value); // Pass 'db'
                    }

                    // [FIX] Thêm ảnh mới vào context
                    ProfilePhoto newPhoto = SavePhoto(db, model.PhotoFile); // Pass 'db'
                    if (newPhoto != null)
                    {
                        accountRecord.ProfilePhoto = newPhoto; // [FIX] Gán thực thể
                    }
                    else
                    {
                        return View(model);
                    }
                }

                // Cập nhật Account
                accountRecord.Username = model.FullName;
                accountRecord.Phone = phone; // Use normalized phone
                accountRecord.ActiveFlag = model.Active ? (byte)1 : (byte)0;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    string salt = PasswordHelper.GenerateSalt();
                    accountRecord.PasswordHash = PasswordHelper.HashPassword(model.Password, salt);
                    accountRecord.Salt = salt;
                }

                // Cập nhật Recruiter
                recruiter.CompanyID = model.CompanyId;
                recruiter.FullName = model.FullName;
                recruiter.PositionTitle = model.PositionTitle;
                recruiter.CompanyEmail = companyEmail; // Use validated email
                recruiter.Phone = phone; // Use normalized phone
                recruiter.ActiveFlag = model.Active ? (byte)1 : (byte)0;

                // [FIX] Submit 1 lần duy nhất
                db.SubmitChanges();

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
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == recruiter.AccountID);
                int? photoId = account?.PhotoID;
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
                    PhotoUrl = photo != null ? photo.FilePath : null,
                    Username = account.Username,
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

                var account = db.Accounts.FirstOrDefault(a => a.AccountID == recruiter.AccountID);

                if (account != null)
                {
                    // [FIX] Xóa ảnh thủ công trước
                    if (account.PhotoID.HasValue)
                    {
                        DeletePhoto(db, account.PhotoID.Value); // Pass 'db'
                    }
                    db.Accounts.DeleteOnSubmit(account);
                }
                else
                {
                    db.Recruiters.DeleteOnSubmit(recruiter);
                }

                db.SubmitChanges(); // Submit 1 lần

                TempData["SuccessMessage"] = "Xóa nhà tuyển dụng (và tài khoản liên kết) thành công!";
                return RedirectToAction("Index");
            }
        }

        // [FIX] Sửa hàm SavePhoto
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

                // [FIX] Save to database - ProfilePhotos table
                var photo = new ProfilePhoto
                {
                    FileName = file.FileName,
                    FilePath = "/Content/Uploads/Photos/" + fileName,
                    FileSizeKB = file.ContentLength / 1024,
                    FileFormat = fileExt.Replace(".", ""),
                    UploadedAt = DateTime.Now
                };

                db.ProfilePhotos.InsertOnSubmit(photo);
                // [FIX] KHÔNG gọi SubmitChanges() ở đây
                return photo; // [FIX] Trả về thực thể
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi upload ảnh: " + ex.Message;
                return null;
            }
        }

        // [FIX] Sửa hàm DeletePhoto
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
                // [FIX] KHÔNG gọi SubmitChanges() ở đây
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa ảnh: " + ex.Message;
            }
        }

    }
}
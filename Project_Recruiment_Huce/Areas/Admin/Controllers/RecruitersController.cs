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
                    Username = recruiter.Account.Username,
                    CompanyId = recruiter.CompanyID,
                    FullName = recruiter.FullName,
                    PositionTitle = recruiter.PositionTitle,
                    Phone = recruiter.Phone,
                    CompanyEmail = recruiter.CompanyEmail,
                    CreatedAt = recruiter.CreatedAt,
                    ActiveFlag = recruiter.ActiveFlag,
                    CompanyName = recruiter.Company != null ? recruiter.Company.CompanyName : "Chưa có công ty",
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

            return View(new CreateRecruiterVm { Active = true });
        }

        // POST: Admin/Recruiters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateRecruiterVm model)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);

                if (model.CompanyId.HasValue)
                {
                    bool isCompanyTaken = db.Recruiters.Any(r => r.CompanyID == model.CompanyId.Value);
                    if (isCompanyTaken)
                    {
                        ModelState.AddModelError("CompanyId", "Công ty này đã có nhà tuyển dụng.");
                        return View(model);
                    }
                }

                if (db.Accounts.Any(a => a.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                }
                if (db.Accounts.Any(a => a.Email.ToLower() == model.Email.ToLower()))
                {
                    ModelState.AddModelError("Email", "Email (login) đã được sử dụng");
                }

                var phone = (model.Phone ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    if (!ValidationHelper.IsValidVietnamesePhone(phone))
                    {
                        ModelState.AddModelError("Phone", ValidationHelper.GetPhoneErrorMessage());
                    }
                    else
                    {
                        phone = ValidationHelper.NormalizePhone(phone);

                        if (!ValidationHelper.IsAccountPhoneUnique(phone))
                        {
                            ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng.");
                        }
                    }
                }
                else
                {
                    phone = null;
                }

                if (!string.IsNullOrWhiteSpace(model.CompanyEmail))
                {
                    var emailLower = model.CompanyEmail.ToLowerInvariant();
                    if (db.Recruiters.Any(r => r.CompanyEmail != null && r.CompanyEmail.ToLower() == emailLower))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email (liên lạc) đã được sử dụng");
                    }
                }

                if (db.Accounts.Any(a => a.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại");
                    return View(model);
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Hash password sử dụng PBKDF2 (không cần salt riêng)
                string passwordHash = PasswordHelper.HashPassword(model.Password);

                var account = new Account
                {
                    Username = model.Username,
                    Email = model.Email,
                    Phone = model.Phone,
                    Role = "Recruiter",
                    PasswordHash = passwordHash,
                    ActiveFlag = model.Active ? (byte)1 : (byte)0,
                    CreatedAt = DateTime.Now
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

                var recruiter = new Recruiter
                {
                    Account = account,
                    CompanyID = model.CompanyId,
                    FullName = model.FullName,
                    PositionTitle = model.PositionTitle,
                    CompanyEmail = model.CompanyEmail,
                    Phone = model.Phone,
                    CreatedAt = DateTime.Now,
                    ActiveFlag = model.Active ? (byte)1 : (byte)0
                };

                db.Recruiters.InsertOnSubmit(recruiter);

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
                    Username = account.Username,
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

                if (model.CompanyId.HasValue)
                {
                    bool isCompanyTaken = db.Recruiters.Any(r => r.CompanyID == model.CompanyId.Value && r.RecruiterID != model.RecruiterId);
                    if (isCompanyTaken)
                    {
                        ModelState.AddModelError("CompanyId", "Công ty này đã có nhà tuyển dụng.");
                        return View(model);
                    }
                }

                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == model.RecruiterId);
                if (recruiter == null) return HttpNotFound();

                var accountRecord = db.Accounts.FirstOrDefault(a => a.AccountID == recruiter.AccountID);
                if (accountRecord == null)
                {
                    TempData["ErrorMessage"] = "Lỗi: Không tìm thấy tài khoản liên kết.";
                    return View(model);
                }

                model.CurrentPhotoId = accountRecord.PhotoID;
                model.CurrentPhotoUrl = accountRecord.ProfilePhoto?.FilePath;

                var phone = (model.Phone ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    if (!ValidationHelper.IsValidVietnamesePhone(phone))
                    {
                        ModelState.AddModelError("Phone", ValidationHelper.GetPhoneErrorMessage());
                    }
                    else
                    {
                        phone = ValidationHelper.NormalizePhone(phone);

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

                if (db.Accounts.Any(a => a.Username == model.Username && a.AccountID != accountRecord.AccountID))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã được tài khoản khác sử dụng");
                }

                if (!string.IsNullOrWhiteSpace(model.CompanyEmail))
                {
                    var emailLower = model.CompanyEmail.ToLowerInvariant();
                    if (db.Recruiters.Any(c => c.CompanyEmail != null && c.CompanyEmail.ToLower() == emailLower && c.RecruiterID != model.RecruiterId))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email (liên lạc) đã được sử dụng");
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Handle photo upload
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    if (accountRecord.PhotoID.HasValue)
                    {
                        DeletePhoto(db, accountRecord.PhotoID.Value);
                    }

                    ProfilePhoto newPhoto = SavePhoto(db, model.PhotoFile);
                    if (newPhoto != null)
                    {
                        accountRecord.ProfilePhoto = newPhoto;
                    }
                    else
                    {
                        return View(model);
                    }
                }

                accountRecord.Username = model.Username;
                accountRecord.Phone = model.Phone;
                accountRecord.ActiveFlag = model.Active ? (byte)1 : (byte)0;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    // Hash password sử dụng PBKDF2 (không cần salt riêng)
                    accountRecord.PasswordHash = PasswordHelper.HashPassword(model.Password);
                }

                recruiter.CompanyID = model.CompanyId;
                recruiter.FullName = model.FullName;
                recruiter.PositionTitle = model.PositionTitle;
                recruiter.CompanyEmail = model.CompanyEmail;
                recruiter.Phone = model.Phone;
                recruiter.ActiveFlag = model.Active ? (byte)1 : (byte)0;

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
                    PhotoId = recruiter.PhotoID,
                    PhotoUrl = recruiter.ProfilePhoto != null ? recruiter.ProfilePhoto.FilePath : null,
                    Username = recruiter.Account.Username,
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
                    if (account.PhotoID.HasValue)
                    {
                        DeletePhoto(db, account.PhotoID.Value);
                    }
                    db.Accounts.DeleteOnSubmit(account);
                }
                else
                {
                    db.Recruiters.DeleteOnSubmit(recruiter);
                }

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Xóa nhà tuyển dụng (và tài khoản liên kết) thành công!";
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
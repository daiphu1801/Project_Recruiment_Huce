using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Helpers; // Sử dụng các Helper như ValidationHelper, PasswordHelper
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class CandidatesController : AdminBaseController
    {
        // GET: Admin/Candidates
        public ActionResult Index(string q, int page = 1)
        {
            ViewBag.Title = "Quản lý ứng viên";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Người ứng tuyển", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var query = db.Candidates.AsQueryable();

                // Search logic
                if (!string.IsNullOrWhiteSpace(q))
                {
                    var searchKeyword = q.ToLower();
                    query = query.Where(c =>
                        (c.FullName != null && c.FullName.ToLower().Contains(searchKeyword)) ||
                        (c.Email != null && c.Email.ToLower().Contains(searchKeyword)) ||
                        (c.Phone != null && c.Phone.Contains(searchKeyword))
                    );
                }
                int pageSize = 10;
                int totalRecords = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var candidatesPage = query.OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var vmList = candidatesPage.Select(c =>
                {
                    // Lấy Account tương ứng để lấy thông tin đăng nhập và ảnh
                    var account = db.Accounts.FirstOrDefault(a => a.AccountID == c.AccountID);
                    var photoUrl = account?.ProfilePhoto?.FilePath;

                    return new CandidatesListVm
                    {
                        CandidateId = c.CandidateID,
                        AccountId = c.AccountID,
                        FullName = c.FullName,
                        DateOfBirth = c.BirthDate,
                        Gender = c.Gender,
                        Phone = c.Phone,
                        PhotoId = account?.PhotoID,
                        CreatedAt = c.CreatedAt,
                        ActiveFlag = c.ActiveFlag,
                        Email = c.Email,
                        Address = c.Address,
                        PhotoUrl = photoUrl,
                        Summary = c.Summary,
                        ApplicationEmail = c.ApplicationEmail,
                        UserName = account?.Username
                    };
                }).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalRecords;
                ViewBag.PageSize = pageSize;

                return View(vmList);
            }
        }

        // GET: Admin/Candidates/Details/5
        public ActionResult Details(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(x => x.CandidateID == id);
                if (candidate == null) return HttpNotFound();

                var account = db.Accounts.FirstOrDefault(a => a.AccountID == candidate.AccountID);

                var vm = new CandidatesListVm
                {
                    CandidateId = candidate.CandidateID,
                    AccountId = candidate.AccountID,
                    FullName = candidate.FullName,
                    DateOfBirth = candidate.BirthDate,
                    Gender = candidate.Gender,
                    Phone = candidate.Phone,
                    PhotoId = account?.PhotoID,
                    CreatedAt = candidate.CreatedAt,
                    ActiveFlag = candidate.ActiveFlag,
                    Email = candidate.Email,
                    Address = candidate.Address,
                    PhotoUrl = account?.ProfilePhoto?.FilePath,
                    Summary = candidate.Summary,
                    ApplicationEmail = candidate.ApplicationEmail,
                    UserName = account?.Username
                };

                ViewBag.Title = "Chi tiết ứng viên";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                    new Tuple<string, string>("Ứng viên", Url.Action("Index")),
                    new Tuple<string, string>($"#{candidate.CandidateID}", null)
                };
                return View(vm);
            }
        }

        // GET: Admin/Candidates/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm người ứng tuyển mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Người ứng tuyển", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };

            LoadCreateDropdowns(); // Load Gender
            return View(new CreateCandidatesListVm { Active = true });
        }

        // POST: Admin/Candidates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateCandidatesListVm model)
        {
            LoadCreateDropdowns();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                

                // Check Username trùng
                if (db.Accounts.Any(a => a.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Check Email login trùng
                if (db.Accounts.Any(a => a.Email.ToLower() == model.Email.ToLower()))
                {
                    ModelState.AddModelError("Email", "Email (đăng nhập) đã được sử dụng");
                    return View(model);
                }

                // Check & Chuẩn hóa Phone
                var phone = (model.Phone ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    if (!ValidationHelper.IsValidVietnamesePhone(phone))
                    {
                        ModelState.AddModelError("Phone", ValidationHelper.GetPhoneErrorMessage());
                        return View(model);
                    }
                    phone = ValidationHelper.NormalizePhone(phone);

                    if (!ValidationHelper.IsAccountPhoneUnique(phone))
                    {
                        ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng.");
                        return View(model);
                    }
                }
                else
                {
                    phone = null;
                }

                string passwordHash = PasswordHelper.HashPassword(model.Password);
                var account = new Account
                {
                    Username = model.Username,
                    Email = model.Email,
                    Phone = phone,
                    Role = "Candidate", 
                    PasswordHash = passwordHash,
                    ActiveFlag = model.Active ? (byte)1 : (byte)0,
                    CreatedAt = DateTime.Now,
                    
                };

                // Xử lý Upload ảnh cho Account
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


                var candidate = new Candidate
                {
                    Account = account, 
                    FullName = model.FullName,
                    Email = model.Email, 
                    ApplicationEmail = model.ApplicationEmail,
                    Phone = phone,
                    CreatedAt = DateTime.Now,
                    ActiveFlag = model.Active ? (byte)1 : (byte)0,
                    Gender = model.Gender,
                    BirthDate = model.DateOfBirth,
                    Address = model.Address,
                    Summary = model.Summary
                };

                db.Candidates.InsertOnSubmit(candidate);

                try
                {
                    db.SubmitChanges();
                    TempData["SuccessMessage"] = "Tạo ứng viên và tài khoản thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống khi lưu dữ liệu: " + ex.Message);

                    
                    if (account.ProfilePhoto != null)
                    {
                        var filePath = Server.MapPath("~" + account.ProfilePhoto.FilePath);
                        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    }
                    return View(model);
                }
            }
        }

        // GET: Admin/Candidates/Edit/5
        public ActionResult Edit(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(x => x.CandidateID == id);
                if (candidate == null) return HttpNotFound();

                var account = db.Accounts.FirstOrDefault(a => a.AccountID == candidate.AccountID);

                var vm = new EditCandidatesListVm
                {
                    CandidateId = candidate.CandidateID,
                    AccountId = candidate.AccountID, 
                    FullName = candidate.FullName,
                    DateOfBirth = candidate.BirthDate,
                    Gender = candidate.Gender,
                    Phone = candidate.Phone,
                    CreatedAt = candidate.CreatedAt,
                    Active = candidate.ActiveFlag == 1,
                    Email = candidate.Email,
                    Address = candidate.Address,
                    Summary = candidate.Summary,
                    ApplicationEmail = candidate.ApplicationEmail,
                    Username = account?.Username,
                    CurrentPhotoId = account?.PhotoID,
                    CurrentPhotoUrl = account?.ProfilePhoto?.FilePath
                };

                ViewBag.Title = "Sửa ứng viên";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Ứng viên", Url.Action("Index")),
                    new Tuple<string, string>($"#{candidate.CandidateID}", Url.Action("Details", new { id = candidate.CandidateID })),
                    new Tuple<string, string>("Sửa", null)
                };

                return View(vm);
            }
        }

        // POST: Admin/Candidates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditCandidatesListVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.CandidateID == model.CandidateId);
                if (candidate == null) return HttpNotFound();

                var account = db.Accounts.FirstOrDefault(a => a.AccountID == candidate.AccountID);
                if (account == null) return HttpNotFound("Tài khoản liên kết không tồn tại.");

                model.CurrentPhotoUrl = account.ProfilePhoto?.FilePath; 

               
                if (db.Accounts.Any(a => a.Username == model.Username && a.AccountID != model.AccountId))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Validate Phone
                var phone = (model.Phone ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    if (!ValidationHelper.IsValidVietnamesePhone(phone))
                    {
                        ModelState.AddModelError("Phone", ValidationHelper.GetPhoneErrorMessage());
                        return View(model);
                    }
                    phone = ValidationHelper.NormalizePhone(phone);

                    if (!ValidationHelper.IsAccountPhoneUnique(phone, model.AccountId))
                    {
                        ModelState.AddModelError("Phone", "Số điện thoại này đã được dùng bởi tài khoản khác.");
                        return View(model);
                    }
                }
                else
                {
                    phone = null;
                }

                //  xử lý ảnh
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

                if (!string.IsNullOrEmpty(model.Password))
                {
                    // Nếu người dùng nhập mật khẩu, tiến hành mã hóa và cập nhật
                    // Sử dụng PasswordHelper.HashPassword giống như hàm Create
                    account.PasswordHash = PasswordHelper.HashPassword(model.Password);
                }
                account.Username = model.Username;
                account.Phone = phone;
                account.ActiveFlag = model.Active ? (byte)1 : (byte)0;
                candidate.FullName = model.FullName;
                candidate.BirthDate = model.DateOfBirth;
                candidate.Gender = model.Gender;
                candidate.Phone = phone; 
                candidate.Email = model.Email; 
                candidate.ApplicationEmail = model.ApplicationEmail;
                candidate.Address = model.Address;
                candidate.Summary = model.Summary;
                candidate.ActiveFlag = model.Active ? (byte)1 : (byte)0;

                try
                {
                    db.SubmitChanges();
                    TempData["SuccessMessage"] = "Cập nhật ứng viên thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
                    return View(model);
                }
            }
        }

        // GET: Admin/Candidates/Delete/5
        public ActionResult Delete(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.CandidateID == id);
                if (candidate == null) return HttpNotFound();

                var account = db.Accounts.FirstOrDefault(a => a.AccountID == candidate.AccountID);

                var vm = new CandidatesListVm
                {
                    CandidateId = candidate.CandidateID,
                    AccountId = candidate.AccountID,
                    FullName = candidate.FullName,
                    Phone = candidate.Phone,
                    Email = candidate.Email,
                    UserName = account?.Username,
                    ActiveFlag = candidate.ActiveFlag
                };

                ViewBag.Title = "Xóa người ứng tuyển";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Người Ứng Tuyển", Url.Action("Index")),
                    new Tuple<string, string>($"#{candidate.CandidateID}", null)
                };

                return View(vm);
            }
        }

        // POST: Admin/Candidates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.CandidateID == id);
                if (candidate == null) return HttpNotFound();

                // Lấy Account liên quan
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == candidate.AccountID);

                try
                {
                    // 1. Xóa ảnh trước (nếu có)
                    if (account != null && account.PhotoID.HasValue)
                    {
                        DeletePhoto(db, account.PhotoID.Value);
                    }

                    // 2. Xóa Candidate
                    db.Candidates.DeleteOnSubmit(candidate);

                    // 3. Xóa Account 

                    if (account != null)
                    {
                        db.Accounts.DeleteOnSubmit(account);
                    }

                    db.SubmitChanges();
                    TempData["SuccessMessage"] = "Xóa ứng viên và tài khoản thành công!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Không thể xóa: " + ex.Message;
                }

                return RedirectToAction("Index");
            }
        }

        

        private void LoadCreateDropdowns()
        {
            var genderList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Nam", Text = "Nam" },
                new SelectListItem { Value = "Nữ", Text = "Nữ" }
            };
            ViewBag.GenderOptions = new SelectList(genderList, "Value", "Text");
        }

        private ProfilePhoto SavePhoto(JOBPORTAL_ENDataContext db, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0) return null;

            try
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExt = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExt))
                {
                    TempData["ErrorMessage"] = "Chỉ cho phép upload file ảnh (jpg, jpeg, png, gif)";
                    return null;
                }

                if (file.ContentLength > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "File ảnh không được vượt quá 5MB";
                    return null;
                }

                var fileName = Guid.NewGuid().ToString() + fileExt;
                var uploadPath = Server.MapPath("~/Content/Uploads/Photos/");

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

        private void DeletePhoto(JOBPORTAL_ENDataContext db, int photoId)
        {
            try
            {
                var photo = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId);
                if (photo == null) return;

                var filePath = Server.MapPath("~" + photo.FilePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                db.ProfilePhotos.DeleteOnSubmit(photo);
                
            }
            catch (Exception ex)
            {
                
                Console.WriteLine("Lỗi xóa ảnh: " + ex.Message);
            }
        }
    }
}
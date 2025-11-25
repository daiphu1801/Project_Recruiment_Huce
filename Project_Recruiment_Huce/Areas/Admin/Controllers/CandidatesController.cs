using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using static Project_Recruiment_Huce.Areas.Admin.Models.CreateCandidatesListVm;


namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class CandidatesController : AdminBaseController
    {
        // Removed unused field: private string photo;

        public string PhotoUrl { get; private set; }


        // GET: Admin/Candidates
        public ActionResult Index(string q)
        {
            ViewBag.Title = "Quản lý ứng viên";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("người ứng tuyển", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var query = db.Candidates.AsQueryable();

                // Search
                if (!string.IsNullOrWhiteSpace(q))
                {
                    
                    var searchKeyword = q.ToLower();

                    query = query.Where(c =>
                        (c.FullName ?? "").ToLower().Contains(searchKeyword) ||
                        (c.Email ?? "").ToLower().Contains(searchKeyword) ||
                        (c.Phone ?? "").ToLower().Contains(searchKeyword)
                    );
                }

                var CandidatesList = query
                                .OrderByDescending(c => c.CreatedAt)
                                .ToList();
               
                var candidatePhotoIds = CandidatesList.Select(c => GetCandidatePhotoID(c, db))
                                                    .Where(id => id.HasValue)
                                                    .Select(id => id.Value)
                                                    .Distinct()
                                                    .ToList();
                var profilePhotos = db.ProfilePhotos
                                            .Where(p => candidatePhotoIds.Contains(p.PhotoID))
                                            .ToDictionary(p => p.PhotoID, p => p.FilePath);


                var candidates = CandidatesList.Select(c =>
                {
                    
                    var account = db.Accounts.FirstOrDefault(a => a.AccountID == c.AccountID);
                    int? photoId = account?.PhotoID;
                    var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;

                    string photoUrl = null;
                    if (photoId.HasValue && profilePhotos.ContainsKey(photoId.Value))
                    {
                        photoUrl = profilePhotos[photoId.Value];
                    }

                    return new CandidatesListVm
                    {
                        CandidateId = c.CandidateID,
                        AccountId = c.AccountID,
                        FullName = c.FullName,
                        DateOfBirth = c.BirthDate,
                        Gender = c.Gender,
                        Phone = c.Phone,
                        PhotoId = photoId,
                        CreatedAt = c.CreatedAt,
                        ActiveFlag = c.ActiveFlag,
                        Email = c.Email,
                        Address = c.Address,
                        PhotoUrl = photoUrl,
                        Summary = c.Summary,
                        ApplicationEmail = c.ApplicationEmail,
                        UserName = account.Username
                    };
                }).ToList();

                return View(candidates);
            }
        }

        private int? GetCandidatePhotoID(Candidate c, JOBPORTAL_ENDataContext db)
        {
            var accountId = c.AccountID;
            // AccountID giờ là non-nullable int, không cần check null
            var photoId = db.Accounts
                .Where(a => a.AccountID == accountId)
                .Select(a => a.PhotoID)
                .FirstOrDefault();
            return photoId;
        }


        // GET: Admin/Candidates/Details/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Details(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(x => x.CandidateID == id);
                if (candidate == null) return HttpNotFound();
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == candidate.AccountID);
                int? photoId = account?.PhotoID;
                var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;

                var vm = new CandidatesListVm
                {
                    CandidateId = candidate.CandidateID,
                    AccountId = candidate.AccountID,
                    FullName = candidate.FullName,
                    DateOfBirth = candidate.BirthDate,
                    Gender = candidate.Gender,
                    Phone = candidate.Phone,
                    PhotoId = photoId,
                    CreatedAt = candidate.CreatedAt,
                    ActiveFlag = candidate.ActiveFlag,
                    Email = candidate.Email,
                    Address = candidate.Address,
                    PhotoUrl = photo?.FilePath,
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
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm người ứng tuyển mới";

            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("người ứng tuyển", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };

            // 🔥 GỌI HÀM HELPER để nạp Dropdowns
            LoadCreateDropdowns();

            return View(new CreateCandidatesListVm { Active = true });
        }

        // Fix for CS0161: Ensure all code paths in Create(CreateCandidateListVm model) return an ActionResult
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateCandidatesListVm model)
        {
            // 🔥 GỌI HÀM HELPER để nạp Dropdowns, sẵn sàng nếu ModelState không hợp lệ
            LoadCreateDropdowns();

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var inputEmail = (model.Email ?? string.Empty).Trim().ToLower();
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

                // Kiểm tra lại Username sau khi Phone được xử lý
                if (db.Accounts.Any(a => a.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại");
                }


                if (!ModelState.IsValid)
                {
                    // Dropdowns đã được nạp ở đầu action
                    return View(model);
                }

                // Validate password not null
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError("Password", "Mật khẩu không được để trống");
                    return View(model);
                }

                // Tạo hash mật khẩu sử dụng PBKDF2
                string passwordHash = PasswordHelper.HashPassword(model.Password);

                var account = new Account
                {
                    Username = model.Username,
                    Email = model.Email,
                    Phone = model.Phone,
                    Role = "Candidate",
                    PasswordHash = passwordHash,
                    ActiveFlag = model.Active ? (byte)1 : (byte)0,
                    CreatedAt = DateTime.Now,
                    UserName = model.Username,
                    ApplicationEmail = model.ApplicationEmail

                };

                // XỬ LÝ UPLOAD ẢNH
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    int? photoId = SavePhoto(model.PhotoFile);

                    if (photoId.HasValue)
                    {
                        account.PhotoID = photoId.Value;
                    }
                    else
                    {
                        // SavePhoto thất bại (validation), trả về view (ViewBag đã nạp)
                        return View(model);
                    }
                }

                db.Accounts.InsertOnSubmit(account);

                var Candidate = new Candidate
                {
                    Account = account,
                    FullName = model.FullName,
                    Username = model.Username,
                    Email = model.Email,
                    AccountID = account.AccountID,
                    ApplicationEmail = model.ApplicationEmail,
                    Phone = model.Phone,
                    CreatedAt = DateTime.Now,
                    ActiveFlag = model.Active ? (byte)1 : (byte)0,
                    Gender = model.Gender,
                    BirthDate = model.DateOfBirth,
                    
                    
                };

                db.Candidates.InsertOnSubmit(Candidate);

                try
                {
                    db.SubmitChanges();
                    TempData["SuccessMessage"] = "Tạo người ứng tuyển và tài khoản thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Bắt lỗi DB nếu có
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu vào cơ sở dữ liệu: " + ex.Message);
                    // Dropdowns đã được nạp ở đầu action
                    return View(model);
                }
            }
        }


        // GET: Admin/Candidates/Edit/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Edit(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(x => x.CandidateID == id);
                if (candidate == null) return HttpNotFound();

                LoadDropdown(candidate.AccountID); // Load dropdown list

                int? photoId = GetCandidatePhotoID(candidate, db);
                var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;
                var vm = new EditCandidatesListVm
                {
                    CandidateId = candidate.CandidateID,
                    FullName = candidate.FullName,
                    DateOfBirth = candidate.BirthDate,
                    Gender = candidate.Gender,
                    Phone = candidate.Phone,
                    PhotoId = photoId,
                    CreatedAt = candidate.CreatedAt,
                    ActiveFlag = candidate.ActiveFlag,
                    Email = candidate.Email,
                    Address = candidate.Address,
                    PhotoUrl = photo?.FilePath, // PhotoFile không tồn tại, dùng photo từ ProfilePhoto
                    Summary = candidate.Summary,
                    ApplicationEmail = candidate.Email, // ApplicationEmail không tồn tại, dùng Email
                    CurrentPhotoId = photoId,
                    CurrentPhotoUrl = photo?.FilePath,
                    Active = candidate.ActiveFlag == 1 ,// Gán giá trị Active cho ViewModel
                    Username = db.Accounts.FirstOrDefault(a => a.AccountID == candidate.AccountID)?.Username
                    
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditCandidatesListVm model)
        {

            if (!ModelState.IsValid)
            {
                LoadDropdown(model.AccountId);
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == model.AccountId);
                var candidate = db.Candidates.FirstOrDefault(c => c.CandidateID == model.CandidateId);
                if (candidate == null)
                {
                    ModelState.AddModelError("", "Ứng viên không tồn tại.");
                    LoadDropdown(model.AccountId);
                    return View(model);
                }

                // Cập nhật thông tin cơ bản
                candidate.FullName = model.FullName;
                candidate.BirthDate = model.DateOfBirth; // DateOfBirth không tồn tại, dùng BirthDate
                candidate.Gender = model.Gender;
                candidate.Phone = model.Phone;
                candidate.Email = model.ApplicationEmail ?? candidate.Email; // ApplicationEmail không tồn tại, dùng Email
                candidate.Address = model.Address;
                candidate.Summary = model.Summary;
                candidate.ActiveFlag = model.Active ? (byte)1 : (byte)0; // Active không tồn tại, dùng ActiveFlag
                candidate.Username = model.Username;
                if (model.Gender != "Nam" && model.Gender != "Nữ")
                {
                    
                    ModelState.AddModelError("Gender", "Giới tính phải là 'Nam' hoặc 'Nữ'.");
                    return View(model);
                }
                    if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    int? newPhotoId = SavePhoto(model.PhotoFile);

                    if (newPhotoId.HasValue)
                    {
                        // Nếu có ảnh mới, cập nhật Account
                        

                        if (account != null)
                        {
                            
                            if (account.PhotoID.HasValue)
                            {
                                DeletePhoto(account.PhotoID.Value);
                            }

                            // Gán PhotoID mới cho Account
                            account.PhotoID = newPhotoId.Value;
                        }
                    }
                    else
                    {
                        // Việc SavePhoto thất bại, đã set TempData["ErrorMessage"] trong helper
                        LoadDropdown(model.AccountId);
                        return View(model);
                    }
                }

                // Lưu tất cả thay đổi
                db.SubmitChanges();
            }

            TempData["Success"] = "Cập nhật ứng viên thành công!";
            return RedirectToAction("Index");
        }

        // 🔥 Helper để nạp Dropdowns cho Action Create
        private void LoadCreateDropdowns()
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // 1. Dropdown Giới tính
                var genderList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Nam", Text = "Nam" },
                    new SelectListItem { Value = "Nữ", Text = "Nữ" }
                };
                ViewBag.GenderOptions = new SelectList(genderList, "Value", "Text");

                // 2. Dropdown Company (Giữ lại logic cũ)
                ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName");
            }
        }

        // Helper này có vẻ không dùng trong Create/Edit, nhưng giữ lại cho Edit/Index/Details
        private void LoadDropdown(int? selectedAccountId = null)
        {
            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var accountData = db.Accounts
                                .Where(a => a.ActiveFlag == 1 && (a.Role == "Candidate" || a.AccountID == selectedAccountId))
                                .Select(a => new
                                {
                                    a.AccountID,
                                    a.Username
                                })
                                .ToList(); // QUAN TRỌNG: Thực thi query ngay

                // Tạo SelectList từ dữ liệu đã load
                ViewBag.AccountOptions = new SelectList(
                    accountData,
                    "AccountID",
                    "Username",
                    selectedAccountId
                );
            }
        }


        // GET: Admin/Candidates/Delete/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Delete(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.CandidateID == id);
                if (candidate == null) return HttpNotFound();


                int? photoId = GetCandidatePhotoID(candidate, db);
                var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;

                var vm = new CandidatesListVm
                {
                    CandidateId = candidate.CandidateID,
                    AccountId = candidate.AccountID,
                    FullName = candidate.FullName,
                    DateOfBirth = candidate.BirthDate,
                    Gender = candidate.Gender,
                    Phone = candidate.Phone,
                    PhotoId = photoId,
                    CreatedAt = candidate.CreatedAt,
                    ActiveFlag = candidate.ActiveFlag,
                    Email = candidate.Email,
                    Address = candidate.Address,
                    PhotoUrl = photo?.FilePath, // PhotoFile không tồn tại, dùng photo từ ProfilePhoto
                    Summary = candidate.Summary,
                    ApplicationEmail = candidate.Email, // ApplicationEmail không tồn tại, dùng Email
                    UserName = db.Accounts.FirstOrDefault(a => a.AccountID == candidate.AccountID)?.Username
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
        // NOTE: This action uses MockData as a template/base.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.CandidateID == id);
                if (candidate == null) return HttpNotFound();

                var account = db.Accounts.FirstOrDefault(a => a.AccountID == candidate.AccountID);

                // Delete photo if exists
                if (account != null && account.PhotoID.HasValue)
                {
                    DeletePhoto(account.PhotoID.Value);
                    // Sau khi xóa ProfilePhoto, cần SubmitChanges để đảm bảo xóa thành công
                    db.SubmitChanges();
                }

                // Xóa Candidate
                db.Candidates.DeleteOnSubmit(candidate);

                // Xóa Account (nếu cần - tùy thuộc vào ràng buộc trong DB)
                // Nếu Account này chỉ dùng cho Candidate này, ta nên xóa nó.
                if (account != null)
                {
                    db.Accounts.DeleteOnSubmit(account);
                }

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Xóa ứng viên thành công!";
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
            catch (Exception)
            {
                // Ghi log lỗi nếu cần
            }
        }

        // Helper: Check if PhotoID property exists on candidate class
        private bool HasPhotoIDProperty()
        {
            return typeof(Candidate).GetProperty("PhotoID") != null;
        }

        // Helper: Get PhotoID from candidate (works with or without property)
        private int? GetcandidatePhotoID(Candidate candidate, JOBPORTAL_ENDataContext db)
        {
            // Thay đổi logic để luôn ưu tiên lấy từ Account (vì logic trong Index và Details đã dùng cách này)
            return GetCandidatePhotoID(candidate, db);
        }

        // Helper: Set PhotoID on candidate (works with or without property)
        // Hàm này không cần thiết vì PhotoID được gán cho Account
        private void SetcandidatePhotoID(Candidate candidate, int? photoId, JOBPORTAL_ENDataContext db)
        {
            throw new NotImplementedException();
        }

    }
}
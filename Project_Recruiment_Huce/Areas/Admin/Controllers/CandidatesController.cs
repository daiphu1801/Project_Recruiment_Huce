using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Models;
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
using static Project_Recruiment_Huce.Areas.Admin.Models.CreateCandidateListVm;


namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class CandidatesController : AdminBaseController
    {
        private string photo;

        public string PhotoUrl { get; private set; }


        // GET: Admin/Candidates
        public ActionResult Index(string q, string role = null, int page = 1)
        {
            ViewBag.Title = "Quản lý ứng viên";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Nhà tuyển dụng", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var query = db.Candidates.AsQueryable();

                // Search
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.Where(c =>
                        (c.FullName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (c.Email ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (c.Phone ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
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


                var candidates = CandidatesList.Select((Candidate c) =>
                {
                    int? photoId = GetCandidatePhotoID(c, db);

                    string photoUrl = null;
                    if (photoId.HasValue && profilePhotos.ContainsKey(photoId.Value))
                    {
                        photoUrl = profilePhotos[photoId.Value];
                    }
                    return new CandidateListVm
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
                        ApplicationEmail = c.Email, // ApplicationEmail không tồn tại, dùng Email
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

                var vm = new CandidateListVm
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
                    PhotoUrl = photo != null ? photo.FilePath : null,
                    Summary = candidate.Summary,
                    ApplicationEmail = candidate.Email // ApplicationEmail không tồn tại, dùng Email
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
            return RedirectToAction("Create", "Accounts");
            /*
                        {

                            ViewBag.Title = "Tạo ứng viên";
                            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                            {
                                new Tuple<string, string>("Ứng viên", Url.Action("Index")),
                                new Tuple<string, string>("Tạo mới", null)
                            };
                            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                            {
                                ViewBag.AccountOptions = new SelectList(
                                    db.Accounts
                                    .Where(a => a.Role == "Candidate")
                                    .Select(a => new { Id = a.AccountID, Name = a.Username })
                                    .ToList(),
                                     "Id",
                                    "Name"
                                    );

                            }
                            return View();
                        }*/
        }

        // Fix for CS0161: Ensure all code paths in Create(CreateCandidateListVm model) return an ActionResult
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateCandidateListVm model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdown(model.AccountId);
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                int? photoId = null;

                // XỬ LÝ UPLOAD ẢNH
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.PhotoFile.FileName);
                    string folder = Server.MapPath("~/Uploads/ProfilePhotos/");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string filePath = Path.Combine(folder, fileName);
                    model.PhotoFile.SaveAs(filePath);

                    var newPhoto = new ProfilePhoto
                    {
                        FileName = fileName,
                        FilePath = "/Uploads/ProfilePhotos/" + fileName,
                        FileFormat = Path.GetExtension(fileName),
                        FileSizeKB = (int)(model.PhotoFile.ContentLength / 1024),
                        UploadedAt = DateTime.Now
                    };

                    db.ProfilePhotos.InsertOnSubmit(newPhoto);
                    db.SubmitChanges();

                    photoId = newPhoto.PhotoID;
                }

                // TẠO CANDIDATE
                var candidate = new Candidate
                {
                    AccountID = model.AccountId,
                    FullName = model.FullName,
                    BirthDate = model.BirthDate,
                    Gender = model.Gender,
                    Phone = model.Phone,
                    Email = model.Email,
                    Address = model.Address,
                    Summary = model.Summary,
                    ActiveFlag = model.ActiveFlag ?? (byte)1, // Cast byte? to byte
                    CreatedAt = DateTime.Now,
                    PhotoID = photoId                      // lưu id ảnh
                };

                db.Candidates.InsertOnSubmit(candidate);
                db.SubmitChanges();
            }

            TempData["SuccessMessage"] = "Tạo ứng viên thành công!";
            return RedirectToAction("Index");
        }


        private bool HasPhotoIDProperty()
        {
            return typeof(Candidate).GetProperty("PhotoID") != null;
        }

        private void SetCandidatePhotoID(Candidate candidate, int? photoId, JOBPORTAL_ENDataContext db)
        {
            throw new NotImplementedException();
        }

        private int? SavePhoto(object photoFile)
        {
            throw new NotImplementedException();
        }

        // GET: Admin/Candidates/Edit/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Edit(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(x => x.CandidateID == id);
                if (candidate == null) return HttpNotFound();


                var accountList = db.Accounts
                        .Where(a => a.ActiveFlag == 1
                            && (a.Role == "Candidate" || a.AccountID == candidate.AccountID))
                        .Select(a => new { a.AccountID, a.Username })
                        .ToList(); // 🔥 Quan trọng – load dữ liệu ra khỏi DB

                // Gán vào ViewBag với accountList đã tách khỏi DB
                ViewBag.AccountOptions = new SelectList(
                    accountList,
                    "AccountID",
                    "Username",
                    candidate.AccountID // Set giá trị được chọn


    );
                int? photoId = GetCandidatePhotoID(candidate, db);
                var photo = photoId.HasValue ? db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value) : null;
                var vm = new EditCandidateListVm
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
                    CurrentPhotoId = photoId,
                    CurrentPhotoUrl = photo?.FilePath
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
        public ActionResult Edit(EditCandidateListVm model)
        {

            if (!ModelState.IsValid)
            {
                LoadDropdown(model.AccountId);
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {

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
                candidate.ActiveFlag = model.Active ? (byte)1 : (byte)0; // Active không tồn tại, dùng ActiveFlag
                candidate.AccountID = model.AccountId;

                // =====================
                // XỬ LÝ UPLOAD ẢNH MỚI
                // =====================
                if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.PhotoFile.FileName);

                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        ModelState.AddModelError("", "Tên file ảnh không hợp lệ.");
                        LoadDropdown(model.AccountId);
                        return View(model);
                    }

                    string folder = Server.MapPath("~/Uploads/ProfilePhotos/");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string savePath = Path.Combine(folder, fileName);

                    // Lưu file mới
                    model.PhotoFile.SaveAs(savePath);

                    // Nếu có ảnh cũ thì xóa file cũ + xóa record cũ
                    if (candidate.PhotoID != null)
                    {
                        var oldPhoto = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == candidate.PhotoID);
                        if (oldPhoto != null)
                        {
                            string oldFilePath = Server.MapPath(oldPhoto.FilePath);
                            if (System.IO.File.Exists(oldFilePath))
                                System.IO.File.Delete(oldFilePath);

                            db.ProfilePhotos.DeleteOnSubmit(oldPhoto);
                            db.SubmitChanges();
                        }
                    }

                    // Thêm ảnh mới vào DB
                    var newPhoto = new ProfilePhoto
                    {
                        FileName = fileName, // Không được NULL
                        FilePath = "/Uploads/ProfilePhotos/" + fileName, // Không được NULL
                        FileFormat = Path.GetExtension(fileName),
                        FileSizeKB = model.PhotoFile.ContentLength,
                        UploadedAt = DateTime.Now
                        // UploadedAt tự động
                    };

                    db.ProfilePhotos.InsertOnSubmit(newPhoto);
                    db.SubmitChanges();

                    // Gán PhotoId cho Candidate
                    var account = db.Accounts.FirstOrDefault(a => a.AccountID == candidate.AccountID);
                    if (account != null)
                    {
                        account.PhotoID = newPhoto.PhotoID;
                    }
                }

                // Lưu tất cả thay đổi
                db.SubmitChanges();
            }

            TempData["Success"] = "Cập nhật ứng viên thành công!";
            return RedirectToAction("Index");
        }

        private void LoadDropdown(int? selectedAccountId = null)
        {
            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // ✅ SỬA LẠI: Thêm .ToList() để thực thi query TRƯỚC KHI tạo SelectList
                var accountData = db.Accounts
                            .Where(a => a.ActiveFlag == 1 && (a.Role == "Candidate" || a.AccountID == selectedAccountId))
                            .Select(a => new
                            {
                                a.AccountID,
                                a.Username
                            })
                            .ToList(); // 🔥 QUAN TRỌNG: Thực thi query ngay

                // Tạo SelectList từ dữ liệu đã load
                ViewBag.AccountOptions = new SelectList(
                    accountData,           // Dùng accountData đã .ToList()
                    "AccountID",           // Sửa từ "ID" thành "AccountID"
                    "Username",            // Sửa từ "UserName" thành "Username"
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

                var vm = new CandidateListVm
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
                };

                ViewBag.Title = "Xóa nhà tuyển dụng";
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

                // Delete photo if exists
                int? photoIdToDelete = GetcandidatePhotoID(candidate, db);
                if (photoIdToDelete.HasValue)
                {
                    DeletePhoto(photoIdToDelete.Value);
                }

                db.Candidates.DeleteOnSubmit(candidate);
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

        // Helper: Check if PhotoID property exists on candidate class


        // Helper: Get PhotoID from candidate (works with or without property)
        private int? GetcandidatePhotoID(Candidate candidate, JOBPORTAL_ENDataContext db)
        {
            if (HasPhotoIDProperty())
            {
                var property = typeof(Candidate).GetProperty("PhotoID");
                var value = property.GetValue(candidate);
                return value as int?;
            }
            else
            {
                // Use SQL query to get PhotoID
                var result = db.ExecuteQuery<int?>("SELECT PhotoID FROM candidates WHERE candidateID = {0}", candidate.CandidateID).FirstOrDefault();
                return candidate.PhotoID;
            }
        }

        // Helper: Set PhotoID on candidate (works with or without property)
        private void SetcandidatePhotoID(Candidate candidate, int? photoId, JOBPORTAL_ENDataContext db)
        {
            if (HasPhotoIDProperty())
            {
                var property = typeof(Candidate).GetProperty("PhotoID");
                property.SetValue(candidate, photoId);
            }
            else
            {
                // Will be set via SQL after SubmitChanges if property doesn't exist
                // This is handled in the calling code
            }
        }

    }
}


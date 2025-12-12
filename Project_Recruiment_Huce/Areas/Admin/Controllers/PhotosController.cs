using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class PhotosController : AdminBaseController
    {
       
        public ActionResult Index(string q, int page = 1)
        {
            ViewBag.Title = "Thư viện ảnh";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Photos", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var query = db.ProfilePhotos.AsQueryable();

                // Search
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.Where(a =>
                        (a.FileName != null && a.FileName.Contains(q)) ||
                        (a.FilePath != null && a.FilePath.Contains(q))
                    );
                }

                // Pagination
                int pageSize = 10;
                int totalRecords = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var data = query
                    .OrderByDescending(a => a.UploadedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new PhotoVm
                    {
                        PhotoId = a.PhotoID,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        SizeKB = a.FileSizeKB,

                        UploadedAt = a.UploadedAt
                    }).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalRecords;
                ViewBag.PageSize = pageSize;

                return View(data);
            }
        }

      
        public ActionResult Details(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var item = db.ProfilePhotos.FirstOrDefault(x => x.PhotoID == id);
                if (item == null) return HttpNotFound();

                var vm = new PhotoVm
                {
                    PhotoId = item.PhotoID,
                    FileName = item.FileName,
                    FilePath = item.FilePath,
                    SizeKB = item.FileSizeKB,

                    UploadedAt = item.UploadedAt
                };

                ViewBag.Title = "Chi tiết ảnh";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Photos", Url.Action("Index")),
                    new Tuple<string, string>($"#{item.PhotoID}", null)
                };

                return View(vm);
            }
        }

        
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm ảnh mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Photos", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };

            return View();
        }
        
        [HttpPost]

        public ActionResult Create(CreatePhotoVm model)
        {
            if (model.FileUpload == null || model.FileUpload.ContentLength == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ảnh để tải lên!");
                return View(model);
            }

            // Kiểm tra thư mục lưu ảnh
            string folderPath = Server.MapPath("~/Uploads/Photos/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Tạo tên file mới tránh trùng
            string fileName = Guid.NewGuid() + Path.GetExtension(model.FileUpload.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            // Lưu file vào thư mục server
            model.FileUpload.SaveAs(filePath);

            // Tính dung lượng KB
            long sizeKB = model.FileUpload.ContentLength / 1024;

            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var photo = new ProfilePhoto
                {
                    FileName = fileName,
                    FilePath = "/Uploads/Photos/" + fileName,
                    FileSizeKB = (int)sizeKB,
                    UploadedAt = DateTime.Now
                };

                db.ProfilePhotos.InsertOnSubmit(photo);
                db.SubmitChanges();
            }

            TempData["SuccessMessage"] = "Tải ảnh lên thành công!";
            return RedirectToAction("Index");
        }
        // GET: Admin/Accounts/Delete/5
        public ActionResult Delete(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var Photo = db.ProfilePhotos.FirstOrDefault(a => a.PhotoID == id);
                if (Photo == null) return HttpNotFound();

                var vm = new PhotoVm
                {
                    PhotoId = Photo.PhotoID,
                    FileName = Photo.FileName,
                    FilePath = Photo.FilePath,
                    SizeKB = Photo.FileSizeKB,

                    UploadedAt = Photo.UploadedAt
                };

                ViewBag.Title = "Xóa ảnh ";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Ảnh ", Url.Action("Index")),
                    new Tuple<string, string>($"#{Photo.PhotoID}", null)
                };

                return View(vm);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                using (var db = new JOBPORTAL_ENDataContext(
                    ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    var photo = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == id);
                    if (photo == null)
                    {
                        TempData["ErrorMessage"] = "Ảnh không tồn tại";
                        return RedirectToAction("Index");
                    }

                    // Đường dẫn file thật trên server
                    string filePath = Server.MapPath("~" + photo.FilePath);

                    // Xóa ảnh vật lý
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Xóa trong database
                    db.ProfilePhotos.DeleteOnSubmit(photo);
                    db.SubmitChanges();

                    TempData["SuccessMessage"] = "Xóa ảnh thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa ảnh: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
        

















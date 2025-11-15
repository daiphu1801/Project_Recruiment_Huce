using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Candidates;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class ResumeFilesController : BaseController
    {

        /// <summary>
        /// GET: Candidates/ResumeFiles/MyResumes
        /// Hiển thị danh sách CV files của candidate
        /// </summary>
        [HttpGet]
        public ActionResult MyResumes()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng hoàn thiện hồ sơ trước khi quản lý CV.";
                    return RedirectToAction("CandidatesManage", "Candidates");
                }

                // Get all resume files for this candidate
                var resumeFiles = db.ResumeFiles
                    .Where(rf => rf.CandidateID == candidate.CandidateID)
                    .OrderByDescending(rf => rf.UploadedAt)
                    .ToList();

                // Map to ViewModel
                var viewModels = resumeFiles.Select(rf => new ResumeFileViewModel
                {
                    ResumeFileID = rf.ResumeFileID,
                    CandidateID = rf.CandidateID,
                    FileName = rf.FileName,
                    FilePath = rf.FilePath,
                    UploadedAt = rf.UploadedAt,
                    FileExtension = Path.GetExtension(rf.FileName ?? "")?.ToLower() ?? "",
                    DisplayName = string.IsNullOrEmpty(rf.FileName) ? "CV không tên" : Path.GetFileNameWithoutExtension(rf.FileName)
                }).ToList();

                return View(viewModels);
            }
        }

        /// <summary>
        /// POST: Candidates/ResumeFiles/UploadResume
        /// Upload CV file mới
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadResume(HttpPostedFileBase resumeFile, string title)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (resumeFile == null || resumeFile.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file CV để upload.";
                return RedirectToAction("MyResumes");
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg", ".gif" };
            var fileExtension = Path.GetExtension(resumeFile.FileName)?.ToLower();
            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
            {
                TempData["ErrorMessage"] = "Chỉ cho phép upload file PDF, DOC, DOCX, PNG, JPG, JPEG, GIF.";
                return RedirectToAction("MyResumes");
            }

            // Validate file size (max 10MB)
            const int maxSize = 10 * 1024 * 1024; // 10MB
            if (resumeFile.ContentLength > maxSize)
            {
                TempData["ErrorMessage"] = "File không được vượt quá 10MB.";
                return RedirectToAction("MyResumes");
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng hoàn thiện hồ sơ trước khi upload CV.";
                    return RedirectToAction("CandidatesManage", "Candidates");
                }

                try
                {
                    // Create upload directory
                    var uploadsRoot = Server.MapPath("~/Content/uploads/resumes/");
                    if (!Directory.Exists(uploadsRoot))
                    {
                        Directory.CreateDirectory(uploadsRoot);
                    }

                    // Generate unique filename
                    var safeFileName = $"CV_{candidate.CandidateID}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{fileExtension}";
                    var physicalPath = Path.Combine(uploadsRoot, safeFileName);
                    resumeFile.SaveAs(physicalPath);

                    // Save to database
                    var relativePath = $"/Content/uploads/resumes/{safeFileName}";
                    var resumeFileEntity = new ResumeFile
                    {
                        CandidateID = candidate.CandidateID,
                        FileName = string.IsNullOrWhiteSpace(title) ? resumeFile.FileName : title + fileExtension,
                        FilePath = relativePath,
                        UploadedAt = DateTime.Now
                    };

                    db.ResumeFiles.InsertOnSubmit(resumeFileEntity);
                    db.SubmitChanges();

                    TempData["SuccessMessage"] = "Upload CV thành công!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi upload CV: " + ex.Message;
                }
            }

            return RedirectToAction("MyResumes");
        }

        /// <summary>
        /// POST: Candidates/ResumeFiles/DeleteResume
        /// Xóa CV file
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteResume(int? id)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy CV cần xóa.";
                return RedirectToAction("MyResumes");
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng hoàn thiện hồ sơ trước.";
                    return RedirectToAction("CandidatesManage", "Candidates");
                }

                var resumeFile = db.ResumeFiles.FirstOrDefault(rf => rf.ResumeFileID == id.Value && rf.CandidateID == candidate.CandidateID);
                if (resumeFile == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy CV hoặc bạn không có quyền xóa CV này.";
                    return RedirectToAction("MyResumes");
                }

                try
                {
                    // Delete physical file
                    if (!string.IsNullOrEmpty(resumeFile.FilePath))
                    {
                        var filePath = Server.MapPath("~" + resumeFile.FilePath);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    // Delete from database
                    db.ResumeFiles.DeleteOnSubmit(resumeFile);
                    db.SubmitChanges();

                    TempData["SuccessMessage"] = "Xóa CV thành công!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi xóa CV: " + ex.Message;
                }
            }

            return RedirectToAction("MyResumes");
        }

        /// <summary>
        /// POST: Candidates/ResumeFiles/EditResume
        /// Chỉnh sửa tên CV
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditResume(int? id, string title)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy CV cần chỉnh sửa.";
                return RedirectToAction("MyResumes");
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng hoàn thiện hồ sơ trước.";
                    return RedirectToAction("CandidatesManage", "Candidates");
                }

                var resumeFile = db.ResumeFiles.FirstOrDefault(rf => rf.ResumeFileID == id.Value && rf.CandidateID == candidate.CandidateID);
                if (resumeFile == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy CV hoặc bạn không có quyền chỉnh sửa CV này.";
                    return RedirectToAction("MyResumes");
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        // Preserve file extension
                        var fileExtension = Path.GetExtension(resumeFile.FileName) ?? "";
                        resumeFile.FileName = title + fileExtension;
                        db.SubmitChanges();
                        TempData["SuccessMessage"] = "Cập nhật tên CV thành công!";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật CV: " + ex.Message;
                }
            }

            return RedirectToAction("MyResumes");
        }
    }
}


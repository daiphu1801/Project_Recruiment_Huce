using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Candidates;
using Project_Recruiment_Huce.Repositories.ResumeFileRepo;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Services.ResumeFileService
{
    /// <summary>
    /// Implementation của IResumeFileService với business logic và validation
    /// </summary>
    public class ResumeFileService : IResumeFileService
    {
        private readonly IResumeFileRepository _repository;

        public ResumeFileService(IResumeFileRepository repository)
        {
            _repository = repository;
        }

        public ResumeFileListResult GetResumeFiles(int accountId)
        {
            var result = new ResumeFileListResult();

            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                result.Success = false;
                result.ErrorMessage = "Vui lòng hoàn thiện hồ sơ trước khi quản lý CV.";
                return result;
            }

            var resumeFiles = _repository.GetResumeFilesByCandidateId(candidate.CandidateID);

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

            result.Success = true;
            result.ResumeFiles = viewModels;
            return result;
        }

        public ValidationResult UploadResume(int accountId, HttpPostedFileBase resumeFile, string title, HttpServerUtilityBase server)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate file exists
            if (resumeFile == null || resumeFile.ContentLength == 0)
            {
                result.IsValid = false;
                result.Errors["General"] = "Vui lòng chọn file CV để upload.";
                return result;
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg", ".gif" };
            var fileExtension = Path.GetExtension(resumeFile.FileName)?.ToLower();
            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
            {
                result.IsValid = false;
                result.Errors["ResumeFile"] = "Chỉ cho phép upload file PDF, DOC, DOCX, PNG, JPG, JPEG, GIF.";
                return result;
            }

            // Validate file size (max 10MB)
            const int maxSize = 10 * 1024 * 1024; // 10MB
            if (resumeFile.ContentLength > maxSize)
            {
                result.IsValid = false;
                result.Errors["ResumeFile"] = "File không được vượt quá 10MB.";
                return result;
            }

            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Vui lòng hoàn thiện hồ sơ trước khi upload CV.";
                return result;
            }

            try
            {
                // Create upload directory
                var uploadsRoot = server.MapPath("~/Content/uploads/resumes/");
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

                _repository.CreateResumeFile(resumeFileEntity);
                _repository.SaveChanges();
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors["General"] = "Lỗi khi upload CV: " + ex.Message;
            }

            return result;
        }

        public ValidationResult DeleteResume(int accountId, int resumeFileId, HttpServerUtilityBase server)
        {
            var result = new ValidationResult { IsValid = true };

            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Vui lòng hoàn thiện hồ sơ trước.";
                return result;
            }

            var resumeFile = _repository.GetResumeFile(resumeFileId, candidate.CandidateID);
            if (resumeFile == null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Không tìm thấy CV hoặc bạn không có quyền xóa CV này.";
                return result;
            }

            try
            {
                // Delete physical file
                if (!string.IsNullOrEmpty(resumeFile.FilePath))
                {
                    var filePath = server.MapPath("~" + resumeFile.FilePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Delete from database
                _repository.DeleteResumeFile(resumeFile);
                _repository.SaveChanges();
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors["General"] = "Lỗi khi xóa CV: " + ex.Message;
            }

            return result;
        }

        public ValidationResult EditResume(int accountId, int resumeFileId, string title)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(title))
            {
                result.IsValid = false;
                result.Errors["Title"] = "Vui lòng nhập tên CV.";
                return result;
            }

            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Vui lòng hoàn thiện hồ sơ trước.";
                return result;
            }

            var resumeFile = _repository.GetResumeFile(resumeFileId, candidate.CandidateID);
            if (resumeFile == null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Không tìm thấy CV hoặc bạn không có quyền chỉnh sửa CV này.";
                return result;
            }

            try
            {
                // Preserve file extension
                var fileExtension = Path.GetExtension(resumeFile.FileName) ?? "";
                var newFileName = title + fileExtension;
                _repository.UpdateResumeFileName(resumeFile, newFileName);
                _repository.SaveChanges();
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors["General"] = "Lỗi khi cập nhật CV: " + ex.Message;
            }

            return result;
        }
    }
}

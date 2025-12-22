using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Candidates;
using Project_Recruiment_Huce.Repositories.CandidateRepo;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Services.CandidateService
{
    /// <summary>
    /// Implementation của ICandidateService với business logic và validation
    /// </summary>
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _repository;

        public CandidateService(ICandidateRepository repository)
        {
            _repository = repository;
        }

        public CandidateManageViewModel GetOrCreateCandidate(int accountId, string username)
        {
            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                candidate = new Candidate
                {
                    AccountID = accountId,
                    FullName = username,
                    Gender = "Nam",
                    CreatedAt = DateTime.Now,
                    ActiveFlag = 1
                };
                _repository.CreateCandidate(candidate);
                _repository.SaveChanges();
            }

            return new CandidateManageViewModel
            {
                CandidateID = candidate.CandidateID,
                FullName = candidate.FullName,
                BirthDate = candidate.BirthDate,
                Gender = candidate.Gender,
                Phone = candidate.Phone,
                Email = candidate.Email,
                Address = candidate.Address,
                Summary = candidate.Summary
            };
        }

        public ValidationResult ValidateAndUpdateProfile(
            CandidateManageViewModel viewModel,
            int accountId,
            HttpPostedFileBase avatar,
            HttpServerUtilityBase server)
        {
            var result = new ValidationResult { IsValid = true };

            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                candidate = new Candidate
                {
                    AccountID = accountId,
                    FullName = viewModel.FullName,
                    Gender = "Nam",
                    CreatedAt = DateTime.Now,
                    ActiveFlag = 1
                };
                _repository.CreateCandidate(candidate);
            }

            // Validate phone
            var phone = (viewModel.Phone ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(phone))
            {
                if (!ValidationHelper.IsValidVietnamesePhone(phone))
                {
                    result.IsValid = false;
                    result.Errors["Phone"] = ValidationHelper.GetPhoneErrorMessage();
                }
                else
                {
                    phone = ValidationHelper.NormalizePhone(phone);

                    if (!_repository.IsPhoneUnique(phone, accountId))
                    {
                        result.IsValid = false;
                        result.Errors["Phone"] = "Số điện thoại này đã được sử dụng bởi tài khoản hoặc hồ sơ khác.";
                    }
                }
            }
            else
            {
                phone = null;
            }

            // Validate email
            var email = (viewModel.Email ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!ValidationHelper.IsValidEmail(email))
                {
                    result.IsValid = false;
                    result.Errors["Email"] = "Email không hợp lệ.";
                }
            }

            if (!result.IsValid)
            {
                return result;
            }

            // Update profile fields
            candidate.FullName = viewModel.FullName;
            candidate.BirthDate = viewModel.BirthDate;
            candidate.Gender = string.IsNullOrWhiteSpace(viewModel.Gender) ? "Nam" : viewModel.Gender;
            candidate.Phone = phone;
            candidate.Email = viewModel.Email;
            candidate.Address = viewModel.Address;

            // Sanitize HTML
            if (!string.IsNullOrEmpty(viewModel.Summary))
            {
                string sanitizedHtml = HtmlSanitizerHelper.Sanitize(viewModel.Summary);
                if (sanitizedHtml.Length > 500)
                {
                    candidate.Summary = sanitizedHtml.Substring(0, 500);
                }
                else
                {
                    candidate.Summary = sanitizedHtml;
                }
            }

            // Handle avatar upload
            if (avatar != null && avatar.ContentLength > 0)
            {
                var contentType = (avatar.ContentType ?? string.Empty).ToLowerInvariant();
                var allowed = new[] { "image/jpeg", "image/jpg", "image/pjpeg", "image/png", "image/x-png", "image/gif", "image/webp" };
                const int maxBytes = 2 * 1024 * 1024; // 2MB

                if (avatar.ContentLength > maxBytes)
                {
                    result.IsValid = false;
                    result.Errors["Avatar"] = "Image must be 2MB or smaller.";
                    return result;
                }

                if (!allowed.Contains(contentType))
                {
                    result.IsValid = false;
                    result.Errors["Avatar"] = "Only JPG, PNG, GIF or WEBP images are allowed.";
                    return result;
                }

                var uploadsRoot = server.MapPath("~/Content/uploads/candidate/");
                if (!Directory.Exists(uploadsRoot))
                {
                    Directory.CreateDirectory(uploadsRoot);
                }

                var ext = Path.GetExtension(avatar.FileName);
                if (string.IsNullOrEmpty(ext))
                {
                    ext = contentType.Contains("png") ? ".png" : contentType.Contains("gif") ? ".gif" : contentType.Contains("webp") ? ".webp" : ".jpg";
                }

                var safeFileName = $"avatar_{accountId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
                var physicalPath = Path.Combine(uploadsRoot, safeFileName);
                avatar.SaveAs(physicalPath);

                var relativePath = $"~/Content/uploads/candidate/{safeFileName}";
                var photo = new ProfilePhoto
                {
                    FileName = safeFileName,
                    FilePath = relativePath,
                    FileSizeKB = (int)Math.Round(avatar.ContentLength / 1024.0),
                    FileFormat = ext.Replace(".", "").ToLower(),
                    UploadedAt = DateTime.UtcNow
                };

                var photoId = _repository.SaveProfilePhoto(photo);
                _repository.UpdatePhotoId(candidate.CandidateID, accountId, photoId);
            }

            _repository.SaveChanges();
            return result;
        }

        public ApplicationListResult GetApplicationsList(int accountId, int pageNumber, int pageSize)
        {
            var result = new ApplicationListResult();

            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                result.Success = false;
                result.ErrorMessage = "Vui lòng hoàn thiện hồ sơ trước khi xem đơn ứng tuyển.";
                return result;
            }

            var applicationsList = _repository.GetApplications(candidate.CandidateID);

            var applications = applicationsList.Select(app => new ApplicationListItemViewModel
            {
                ApplicationID = app.ApplicationID,
                JobPostID = app.JobPostID,
                JobTitle = app.JobPost?.Title ?? "N/A",
                CompanyName = app.JobPost?.Company != null ? app.JobPost.Company.CompanyName :
                             (app.JobPost?.Recruiter?.Company != null ? app.JobPost.Recruiter.Company.CompanyName : "N/A"),
                Location = app.JobPost?.Location ?? string.Empty,
                SalaryRange = SalaryHelper.FormatSalaryRange(app.JobPost?.SalaryFrom, app.JobPost?.SalaryTo, app.JobPost?.SalaryCurrency),
                AppliedAt = app.AppliedAt,
                Status = app.Status ?? "Under review",
                ApplicationDeadline = app.JobPost?.ApplicationDeadline,
                ResumeFilePath = app.ResumeFilePath,
                CertificateFilePath = app.CertificateFilePath,
                Note = app.Note
            }).ToList();

            var totalItems = applications.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            result.Success = true;
            result.Applications = applications.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            result.TotalItems = totalItems;
            result.TotalPages = totalPages;
            result.CurrentPage = pageNumber;

            return result;
        }

        public ApplyFormResult GetApplyFormData(int accountId, int jobPostId, HttpServerUtilityBase server)
        {
            var result = new ApplyFormResult();

            var job = _repository.GetJobPost(jobPostId);
            if (job == null)
            {
                result.Success = false;
                result.ErrorMessage = "Không tìm thấy công việc.";
                return result;
            }

            job.Status = JobStatusHelper.NormalizeStatus(job.Status);

            if (!JobStatusHelper.IsPublished(job.Status))
            {
                result.Success = false;
                result.ErrorMessage = "Công việc này không còn nhận đơn ứng tuyển.";
                return result;
            }

            if (job.ApplicationDeadline.HasValue && job.ApplicationDeadline.Value < DateTime.Now.Date)
            {
                result.Success = false;
                result.ErrorMessage = "Hạn nộp hồ sơ đã qua.";
                return result;
            }

            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                result.Success = false;
                result.ErrorMessage = "Vui lòng hoàn thiện hồ sơ trước khi ứng tuyển.";
                return result;
            }

            var existingApplication = _repository.GetExistingApplication(candidate.CandidateID, jobPostId);
            if (existingApplication != null)
            {
                result.Success = false;
                result.ErrorMessage = "Bạn đã ứng tuyển công việc này rồi.";
                return result;
            }

            // Get photo
            string photoUrl = "/Content/images/person_1.jpg";
            if (candidate.PhotoID.HasValue)
            {
                var photo = _repository.GetProfilePhoto(candidate.PhotoID.Value);
                if (photo != null && !string.IsNullOrEmpty(photo.FilePath))
                {
                    photoUrl = photo.FilePath;
                }
            }

            // Get resumes
            var resumeFiles = _repository.GetResumeFiles(candidate.CandidateID);
            var availableResumes = resumeFiles.Select(rf => new ResumeFileViewModel
            {
                ResumeFileID = rf.ResumeFileID,
                CandidateID = rf.CandidateID,
                FileName = rf.FileName,
                FilePath = rf.FilePath,
                UploadedAt = rf.UploadedAt,
                FileExtension = Path.GetExtension(rf.FileName ?? "")?.ToLower() ?? "",
                DisplayName = string.IsNullOrEmpty(rf.FileName) ? "CV không tên" : Path.GetFileNameWithoutExtension(rf.FileName)
            }).ToList();

            string companyName = job.Company != null ? job.Company.CompanyName :
                               (job.Recruiter?.Company != null ? job.Recruiter.Company.CompanyName : "N/A");

            result.Success = true;
            result.ViewModel = new ApplicationApplyViewModel
            {
                JobPostID = job.JobPostID,
                JobTitle = job.Title,
                CompanyName = companyName,
                Location = job.Location,
                SalaryRange = SalaryHelper.FormatSalaryRange(job.SalaryFrom, job.SalaryTo, job.SalaryCurrency),
                ApplicationDeadline = job.ApplicationDeadline,
                CandidateID = candidate.CandidateID,
                FullName = candidate.FullName,
                BirthDate = candidate.BirthDate,
                Gender = candidate.Gender,
                Phone = candidate.Phone,
                Email = candidate.Email,
                Address = candidate.Address,
                Summary = candidate.Summary,
                PhotoUrl = photoUrl,
                AvailableResumes = availableResumes
            };

            return result;
        }

        public ValidationResult SubmitApplication(
            ApplicationApplyViewModel viewModel,
            int accountId,
            HttpPostedFileBase newResumeFile,
            HttpServerUtilityBase server)
        {
            var result = new ValidationResult { IsValid = true };

            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Vui lòng hoàn thiện hồ sơ trước khi ứng tuyển.";
                return result;
            }

            var job = _repository.GetJobPost(viewModel.JobPostID);
            if (job == null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Công việc này không còn nhận đơn ứng tuyển.";
                return result;
            }

            job.Status = JobStatusHelper.NormalizeStatus(job.Status);
            if (!JobStatusHelper.IsPublished(job.Status))
            {
                result.IsValid = false;
                result.Errors["General"] = "Công việc này không còn nhận đơn ứng tuyển.";
                return result;
            }

            var existingApplication = _repository.GetExistingApplication(candidate.CandidateID, viewModel.JobPostID);
            if (existingApplication != null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Bạn đã ứng tuyển công việc này rồi.";
                return result;
            }

            string resumeFilePath = null;

            // Handle resume
            if (viewModel.SelectedResumeFileID.HasValue)
            {
                var selectedResume = _repository.GetResumeFile(viewModel.SelectedResumeFileID.Value, candidate.CandidateID);
                if (selectedResume != null)
                {
                    resumeFilePath = selectedResume.FilePath;
                }
            }
            else if (newResumeFile != null && newResumeFile.ContentLength > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg", ".gif" };
                var fileExtension = Path.GetExtension(newResumeFile.FileName)?.ToLower();
                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    result.IsValid = false;
                    result.Errors["NewResumeFile"] = "Chỉ cho phép upload file PDF, DOC, DOCX, PNG, JPG, JPEG, GIF.";
                    return result;
                }

                const int maxSize = 10 * 1024 * 1024; // 10MB
                if (newResumeFile.ContentLength > maxSize)
                {
                    result.IsValid = false;
                    result.Errors["NewResumeFile"] = "File không được vượt quá 10MB.";
                    return result;
                }

                try
                {
                    var uploadsRoot = server.MapPath("~/Content/uploads/resumes/");
                    if (!Directory.Exists(uploadsRoot))
                    {
                        Directory.CreateDirectory(uploadsRoot);
                    }

                    var safeFileName = $"CV_{candidate.CandidateID}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{fileExtension}";
                    var physicalPath = Path.Combine(uploadsRoot, safeFileName);
                    newResumeFile.SaveAs(physicalPath);

                    var relativePath = $"/Content/uploads/resumes/{safeFileName}";
                    var resumeFileEntity = new ResumeFile
                    {
                        CandidateID = candidate.CandidateID,
                        FileName = string.IsNullOrWhiteSpace(viewModel.NewResumeTitle) ? newResumeFile.FileName : viewModel.NewResumeTitle + fileExtension,
                        FilePath = relativePath,
                        UploadedAt = DateTime.Now
                    };

                    _repository.SaveResumeFile(resumeFileEntity);
                    resumeFilePath = relativePath;
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Errors["NewResumeFile"] = "Lỗi khi upload CV: " + ex.Message;
                    return result;
                }
            }
            else
            {
                result.IsValid = false;
                result.Errors["General"] = "Vui lòng chọn CV từ danh sách hoặc tải CV mới lên.";
                return result;
            }

            var application = new Application
            {
                CandidateID = candidate.CandidateID,
                JobPostID = viewModel.JobPostID,
                AppliedAt = DateTime.Now,
                Status = "Under review",
                ResumeFilePath = resumeFilePath,
                Note = viewModel.Note,
                UpdatedAt = DateTime.Now
            };

            _repository.CreateApplication(application);
            _repository.SaveChanges();

            return result;
        }

        public ApplicationDetailsResult GetApplicationDetails(int applicationId, int accountId)
        {
            var result = new ApplicationDetailsResult();

            // Get candidate by accountId
            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                result.Success = false;
                result.ErrorMessage = "Vui lòng hoàn thiện hồ sơ trước.";
                return result;
            }

            // Get application and verify ownership
            var application = _repository.GetApplicationById(applicationId, candidate.CandidateID);
            if (application == null)
            {
                result.Success = false;
                result.ErrorMessage = "Không tìm thấy đơn ứng tuyển hoặc bạn không có quyền xem.";
                return result;
            }

            // Get job post details
            var jobPost = application.JobPost;
            if (jobPost == null)
            {
                result.Success = false;
                result.ErrorMessage = "Không tìm thấy thông tin công việc.";
                return result;
            }

            // Get company details
            var company = jobPost.Company;
            
            // Get job post detail for additional info
            var jobDetail = jobPost.JobPostDetails.FirstOrDefault();
            
            // Build salary range string
            string salaryRange = "Thỏa thuận";
            if (jobPost.SalaryFrom.HasValue && jobPost.SalaryTo.HasValue)
            {
                salaryRange = $"{jobPost.SalaryFrom:N0} - {jobPost.SalaryTo:N0} {jobPost.SalaryCurrency ?? "VND"}";
            }
            else if (jobPost.SalaryFrom.HasValue)
            {
                salaryRange = $"Từ {jobPost.SalaryFrom:N0} {jobPost.SalaryCurrency ?? "VND"}";
            }

            // Get company logo path from ProfilePhoto
            string companyLogo = null;
            if (company?.PhotoID.HasValue == true && company.PhotoID.Value > 0)
            {
                var photo = company.ProfilePhoto;
                if (photo != null && !string.IsNullOrEmpty(photo.FilePath))
                {
                    companyLogo = photo.FilePath;
                }
            }

            // Build view model
            var viewModel = new ApplicationDetailsViewModel
            {
                ApplicationID = application.ApplicationID,
                JobPostID = jobPost.JobPostID,
                JobTitle = jobPost.Title,
                JobDescription = jobPost.Description,
                JobRequirements = jobPost.Requirements,
                CompanyName = company?.CompanyName ?? "N/A",
                CompanyLogo = companyLogo,
                Location = jobPost.Location,
                SalaryRange = salaryRange,
                JobType = jobPost.EmploymentType,
                ExperienceLevel = jobDetail != null ? $"{jobDetail.YearsExperience} năm" : "N/A",
                ApplicationDeadline = jobPost.ApplicationDeadline,
                Status = application.Status,
                AppliedAt = application.AppliedAt,
                Note = application.Note,
                ResumeFilePath = application.ResumeFilePath,
                CertificateFilePath = application.CertificateFilePath,
                CandidateName = candidate.FullName,
                CandidateEmail = candidate.Email,
                CandidatePhone = candidate.Phone,
                UpdatedAt = application.UpdatedAt
            };

            result.Success = true;
            result.ViewModel = viewModel;
            return result;
        }
    }
}

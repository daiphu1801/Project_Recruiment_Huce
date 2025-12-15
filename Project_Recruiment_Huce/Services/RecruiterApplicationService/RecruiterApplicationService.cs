using System;
using System.Collections.Generic;
using System.Linq;
using Project_Recruiment_Huce.Models.Recruiters;
using Project_Recruiment_Huce.Repositories.RecruiterApplicationRepo;
using Hangfire;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Services;

namespace Project_Recruiment_Huce.Services.RecruiterApplicationService
{
    /// <summary>
    /// Service implementation for Recruiter Application operations
    /// </summary>
    public class RecruiterApplicationService : IRecruiterApplicationService
    {
        private readonly IRecruiterApplicationRepository _repository;
        private static readonly string[] ValidStatuses = { "Under review", "Interview", "Offered", "Hired", "Rejected" };

        public RecruiterApplicationService(IRecruiterApplicationRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public ApplicationListResult GetApplicationsList(int recruiterId, int? jobId, string status, int page, int pageSize)
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            try
            {
                // Get applications for recruiter
                var query = _repository.GetApplicationsByRecruiter(recruiterId);

                // Apply filters
                if (jobId.HasValue)
                {
                    query = query.Where(a => a.JobPostID == jobId.Value);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(a => a.Status == status);
                }

                var applications = query.ToList();

                // Map to ViewModels
                var viewModels = applications.Select(app => MapToViewModel(app)).ToList();

                // Apply pagination
                int totalItems = viewModels.Count;
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var pagedApplications = viewModels.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return new ApplicationListResult
                {
                    Success = true,
                    Applications = pagedApplications,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalItems = totalItems,
                    JobId = jobId,
                    Status = status
                };
            }
            catch (Exception ex)
            {
                return new ApplicationListResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi khi tải danh sách đơn ứng tuyển: {ex.Message}"
                };
            }
        }

        public ApplicationDetailsResult GetApplicationDetails(int applicationId, int recruiterId)
        {
            try
            {
                // Get application with details
                var application = _repository.GetApplicationByIdWithDetails(applicationId);
                if (application == null)
                {
                    return new ApplicationDetailsResult
                    {
                        Success = false,
                        ErrorMessage = "Không tìm thấy đơn ứng tuyển."
                    };
                }

                // Verify ownership
                if (!_repository.IsApplicationOwnedByRecruiter(applicationId, recruiterId))
                {
                    return new ApplicationDetailsResult
                    {
                        Success = false,
                        ErrorMessage = "Bạn không có quyền xem đơn ứng tuyển này."
                    };
                }

                var viewModel = MapToViewModel(application);

                return new ApplicationDetailsResult
                {
                    Success = true,
                    Application = viewModel
                };
            }
            catch (Exception ex)
            {
                return new ApplicationDetailsResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi khi tải chi tiết đơn ứng tuyển: {ex.Message}"
                };
            }
        }

        public ApplicationStatusFormResult GetApplicationForStatusUpdate(int applicationId, int recruiterId)
        {
            try
            {
                // Get application with details
                var application = _repository.GetApplicationByIdWithDetails(applicationId);
                if (application == null)
                {
                    return new ApplicationStatusFormResult
                    {
                        Success = false,
                        ErrorMessage = "Không tìm thấy đơn ứng tuyển."
                    };
                }

                // Verify ownership
                if (!_repository.IsApplicationOwnedByRecruiter(applicationId, recruiterId))
                {
                    return new ApplicationStatusFormResult
                    {
                        Success = false,
                        ErrorMessage = "Bạn không có quyền cập nhật đơn ứng tuyển này."
                    };
                }

                var viewModel = new UpdateApplicationStatusViewModel
                {
                    ApplicationID = application.ApplicationID,
                    JobPostID = application.JobPostID,
                    CandidateName = !string.IsNullOrEmpty(application.Candidate?.FullName)
                        ? application.Candidate.FullName
                        : "Ứng viên ẩn danh",
                    JobTitle = !string.IsNullOrEmpty(application.JobPost?.Title)
                        ? application.JobPost.Title
                        : "Công việc không có tiêu đề",
                    Status = application.Status ?? "Under review",
                    Note = application.Note
                };

                return new ApplicationStatusFormResult
                {
                    Success = true,
                    ViewModel = viewModel
                };
            }
            catch (Exception ex)
            {
                return new ApplicationStatusFormResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi khi tải form cập nhật: {ex.Message}"
                };
            }
        }

        public UpdateStatusResult UpdateApplicationStatus(int applicationId, int recruiterId, string status, string note)
        {
            try
            {
                // Verify ownership
                if (!_repository.IsApplicationOwnedByRecruiter(applicationId, recruiterId))
                {
                    return new UpdateStatusResult
                    {
                        Success = false,
                        ErrorMessage = "Bạn không có quyền cập nhật đơn ứng tuyển này."
                    };
                }

                // Validate status
                if (!ValidStatuses.Contains(status))
                {
                    return new UpdateStatusResult
                    {
                        Success = false,
                        ErrorMessage = "Trạng thái không hợp lệ."
                    };
                }

                // Update application
                _repository.UpdateApplicationStatus(applicationId, status, note);
                _repository.SaveChanges();

                return new UpdateStatusResult
                {
                    Success = true,
                    SuccessMessage = $"Đã cập nhật trạng thái đơn ứng tuyển thành '{GetApplicationStatusDisplay(status)}'.",
                    ApplicationId = applicationId
                };
            }
            catch (Exception ex)
            {
                return new UpdateStatusResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi khi cập nhật trạng thái: {ex.Message}"
                };
            }
        }

        public List<JobFilterItem> GetJobsForFilter(int recruiterId)
        {
            var jobs = _repository.GetJobsByRecruiter(recruiterId);
            return jobs.Select(j => new JobFilterItem
            {
                JobPostID = j.JobPostID,
                Title = j.Title,
                JobCode = j.JobCode
            }).ToList();
        }

        #region Private Helper Methods

        private RecruiterApplicationViewModel MapToViewModel(Models.Application app)
        {
            string companyName = "Không có thông tin";
            if (app.JobPost?.Company?.CompanyName != null)
            {
                companyName = app.JobPost.Company.CompanyName;
            }
            else if (app.JobPost?.Recruiter?.Company?.CompanyName != null)
            {
                companyName = app.JobPost.Recruiter.Company.CompanyName;
            }

            return new RecruiterApplicationViewModel
            {
                ApplicationID = app.ApplicationID,
                JobPostID = app.JobPostID,
                CandidateID = app.CandidateID,
                CandidateName = !string.IsNullOrEmpty(app.Candidate?.FullName)
                    ? app.Candidate.FullName
                    : "Ứng viên ẩn danh",
                CandidateEmail = app.Candidate?.Email ?? "",
                CandidatePhone = app.Candidate?.Phone ?? "",
                JobTitle = !string.IsNullOrEmpty(app.JobPost?.Title)
                    ? app.JobPost.Title
                    : "Công việc không có tiêu đề",
                JobCode = app.JobPost?.JobCode ?? "",
                CompanyName = companyName,
                AppliedAt = app.AppliedAt,
                Status = app.Status ?? "Under review",
                ResumeFilePath = app.ResumeFilePath,
                CertificateFilePath = app.CertificateFilePath,
                Note = app.Note,
                UpdatedAt = app.UpdatedAt,
                StatusDisplay = GetApplicationStatusDisplay(app.Status ?? "Under review"),
                StatusBadgeClass = GetApplicationStatusBadgeClass(app.Status ?? "Under review")
            };
        }

        private string GetApplicationStatusDisplay(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "Đang xem xét";

            switch (status.ToLower())
            {
                case "under review":
                    return "Đang xem xét";
                case "interview":
                    return "Phỏng vấn";
                case "offered":
                    return "Đã đề xuất";
                case "hired":
                    return "Đã tuyển";
                case "rejected":
                    return "Đã từ chối";
                default:
                    return status;
            }
        }

        private string GetApplicationStatusBadgeClass(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "badge-warning";

            switch (status.ToLower())
            {
                case "under review":
                    return "badge-warning";
                case "interview":
                    return "badge-info";
                case "offered":
                    return "badge-primary";
                case "hired":
                    return "badge-success";
                case "rejected":
                    return "badge-danger";
                default:
                    return "badge-secondary";
            }
        }

        public ServiceResult ScheduleInterview(InterviewScheduleViewModel viewModel, int recruiterId)
        {
            try
            {
                // 1. Lấy dữ liệu
                var application = _repository.GetApplicationById(viewModel.ApplicationID);
                if (application == null)
                    return new ServiceResult { Success = false, ErrorMessage = "Không tìm thấy đơn ứng tuyển." };

                // 2. Check quyền
                if (!_repository.IsApplicationOwnedByRecruiter(viewModel.ApplicationID, recruiterId))
                    return new ServiceResult { Success = false, ErrorMessage = "Bạn không có quyền thao tác trên đơn ứng tuyển này." };

                // 3. Map dữ liệu
                var emailData = new InterviewEmailData
                {
                    ApplicationID = viewModel.ApplicationID,
                    CandidateName = viewModel.CandidateName,
                    CandidateEmail = viewModel.CandidateEmail,
                    JobTitle = viewModel.JobTitle,
                    InterviewDate = viewModel.InterviewDate,
                    InterviewTime = viewModel.InterviewTime,
                    Duration = viewModel.Duration ?? 0,
                    Location = viewModel.Location,
                    InterviewType = viewModel.InterviewType,
                    Interviewer = viewModel.Interviewer,
                    RequiredDocuments = viewModel.RequiredDocuments,
                    AdditionalNotes = viewModel.AdditionalNotes
                };

                // 4. Gọi Hangfire (QUAN TRỌNG)
                BackgroundJob.Enqueue<EmailService>(service => service.SendInterviewInvitation(emailData));

                // 5. Update DB
                string logNote = $"Đã gửi lịch phỏng vấn: {viewModel.InterviewDate:dd/MM/yyyy} lúc {viewModel.InterviewTime}.";
                _repository.UpdateApplicationStatus(viewModel.ApplicationID, "Interview", logNote);
                _repository.SaveChanges();

                return new ServiceResult
                {
                    Success = true,
                    SuccessMessage = $"Đã lên lịch thành công! Email mời phỏng vấn đang được gửi đến {viewModel.CandidateEmail}."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Lỗi hệ thống: " + ex.Message };
            }
        }

        // Helper


        #endregion
    }
}

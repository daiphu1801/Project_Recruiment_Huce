using System;
using System.Collections.Generic;
using System.Linq;
using Project_Recruiment_Huce.Models.Recruiters;
using Project_Recruiment_Huce.Repositories.RecruiterApplicationRepo;
using Hangfire;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Services;
using Project_Recruiment_Huce.Services.SubscriptionService;

namespace Project_Recruiment_Huce.Services.RecruiterApplicationService
{
    /// <summary>
    /// Service implementation for Recruiter Application operations
    /// </summary>
    public class RecruiterApplicationService : IRecruiterApplicationService
    {
        private readonly IRecruiterApplicationRepository _repository;
        private readonly ISubscriptionService _subscriptionService;
        private static readonly string[] ValidStatuses = { "Under review", "Interview", "Offered", "Hired", "Rejected" };

        public RecruiterApplicationService(IRecruiterApplicationRepository repository, ISubscriptionService subscriptionService = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _subscriptionService = subscriptionService ?? new SubscriptionService.SubscriptionService();
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
                // 1. Verify ownership (giữ nguyên logic cũ)
                if (!_repository.IsApplicationOwnedByRecruiter(applicationId, recruiterId))
                {
                    return new UpdateStatusResult
                    {
                        Success = false,
                        ErrorMessage = "Bạn không có quyền cập nhật đơn ứng tuyển này."
                    };
                }

                // --- ĐOẠN CODE THÊM MỚI ---
                // 2. Kiểm tra trạng thái tin tuyển dụng (JobPost)
                // Lấy thông tin chi tiết để check JobPost.Status
                var application = _repository.GetApplicationByIdWithDetails(applicationId);

                if (application?.JobPost != null)
                {
                    string jobStatus = application.JobPost.Status;
                    if (jobStatus == "Closed" || jobStatus == "Expired")
                    {
                        return new UpdateStatusResult
                        {
                            Success = false,
                            ErrorMessage = $"Không thể cập nhật đơn ứng tuyển vì tin tuyển dụng này đã đóng hoặc hết hạn (Trạng thái: {jobStatus})."
                        };
                    }
                }
                // ---------------------------

                // 3. Validate status (giữ nguyên logic cũ)
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

        public List<StatusOption> GetStatusOptions()
        {
            return new List<StatusOption>
            {
                new StatusOption { Value = "", Text = "Tất cả trạng thái" },
                new StatusOption { Value = "Under review", Text = "Đang xem xét" },
                new StatusOption { Value = "Interview", Text = "Phỏng vấn" },
                new StatusOption { Value = "Offered", Text = "Đã đề xuất" },
                new StatusOption { Value = "Hired", Text = "Đã tuyển" },
                new StatusOption { Value = "Rejected", Text = "Đã từ chối" }
            };
        }

        public List<InterviewTypeOption> GetInterviewTypeOptions()
        {
            return new List<InterviewTypeOption>
            {
                new InterviewTypeOption { Value = "Trực tiếp", Text = "Phỏng vấn trực tiếp tại văn phòng" },
                new InterviewTypeOption { Value = "Trực tuyến", Text = "Phỏng vấn trực tuyến (Online)" },
                new InterviewTypeOption { Value = "Điện thoại", Text = "Phỏng vấn qua điện thoại" }
            };
        }

        public ScheduleInterviewFormResult GetScheduleInterviewForm(int applicationId, int recruiterId)
        {
            try
            {
                // Business rule 1: Check subscription
                if (!_subscriptionService.HasActiveSubscription(recruiterId))
                {
                    return new ScheduleInterviewFormResult
                    {
                        Success = false,
                        RequiresSubscription = true,
                        ErrorMessage = "Tính năng gửi email mời phỏng vấn chỉ dành cho gói đăng ký trả phí. Vui lòng nâng cấp!"
                    };
                }

                // Get application details
                var detailResult = GetApplicationDetails(applicationId, recruiterId);
                if (!detailResult.Success)
                {
                    return new ScheduleInterviewFormResult
                    {
                        Success = false,
                        ErrorMessage = detailResult.ErrorMessage
                    };
                }

                // Business rule 2: Status must be Interview
                if (detailResult.Application.Status != "Interview")
                {
                    return new ScheduleInterviewFormResult
                    {
                        Success = false,
                        ErrorMessage = "Chỉ có thể đặt lịch phỏng vấn cho đơn ứng tuyển có trạng thái 'Phỏng vấn'."
                    };
                }

                // Build ViewModel with default values
                return new ScheduleInterviewFormResult
                {
                    Success = true,
                    ViewModel = new InterviewScheduleViewModel
                    {
                        ApplicationID = detailResult.Application.ApplicationID,
                        CandidateName = detailResult.Application.CandidateName,
                        CandidateEmail = detailResult.Application.CandidateEmail,
                        JobTitle = detailResult.Application.JobTitle,
                        InterviewDate = DateTime.Today.AddDays(3),
                        Duration = 60
                    }
                };
            }
            catch (Exception ex)
            {
                return new ScheduleInterviewFormResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi khi tải form đặt lịch phỏng vấn: {ex.Message}"
                };
            }
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
                // Business rule: Check subscription
                if (!_subscriptionService.HasActiveSubscription(recruiterId))
                {
                    return new ServiceResult { Success = false, ErrorMessage = "SUBSCRIPTION_REQUIRED" };
                }

                // 1. Lấy dữ liệu
                var application = _repository.GetApplicationById(viewModel.ApplicationID);
                if (application == null)
                    return new ServiceResult { Success = false, ErrorMessage = "Không tìm thấy đơn ứng tuyển." };

                if (application.JobPost != null)
                {
                    if (application.JobPost.Status == "Closed" || application.JobPost.Status == "Expired")
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            ErrorMessage = "Không thể xếp lịch phỏng vấn vì tin tuyển dụng đã đóng hoặc hết hạn."
                        };
                    }
                }

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

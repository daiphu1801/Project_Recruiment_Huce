using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Jobs;
using Project_Recruiment_Huce.Repositories.JobRepo;

namespace Project_Recruiment_Huce.Services.JobService
{
    /// <summary>
    /// Service triển khai business logic cho tin tuyển dụng
    /// Xử lý validation, HTML sanitization, employment type conversion
    /// </summary>
    public class JobService : IJobService
    {
        private readonly IJobRepository _repository;

        public JobService(IJobRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public CreateJobResult ValidateAndCreateJob(JobCreateViewModel viewModel, int recruiterId)
        {
            var result = new CreateJobResult();

            // Get recruiter to get CompanyID
            var recruiter = _repository.GetRecruiterById(recruiterId);
            if (recruiter == null)
            {
                result.ErrorMessage = "Không tìm thấy thông tin Recruiter.";
                return result;
            }

            // Generate JobCode if not provided
            string jobCode = viewModel.JobCode;
            if (string.IsNullOrWhiteSpace(jobCode))
            {
                jobCode = _repository.GenerateNextJobCode();
            }

            // Validate salary range
            if (viewModel.SalaryFrom.HasValue && viewModel.SalaryTo.HasValue && viewModel.SalaryTo < viewModel.SalaryFrom)
            {
                result.Errors["SalaryTo"] = "Lương đến phải lớn hơn hoặc bằng lương từ";
                return result;
            }

            // Validate age range
            if (viewModel.AgeFrom.HasValue && viewModel.AgeTo.HasValue && viewModel.AgeTo < viewModel.AgeFrom)
            {
                result.Errors["AgeTo"] = "Độ tuổi đến phải lớn hơn hoặc bằng độ tuổi từ";
                return result;
            }

            // Sanitize HTML content
            string sanitizedDescription = SanitizeHtmlField(viewModel.Description);
            string sanitizedRequirements = SanitizeHtmlField(viewModel.Requirements);
            string sanitizedSkills = SanitizeHtmlField(viewModel.Skills);

            // Convert employment type from Vietnamese to database format
            string employmentType = ConvertEmploymentTypeToDatabaseFormat(viewModel.EmploymentType);

            try
            {
                // Create JobPost
                var jobPost = new JobPost
                {
                    JobCode = jobCode,
                    RecruiterID = recruiterId,
                    CompanyID = recruiter.CompanyID,
                    Title = viewModel.Title,
                    Description = sanitizedDescription,
                    Requirements = sanitizedRequirements,
                    SalaryFrom = viewModel.SalaryFrom,
                    SalaryTo = viewModel.SalaryTo,
                    SalaryCurrency = viewModel.SalaryCurrency ?? "VND",
                    Location = viewModel.Location,
                    EmploymentType = employmentType,
                    ApplicationDeadline = viewModel.ApplicationDeadline,
                    Status = "Published",
                    PostedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _repository.CreateJobPost(jobPost);
                _repository.SaveChanges();

                // Create JobPostDetail
                var jobPostDetail = new JobPostDetail
                {
                    JobPostID = jobPost.JobPostID,
                    Industry = viewModel.Industry ?? "Khác",
                    Major = viewModel.Major,
                    YearsExperience = viewModel.YearsExperience ?? 0,
                    DegreeRequired = viewModel.DegreeRequired,
                    Skills = sanitizedSkills,
                    Headcount = viewModel.Headcount ?? 1,
                    GenderRequirement = viewModel.GenderRequirement ?? "Not required",
                    AgeFrom = viewModel.AgeFrom,
                    AgeTo = viewModel.AgeTo,
                    Status = "Published"
                };

                _repository.CreateJobPostDetail(jobPostDetail);
                _repository.SaveChanges();

                result.Success = true;
                result.JobPostId = jobPost.JobPostID;
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Lỗi khi lưu công việc: {ex.Message}";
                if (ex.InnerException != null)
                {
                    result.ErrorMessage += $" | Chi tiết: {ex.InnerException.Message}";
                }
                return result;
            }
        }

        public EditJobResult GetJobForEdit(int jobPostId, int recruiterId)
        {
            var result = new EditJobResult();

            // Get job and verify ownership
            var job = _repository.GetJobPostByIdAndRecruiterId(jobPostId, recruiterId);
            if (job == null)
            {
                result.ErrorMessage = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền chỉnh sửa tin này.";
                return result;
            }

            // Get job details
            var jobDetail = _repository.GetJobPostDetailByJobPostId(job.JobPostID);

            // Convert employment type from database format to Vietnamese
            string employmentType = ConvertEmploymentTypeToVietnamese(job.EmploymentType);

            // Map to ViewModel
            result.ViewModel = new JobCreateViewModel
            {
                Title = job.Title,
                Description = job.Description,
                Requirements = job.Requirements,
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                SalaryCurrency = job.SalaryCurrency ?? "VND",
                Location = job.Location,
                EmploymentType = employmentType,
                ApplicationDeadline = job.ApplicationDeadline,
                JobCode = job.JobCode,
                CompanyID = job.CompanyID,
                Industry = jobDetail?.Industry,
                Major = jobDetail?.Major,
                YearsExperience = jobDetail?.YearsExperience,
                DegreeRequired = jobDetail?.DegreeRequired,
                Skills = jobDetail?.Skills,
                Headcount = jobDetail?.Headcount ?? 1,
                GenderRequirement = jobDetail?.GenderRequirement ?? "Not required",
                AgeFrom = jobDetail?.AgeFrom,
                AgeTo = jobDetail?.AgeTo
            };

            // Load companies for dropdown (using separate read-only repository)
            var readOnlyRepo = new JobRepository(readOnly: true);
            result.Companies = GetCompaniesSelectList(readOnlyRepo, job.CompanyID);
            result.JobPostId = job.JobPostID;
            result.Success = true;

            return result;
        }

        public UpdateJobResult ValidateAndUpdateJob(int jobPostId, JobCreateViewModel viewModel, int recruiterId)
        {
            var result = new UpdateJobResult();

            // Get job and verify ownership
            var job = _repository.GetJobPostByIdAndRecruiterId(jobPostId, recruiterId);
            if (job == null)
            {
                result.ErrorMessage = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền chỉnh sửa tin này.";
                return result;
            }

            // Validate salary range
            if (viewModel.SalaryFrom.HasValue && viewModel.SalaryTo.HasValue && viewModel.SalaryTo < viewModel.SalaryFrom)
            {
                result.Errors["SalaryTo"] = "Lương đến phải lớn hơn hoặc bằng lương từ";
                return result;
            }

            // Validate age range
            if (viewModel.AgeFrom.HasValue && viewModel.AgeTo.HasValue && viewModel.AgeTo < viewModel.AgeFrom)
            {
                result.Errors["AgeTo"] = "Độ tuổi đến phải lớn hơn hoặc bằng độ tuổi từ";
                return result;
            }

            // Sanitize HTML content
            string sanitizedDescription = SanitizeHtmlField(viewModel.Description);
            string sanitizedRequirements = SanitizeHtmlField(viewModel.Requirements);
            string sanitizedSkills = SanitizeHtmlField(viewModel.Skills);

            // Convert employment type from Vietnamese to database format
            string employmentType = ConvertEmploymentTypeToDatabaseFormat(viewModel.EmploymentType);

            try
            {
                // Update JobPost
                job.Title = viewModel.Title;
                job.Description = sanitizedDescription;
                job.Requirements = sanitizedRequirements;
                job.SalaryFrom = viewModel.SalaryFrom;
                job.SalaryTo = viewModel.SalaryTo;
                job.SalaryCurrency = viewModel.SalaryCurrency ?? "VND";
                job.Location = viewModel.Location;
                job.EmploymentType = employmentType;
                job.ApplicationDeadline = viewModel.ApplicationDeadline;
                job.UpdatedAt = DateTime.Now;

                _repository.UpdateJobPost(job);
                _repository.SaveChanges();

                // Update or create JobPostDetail
                var jobDetail = _repository.GetJobPostDetailByJobPostId(job.JobPostID);
                if (jobDetail == null)
                {
                    jobDetail = new JobPostDetail
                    {
                        JobPostID = job.JobPostID,
                        Status = job.Status ?? "Published"
                    };
                    _repository.CreateJobPostDetail(jobDetail);
                }

                jobDetail.Industry = viewModel.Industry ?? "Khác";
                jobDetail.Major = viewModel.Major;
                jobDetail.YearsExperience = viewModel.YearsExperience ?? 0;
                jobDetail.DegreeRequired = viewModel.DegreeRequired;
                jobDetail.Skills = sanitizedSkills;
                jobDetail.Headcount = viewModel.Headcount ?? 1;
                jobDetail.GenderRequirement = viewModel.GenderRequirement ?? "Not required";
                jobDetail.AgeFrom = viewModel.AgeFrom;
                jobDetail.AgeTo = viewModel.AgeTo;

                _repository.UpdateJobPostDetail(jobDetail);
                _repository.SaveChanges();

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Lỗi khi cập nhật tin tuyển dụng: {ex.Message}";
                return result;
            }
        }

        public ServiceResult CloseJob(int jobPostId, int recruiterId, string status)
        {
            var result = new ServiceResult();

            // Validate status values
            if (status != "Closed" && status != "Expired")
            {
                status = "Closed";
            }

            // Get job and verify ownership
            var job = _repository.GetJobPostByIdAndRecruiterId(jobPostId, recruiterId);
            if (job == null)
            {
                result.ErrorMessage = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền thực hiện thao tác này.";
                return result;
            }

            // Check for pending applications
            var pendingApplicationsCount = _repository.GetPendingApplicationsCount(jobPostId);
            if (pendingApplicationsCount > 0)
            {
                result.ErrorMessage = $"Không thể đóng tin tuyển dụng này. Vui lòng xử lý hết {pendingApplicationsCount} hồ sơ ứng viên đang trong quá trình xử lý (Đang xem xét, Phỏng vấn, hoặc Đã đề xuất) trước khi đóng tin.";
                return result;
            }

            try
            {
                // Update status
                job.Status = status;
                job.UpdatedAt = DateTime.Now;

                _repository.UpdateJobPost(job);
                _repository.SaveChanges();

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Lỗi khi đóng tin tuyển dụng: {ex.Message}";
                return result;
            }
        }

        public ServiceResult ReopenJob(int jobPostId, int recruiterId)
        {
            var result = new ServiceResult();

            // Get job and verify ownership
            var job = _repository.GetJobPostByIdAndRecruiterId(jobPostId, recruiterId);
            if (job == null)
            {
                result.ErrorMessage = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền thực hiện thao tác này.";
                return result;
            }

            // Only allow reopening "Closed" status
            if (job.Status != "Closed")
            {
                if (job.Status == "Expired")
                {
                    result.ErrorMessage = "Không thể mở lại tin đã hết hạn. Vui lòng tạo tin tuyển dụng mới hoặc cập nhật hạn nộp trước khi mở lại.";
                }
                else
                {
                    result.ErrorMessage = "Chỉ có thể mở lại tin tuyển dụng đã đóng.";
                }
                return result;
            }

            // Check if ApplicationDeadline has passed
            bool isDeadlinePassed = job.ApplicationDeadline.HasValue && 
                                   job.ApplicationDeadline.Value < DateTime.Now.Date;

            if (isDeadlinePassed)
            {
                result.WarningMessage = "Hạn nộp hồ sơ đã qua. Vui lòng cập nhật hạn nộp mới trước khi mở lại tin tuyển dụng.";
                result.RedirectAction = "Edit";
                result.RedirectRouteValues = new { id = jobPostId };
                return result;
            }

            try
            {
                // Update status to Published
                job.Status = "Published";
                job.UpdatedAt = DateTime.Now;

                _repository.UpdateJobPost(job);
                _repository.SaveChanges();

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Lỗi khi mở lại tin tuyển dụng: {ex.Message}";
                return result;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Sanitize HTML field with whitespace trimming
        /// </summary>
        private string SanitizeHtmlField(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            string trimmed = input.Trim();
            if (string.IsNullOrEmpty(trimmed))
                return null;

            string sanitized = HtmlSanitizerHelper.Sanitize(trimmed);
            return string.IsNullOrWhiteSpace(sanitized) ? trimmed : sanitized;
        }

        /// <summary>
        /// Convert employment type from Vietnamese to database format
        /// </summary>
        private string ConvertEmploymentTypeToDatabaseFormat(string employmentType)
        {
            if (string.IsNullOrWhiteSpace(employmentType))
                return null;

            if (employmentType == "Bán thời gian")
                return "Part-time";
            else if (employmentType == "Toàn thời gian")
                return "Full-time";

            return employmentType;
        }

        /// <summary>
        /// Convert employment type from database format to Vietnamese
        /// </summary>
        private string ConvertEmploymentTypeToVietnamese(string employmentType)
        {
            if (string.IsNullOrWhiteSpace(employmentType))
                return null;

            if (employmentType == "Part-time")
                return "Bán thời gian";
            else if (employmentType == "Full-time")
                return "Toàn thời gian";

            return employmentType;
        }

        /// <summary>
        /// Get companies select list for dropdown
        /// </summary>
        private List<SelectListItem> GetCompaniesSelectList(IJobRepository repository, int? selectedCompanyId)
        {
            var companies = new List<SelectListItem>();
            
            // Create a new read-only repository instance to query companies
            var readOnlyRepo = new JobRepository(readOnly: true);
            
            try
            {
                // Get companies (this is a simplification - ideally use CompanyRepository)
                var companyList = new List<Company>();
                for (int i = 1; i <= 100; i++) // Adjust range as needed
                {
                    var company = readOnlyRepo.GetCompanyById(i);
                    if (company != null && company.ActiveFlag == 1)
                    {
                        companyList.Add(company);
                    }
                }

                companies = companyList.Select(c => new SelectListItem
                {
                    Value = c.CompanyID.ToString(),
                    Text = c.CompanyName,
                    Selected = c.CompanyID == selectedCompanyId
                }).ToList();
            }
            finally
            {
                // Cleanup handled by garbage collector
            }

            return companies;
        }

        #endregion
    }
}

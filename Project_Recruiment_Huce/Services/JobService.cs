using System;
using System.Collections.Generic;
using System.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Jobs;
using Project_Recruiment_Huce.Repositories;
using Project_Recruiment_Huce.Mappers;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Infrastructure;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// Service class for job-related business logic
    /// Handles complex operations and coordinates between repositories
    /// </summary>
    public class JobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly JOBPORTAL_ENDataContext _db;

        public JobService(IJobRepository jobRepository, JOBPORTAL_ENDataContext db)
        {
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Get job details with related information
        /// </summary>
        public JobDetailsViewModel GetJobDetails(int jobId)
        {
            var job = _jobRepository.GetByIdWithDetails(jobId);
            if (job == null) return null;

            JobStatusHelper.NormalizeStatuses(_db);
            return JobMapper.MapToDetails(job);
        }

        /// <summary>
        /// Get related jobs for a specific job
        /// </summary>
        public List<RelatedJobViewModel> GetRelatedJobs(int jobId, int companyId, string location, int take = 5)
        {
            var relatedJobs = _jobRepository.GetRelatedJobs(jobId, companyId, location, take);
            return relatedJobs.Select(j => JobMapper.MapToRelatedJob(j)).ToList();
        }

        /// <summary>
        /// Search jobs with filters and pagination
        /// </summary>
        public (List<JobListingItemViewModel> Jobs, int TotalItems, int TotalPages) SearchJobs(
            string keyword, 
            string location, 
            string employmentType, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            // Get filtered jobs
            var jobs = _jobRepository.SearchJobs(keyword, location, employmentType);

            // Pagination
            int totalItems = jobs.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedJobs = jobs
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Map to ViewModels
            var jobViewModels = pagedJobs.Select(j => JobMapper.MapToListingItem(j)).ToList();

            return (jobViewModels, totalItems, totalPages);
        }

        /// <summary>
        /// Get jobs by recruiter with pagination
        /// </summary>
        public (List<JobListingItemViewModel> Jobs, int TotalItems, int TotalPages) GetJobsByRecruiter(
            int recruiterId, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            var jobs = _jobRepository.GetJobsByRecruiter(recruiterId);

            // Pagination
            int totalItems = jobs.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedJobs = jobs
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Map to ViewModels with db context for pending applications count
            var jobViewModels = pagedJobs.Select(j => JobMapper.MapToListingItem(j, _db)).ToList();

            return (jobViewModels, totalItems, totalPages);
        }

        /// <summary>
        /// Get published jobs for homepage
        /// </summary>
        public List<JobListingItemViewModel> GetRecentPublishedJobs(int take = 7)
        {
            JobStatusHelper.NormalizeStatuses(_db);
            
            var recentJobs = _db.JobPosts
                .Where(j => j.Status == JobStatusHelper.Published)
                .OrderByDescending(j => j.PostedAt > j.UpdatedAt ? j.PostedAt : j.UpdatedAt)
                .Take(take)
                .ToList();

            return recentJobs.Select(j => JobMapper.MapToListingItem(j, _db)).ToList();
        }

        /// <summary>
        /// Create new job post
        /// </summary>
        public (bool Success, string Message, int? JobId) CreateJob(JobCreateViewModel model, int recruiterId)
        {
            try
            {
                // Get recruiter to get CompanyID
                var recruiter = _db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId);
                if (recruiter == null)
                {
                    return (false, "Không tìm thấy thông tin Recruiter.", null);
                }

                // Generate JobCode if not provided
                string jobCode = model.JobCode;
                if (string.IsNullOrWhiteSpace(jobCode))
                {
                    var lastJob = _db.JobPosts.OrderByDescending(j => j.JobPostID).FirstOrDefault();
                    int nextNumber = 1;
                    if (lastJob != null && !string.IsNullOrEmpty(lastJob.JobCode))
                    {
                        string code = lastJob.JobCode.ToUpper();
                        if (code.StartsWith("JOB"))
                        {
                            string numStr = code.Substring(3);
                            if (int.TryParse(numStr, out int lastNum))
                            {
                                nextNumber = lastNum + 1;
                            }
                        }
                        else
                        {
                            nextNumber = lastJob.JobPostID + 1;
                        }
                    }
                    jobCode = $"JOB{nextNumber:D4}";
                }

                // Validate salary range
                if (model.SalaryFrom.HasValue && model.SalaryTo.HasValue && model.SalaryTo < model.SalaryFrom)
                {
                    return (false, "Lương đến phải lớn hơn hoặc bằng lương từ", null);
                }

                // Validate age range
                if (model.AgeFrom.HasValue && model.AgeTo.HasValue && model.AgeTo < model.AgeFrom)
                {
                    return (false, "Độ tuổi đến phải lớn hơn hoặc bằng độ tuổi từ", null);
                }

                // Sanitize HTML content
                string sanitizedDescription = HtmlSanitizerHelper.Sanitize(model.Description?.Trim());
                string sanitizedRequirements = HtmlSanitizerHelper.Sanitize(model.Requirements?.Trim());

                // Create JobPost
                var newJob = new JobPost
                {
                    JobCode = jobCode,
                    Title = model.Title?.Trim(),
                    Description = sanitizedDescription,
                    Requirements = sanitizedRequirements,
                    Location = model.Location?.Trim(),
                    EmploymentType = model.EmploymentType?.Trim(),
                    SalaryFrom = model.SalaryFrom,
                    SalaryTo = model.SalaryTo,
                    SalaryCurrency = model.SalaryCurrency ?? "VND",
                    ApplicationDeadline = model.ApplicationDeadline,
                    RecruiterID = recruiterId,
                    CompanyID = recruiter.CompanyID,
                    PostedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = "Published"
                };

                _jobRepository.Add(newJob);
                _jobRepository.SaveChanges();

                // Create JobPostDetail if additional details provided
                if (!string.IsNullOrWhiteSpace(model.Industry) || !string.IsNullOrWhiteSpace(model.Major))
                {
                    var jobDetail = new JobPostDetail
                    {
                        JobPostID = newJob.JobPostID,
                        Industry = model.Industry?.Trim(),
                        Major = model.Major?.Trim(),
                        YearsExperience = model.YearsExperience ?? 0,
                        DegreeRequired = model.DegreeRequired?.Trim(),
                        Skills = model.Skills?.Trim(),
                        Headcount = model.Headcount ?? 1,
                        GenderRequirement = model.GenderRequirement?.Trim(),
                        AgeFrom = model.AgeFrom,
                        AgeTo = model.AgeTo,
                        Status = "Active"
                    };

                    _db.JobPostDetails.InsertOnSubmit(jobDetail);
                    _db.SubmitChanges();
                }

                return (true, "Tạo tin tuyển dụng thành công!", newJob.JobPostID);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo tin tuyển dụng: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Check if job is saved by candidate
        /// </summary>
        public bool IsJobSavedByCandidate(int jobId, int candidateId)
        {
            return _db.SavedJobs.Any(sj => sj.CandidateID == candidateId && sj.JobPostID == jobId);
        }

        /// <summary>
        /// Check if candidate has applied to job
        /// </summary>
        public bool HasCandidateApplied(int jobId, int candidateId)
        {
            return _db.Applications.Any(app => app.CandidateID == candidateId && app.JobPostID == jobId);
        }

        /// <summary>
        /// Check if job is open for applications
        /// </summary>
        public bool IsJobOpen(JobPost job)
        {
            if (job == null) return false;
            
            bool isExpired = job.ApplicationDeadline.HasValue && job.ApplicationDeadline.Value < DateTime.Now;
            return JobStatusHelper.IsPublished(job.Status) && !isExpired;
        }
    }
}


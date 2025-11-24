using System;
using System.Collections.Generic;
using System.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Candidates;
using Project_Recruiment_Huce.Repositories.SavedJobRepo;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Mappers;

namespace Project_Recruiment_Huce.Services.SavedJobService
{
    /// <summary>
    /// Implementation của ISavedJobService với business logic
    /// </summary>
    public class SavedJobService : ISavedJobService
    {
        private readonly ISavedJobRepository _repository;

        public SavedJobService(ISavedJobRepository repository)
        {
            _repository = repository;
        }

        public SavedJobListResult GetSavedJobs(
            int accountId,
            string keyword,
            string location,
            string sortBy,
            int pageNumber,
            int pageSize)
        {
            var result = new SavedJobListResult
            {
                Keyword = keyword,
                Location = location,
                SortBy = sortBy,
                CurrentPage = pageNumber
            };

            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                result.Success = false;
                result.ErrorMessage = "Vui lòng hoàn thiện hồ sơ trước khi xem công việc đã lưu.";
                return result;
            }

            // Get saved jobs - only published jobs
            var savedJobs = _repository.GetSavedJobs(candidate.CandidateID)
                .Where(sj => sj.JobPost != null && 
                            JobStatusHelper.IsPublished(JobStatusHelper.NormalizeStatus(sj.JobPost.Status)))
                .ToList();

            // Get distinct locations for filter
            var allLocations = savedJobs
                .Where(sj => sj.JobPost != null && !string.IsNullOrEmpty(sj.JobPost.Location))
                .Select(sj => sj.JobPost.Location)
                .Distinct()
                .OrderBy(l => l)
                .ToList();

            // Apply filters
            var filteredJobs = savedJobs.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string keywordLower = keyword.ToLower();
                filteredJobs = filteredJobs.Where(sj =>
                    (sj.JobPost.Title != null && sj.JobPost.Title.ToLower().Contains(keywordLower)) ||
                    (sj.JobPost.Company != null && sj.JobPost.Company.CompanyName != null && 
                     sj.JobPost.Company.CompanyName.ToLower().Contains(keywordLower)) ||
                    (sj.JobPost.Description != null && sj.JobPost.Description.ToLower().Contains(keywordLower))
                );
            }

            // Location filter
            if (!string.IsNullOrWhiteSpace(location) && location != "Tất cả")
            {
                string locationLower = location.ToLower();
                filteredJobs = filteredJobs.Where(sj =>
                    sj.JobPost.Location != null &&
                    sj.JobPost.Location.ToLower().Contains(locationLower)
                );
            }

            // Apply sorting
            switch (sortBy?.ToLower())
            {
                case "title":
                    filteredJobs = filteredJobs.OrderBy(sj => sj.JobPost.Title);
                    break;
                case "company":
                    filteredJobs = filteredJobs.OrderBy(sj => sj.JobPost.Company != null ? sj.JobPost.Company.CompanyName : "");
                    break;
                case "deadline":
                    filteredJobs = filteredJobs.OrderBy(sj => sj.JobPost.ApplicationDeadline ?? DateTime.MaxValue);
                    break;
                case "saveddate":
                default:
                    filteredJobs = filteredJobs.OrderByDescending(sj => sj.SavedAt);
                    break;
            }

            // Pagination
            var totalItems = filteredJobs.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedJobs = filteredJobs
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Map to ViewModels
            var viewModels = pagedJobs.Select(sj => JobMapper.MapToSavedJob(sj)).ToList();

            result.Success = true;
            result.SavedJobs = viewModels;
            result.Locations = allLocations;
            result.TotalItems = totalItems;
            result.TotalPages = totalPages;

            return result;
        }

        public ValidationResult SaveJob(int accountId, int jobPostId)
        {
            var result = new ValidationResult { IsValid = true };

            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Vui lòng hoàn thiện hồ sơ trước.";
                return result;
            }

            // Check if job exists and is published
            var job = _repository.GetJobPost(jobPostId);
            if (job == null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Không tìm thấy công việc.";
                return result;
            }

            job.Status = JobStatusHelper.NormalizeStatus(job.Status);
            if (!JobStatusHelper.IsPublished(job.Status))
            {
                result.IsValid = false;
                result.Errors["General"] = "Công việc này không còn nhận ứng viên.";
                return result;
            }

            // Check if already saved
            var existingSaved = _repository.GetSavedJob(candidate.CandidateID, jobPostId);
            if (existingSaved != null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Bạn đã lưu công việc này rồi.";
                return result;
            }

            try
            {
                var savedJob = new SavedJob
                {
                    CandidateID = candidate.CandidateID,
                    JobPostID = jobPostId,
                    SavedAt = DateTime.Now
                };

                _repository.CreateSavedJob(savedJob);
                _repository.SaveChanges();
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                // Handle unique constraint violation
                if (ex.Message.Contains("UNIQUE") || ex.Message.Contains("duplicate"))
                {
                    result.Errors["General"] = "Bạn đã lưu công việc này rồi.";
                }
                else
                {
                    result.Errors["General"] = "Lỗi khi lưu công việc: " + ex.Message;
                }
            }

            return result;
        }

        public ValidationResult UnsaveJob(int accountId, int jobPostId)
        {
            var result = new ValidationResult { IsValid = true };

            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Vui lòng hoàn thiện hồ sơ trước.";
                return result;
            }

            var savedJob = _repository.GetSavedJob(candidate.CandidateID, jobPostId);
            if (savedJob == null)
            {
                result.IsValid = false;
                result.Errors["General"] = "Bạn chưa lưu công việc này.";
                return result;
            }

            try
            {
                _repository.DeleteSavedJob(savedJob);
                _repository.SaveChanges();
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors["General"] = "Lỗi khi bỏ lưu công việc: " + ex.Message;
            }

            return result;
        }

        public bool IsJobSaved(int accountId, int jobPostId)
        {
            var candidate = _repository.GetCandidateByAccountId(accountId);
            if (candidate == null)
            {
                return false;
            }

            return _repository.IsJobSaved(candidate.CandidateID, jobPostId);
        }
    }
}

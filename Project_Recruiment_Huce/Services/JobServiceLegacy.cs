using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Jobs;
using Project_Recruiment_Huce.Repositories;
using Project_Recruiment_Huce.Mappers;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// LEGACY: Old JobService - kept for backward compatibility with existing code
    /// New code should use Project_Recruiment_Huce.Services.JobService.JobService
    /// </summary>
    public class LegacyJobService
    {
        private readonly JobRepository _jobRepository;
        private readonly JOBPORTAL_ENDataContext _db;

        public LegacyJobService(JobRepository jobRepository, JOBPORTAL_ENDataContext db)
        {
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public List<RelatedJobViewModel> GetRelatedJobs(int jobId, int companyId, string location, int take = 5)
        {
            var relatedJobs = _jobRepository.GetRelatedJobs(jobId, companyId, location, take);
            return relatedJobs.Select(j => JobMapper.MapToRelatedJob(j)).ToList();
        }

        public (List<JobListingItemViewModel> Jobs, int TotalItems, int TotalPages) SearchJobs(
            string keyword, 
            string location, 
            string employmentType, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            var jobs = _jobRepository.SearchJobs(keyword, location, employmentType);

            int totalItems = jobs.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedJobs = jobs
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var jobViewModels = pagedJobs.Select(j => JobMapper.MapToListingItem(j, _db)).ToList();

            return (jobViewModels, totalItems, totalPages);
        }

        public (List<JobListingItemViewModel> Jobs, int TotalItems, int TotalPages) GetJobsByRecruiter(
            int recruiterId, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            var jobs = _jobRepository.GetJobsByRecruiter(recruiterId);

            int totalItems = jobs.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedJobs = jobs
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var jobViewModels = pagedJobs.Select(j => JobMapper.MapToListingItem(j, _db)).ToList();

            return (jobViewModels, totalItems, totalPages);
        }

        public List<JobListingItemViewModel> GetRecentPublishedJobs(int take = 7)
        {
            if (_db.LoadOptions == null)
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<JobPost>(j => j.Company);
                loadOptions.LoadWith<JobPost>(j => j.Recruiter);
                loadOptions.LoadWith<Recruiter>(r => r.Company);
                _db.LoadOptions = loadOptions;
            }

            JobStatusHelper.NormalizeStatuses(_db);
            
            var recentJobs = _db.JobPosts
                .Where(j => j.Status == JobStatusHelper.Published)
                .OrderByDescending(j => j.PostedAt > j.UpdatedAt ? j.PostedAt : j.UpdatedAt)
                .Take(take)
                .ToList();

            return recentJobs.Select(j => JobMapper.MapToListingItem(j, _db)).ToList();
        }

        public bool IsJobSavedByCandidate(int jobId, int candidateId)
        {
            return _db.SavedJobs.Any(sj => sj.CandidateID == candidateId && sj.JobPostID == jobId);
        }

        public bool HasCandidateApplied(int jobId, int candidateId)
        {
            return _db.Applications.Any(app => app.CandidateID == candidateId && app.JobPostID == jobId);
        }

        public bool IsJobOpen(JobPost job)
        {
            if (job == null) return false;
            
            bool isExpired = job.ApplicationDeadline.HasValue && job.ApplicationDeadline.Value < DateTime.Now;
            return JobStatusHelper.IsPublished(job.Status) && !isExpired;
        }
    }
}

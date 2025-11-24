using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Jobs;
using Project_Recruiment_Huce.Mappers;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// LEGACY: Old JobRepository - kept for backward compatibility with existing code
    /// New code should use Project_Recruiment_Huce.Repositories.JobRepo.JobRepository
    /// </summary>
    public class JobRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public JobRepository(JOBPORTAL_ENDataContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public JobPost GetByIdWithDetails(int id)
        {
            if (_db.LoadOptions == null)
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<JobPost>(j => j.Company);
                loadOptions.LoadWith<JobPost>(j => j.Recruiter);
                loadOptions.LoadWith<JobPost>(j => j.JobPostDetails);
                _db.LoadOptions = loadOptions;
            }

            return _db.JobPosts.FirstOrDefault(j => j.JobPostID == id);
        }

        public IEnumerable<JobPost> GetPublishedJobs()
        {
            JobStatusHelper.NormalizeStatuses(_db);
            return _db.JobPosts
                .Where(j => j.Status == JobStatusHelper.Published)
                .OrderByDescending(j => j.PostedAt > j.UpdatedAt ? j.PostedAt : j.UpdatedAt)
                .ToList();
        }

        public IEnumerable<JobPost> GetJobsByRecruiter(int recruiterId)
        {
            if (_db.LoadOptions == null)
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<JobPost>(j => j.Company);
                loadOptions.LoadWith<JobPost>(j => j.Recruiter);
                loadOptions.LoadWith<Recruiter>(r => r.Company);
                _db.LoadOptions = loadOptions;
            }

            return _db.JobPosts
                .Where(j => j.RecruiterID == recruiterId)
                .OrderByDescending(j => j.PostedAt > j.UpdatedAt ? j.PostedAt : j.UpdatedAt)
                .ToList();
        }

        public IEnumerable<JobPost> GetRelatedJobs(int jobId, int companyId, string location, int take = 5)
        {
            JobStatusHelper.NormalizeStatuses(_db);
            return _db.JobPosts
                .Where(j => j.JobPostID != jobId &&
                           j.Status == JobStatusHelper.Published &&
                           (j.CompanyID == companyId ||
                            (j.Location != null && location != null && j.Location == location)))
                .OrderByDescending(j => j.PostedAt > j.UpdatedAt ? j.PostedAt : j.UpdatedAt)
                .Take(take)
                .ToList();
        }

        public IEnumerable<JobPost> SearchJobs(string keyword, string location, string employmentType)
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
            var query = _db.JobPosts.Where(j => j.Status == JobStatusHelper.Published);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(j => j.Title.Contains(keyword) ||
                                       (j.Description != null && j.Description.Contains(keyword)) ||
                                       (j.Company != null && j.Company.CompanyName.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(location) && location != "Bất kỳ đâu")
            {
                query = query.Where(j => j.Location != null && j.Location.Contains(location));
            }

            if (!string.IsNullOrWhiteSpace(employmentType))
            {
                query = query.Where(j => j.EmploymentType == employmentType);
            }

            return query.OrderByDescending(j => j.PostedAt > j.UpdatedAt ? j.PostedAt : j.UpdatedAt).ToList();
        }
    }
}

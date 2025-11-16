using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Repository implementation for JobPost entity
    /// Handles all data access for job posts
    /// </summary>
    public class JobRepository : IJobRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public JobRepository(JOBPORTAL_ENDataContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        #region Basic CRUD Operations

        public JobPost GetById(int id)
        {
            return _db.JobPosts.FirstOrDefault(j => j.JobPostID == id);
        }

        public IEnumerable<JobPost> GetAll()
        {
            return _db.JobPosts.ToList();
        }

        public void Add(JobPost job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            _db.JobPosts.InsertOnSubmit(job);
        }

        public void Update(JobPost job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            
            var existing = _db.JobPosts.FirstOrDefault(j => j.JobPostID == job.JobPostID);
            if (existing == null)
            {
                throw new InvalidOperationException($"JobPost with ID {job.JobPostID} not found");
            }
            
            // Update properties
            existing.Title = job.Title;
            existing.Description = job.Description;
            existing.Requirements = job.Requirements;
            existing.Location = job.Location;
            existing.EmploymentType = job.EmploymentType;
            existing.SalaryFrom = job.SalaryFrom;
            existing.SalaryTo = job.SalaryTo;
            existing.SalaryCurrency = job.SalaryCurrency;
            existing.ApplicationDeadline = job.ApplicationDeadline;
            existing.Status = job.Status;
            existing.UpdatedAt = DateTime.Now;
        }

        public void Delete(int id)
        {
            var job = _db.JobPosts.FirstOrDefault(j => j.JobPostID == id);
            if (job != null)
            {
                _db.JobPosts.DeleteOnSubmit(job);
            }
        }

        public void SaveChanges()
        {
            _db.SubmitChanges();
        }

        #endregion

        #region Query Operations

        public IEnumerable<JobPost> Find(Expression<Func<JobPost, bool>> predicate)
        {
            return _db.JobPosts.Where(predicate).ToList();
        }

        public JobPost FirstOrDefault(Expression<Func<JobPost, bool>> predicate)
        {
            return _db.JobPosts.FirstOrDefault(predicate);
        }

        public bool Any(Expression<Func<JobPost, bool>> predicate)
        {
            return _db.JobPosts.Any(predicate);
        }

        public int Count(Expression<Func<JobPost, bool>> predicate)
        {
            return _db.JobPosts.Count(predicate);
        }

        #endregion

        #region Specific Business Queries

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
            // Set LoadOptions to eager load Company and Recruiter for logo
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

        public IEnumerable<JobPost> GetJobsByCompany(int companyId)
        {
            return _db.JobPosts
                .Where(j => j.CompanyID == companyId)
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
            // Set LoadOptions to eager load Company and Recruiter for logo
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

            // Search by keyword
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(j => j.Title.Contains(keyword) ||
                                       (j.Description != null && j.Description.Contains(keyword)) ||
                                       (j.Company != null && j.Company.CompanyName.Contains(keyword)));
            }

            // Filter by location
            if (!string.IsNullOrWhiteSpace(location) && location != "Bất kỳ đâu")
            {
                query = query.Where(j => j.Location != null && j.Location.Contains(location));
            }

            // Filter by employment type
            if (!string.IsNullOrWhiteSpace(employmentType))
            {
                query = query.Where(j => j.EmploymentType == employmentType);
            }

            return query.OrderByDescending(j => j.PostedAt > j.UpdatedAt ? j.PostedAt : j.UpdatedAt).ToList();
        }

        #endregion

        #region With Eager Loading

        public JobPost GetByIdWithDetails(int id)
        {
            // Set LoadOptions BEFORE any query is executed
            // Check if LoadOptions is already set to avoid error
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

        public IEnumerable<JobPost> GetAllWithDetails()
        {
            // Set LoadOptions BEFORE any query is executed
            // Check if LoadOptions is already set to avoid error
            if (_db.LoadOptions == null)
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<JobPost>(j => j.Company);
                loadOptions.LoadWith<JobPost>(j => j.Recruiter);
                _db.LoadOptions = loadOptions;
            }

            return _db.JobPosts.ToList();
        }

        #endregion
    }
}


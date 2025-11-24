using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Repository implementation for Application entity
    /// Handles all data access for job applications
    /// </summary>
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public ApplicationRepository(JOBPORTAL_ENDataContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        #region Basic CRUD Operations

        public Application GetById(int id)
        {
            return _db.Applications.FirstOrDefault(a => a.ApplicationID == id);
        }

        public IEnumerable<Application> GetAll()
        {
            return _db.Applications.ToList();
        }

        public void Add(Application application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
            _db.Applications.InsertOnSubmit(application);
        }

        public void Update(Application application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
            
            var existing = _db.Applications.FirstOrDefault(a => a.ApplicationID == application.ApplicationID);
            if (existing == null)
            {
                throw new InvalidOperationException($"Application with ID {application.ApplicationID} not found");
            }
            
            // Update properties
            existing.Status = application.Status;
            existing.Note = application.Note;
            existing.UpdatedAt = DateTime.Now;
        }

        public void Delete(int id)
        {
            var application = _db.Applications.FirstOrDefault(a => a.ApplicationID == id);
            if (application != null)
            {
                _db.Applications.DeleteOnSubmit(application);
            }
        }

        public void SaveChanges()
        {
            _db.SubmitChanges();
        }

        #endregion

        #region Query Operations

        public IEnumerable<Application> Find(Expression<Func<Application, bool>> predicate)
        {
            return _db.Applications.Where(predicate).ToList();
        }

        public Application FirstOrDefault(Expression<Func<Application, bool>> predicate)
        {
            return _db.Applications.FirstOrDefault(predicate);
        }

        public bool Any(Expression<Func<Application, bool>> predicate)
        {
            return _db.Applications.Any(predicate);
        }

        public int Count(Expression<Func<Application, bool>> predicate)
        {
            return _db.Applications.Count(predicate);
        }

        #endregion

        #region Specific Business Queries

        public IEnumerable<Application> GetApplicationsByCandidate(int candidateId)
        {
            return _db.Applications
                .Where(a => a.CandidateID == candidateId)
                .OrderByDescending(a => a.AppliedAt)
                .ToList();
        }

        public IEnumerable<Application> GetApplicationsByJob(int jobPostId)
        {
            return _db.Applications
                .Where(a => a.JobPostID == jobPostId)
                .OrderByDescending(a => a.AppliedAt)
                .ToList();
        }

        public IEnumerable<Application> GetApplicationsByRecruiter(int recruiterId)
        {
            var applications = from app in _db.Applications
                              join job in _db.JobPosts on app.JobPostID equals job.JobPostID
                              where job.RecruiterID == recruiterId
                              orderby app.AppliedAt descending
                              select app;
            
            return applications.ToList();
        }

        public IEnumerable<Application> GetApplicationsByStatus(string status)
        {
            return _db.Applications
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.AppliedAt)
                .ToList();
        }

        public int GetPendingApplicationsCount(int jobPostId)
        {
            var pendingStatuses = new[] { "Under review", "Interview", "Offered" };
            return _db.Applications
                .Count(a => a.JobPostID == jobPostId &&
                           a.Status != null &&
                           pendingStatuses.Contains(a.Status));
        }

        #endregion

        #region With Eager Loading

        public Application GetByIdWithDetails(int id)
        {
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<Application>(a => a.JobPost);
            loadOptions.LoadWith<Application>(a => a.Candidate);
            loadOptions.LoadWith<JobPost>(j => j.Company);
            _db.LoadOptions = loadOptions;

            return _db.Applications.FirstOrDefault(a => a.ApplicationID == id);
        }

        public IEnumerable<Application> GetAllWithDetails()
        {
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<Application>(a => a.JobPost);
            loadOptions.LoadWith<Application>(a => a.Candidate);
            loadOptions.LoadWith<JobPost>(j => j.Company);
            _db.LoadOptions = loadOptions;

            return _db.Applications.ToList();
        }

        #endregion
    }
}


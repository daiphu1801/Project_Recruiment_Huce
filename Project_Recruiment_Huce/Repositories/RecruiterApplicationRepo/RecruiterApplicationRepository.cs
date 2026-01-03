using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.RecruiterApplicationRepo
{
    /// <summary>
    /// Repository implementation for Recruiter Application operations
    /// </summary>
    public class RecruiterApplicationRepository : IRecruiterApplicationRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public RecruiterApplicationRepository(JOBPORTAL_ENDataContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));

            // Set up eager loading for related entities
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<Application>(a => a.JobPost);
            loadOptions.LoadWith<Application>(a => a.Candidate);
            loadOptions.LoadWith<JobPost>(j => j.Company);
            loadOptions.LoadWith<JobPost>(j => j.Recruiter);
            loadOptions.LoadWith<Recruiter>(r => r.Company);
            _db.LoadOptions = loadOptions;
        }

        public int? GetRecruiterIdByAccountId(int accountId)
        {
            var recruiter = _db.Recruiters.FirstOrDefault(r => r.AccountID == accountId);
            return recruiter?.RecruiterID;
        }

        public Application GetApplicationByIdWithDetails(int applicationId)
        {
            return _db.Applications.FirstOrDefault(a => a.ApplicationID == applicationId);
        }

        public IEnumerable<Application> GetApplicationsByRecruiter(int recruiterId)
        {
            var query = from app in _db.Applications
                        join job in _db.JobPosts on app.JobPostID equals job.JobPostID
                        where job.RecruiterID == recruiterId
                        orderby app.AppliedAt descending
                        select app;
            return query.ToList();
        }

        public List<JobPost> GetJobsByRecruiter(int recruiterId)
        {
            return _db.JobPosts
                .Where(j => j.RecruiterID == recruiterId)
                .OrderByDescending(j => j.PostedAt)
                .ToList();
        }

        public bool IsApplicationOwnedByRecruiter(int applicationId, int recruiterId)
        {
            return _db.Applications
                .Any(a => a.ApplicationID == applicationId &&
                         a.JobPost.RecruiterID == recruiterId);
        }

        public void UpdateApplicationStatus(int applicationId, string status, string note)
        {
            var application = _db.Applications.FirstOrDefault(a => a.ApplicationID == applicationId);

            if (application == null)
            {
                throw new InvalidOperationException($"Application with ID {applicationId} not found");
            }

            // Chỉ cập nhật trạng thái, không chạm vào Note (Note chỉ dành cho Candidate)
            application.Status = status;
            application.UpdatedAt = DateTime.Now;
        }


        public void SaveChanges()
        {
            _db.SubmitChanges();
        }

        public Application GetApplicationById(int id)
        {
            return _db.Applications.FirstOrDefault(a => a.ApplicationID == id);
        }
    }
}

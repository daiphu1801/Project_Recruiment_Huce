using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.SavedJobRepo
{
    /// <summary>
    /// Implementation của ISavedJobRepository cho SavedJob data access
    /// </summary>
    public class SavedJobRepository : ISavedJobRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;
        private readonly bool _isReadOnly;

        public SavedJobRepository(bool readOnly = false)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            _db = new JOBPORTAL_ENDataContext(connectionString);
            _isReadOnly = readOnly;

            if (_isReadOnly)
            {
                _db.ObjectTrackingEnabled = false;
            }

            // Setup eager loading
            var loadOptions = new System.Data.Linq.DataLoadOptions();
            loadOptions.LoadWith<SavedJob>(sj => sj.JobPost);
            loadOptions.LoadWith<JobPost>(j => j.Company);
            _db.LoadOptions = loadOptions;
        }

        public Candidate GetCandidateByAccountId(int accountId)
        {
            return _db.Candidates.FirstOrDefault(c => c.AccountID == accountId);
        }

        public List<SavedJob> GetSavedJobs(int candidateId)
        {
            return _db.SavedJobs
                .Where(sj => sj.CandidateID == candidateId)
                .ToList();
        }

        public JobPost GetJobPost(int jobPostId)
        {
            return _db.JobPosts.FirstOrDefault(j => j.JobPostID == jobPostId);
        }

        public SavedJob GetSavedJob(int candidateId, int jobPostId)
        {
            return _db.SavedJobs.FirstOrDefault(sj => 
                sj.CandidateID == candidateId && 
                sj.JobPostID == jobPostId);
        }

        public void CreateSavedJob(SavedJob savedJob)
        {
            _db.SavedJobs.InsertOnSubmit(savedJob);
        }

        public void DeleteSavedJob(SavedJob savedJob)
        {
            _db.SavedJobs.DeleteOnSubmit(savedJob);
        }

        public bool IsJobSaved(int candidateId, int jobPostId)
        {
            return _db.SavedJobs.Any(sj => 
                sj.CandidateID == candidateId && 
                sj.JobPostID == jobPostId);
        }

        public void SaveChanges()
        {
            _db.SubmitChanges();
        }
    }
}

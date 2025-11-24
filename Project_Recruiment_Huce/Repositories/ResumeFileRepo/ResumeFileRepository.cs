using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.ResumeFileRepo
{
    /// <summary>
    /// Implementation của IResumeFileRepository cho ResumeFile data access
    /// </summary>
    public class ResumeFileRepository : IResumeFileRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;
        private readonly bool _isReadOnly;

        public ResumeFileRepository(bool readOnly = false)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            _db = new JOBPORTAL_ENDataContext(connectionString);
            _isReadOnly = readOnly;

            if (_isReadOnly)
            {
                _db.ObjectTrackingEnabled = false;
            }
        }

        public Candidate GetCandidateByAccountId(int accountId)
        {
            return _db.Candidates.FirstOrDefault(c => c.AccountID == accountId);
        }

        public List<ResumeFile> GetResumeFilesByCandidateId(int candidateId)
        {
            return _db.ResumeFiles
                .Where(rf => rf.CandidateID == candidateId)
                .OrderByDescending(rf => rf.UploadedAt)
                .ToList();
        }

        public ResumeFile GetResumeFile(int resumeFileId, int candidateId)
        {
            return _db.ResumeFiles.FirstOrDefault(rf => rf.ResumeFileID == resumeFileId && rf.CandidateID == candidateId);
        }

        public void CreateResumeFile(ResumeFile resumeFile)
        {
            _db.ResumeFiles.InsertOnSubmit(resumeFile);
        }

        public void DeleteResumeFile(ResumeFile resumeFile)
        {
            _db.ResumeFiles.DeleteOnSubmit(resumeFile);
        }

        public void UpdateResumeFileName(ResumeFile resumeFile, string newFileName)
        {
            var existingFile = _db.ResumeFiles.FirstOrDefault(rf => rf.ResumeFileID == resumeFile.ResumeFileID);
            if (existingFile != null)
            {
                existingFile.FileName = newFileName;
            }
        }

        public void SaveChanges()
        {
            _db.SubmitChanges();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.CandidateRepo
{
    /// <summary>
    /// Implementation của ICandidateRepository cho candidate data access
    /// </summary>
    public class CandidateRepository : ICandidateRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public CandidateRepository()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            _db = new JOBPORTAL_ENDataContext(connectionString);

            // Setup eager loading
            var loadOptions = new System.Data.Linq.DataLoadOptions();
            loadOptions.LoadWith<Application>(a => a.JobPost);
            loadOptions.LoadWith<JobPost>(j => j.Company);
            loadOptions.LoadWith<JobPost>(j => j.Recruiter);
            loadOptions.LoadWith<Recruiter>(r => r.Company);
            _db.LoadOptions = loadOptions;
        }

        public Candidate GetCandidateByAccountId(int accountId)
        {
            return _db.Candidates.FirstOrDefault(c => c.AccountID == accountId);
        }

        public Candidate GetCandidateById(int candidateId)
        {
            return _db.Candidates.FirstOrDefault(c => c.CandidateID == candidateId);
        }

        public void CreateCandidate(Candidate candidate)
        {
            _db.Candidates.InsertOnSubmit(candidate);
        }

        public void UpdateCandidate(Candidate candidate)
        {
            // LINQ to SQL tracks changes automatically
            // No explicit update needed if entity is already attached
            var existingCandidate = _db.Candidates.FirstOrDefault(c => c.CandidateID == candidate.CandidateID);
            if (existingCandidate != null)
            {
                existingCandidate.FullName = candidate.FullName;
                existingCandidate.BirthDate = candidate.BirthDate;
                existingCandidate.Gender = candidate.Gender;
                existingCandidate.Phone = candidate.Phone;
                existingCandidate.Email = candidate.Email;
                existingCandidate.Address = candidate.Address;
                existingCandidate.Summary = candidate.Summary;
                existingCandidate.PhotoID = candidate.PhotoID;
            }
        }

        public ProfilePhoto GetProfilePhoto(int photoId)
        {
            return _db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId);
        }

        public int SaveProfilePhoto(ProfilePhoto photo)
        {
            _db.ProfilePhotos.InsertOnSubmit(photo);
            _db.SubmitChanges();
            return photo.PhotoID;
        }

        public void UpdatePhotoId(int candidateId, int accountId, int photoId)
        {
            var candidate = _db.Candidates.FirstOrDefault(c => c.CandidateID == candidateId);
            if (candidate != null)
            {
                candidate.PhotoID = photoId;
            }

            var account = _db.Accounts.FirstOrDefault(a => a.AccountID == accountId);
            if (account != null)
            {
                account.PhotoID = photoId;
            }
        }

        public List<ResumeFile> GetResumeFiles(int candidateId)
        {
            return _db.ResumeFiles
                .Where(rf => rf.CandidateID == candidateId)
                .OrderByDescending(rf => rf.UploadedAt)
                .ToList();
        }

        public int SaveResumeFile(ResumeFile resumeFile)
        {
            _db.ResumeFiles.InsertOnSubmit(resumeFile);
            _db.SubmitChanges();
            return resumeFile.ResumeFileID;
        }

        public ResumeFile GetResumeFile(int resumeFileId, int candidateId)
        {
            return _db.ResumeFiles.FirstOrDefault(rf => rf.ResumeFileID == resumeFileId && rf.CandidateID == candidateId);
        }

        public List<Application> GetApplications(int candidateId)
        {
            return _db.Applications
                .Where(a => a.CandidateID == candidateId)
                .OrderByDescending(a => a.AppliedAt)
                .ToList();
        }

        public JobPost GetJobPost(int jobPostId)
        {
            return _db.JobPosts.FirstOrDefault(j => j.JobPostID == jobPostId);
        }

        public Application GetExistingApplication(int candidateId, int jobPostId)
        {
            return _db.Applications.FirstOrDefault(a => a.CandidateID == candidateId && a.JobPostID == jobPostId);
        }

        public void CreateApplication(Application application)
        {
            _db.Applications.InsertOnSubmit(application);
        }

        public Application GetApplicationById(int applicationId, int candidateId)
        {
            return _db.Applications.FirstOrDefault(a => a.ApplicationID == applicationId && a.CandidateID == candidateId);
        }

        public bool IsPhoneUnique(string phone, int accountId)
        {
            // Check trong Accounts (trừ account hiện tại)
            var accountExists = _db.Accounts.Any(a => a.Phone == phone && a.AccountID != accountId);
            if (accountExists)
            {
                return false;
            }

            // Check trong Candidates (trừ candidate của account hiện tại)
            var candidateExists = _db.Candidates.Any(c => c.Phone == phone && c.AccountID != accountId);
            if (candidateExists)
            {
                return false;
            }

            // Check trong Recruiters (trừ recruiter của account hiện tại)
            var recruiterExists = _db.Recruiters.Any(r => r.Phone == phone && r.AccountID != accountId);
            if (recruiterExists)
            {
                return false;
            }

            return true;
        }

        public void SaveChanges()
        {
            _db.SubmitChanges();
        }
    }
}

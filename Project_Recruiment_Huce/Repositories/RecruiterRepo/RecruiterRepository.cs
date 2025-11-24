using System;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.RecruiterRepo
{
    /// <summary>
    /// Repository implementation for Recruiter profile operations
    /// </summary>
    public class RecruiterRepository : IRecruiterRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public RecruiterRepository(JOBPORTAL_ENDataContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Recruiter GetByAccountId(int accountId)
        {
            return _db.Recruiters.FirstOrDefault(r => r.AccountID == accountId);
        }

        public void Create(Recruiter recruiter)
        {
            if (recruiter == null)
                throw new ArgumentNullException(nameof(recruiter));

            _db.Recruiters.InsertOnSubmit(recruiter);
        }

        public void Update(Recruiter recruiter)
        {
            // LINQ to SQL tracks changes automatically
            // No explicit update needed
        }

        public bool IsPhoneUnique(string phone, int currentAccountId)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            // Check in Accounts table (excluding current account)
            var existsInAccount = _db.Accounts
                .Any(a => a.AccountID != currentAccountId && a.Phone == phone);

            if (existsInAccount)
                return false;

            // Check in Candidates table (excluding current account)
            var existsInCandidate = _db.Candidates
                .Any(c => c.AccountID != currentAccountId && c.Phone == phone);

            if (existsInCandidate)
                return false;

            // Check in Recruiters table (excluding current account)
            var existsInRecruiter = _db.Recruiters
                .Any(r => r.AccountID != currentAccountId && r.Phone == phone);

            return !existsInRecruiter;
        }

        public bool IsCompanyEmailUnique(string email, int currentAccountId)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            // Check if email exists in Accounts table (excluding current account)
            return !_db.Accounts.Any(a => a.AccountID != currentAccountId && a.Email == email);
        }

        public Account GetAccountById(int accountId)
        {
            return _db.Accounts.FirstOrDefault(a => a.AccountID == accountId);
        }

        public ProfilePhoto GetProfilePhotoById(int photoId)
        {
            return _db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId);
        }

        public void DeleteProfilePhoto(ProfilePhoto photo)
        {
            if (photo != null)
            {
                _db.ProfilePhotos.DeleteOnSubmit(photo);
            }
        }

        public void CreateProfilePhoto(ProfilePhoto photo)
        {
            if (photo == null)
                throw new ArgumentNullException(nameof(photo));

            _db.ProfilePhotos.InsertOnSubmit(photo);
        }

        public void SaveChanges()
        {
            _db.SubmitChanges();
        }
    }
}

using System;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Triển khai ICompanyRepository sử dụng JOBPORTAL_ENDataContext
    /// </summary>
    public class CompanyRepository : ICompanyRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public CompanyRepository(JOBPORTAL_ENDataContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Recruiter GetRecruiterByAccountId(int accountId)
        {
            return _db.Recruiters.FirstOrDefault(r => r.AccountID == accountId);
        }

        public Company GetCompanyById(int companyId)
        {
            return _db.Companies.FirstOrDefault(c => c.CompanyID == companyId);
        }

        public ProfilePhoto GetProfilePhotoById(int photoId)
        {
            return _db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId);
        }

        public void InsertCompany(Company company)
        {
            if (company == null) throw new ArgumentNullException(nameof(company));
            _db.Companies.InsertOnSubmit(company);
        }

        public void DeleteProfilePhoto(ProfilePhoto photo)
        {
            if (photo == null) throw new ArgumentNullException(nameof(photo));
            _db.ProfilePhotos.DeleteOnSubmit(photo);
        }

        public void SaveChanges()
        {
            _db.SubmitChanges();
        }

    }
}

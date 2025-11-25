using System;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Giao diện cho các thao tác dữ liệu liên quan đến Company/Recruiter
    /// Cung cấp các phương thức truy vấn và cập nhật tối thiểu được sử dụng bởi controller/service.
    /// </summary>
    public interface ICompanyRepository
    {
        Recruiter GetRecruiterByAccountId(int accountId);
        Company GetCompanyById(int companyId);
        ProfilePhoto GetProfilePhotoById(int photoId);
        void InsertCompany(Company company);
        void DeleteProfilePhoto(ProfilePhoto photo);
        void SaveChanges();
    }
}

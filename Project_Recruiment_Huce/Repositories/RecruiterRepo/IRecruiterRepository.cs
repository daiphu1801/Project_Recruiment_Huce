using System;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.RecruiterRepo
{
    /// <summary>
    /// Repository interface for Recruiter profile operations
    /// </summary>
    public interface IRecruiterRepository
    {
        /// <summary>
        /// Get recruiter by account ID
        /// </summary>
        Recruiter GetByAccountId(int accountId);

        /// <summary>
        /// Create new recruiter profile
        /// </summary>
        void Create(Recruiter recruiter);

        /// <summary>
        /// Update existing recruiter profile
        /// </summary>
        void Update(Recruiter recruiter);

        /// <summary>
        /// Check if phone is unique (excluding current account)
        /// </summary>
        bool IsPhoneUnique(string phone, int currentAccountId);

        /// <summary>
        /// Check if company email is unique in Accounts table (excluding current account)
        /// </summary>
        bool IsCompanyEmailUnique(string email, int currentAccountId);

        /// <summary>
        /// Get account by ID
        /// </summary>
        Account GetAccountById(int accountId);

        /// <summary>
        /// Get profile photo by ID
        /// </summary>
        ProfilePhoto GetProfilePhotoById(int photoId);

        /// <summary>
        /// Delete profile photo
        /// </summary>
        void DeleteProfilePhoto(ProfilePhoto photo);

        /// <summary>
        /// Create new profile photo
        /// </summary>
        void CreateProfilePhoto(ProfilePhoto photo);

        /// <summary>
        /// Save changes to database
        /// </summary>
        void SaveChanges();
    }
}

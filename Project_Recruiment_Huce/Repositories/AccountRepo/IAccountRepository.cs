using System;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Giao diện cho các thao tác truy xuất dữ liệu liên quan tới Account
    /// (được sử dụng bởi các service/validation).
    /// </summary>
    public interface IAccountRepository
    {
        bool UsernameExists(string username);
        bool EmailExists(string email);
        bool PhoneExistsNormalized(string normalizedPhone);
        PasswordResetToken GetPasswordResetToken(string emailLower, string resetCodeUpper);
        Account GetById(int accountId);
        Account FindByUsernameOrEmail(string emailOrUsername);
        Account Create(string username, string email, string phone, string role, string passwordHash, string salt);
        void UpdatePassword(int accountId, string passwordHash, string salt);
        void UpdateAccount(Account account);
        void UpdatePasswordResetToken(PasswordResetToken token);
        void SaveChanges();

        // ============================================================
        // GOOGLE LOGIN REPOSITORY METHODS
        // ============================================================
        
        /// <summary>
        /// Tìm account theo GoogleId
        /// </summary>
        Account FindByGoogleId(string googleId);
        
        /// <summary>
        /// Tìm account theo Email chính xác
        /// </summary>
        Account FindByEmail(string email);
        
        /// <summary>
        /// Tạo tài khoản Google mới (chỉ tạo Account, không tạo Candidate)
        /// </summary>
        Account CreateGoogleAccount(string email, string fullName, string googleId, string username);
        
        /// <summary>
        /// Tạo Candidate profile liên kết với Account
        /// </summary>
        void CreateCandidateProfile(int accountId, string fullName, string email);
        
        /// <summary>
        /// Lưu ảnh đại diện (Google avatar URL)
        /// </summary>
        ProfilePhoto SaveProfilePhoto(string photoUrl, string fileName);
        
        /// <summary>
        /// Cập nhật PhotoID cho Account
        /// </summary>
        void UpdateAccountPhotoId(int accountId, int photoId);

        /// <summary>
        /// Generate username duy nhất từ email
        /// </summary>
        string GenerateUniqueUsername(string email);
    }
}


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
        void CreateGoogleProfile(string email, string firstName, string lastName, int userId, string FullName, int userType, string Avatar, DateTime Birthdate);
        void UpdatePassword(int accountId, string passwordHash, string salt);
        void UpdateAccount(Account account);
        void UpdatePasswordResetToken(PasswordResetToken token);
        void SaveChanges();

    }
}


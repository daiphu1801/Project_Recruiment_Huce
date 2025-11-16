using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Interface for Account data access operations
    /// </summary>
    public interface IAccountRepository
    {
        Account GetById(int accountId);
        Account FindByUsernameOrEmail(string emailOrUsername);
        bool ExistsUsername(string username);
        bool ExistsEmail(string email);
        Account Create(string username, string email, string phone, string role, string passwordHash);
        void UpdatePassword(int accountId, string passwordHash, string salt);
        void SaveChanges();
    }
}


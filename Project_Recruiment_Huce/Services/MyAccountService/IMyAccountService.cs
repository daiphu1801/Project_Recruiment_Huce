using Project_Recruiment_Huce.Models.Accounts;
using Project_Recruiment_Huce.Services;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// Giao diện cho service xử lý business logic liên quan quản lý tài khoản cá nhân
    /// (xem thông tin tài khoản, đổi mật khẩu, validation)
    /// </summary>
    public interface IMyAccountService
    {
        MyAccountViewModel GetAccountInfo(int accountId);
        ValidationResult ValidateChangePassword(ChangePasswordViewModel model, int accountId);
        void ChangePassword(int accountId, string newPassword);
    }
}

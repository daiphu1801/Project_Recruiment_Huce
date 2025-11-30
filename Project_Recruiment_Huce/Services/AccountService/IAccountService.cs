using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
using System;

namespace Project_Recruiment_Huce.Services
{
    public interface IAccountService
    {
        ValidationResult ValidateRegister(RegisterViewModel model);
        ValidationResult Register(RegisterViewModel model);
        Account Authenticate(string userOrEmail, string password);
        ValidationResult ValidateResetPassword(ResetPasswordViewModel model);
        ValidationResult ResetPassword(ResetPasswordViewModel model);
        void CreateGoogleProfile(string email, string fullName, string avatarUrl, int userType, int userId);
    }
}

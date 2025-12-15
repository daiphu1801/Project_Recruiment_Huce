using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
<<<<<<< HEAD
=======
using System;
>>>>>>> b5687619104f46f9178da37581c63d949fa94225

namespace Project_Recruiment_Huce.Services
{
    public interface IAccountService
    {
        ValidationResult ValidateRegister(RegisterViewModel model);
        ValidationResult Register(RegisterViewModel model);
        Account Authenticate(string userOrEmail, string password);
        ValidationResult ValidateResetPassword(ResetPasswordViewModel model);
        ValidationResult ResetPassword(ResetPasswordViewModel model);
<<<<<<< HEAD
=======
        void CreateGoogleProfile(string email, string fullName, string avatarUrl, int userType, int userId);
>>>>>>> b5687619104f46f9178da37581c63d949fa94225
    }
}

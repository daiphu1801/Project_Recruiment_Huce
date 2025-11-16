using System;
using System.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
using Project_Recruiment_Huce.Repositories;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Services;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// Service layer for MyAccount operations
    /// Encapsulates business logic for account management
    /// </summary>
    public class MyAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly JOBPORTAL_ENDataContext _db;

        public MyAccountService(IAccountRepository accountRepository, JOBPORTAL_ENDataContext db)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Get account information with contact email from profile
        /// </summary>
        public MyAccountViewModel GetAccountInfo(int accountId)
        {
            var account = _accountRepository.GetById(accountId);
            if (account == null) return null;

            // Get contact email from profile
            string contactEmail = null;
            if (account.Role == "Candidate")
            {
                var candidate = _db.Candidates.FirstOrDefault(c => c.AccountID == accountId);
                contactEmail = candidate?.Email;
            }
            else if (account.Role == "Recruiter")
            {
                var recruiter = _db.Recruiters.FirstOrDefault(r => r.AccountID == accountId);
                contactEmail = recruiter?.CompanyEmail;
            }

            return new MyAccountViewModel
            {
                AccountId = account.AccountID,
                Username = account.Username,
                LoginEmail = account.Email,
                ContactEmail = contactEmail,
                Phone = account.Phone,
                Role = account.Role,
                CreatedAt = account.CreatedAt
            };
        }

        /// <summary>
        /// Verify current password
        /// </summary>
        public bool VerifyCurrentPassword(int accountId, string currentPassword)
        {
            var account = _accountRepository.GetById(accountId);
            if (account == null || string.IsNullOrEmpty(currentPassword))
                return false;

            // Use same logic as AccountController.Login
            return string.IsNullOrEmpty(account.Salt)
                ? PasswordHelper.VerifyPassword(currentPassword, account.PasswordHash)
                : PasswordHelper.VerifyPassword(currentPassword, account.PasswordHash, account.Salt);
        }

        /// <summary>
        /// Validate change password request
        /// </summary>
        public ValidationService.ValidationResult ValidateChangePassword(ChangePasswordViewModel model, int accountId)
        {
            var result = new ValidationService.ValidationResult();

            if (model == null)
            {
                result.AddError("", "Dữ liệu form không hợp lệ. Vui lòng thử lại.");
                return result;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                result.AddError("CurrentPassword", "Mật khẩu hiện tại là bắt buộc");
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                result.AddError("NewPassword", "Mật khẩu mới là bắt buộc");
            }

            if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                result.AddError("ConfirmPassword", "Xác nhận mật khẩu là bắt buộc");
            }
            else if (model.NewPassword != model.ConfirmPassword)
            {
                result.AddError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
            }

            // If basic validation passes, check password verification and complexity
            if (result.IsValid)
            {
                // Verify current password
                if (!VerifyCurrentPassword(accountId, model.CurrentPassword))
                {
                    result.AddError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                }

                // Validate new password complexity
                var passwordValidation = ValidateNewPasswordComplexity(model.NewPassword);
                if (!passwordValidation.IsValid)
                {
                    foreach (var error in passwordValidation.Errors)
                    {
                        result.AddError(error.Key, error.Value);
                    }
                }

                // Check if new password is same as current
                if (IsNewPasswordSameAsCurrent(accountId, model.NewPassword))
                {
                    result.AddError("NewPassword", "Mật khẩu mới phải khác mật khẩu hiện tại.");
                }
            }

            return result;
        }

        /// <summary>
        /// Validate new password complexity only
        /// </summary>
        public ValidationService.ValidationResult ValidateNewPasswordComplexity(string newPassword)
        {
            var result = new ValidationService.ValidationResult();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                result.AddError("NewPassword", "Mật khẩu mới là bắt buộc");
            }
            else
            {
                bool hasLower = newPassword.Any(char.IsLower);
                bool hasUpper = newPassword.Any(char.IsUpper);
                bool hasDigit = newPassword.Any(char.IsDigit);

                if (newPassword.Length < 6 || !hasLower || !hasUpper || !hasDigit)
                {
                    result.AddError("NewPassword", "Mật khẩu mới phải tối thiểu 6 ký tự gồm chữ hoa, chữ thường và số.");
                }
            }

            return result;
        }

        /// <summary>
        /// Check if new password is same as current password
        /// </summary>
        public bool IsNewPasswordSameAsCurrent(int accountId, string newPassword)
        {
            var account = _accountRepository.GetById(accountId);
            if (account == null || string.IsNullOrEmpty(newPassword))
                return false;

            return string.IsNullOrEmpty(account.Salt)
                ? PasswordHelper.VerifyPassword(newPassword, account.PasswordHash)
                : PasswordHelper.VerifyPassword(newPassword, account.PasswordHash, account.Salt);
        }

        public void ChangePassword(int accountId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("New password cannot be empty", nameof(newPassword));

            if (!_db.ObjectTrackingEnabled)
                throw new InvalidOperationException("Cannot change password: ObjectTrackingEnabled is false. Use DbContextFactory.Create() instead of CreateReadOnly()");

            string salt = PasswordHelper.GenerateSalt();
            string passwordHash = PasswordHelper.HashPassword(newPassword, salt);
            _accountRepository.UpdatePassword(accountId, passwordHash, salt);
            _accountRepository.SaveChanges();
        }
    }
}
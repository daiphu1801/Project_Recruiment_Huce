using System;
using System.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
using Project_Recruiment_Huce.Repositories;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// Service layer cho các thao tác quản lý tài khoản cá nhân
    /// Đóng gói business logic cho quản lý account, đổi mật khẩu, và validation
    /// </summary>
    public class MyAccountService : IMyAccountService
    {
        private readonly IMyAccountRepository _repo;

        public MyAccountService(IMyAccountRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <summary>
        /// Lấy thông tin tài khoản bao gồm email liên lạc từ profile
        /// </summary>
        /// <param name="accountId">ID của tài khoản</param>
        /// <returns>ViewModel chứa thông tin tài khoản</returns>
        public MyAccountViewModel GetAccountInfo(int accountId)
        {
            var account = _repo.GetAccountById(accountId);
            if (account == null) return null;

            // Get contact email from profile
            string contactEmail = null;
            if (account.Role == "Candidate")
            {
                var candidate = _repo.GetCandidateByAccountId(accountId);
                contactEmail = candidate?.Email;
            }
            else if (account.Role == "Recruiter")
            {
                var recruiter = _repo.GetRecruiterByAccountId(accountId);
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
        /// Xác thực mật khẩu hiện tại của người dùng
        /// </summary>
        /// <param name="accountId">ID của tài khoản</param>
        /// <param name="currentPassword">Mật khẩu hiện tại</param>
        /// <returns>True nếu mật khẩu đúng, false nếu sai</returns>
        private bool VerifyCurrentPassword(int accountId, string currentPassword)
        {
            var account = _repo.GetAccountById(accountId);
            if (account == null || string.IsNullOrEmpty(currentPassword))
                return false;

            // Sử dụng VerifyPasswordV2 - hỗ trợ cả format cũ và mới
            var verifyResult = PasswordHelper.VerifyPasswordV2(currentPassword, account.PasswordHash, account.Salt);
            return verifyResult != PasswordHelper.VerifyResult.Failed;
        }

        /// <summary>
        /// Validate yêu cầu đổi mật khẩu
        /// Kiểm tra mật khẩu hiện tại, độ phức tạp mật khẩu mới, và xác nhận
        /// </summary>
        /// <param name="model">Model chứa thông tin đổi mật khẩu</param>
        /// <param name="accountId">ID của tài khoản</param>
        /// <returns>Kết quả validation</returns>
        public ValidationResult ValidateChangePassword(ChangePasswordViewModel model, int accountId)
        {
            var result = new ValidationResult();

            if (model == null)
            {
                result.IsValid = false;
                result.Errors[""] = "Dữ liệu form không hợp lệ. Vui lòng thử lại.";
                return result;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                result.IsValid = false;
                result.Errors["CurrentPassword"] = "Mật khẩu hiện tại là bắt buộc";
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                result.IsValid = false;
                result.Errors["NewPassword"] = "Mật khẩu mới là bắt buộc";
            }

            if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                result.IsValid = false;
                result.Errors["ConfirmPassword"] = "Xác nhận mật khẩu là bắt buộc";
            }
            else if (model.NewPassword != model.ConfirmPassword)
            {
                result.IsValid = false;
                result.Errors["ConfirmPassword"] = "Mật khẩu xác nhận không khớp.";
            }

            // If basic validation passes, check password verification and complexity
            if (result.IsValid)
            {
                // Verify current password
                if (!VerifyCurrentPassword(accountId, model.CurrentPassword))
                {
                    result.IsValid = false;
                    result.Errors["CurrentPassword"] = "Mật khẩu hiện tại không đúng";
                }

                // Validate new password complexity
                var passwordValidation = ValidateNewPasswordComplexity(model.NewPassword);
                if (!passwordValidation.IsValid)
                {
                    result.IsValid = false;
                    foreach (var error in passwordValidation.Errors)
                    {
                        result.Errors[error.Key] = error.Value;
                    }
                }

                // Check if new password is same as current
                if (IsNewPasswordSameAsCurrent(accountId, model.NewPassword))
                {
                    result.IsValid = false;
                    result.Errors["NewPassword"] = "Mật khẩu mới phải khác mật khẩu hiện tại.";
                }
            }

            return result;
        }

        /// <summary>
        /// Validate new password complexity only
        /// </summary>
        private ValidationResult ValidateNewPasswordComplexity(string newPassword)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                result.IsValid = false;
                result.Errors["NewPassword"] = "Mật khẩu mới là bắt buộc";
            }
            else
            {
                bool hasLower = newPassword.Any(char.IsLower);
                bool hasUpper = newPassword.Any(char.IsUpper);
                bool hasDigit = newPassword.Any(char.IsDigit);

                if (newPassword.Length < 6 || !hasLower || !hasUpper || !hasDigit)
                {
                    result.IsValid = false;
                    result.Errors["NewPassword"] = "Mật khẩu mới phải tối thiểu 6 ký tự gồm chữ hoa, chữ thường và số.";
                }
            }

            return result;
        }

        /// <summary>
        /// Check if new password is same as current password
        /// </summary>
        private bool IsNewPasswordSameAsCurrent(int accountId, string newPassword)
        {
            var account = _repo.GetAccountById(accountId);
            if (account == null || string.IsNullOrEmpty(newPassword))
                return false;

            // Sử dụng VerifyPasswordV2 - hỗ trợ cả format cũ và mới
            var verifyResult = PasswordHelper.VerifyPasswordV2(newPassword, account.PasswordHash, account.Salt);
            return verifyResult != PasswordHelper.VerifyResult.Failed;
        }

        /// <summary>
        /// Đổi mật khẩu cho tài khoản
        /// </summary>
        public void ChangePassword(int accountId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Mật khẩu mới không được rỗng", nameof(newPassword));

            // Sử dụng HashPassword mới (PBKDF2) - không cần salt riêng
            string passwordHash = PasswordHelper.HashPassword(newPassword);
            _repo.UpdatePassword(accountId, passwordHash, null);
            _repo.SaveChanges();
        }
    }
}

using System;
using System.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// Service để tập trung các validation liên quan tới Account (Register, ResetPassword,...)
    /// Các method nhận `JOBPORTAL_ENDataContext` để kiểm tra uniqueness và token.
    /// Trả về `ValidationResult` chứa errors và optional data như NormalizedPhone, MappedRole hoặc Token.
    /// </summary>
    public class AccountValidationService
    {
        private readonly ValidationService _validationService = new ValidationService();
        private readonly Repositories.IAccountRepository _repo;

        public AccountValidationService(Repositories.IAccountRepository repo)
        {
            _repo = repo;
        }

        public ValidationResult ValidateRegister(RegisterViewModel model)
        {
            var result = new ValidationResult();

            // Username unique
            if (_repo.UsernameExists(model.TenDangNhap))
            {
                result.IsValid = false;
                result.Errors["TenDangNhap"] = "Tên đăng nhập đã tồn tại.";
            }

            // Email unique
            var email = (model.Email ?? string.Empty).Trim();
            if (_repo.EmailExists(email))
            {
                result.IsValid = false;
                result.Errors["Email"] = "Email đã được sử dụng.";
            }

            // Phone format & uniqueness (optional)
            var phone = (model.SoDienThoai ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(phone))
            {
                if (!ValidationHelper.IsValidVietnamesePhone(phone))
                {
                    result.IsValid = false;
                    result.Errors["SoDienThoai"] = ValidationHelper.GetPhoneErrorMessage();
                }
                else
                {
                    var normalized = ValidationHelper.NormalizePhone(phone);
                    if (_repo.PhoneExistsNormalized(normalized))
                    {
                        result.IsValid = false;
                        result.Errors["SoDienThoai"] = "Số điện thoại này đã được sử dụng. Mỗi số điện thoại chỉ có thể đăng ký một tài khoản.";
                    }
                    else
                    {
                        result.Data["NormalizedPhone"] = normalized;
                    }
                }
            }
            else
            {
                result.Data["NormalizedPhone"] = null;
            }

            // Password complexity via existing ValidationService
            var pwdValidation = _validationService.ValidatePassword(model.Password, model.ConfirmPassword);
            if (!pwdValidation.IsValid)
            {
                result.IsValid = false;
                foreach (var e in pwdValidation.Errors)
                {
                    result.Errors[e.Key] = e.Value;
                }
            }

            // Role mapping validation
            var validVaiTro = new[] { "NhaTuyenDung", "NguoiUngTuyen" };
            if (!validVaiTro.Contains(model.VaiTro))
            {
                result.IsValid = false;
                result.Errors["VaiTro"] = "Vai trò không hợp lệ.";
            }

            string mappedRole = "Candidate";
            if (model.VaiTro == "NhaTuyenDung") mappedRole = "Recruiter";
            else if (model.VaiTro == "NguoiUngTuyen") mappedRole = "Candidate";
            result.Data["MappedRole"] = mappedRole;

            return result;
        }

        public ValidationResult ValidateResetPassword(ResetPasswordViewModel model)
        {
            var result = new ValidationResult();
            var email = (model.Email ?? string.Empty).Trim().ToLower();
            var resetCode = (model.ResetCode ?? string.Empty).Trim().ToUpper();

            var token = _repo.GetPasswordResetToken(email, resetCode);
            if (token == null)
            {
                result.IsValid = false;
                result.Errors["ResetCode"] = "Mã xác thực không đúng hoặc đã hết hạn. Vui lòng thử lại.";
                return result;
            }

            if (token.AttemptCount > 5)
            {
                result.IsValid = false;
                result.Errors["ResetCode"] = "Bạn đã nhập sai mã quá nhiều lần. Vui lòng yêu cầu mã mới.";
                return result;
            }

            // Password complexity (same rules as before)
            var password = model.NewPassword ?? string.Empty;
            bool hasLower = password.Any(char.IsLower);
            bool hasUpper = password.Any(char.IsUpper);
            bool hasDigit = password.Any(char.IsDigit);
            if (password.Length < 6 || !hasLower || !hasUpper || !hasDigit)
            {
                result.IsValid = false;
                result.Errors["NewPassword"] = "Mật khẩu phải tối thiểu 6 ký tự gồm chữ hoa, chữ thường và số.";
            }

            result.Data["Token"] = token;
            return result;
        }
    }
}

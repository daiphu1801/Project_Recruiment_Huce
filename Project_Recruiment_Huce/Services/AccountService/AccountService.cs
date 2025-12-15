using System;
using System.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Repositories;


namespace Project_Recruiment_Huce.Services
{
    public class AccountService : IAccountService
    {
        private readonly Repositories.IAccountRepository _repo;
        private readonly ValidationService _validationService = new ValidationService();

        public AccountService(Repositories.IAccountRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public ValidationResult ValidateRegister(RegisterViewModel model)
        {
            var result = new ValidationResult();

            if (_repo.UsernameExists(model.TenDangNhap))
            {
                result.IsValid = false;
                result.Errors["TenDangNhap"] = "Tên đăng nhập đã tồn tại.";
            }

            var email = (model.Email ?? string.Empty).Trim();
            if (_repo.EmailExists(email))
            {
                result.IsValid = false;
                result.Errors["Email"] = "Email đã được sử dụng.";
            }

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
                        result.Errors["SoDienThoai"] = "Số điện thoại này đã được sử dụng.";
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

            var pwdValidation = _validationService.ValidatePassword(model.Password, model.ConfirmPassword);
            if (!pwdValidation.IsValid)
            {
                result.IsValid = false;
                foreach (var e in pwdValidation.Errors) result.Errors[e.Key] = e.Value;
            }

            var validVaiTro = new[] { "NhaTuyenDung", "NguoiUngTuyen" };
            if (!validVaiTro.Contains(model.VaiTro))
            {
                result.IsValid = false;
                result.Errors["VaiTro"] = "Vai trò không hợp lệ.";
            }

            return result;
        }

        public ValidationResult Register(RegisterViewModel model)
        {
            var result = ValidateRegister(model);
            if (!result.IsValid) return result;

            // Sử dụng HashPassword mới (PBKDF2) - không cần salt riêng
            var passwordHash = PasswordHelper.HashPassword(model.Password);

            // map role
            string mappedRole = "Candidate";
            if (model.VaiTro == "NhaTuyenDung") mappedRole = "Recruiter";

            var account = _repo.Create(model.TenDangNhap, (model.Email ?? string.Empty).Trim(), result.Data.ContainsKey("NormalizedPhone") ? result.Data["NormalizedPhone"] as string : null, mappedRole, passwordHash, null);

            // try create profile sync but do not fail registration on profile error
            try
            {
                using (var dbForProfile = Infrastructure.DbContextFactory.Create())
                {
                    EmailSyncHelper.CreateProfile(dbForProfile, account.AccountID, mappedRole);
                    dbForProfile.SubmitChanges();
                }
            }
            catch
            {
                // ignore
            }

            result.Data["Account"] = account;
            return result;
        }

        public Account Authenticate(string userOrEmail, string password)
        {
            var user = _repo.FindByUsernameOrEmail(userOrEmail);
            if (user == null) return null;

            // Xác thực mật khẩu sử dụng PBKDF2
            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
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
                result.Errors["ResetCode"] = "Mã xác thực không đúng hoặc đã hết hạn.";
                return result;
            }

            if (token.AttemptCount > 5)
            {
                result.IsValid = false;
                result.Errors["ResetCode"] = "Bạn đã nhập sai mã quá nhiều lần. Vui lòng yêu cầu mã mới.";
                return result;
            }

            result.Data["Token"] = token;
            return result;
        }

        public ValidationResult ResetPassword(ResetPasswordViewModel model)
        {
            var result = ValidateResetPassword(model);
            if (!result.IsValid) return result;

            var token = result.Data.ContainsKey("Token") ? result.Data["Token"] as PasswordResetToken : null;
            if (token == null)
            {
                result.IsValid = false;
                result.Errors["ResetCode"] = "Mã xác thực không hợp lệ.";
                return result;
            }

            var account = _repo.GetById(token.AccountID);
            if (account == null)
            {
                result.IsValid = false;
                result.Errors["Email"] = "Tài khoản không tồn tại.";
                return result;
            }

            var password = model.NewPassword ?? string.Empty;
            bool hasLower = password.Any(char.IsLower);
            bool hasUpper = password.Any(char.IsUpper);
            bool hasDigit = password.Any(char.IsDigit);
            if (password.Length < 6 || !hasLower || !hasUpper || !hasDigit)
            {
                result.IsValid = false;
                result.Errors["NewPassword"] = "Mật khẩu phải tối thiểu 6 ký tự gồm chữ hoa, chữ thường và số.";
                return result;
            }

            // Sử dụng HashPassword mới (PBKDF2) - không cần salt riêng
            var passwordHash = PasswordHelper.HashPassword(password);

            // update account password
            _repo.UpdatePassword(account.AccountID, passwordHash, null);

            // mark token used
            token.UsedFlag = 1;
            _repo.UpdatePasswordResetToken(token);

            _repo.SaveChanges();

            result.Data["Account"] = account;
            return result;
        }
<<<<<<< HEAD
=======
       
        public void CreateGoogleProfile(string email, string fullName, string avatarUrl, int userType, int userId)
        {
            // 1. Xử lý Avatar mặc định nếu trống
            if (string.IsNullOrEmpty(avatarUrl))
            {
                avatarUrl = "/Content/images/default-avatar.png";
            }

            // 2. Xử lý tách tên (Vì Repository yêu cầu firstName/lastName)
            string firstName = "";
            string lastName = "";

            if (!string.IsNullOrEmpty(fullName))
            {
                var names = fullName.Trim().Split(' ');
                if (names.Length > 0)
                {
                    lastName = names[names.Length - 1]; // Tên thật
                    if (names.Length > 1)
                    {
                        firstName = fullName.Substring(0, fullName.Length - lastName.Length).Trim(); // Họ đệm
                    }
                    else
                    {
                        firstName = ""; // Trường hợp chỉ có 1 chữ
                    }
                }
            }
            else
            {
                // Nếu không có tên, lấy email làm tên tạm
                lastName = email;
            }

            // 3. Xử lý ngày sinh mặc định (Repository yêu cầu tham số này)
            DateTime defaultBirthDate = DateTime.Now.AddYears(-20);

            // 4. Gọi Repository (Đảm bảo gọi đúng thứ tự tham số của Repository bạn đã viết)
            _repo.CreateGoogleProfile(
                email: email,
                firstName: firstName,
                lastName: lastName,
                userId: userId,         // int
                FullName: fullName,
                userType: userType,
                Avatar: avatarUrl,
                Birthdate: defaultBirthDate
            );
        }
>>>>>>> b5687619104f46f9178da37581c63d949fa94225
    }
}

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
       
        // ============================================================
        // GOOGLE LOGIN SERVICE METHOD
        // ============================================================

        /// <summary>
        /// Xử lý đăng nhập Google (Option A: Đơn giản, từ chối nếu email đã tồn tại với non-Google account)
        /// </summary>
        public GoogleLoginResult ProcessGoogleLogin(GoogleUserInfoViewModel googleUser)
        {
            try
            {
                // 1. Kiểm tra xem đã có account với GoogleId này chưa?
                var existingGoogleAccount = _repo.FindByGoogleId(googleUser.GoogleId);
                if (existingGoogleAccount != null)
                {
                    // Đã có tài khoản, cho phép đăng nhập
                    return new GoogleLoginResult
                    {
                        Success = true,
                        Account = existingGoogleAccount,
                        IsNewAccount = false
                    };
                }

                // 2. Kiểm tra email đã tồn tại chưa?
                var existingEmailAccount = _repo.FindByEmail(googleUser.Email);
                if (existingEmailAccount != null)
                {
                    // Option A: Từ chối nếu email đã tồn tại với tài khoản thường
                    if (existingEmailAccount.IsGoogleAccount != true)
                    {
                        return new GoogleLoginResult
                        {
                            Success = false,
                            ErrorMessage = "Email này đã được đăng ký với tài khoản thông thường. Vui lòng đăng nhập bằng mật khẩu."
                        };
                    }
                    
                    // Nếu là Google account nhưng GoogleId khác (trường hợp hiếm)
                    return new GoogleLoginResult
                    {
                        Success = false,
                        ErrorMessage = "Email này đã được liên kết với một tài khoản Google khác."
                    };
                }

                // 3. Tạo tài khoản mới
                // Generate username từ email
                var username = _repo.GenerateUniqueUsername(googleUser.Email);

                // Tạo Account
                var newAccount = _repo.CreateGoogleAccount(googleUser.Email, googleUser.FullName, googleUser.GoogleId, username);

                // Tạo Candidate profile
                _repo.CreateCandidateProfile(newAccount.AccountID, googleUser.FullName, googleUser.Email);

                // Lưu avatar nếu có
                if (!string.IsNullOrWhiteSpace(googleUser.AvatarUrl))
                {
                    try
                    {
                        var photo = _repo.SaveProfilePhoto(googleUser.AvatarUrl, $"google_avatar_{newAccount.AccountID}.jpg");
                        if (photo != null)
                        {
                            _repo.UpdateAccountPhotoId(newAccount.AccountID, photo.PhotoID);
                        }
                    }
                    catch
                    {
                        // Không để lỗi avatar làm gián đoạn đăng nhập
                    }
                }

                return new GoogleLoginResult
                {
                    Success = true,
                    Account = newAccount,
                    IsNewAccount = true
                };
            }
            catch (Exception ex)
            {
                return new GoogleLoginResult
                {
                    Success = false,
                    ErrorMessage = "Đã xảy ra lỗi trong quá trình xử lý đăng nhập Google: " + ex.Message
                };
            }
        }
    }
}

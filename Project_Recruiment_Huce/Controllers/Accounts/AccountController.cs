using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Services;
using Project_Recruiment_Huce.Infrastructure;

namespace Project_Recruiment_Huce.Controllers
{
    public class AccountController : Controller
    {
        // Using ValidationService for centralized validation
        private readonly ValidationService _validationService = new ValidationService();

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = DbContextFactory.Create())
            {
                // Normalize input (allow username OR email)
                var input = (model.EmailOrUsername ?? string.Empty).Trim();
                bool isEmail = input.Contains("@");
                var lower = input.ToLower();
                var user = db.Accounts.FirstOrDefault(a =>
                    (isEmail ? a.Email.ToLower() == lower : a.Username == input) && a.ActiveFlag == 1);

                if (user != null && (string.IsNullOrEmpty(user.Salt) 
                    ? PasswordHelper.VerifyPassword(model.Password, user.PasswordHash) 
                    : PasswordHelper.VerifyPassword(model.Password, user.PasswordHash, user.Salt)))
                {
                    // Reject Admin users from main site login
                    if (user.Role == "Admin")
                    {
                        ModelState.AddModelError("", "Tài khoản Admin phải đăng nhập tại trang Quản trị.");
                        return View(model);
                    }

                    // Create claims identity for OWIN authentication (User Cookie)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.AccountID.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("VaiTro", user.Role),
                    new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Local")
                };

                    var identity = new ClaimsIdentity(claims, "UserCookie");
                    AuthenticationManager.SignIn(new AuthenticationProperties 
                    { 
                        IsPersistent = model.RememberMe 
                    }, identity);

                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }


        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = DbContextFactory.Create())
                {
                    // Check if Username already exists
                    if (db.Accounts.Any(a => a.Username == model.TenDangNhap))
                    {
                        ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                        return View(model);
                    }

                    // Check if Email already exists
                    var email = (model.Email ?? string.Empty).Trim();
                    if (db.Accounts.Any(a => a.Email.ToLower() == email.ToLower()))
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng.");
                        return View(model);
                    }

                    // Validate phone number format and uniqueness (if provided)
                    var phone = (model.SoDienThoai ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(phone))
                    {
                        // Validate phone format
                        if (!ValidationHelper.IsValidVietnamesePhone(phone))
                        {
                            ModelState.AddModelError("SoDienThoai", ValidationHelper.GetPhoneErrorMessage());
                            return View(model);
                        }

                        // Normalize phone number
                        phone = ValidationHelper.NormalizePhone(phone);

                        // Check if phone already exists
                        if (!ValidationHelper.IsAccountPhoneUnique(phone))
                        {
                            ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã được sử dụng. Mỗi số điện thoại chỉ có thể đăng ký một tài khoản.");
                            return View(model);
                        }
                    }
                    else
                    {
                        phone = null; // Set to null if empty
                    }

                    // Extra server-side password complexity validation using ValidationService
                    var passwordValidation = _validationService.ValidatePassword(model.Password, model.ConfirmPassword);
                    if (!passwordValidation.IsValid)
                    {
                        foreach (var error in passwordValidation.Errors)
                        {
                            ModelState.AddModelError(error.Key, error.Value);
                        }
                        return View(model);
                    }

                    // Validate VaiTro -> Role mapping to new schema (Company is now a profile, not an account type)
                    var validVaiTro = new[] { "NhaTuyenDung", "NguoiUngTuyen" };
                    if (!validVaiTro.Contains(model.VaiTro))
                    {
                        ModelState.AddModelError("VaiTro", "Vai trò không hợp lệ.");
                        return View(model);
                    }

                    // Map roles from old labels to new schema values (Company removed - it's now a profile only)
                    string mappedRole;
                    if (model.VaiTro == "NhaTuyenDung")
                    {
                        mappedRole = "Recruiter";
                    }
                    else if (model.VaiTro == "NguoiUngTuyen")
                    {
                        mappedRole = "Candidate";
                    }
                    else
                    {
                        mappedRole = "Candidate"; // Default to Candidate
                    }

                    // Generate salt and hash password
                    string salt = PasswordHelper.GenerateSalt();
                    string passwordHash = PasswordHelper.HashPassword(model.Password, salt);

                    // Create new Account
                    var newAccount = new Account
                    {
                        Username = model.TenDangNhap,
                        Email = email,
                        Phone = phone, // Use normalized phone
                        Role = mappedRole,
                        PasswordHash = passwordHash,
                        Salt = salt,
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };

                    db.Accounts.InsertOnSubmit(newAccount);
                    db.SubmitChanges();

                    // Tự động tạo profile Candidate/Recruiter (không set email - email trong profile là email liên lạc riêng)
                    if (mappedRole == "Candidate" || mappedRole == "Recruiter")
                    {
                        EmailSyncHelper.CreateProfile(db, newAccount.AccountID, mappedRole);
                        db.SubmitChanges();
                    }

                    // Auto login after registration (User Cookie)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, newAccount.AccountID.ToString()),
                    new Claim(ClaimTypes.Name, newAccount.Username),
                    new Claim(ClaimTypes.Email, newAccount.Email),
                    new Claim("VaiTro", newAccount.Role),
                    new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Local")
                };

                    var identity = new ClaimsIdentity(claims, "UserCookie");
                    AuthenticationManager.SignIn(identity);

                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = DbContextFactory.Create())
            {
                var email = (model.Email ?? string.Empty).Trim().ToLower();
                var account = db.Accounts.FirstOrDefault(a => a.Email.ToLower() == email && a.ActiveFlag == 1);

                // Luôn hiển thị thông báo thành công để bảo mật (không tiết lộ email có tồn tại hay không)
                if (account != null)
                {
                    // Tạo mã reset và lưu vào database
                    var token = PasswordResetHelper.CreateResetToken(db, account.AccountID, account.Email);
                    
                    // Gửi email chứa mã reset
                    bool emailSent = PasswordResetHelper.SendResetCodeEmail(account.Email, token.ResetCode, account.Username);
                    
                    if (emailSent)
                    {
                        // Lưu email vào session để dùng ở trang ResetPassword
                        Session["ResetPasswordEmail"] = account.Email;
                        TempData["SuccessMessage"] = $"Mã xác thực đã được gửi đến email {account.Email}. Vui lòng kiểm tra hộp thư của bạn.";
                        return RedirectToAction("ResetPassword");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể gửi email. Vui lòng thử lại sau hoặc liên hệ hỗ trợ.");
                    }
                }
                else
                {
                    // Vẫn hiển thị thông báo thành công để bảo mật
                    TempData["SuccessMessage"] = "Nếu email tồn tại trong hệ thống, mã xác thực đã được gửi đến email của bạn.";
                    return RedirectToAction("Login");
                }
            }

            return View(model);
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            // Kiểm tra xem có email trong session không (từ ForgotPassword)
            var email = Session["ResetPasswordEmail"] as string;
            if (string.IsNullOrEmpty(email))
            {
                // Nếu không có email trong session, redirect về ForgotPassword
                TempData["ErrorMessage"] = "Vui lòng nhập email để nhận mã xác thực trước.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email
            };

            return View(model);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = DbContextFactory.Create())
            {
                var email = (model.Email ?? string.Empty).Trim().ToLower();
                var resetCode = (model.ResetCode ?? string.Empty).Trim().ToUpper();

                // Xác thực mã reset
                var token = PasswordResetHelper.ValidateResetCode(db, email, resetCode);

                if (token == null)
                {
                    ModelState.AddModelError("ResetCode", "Mã xác thực không đúng hoặc đã hết hạn. Vui lòng thử lại.");
                    return View(model);
                }

                // Kiểm tra số lần thử (tối đa 5 lần)
                if (token.AttemptCount > 5)
                {
                    ModelState.AddModelError("ResetCode", "Bạn đã nhập sai mã quá nhiều lần. Vui lòng yêu cầu mã mới.");
                    return View(model);
                }

                // Validate password complexity
                var password = model.NewPassword ?? string.Empty;
                bool hasLower = password.Any(char.IsLower);
                bool hasUpper = password.Any(char.IsUpper);
                bool hasDigit = password.Any(char.IsDigit);
                if (password.Length < 6 || !hasLower || !hasUpper || !hasDigit)
                {
                    ModelState.AddModelError("NewPassword", "Mật khẩu phải tối thiểu 6 ký tự gồm chữ hoa, chữ thường và số.");
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == token.AccountID);
                if (account != null)
                {
                    string salt = PasswordHelper.GenerateSalt();
                    string passwordHash = PasswordHelper.HashPassword(model.NewPassword, salt);
                    
                    account.PasswordHash = passwordHash;
                    account.Salt = salt;
                    
                    // Đánh dấu token đã sử dụng
                    PasswordResetHelper.MarkTokenAsUsed(db, token.TokenID);
                    
                    db.SubmitChanges();

                    // Xóa email khỏi session
                    Session.Remove("ResetPasswordEmail");

                    TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập với mật khẩu mới.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy tài khoản. Vui lòng thử lại.");
                }
            }

            return View(model);
        }


        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut("UserCookie");
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/LogOff (for easy testing)
        [AllowAnonymous]
        public ActionResult LogOffGet()
        {
            AuthenticationManager.SignOut("UserCookie");
            return RedirectToAction("Login", "Account");
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Helpers
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}
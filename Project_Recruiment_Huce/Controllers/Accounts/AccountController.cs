using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
using Project_Recruiment_Huce.Repositories;
using Project_Recruiment_Huce.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks; // Cần thêm cái này cho Async
using System.Web;
using System.Web.Mvc;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller xử lý đăng nhập, đăng ký, quên mật khẩu và authentication
    /// Sử dụng OWIN Cookie Authentication với Claims-based identity
    /// </summary>
    public class AccountController : Controller
    {
        // Khai báo Service để dùng chung
        private readonly IAccountService _accountService;
        private const string USER_AUTH_TYPE = "UserCookie";
        public AccountController()
        {
            // Khởi tạo Service thủ công
            var dbContext = DbContextFactory.Create();
            
            var repo = new Repositories.AccountRepository(dbContext);
            _accountService = new Services.AccountService(repo);
        }

        // ============================================================
        // 1. CÁC HÀM CŨ (GIỮ NGUYÊN HOẶC TỐI ƯU NHẸ)
        // ============================================================

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // Nếu đã đăng nhập, hiển thị thông báo và redirect về trang phù hợp
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập
        /// POST: /Account/Login
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl, string userType)
        {
            // Chặn login attempts khi đã authenticated
            if (User?.Identity?.IsAuthenticated == true)
            {
                if (Request.IsAjaxRequest())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Already authenticated");
                }
                return RedirectToLocal(returnUrl);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.UserType = userType;
                return View(model);
            }

            using (var db = DbContextFactory.Create())
            {
                // Normalize input (allow username OR email)
                var input = (model.EmailOrUsername ?? string.Empty).Trim();
                bool isEmail = input.Contains("@");
                var lower = input.ToLower();

                var account = db.Accounts.FirstOrDefault(a =>
                    (isEmail ? a.Email.ToLower() == lower : a.Username == input) && a.ActiveFlag == 1);

                // Kiểm tra tài khoản không tồn tại hoặc mật khẩu sai
                if (account == null || !PasswordHelper.VerifyPassword(model.Password, account.PasswordHash))
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.UserType = userType;
                    return View(model);
                }

                // Kiểm tra role có khớp với userType không
                if (!string.IsNullOrEmpty(userType))
                {
                    bool roleMatched = false;
                    if (userType.ToLower() == "candidate" && account.Role == "Candidate")
                    {
                        roleMatched = true;
                    }
                    else if (userType.ToLower() == "recruiter" && account.Role == "Recruiter")
                    {
                        roleMatched = true;
                    }

                    if (!roleMatched)
                    {
                        ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                        ViewBag.ReturnUrl = returnUrl;
                        ViewBag.UserType = userType;
                        return View(model);
                    }
                }

                // Gọi hàm đăng nhập dùng chung
                SignInUser(account, model.RememberMe);

                // Tạo claims và đăng nhập
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, account.AccountID.ToString()),
                    new Claim(ClaimTypes.Name, account.Username),
                    new Claim(ClaimTypes.Email, account.Email),
                    new Claim(ClaimTypes.Role, account.Role),
                    new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Local")
                };

                var identity = new ClaimsIdentity(claims, "UserCookie");
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = model.RememberMe }, identity);

                return RedirectToLocal(returnUrl);
            }
        }

        /// <summary>
        /// Hiển thị trang đăng ký
        /// GET: /Account/Register
        /// </summary>
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (User?.Identity?.IsAuthenticated == true) return RedirectToLocal(null);
            return View();
        }

        /// <summary>
        /// Xử lý đăng ký tài khoản mới
        /// POST: /Account/Register
        /// Validate: username unique, email unique, phone unique, password strength, role mapping
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (User?.Identity?.IsAuthenticated == true) return RedirectToLocal(null);

            if (ModelState.IsValid)
            {
                var res = _accountService.Register(model);
                if (!res.IsValid)
                {
                    foreach (var err in res.Errors) ModelState.AddModelError(err.Key, err.Value);
                    return View(model);
                }

                var account = res.Data.ContainsKey("Account") ? res.Data["Account"] as Account : null;
                if (account != null)
                {
                    SignInUser(account, false);
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }

        // ... (Giữ nguyên ForgotPassword và ResetPassword như code cũ của bạn) ...
        [AllowAnonymous]
        public ActionResult ForgotPassword() { return View(); }

        /// <summary>
        /// Xử lý yêu cầu quên mật khẩu - gửi mã reset qua email
        /// POST: /Account/ForgotPassword
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            // (Code cũ của bạn giữ nguyên ở đây để tiết kiệm chỗ hiển thị)
            // ... Logic gửi mail ...
            if (!ModelState.IsValid) return View(model);
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
                        TempData["SuccessMessage"] = $"Mã xác thực đã gửi đến {account.Email}.";
                        return RedirectToAction("ResetPassword");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể gửi email. Vui lòng thử lại sau hoặc liên hệ hỗ trợ.");
                }
                TempData["SuccessMessage"] = "Nếu email tồn tại, mã xác thực đã được gửi.";
                return RedirectToAction("Login");
            }
        }

            return View(model);
        }

        /// <summary>
        /// Hiển thị trang đặt lại mật khẩu với mã xác thực
        /// GET: /Account/ResetPassword
        /// </summary>
        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            // Kiểm tra xem có email trong session không (từ ForgotPassword)
            var email = Session["ResetPasswordEmail"] as string;
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");
            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = _accountService.ResetPassword(model);
            if (!res.IsValid)
            {
                foreach (var err in res.Errors) ModelState.AddModelError(err.Key, err.Value);
                return View(model);
            }
            Session.Remove("ResetPasswordEmail");
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(USER_AUTH_TYPE);
            return RedirectToAction("Index", "Home");
        }

        // ============================================================
        // 2. PHẦN BỔ SUNG: GOOGLE AUTHENTICATION
        // ============================================================

        // ============================================================
        // GOOGLE LOGIN CONTROLLER METHODS (Rebuilt from scratch)
        // ============================================================

        /// <summary>
        /// Khởi tạo đăng nhập Google
        /// POST: /Account/ExternalLogin
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl, string userType)
        {
            // Lưu userType vào session để dùng trong callback
            Session["LoginUserType"] = userType;
            
            // Trigger OWIN Google authentication
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        /// <summary>
        /// Callback từ Google sau khi xác thực
        /// GET: /Account/ExternalLoginCallback
        /// </summary>
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            // Lấy userType từ session
            var userType = Session["LoginUserType"] as string;
            
            // 1. Lấy thông tin từ Google thông qua OWIN
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                TempData["ErrorMessage"] = "Lỗi xác thực với Google. Vui lòng thử lại.";
                ViewBag.UserType = userType;
                return RedirectToAction("Login");
            }

            // 2. Extract thông tin từ Google claims và tạo ViewModel
            var googleUser = new GoogleUserInfoViewModel
            {
                Email = loginInfo.Email,
                GoogleId = loginInfo.Login.ProviderKey,
                FullName = loginInfo.ExternalIdentity.FindFirstValue(ClaimTypes.Name) ?? loginInfo.Email,
                AvatarUrl = loginInfo.ExternalIdentity.FindFirstValue("picture") ?? ""
            };

            // 3. Sign out external cookie TRƯỚC KHI xử lý (tránh conflict)
            AuthenticationManager.SignOut(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ExternalCookie);

            // 4. Xử lý Google Login qua Service Layer
            var result = _accountService.ProcessGoogleLogin(googleUser);

            // 5. Xử lý kết quả
            if (result.Success)
            {
                // Kiểm tra role có khớp với userType không
                if (!string.IsNullOrEmpty(userType))
                {
                    bool roleMatched = false;
                    if (userType.ToLower() == "candidate" && result.Account.Role == "Candidate")
                    {
                        roleMatched = true;
                    }
                    else if (userType.ToLower() == "recruiter" && result.Account.Role == "Recruiter")
                    {
                        roleMatched = true;
                    }

                    if (!roleMatched)
                    {
                        TempData["ErrorMessage"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
                        ViewBag.UserType = userType;
                        Session.Remove("LoginUserType");
                        return RedirectToAction("Login");
                    }
                }
                
                // Đăng nhập thành công
                SignInUser(result.Account, false);

                // Hiển thị thông báo nếu là tài khoản mới
                if (result.IsNewAccount)
                {
                    TempData["SuccessMessage"] = "Chào mừng bạn! Tài khoản đã được tạo thành công từ Google.";
                }

                Session.Remove("LoginUserType");
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // Đăng nhập thất bại
                TempData["ErrorMessage"] = result.ErrorMessage;
                ViewBag.UserType = userType;
                Session.Remove("LoginUserType");
                return RedirectToAction("Login");
            }
        }

        // ============================================================
        // 3. HELPERS
        // ============================================================

        // Hàm đăng nhập dùng chung (Tránh lặp code tạo Claims)
        private void SignInUser(Account account, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountID.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role ?? "Candidate"),
                new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Local")
            };

            var identity = new ClaimsIdentity(claims, USER_AUTH_TYPE);
            var authProperties = new AuthenticationProperties 
            { 
                IsPersistent = rememberMe,
                AllowRefresh = true
            };
            
            // Sign in với authentication type rõ ràng
            AuthenticationManager.SignIn(authProperties, identity);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

            // Check role để redirect về đúng trang chủ
            if (User.Identity.IsAuthenticated)
            {
                var role = ((ClaimsIdentity)User.Identity).FindFirstValue(ClaimTypes.Role);
                if (role == "Recruiter") return RedirectToAction("Index", "Home"); // Hoặc trang Dashboard
            }
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        // Class hỗ trợ Challenge (Bắt buộc phải có để ExternalLogin hoạt động)
        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            private const string XsrfKey = "XsrfId";

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}
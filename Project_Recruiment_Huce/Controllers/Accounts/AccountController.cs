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
    public class AccountController : Controller
    {
        // Khai báo Service để dùng chung
        private readonly IAccountService _accountService;

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
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            using (var db = DbContextFactory.Create())
            {
                var input = (model.EmailOrUsername ?? string.Empty).Trim();
                bool isEmail = input.Contains("@");
                var lower = input.ToLower();

                var account = db.Accounts.FirstOrDefault(a =>
                    (isEmail ? a.Email.ToLower() == lower : a.Username == input) && a.ActiveFlag == 1);

                if (account == null || !PasswordHelper.VerifyPassword(model.Password, account.PasswordHash))
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                // Gọi hàm đăng nhập dùng chung
                SignInUser(account, model.RememberMe);

                return RedirectToLocal(returnUrl);
            }
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            if (User?.Identity?.IsAuthenticated == true) return RedirectToLocal(null);
            return View();
        }

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
                if (account != null)
                {
                    var token = PasswordResetHelper.CreateResetToken(db, account.AccountID, account.Email);
                    bool emailSent = PasswordResetHelper.SendResetCodeEmail(account.Email, token.ResetCode, account.Username);
                    if (emailSent)
                    {
                        Session["ResetPasswordEmail"] = account.Email;
                        TempData["SuccessMessage"] = $"Mã xác thực đã gửi đến {account.Email}.";
                        return RedirectToAction("ResetPassword");
                    }
                }
                TempData["SuccessMessage"] = "Nếu email tồn tại, mã xác thực đã được gửi.";
                return RedirectToAction("Login");
            }
        }

        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
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
            AuthenticationManager.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return RedirectToAction("Index", "Home");
        }

        // ============================================================
        // 2. PHẦN BỔ SUNG: GOOGLE AUTHENTICATION
        // ============================================================

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Gọi Google thông qua OWIN
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            // 1. Lấy thông tin từ Google
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // 2. Kiểm tra xem Email Google này đã có trong DB Account chưa?
            using (var db = DbContextFactory.Create())
            {
                var email = loginInfo.Email;
                var account = db.Accounts.FirstOrDefault(a => a.Email == email && a.ActiveFlag == 1);

                if (account != null)
                {
                    // CASE A: Tài khoản đã tồn tại -> Đăng nhập luôn
                    SignInUser(account, false);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    // CASE B: Chưa có tài khoản -> Chuyển sang trang xác nhận tạo mới
                    var nameClaim = loginInfo.ExternalIdentity.FindFirstValue(ClaimTypes.Name);
                    // Lấy avatar
                    var avatarUrl = loginInfo.ExternalIdentity.FindFirstValue("picture")
                                 ?? loginInfo.ExternalIdentity.FindFirstValue("urn:google:picture")
                                 ?? "";

                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;

                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel
                    {
                        Email = email,
                        FullName = nameClaim ?? email,
                        AvatarUrl = avatarUrl,
                        UserType = 1 // Mặc định là Ứng viên
                    });
                }
            }
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]

        public ActionResult ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                // 1. Tạo tài khoản Account mới thủ công (Vì bạn không dùng UserManager)
                using (var db = DbContextFactory.Create())
                {
                    // Kiểm tra trùng email lần cuối
                    if (db.Accounts.Any(a => a.Email == model.Email))
                    {
                        ModelState.AddModelError("", "Email này đã tồn tại trong hệ thống.");
                        return View(model);
                    }

                    var newAccount = new Account
                    {
                        Username = model.Email, // Dùng email làm username cho Google
                        Email = model.Email,
                        PasswordHash = "", // Không có pass vì dùng Google
                        ActiveFlag = 1,
                        CreatedAt = DateTime.Now,
                        Role = model.UserType == 2 ? "Recruiter" : "Candidate"
                    };

                    db.Accounts.InsertOnSubmit(newAccount);
                    db.SubmitChanges(); // Lưu để lấy AccountID

                    // 2. Gọi Service để tạo Profile (Candidate/Recruiter)
                    try
                    {
                        _accountService.CreateGoogleProfile(
                            email: model.Email,
                            fullName: model.FullName,
                            avatarUrl: model.AvatarUrl,
                            userType: model.UserType,
                            userId: newAccount.AccountID // ID dạng int vừa sinh ra
                        );

                        // 3. Đăng nhập và chuyển hướng
                        SignInUser(newAccount, false);
                        return RedirectToLocal(returnUrl);
                    }
                    catch (Exception ex)
                    {
                        // Nếu lỗi tạo profile thì xóa account để tránh rác
                        db.Accounts.DeleteOnSubmit(newAccount);
                        db.SubmitChanges();
                        ModelState.AddModelError("", "Lỗi tạo hồ sơ: " + ex.Message);
                    }
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
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

            var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationType
             );
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = rememberMe }, identity);
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
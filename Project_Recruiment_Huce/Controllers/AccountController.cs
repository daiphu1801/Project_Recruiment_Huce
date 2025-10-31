using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Controllers
{
    public class AccountController : Controller
    {
        // Keep ASP.NET Identity for backward compatibility if needed
        // But we'll use custom authentication with TaiKhoan table

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

            using (var db = new JOBPROTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Normalize input (allow username OR email)
                var input = (model.EmailOrUsername ?? string.Empty).Trim();
                bool isEmail = input.Contains("@");
                var lower = input.ToLower();
                var user = db.Accounts.FirstOrDefault(a =>
                    (isEmail ? a.Email.ToLower() == lower : a.Username == input) && a.ActiveFlag == 1);

                if (user != null && PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
                {
                    // Create claims identity for OWIN authentication
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.AccountId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("VaiTro", user.Role),
                    new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Local")
                };

                    var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
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

        // ASP.NET Identity methods commented out - using custom authentication with TaiKhoan table
        /*
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            return View("Error");
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            return View("Error");
        }
        */

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
                using (var db = new JOBPROTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
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

                    // Extra server-side password complexity validation
                    var password = model.Password ?? string.Empty;
                    bool hasLower = password.Any(char.IsLower);
                    bool hasUpper = password.Any(char.IsUpper);
                    bool hasDigit = password.Any(char.IsDigit);
                    if (password.Length < 6 || !hasLower || !hasUpper || !hasDigit)
                    {
                        ModelState.AddModelError("Password", "Mật khẩu phải tối thiểu 6 ký tự gồm chữ hoa, chữ thường và số.");
                        return View(model);
                    }

                    // Validate VaiTro -> Role mapping to new schema (hide Admin from public registration)
                    var validVaiTro = new[] { "CongTy", "NhaTuyenDung", "NguoiUngTuyen" };
                    if (!validVaiTro.Contains(model.VaiTro))
                    {
                        ModelState.AddModelError("VaiTro", "Vai trò không hợp lệ.");
                        return View(model);
                    }

                    // Map roles from old labels to new schema values (no C# 8 switch for compatibility)
                    string mappedRole;
                    if (model.VaiTro == "CongTy")
                    {
                        mappedRole = "Company";
                    }
                    else if (model.VaiTro == "NhaTuyenDung")
                    {
                        mappedRole = "Recruiter";
                    }
                    else if (model.VaiTro == "NguoiUngTuyen")
                    {
                        mappedRole = "Candidate";
                    }
                    else
                    {
                        mappedRole = "Candidate";
                    }

                    // Create new Account
                    var newAccount = new Account
                    {
                        Username = model.TenDangNhap,
                        Email = email,
                        Phone = model.SoDienThoai,
                        Role = mappedRole,
                        PasswordHash = PasswordHelper.HashPassword(model.Password),
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };

                    db.Accounts.InsertOnSubmit(newAccount);
                    db.SubmitChanges();

                    // Auto login after registration
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, newAccount.AccountId.ToString()),
                    new Claim(ClaimTypes.Name, newAccount.Username),
                    new Claim(ClaimTypes.Email, newAccount.Email),
                    new Claim("VaiTro", newAccount.Role),
                    new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Local")
                };

                    var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                    AuthenticationManager.SignIn(identity);

                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }

        // ASP.NET Identity methods commented out - using custom authentication
        /*
        // Methods for password reset, email confirmation, external login, etc. 
        // Can be implemented later if needed using TaiKhoan table
        */

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/LogOff (for easy testing)
        [AllowAnonymous]
        public ActionResult LogOffGet()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [Authorize]
        public ActionResult Manage()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        public const string XsrfKey = "XsrfId";

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

        public class ChallengeResult : HttpUnauthorizedResult
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
        #endregion
    }
}
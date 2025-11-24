using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Microsoft.Owin.Security;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Services;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Repositories;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller xử lý đăng nhập, đăng ký, quên mật khẩu và authentication
    /// Sử dụng OWIN Cookie Authentication với Claims-based identity
    /// </summary>
    public class AccountController : Controller
    {
        // validation được chuyển vào AccountValidationService
        // (nếu cần, có thể inject AccountValidationService thay vì new'ing)

        /// <summary>
        /// Hiển thị trang đăng nhập
        /// GET: /Account/Login
        /// </summary>
        /// <param name="returnUrl">URL để redirect sau khi đăng nhập thành công</param>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // Nếu đã đăng nhập, hiển thị thông báo và redirect về trang phù hợp
            if (User?.Identity?.IsAuthenticated == true)
            {
                TempData["InfoMessage"] = "Bạn đã đăng nhập rồi.";
                // Redirect tới dashboard recruiter nếu là recruiter, ngược lại về home
                var roleClaim = ((System.Security.Claims.ClaimsIdentity)User.Identity).FindFirst("VaiTro");
                if (roleClaim != null && roleClaim.Value == "Recruiter")
                {
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("Index", "Home");
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
        public ActionResult Login(LoginViewModel model, string returnUrl)
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

                if (account == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                // Sử dụng VerifyPasswordV2 - hỗ trợ cả format cũ (SHA256) và mới (PBKDF2)
                var verifyResult = PasswordHelper.VerifyPasswordV2(model.Password, account.PasswordHash, account.Salt);
                
                if (verifyResult == PasswordHelper.VerifyResult.Failed)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                // Tự động upgrade password sang format mới nếu đang dùng format cũ
                if (verifyResult == PasswordHelper.VerifyResult.SuccessRehashNeeded)
                {
                    account.PasswordHash = PasswordHelper.HashPassword(model.Password);
                    account.Salt = null; // Không cần salt riêng nữa
                    db.SubmitChanges();
                }

                // Tạo claims và đăng nhập
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, account.AccountID.ToString()),
                    new Claim(ClaimTypes.Name, account.Username),
                    new Claim(ClaimTypes.Email, account.Email),
                    new Claim("VaiTro", account.Role),
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
            if (User?.Identity?.IsAuthenticated == true)
            {
                TempData["InfoMessage"] = "Bạn đã đăng nhập rồi. Nếu muốn tạo tài khoản khác, vui lòng đăng xuất trước.";
                var roleClaim = ((System.Security.Claims.ClaimsIdentity)User.Identity).FindFirst("VaiTro");
                if (roleClaim != null && roleClaim.Value == "Recruiter")
                {
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("Index", "Home");
            }

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
            // Chặn register attempts khi đã authenticated
            if (User?.Identity?.IsAuthenticated == true)
            {
                if (Request.IsAjaxRequest())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Already authenticated");
                }
                TempData["InfoMessage"] = "Bạn đã đăng nhập rồi. Nếu muốn tạo tài khoản khác, vui lòng đăng xuất trước.";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                using (var db = DbContextFactory.Create())
                {
                    var repo = new Repositories.AccountRepository(db);
                    var svc = new Project_Recruiment_Huce.Services.AccountService(repo);

                    var res = svc.Register(model);
                    if (!res.IsValid)
                    {
                        foreach (var err in res.Errors)
                        {
                            ModelState.AddModelError(err.Key, err.Value);
                        }
                        return View(model);
                    }

                    var account = res.Data.ContainsKey("Account") ? res.Data["Account"] as Account : null;
                    if (account == null)
                    {
                        ModelState.AddModelError("", "Không thể tạo tài khoản, vui lòng thử lại sau.");
                        return View(model);
                    }

                    // Auto login after registration (User Cookie)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, account.AccountID.ToString()),
                        new Claim(ClaimTypes.Name, account.Username),
                        new Claim(ClaimTypes.Email, account.Email),
                        new Claim("VaiTro", account.Role),
                        new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Local")
                    };

                    var identity = new ClaimsIdentity(claims, "UserCookie");
                    AuthenticationManager.SignIn(identity);

                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }

        /// <summary>
        /// Hiển thị trang quên mật khẩu
        /// GET: /Account/ForgotPassword
        /// </summary>
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Xử lý yêu cầu quên mật khẩu - gửi mã reset qua email
        /// POST: /Account/ForgotPassword
        /// </summary>
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

        /// <summary>
        /// Hiển thị trang đặt lại mật khẩu với mã xác thực
        /// GET: /Account/ResetPassword
        /// </summary>
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

        /// <summary>
        /// Xử lý đặt lại mật khẩu với mã xác thực
        /// POST: /Account/ResetPassword
        /// Validate: reset code, password complexity, attempts count
        /// </summary>
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
                var repo = new Repositories.AccountRepository(db);
                var svc = new Project_Recruiment_Huce.Services.AccountService(repo);

                var res = svc.ResetPassword(model);
                if (!res.IsValid)
                {
                    foreach (var err in res.Errors)
                    {
                        ModelState.AddModelError(err.Key, err.Value);
                    }
                    return View(model);
                }

                // remove reset email from session if present
                Session.Remove("ResetPasswordEmail");

                TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập với mật khẩu mới.";
                return RedirectToAction("Login");
            }
        }


        /// <summary>
        /// Xử lý đăng xuất
        /// POST: /Account/LogOff
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut("UserCookie");
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Đăng xuất qua GET (cho testing thuận tiện)
        /// GET: /Account/LogOff
        /// </summary>
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
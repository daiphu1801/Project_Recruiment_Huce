using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Project_Recruiment_Huce.DbContext;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class AuthController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("loginAd", new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View("loginAd", model);
            }

            using (var db = new RecruitmentDbContext())
            {
                var input = (model.EmailOrUsername ?? string.Empty).Trim();
                bool isEmail = input.Contains("@");
                var lower = input.ToLower();
                var user = db.Accounts.FirstOrDefault(a =>
                    (isEmail ? a.Email.ToLower() == lower : a.Username == input) && a.ActiveFlag == 1);

                if (user == null || user.Role != "Admin")
                {
                    ModelState.AddModelError("", "Tài khoản không có quyền quản trị.");
                    return View("loginAd", model);
                }

                if (!PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    return View("loginAd", model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.AccountId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim("VaiTro", user.Role),
                    new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Local")
                };

                var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                AuthenticationManager.SignIn(new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                }, identity);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View("registerAd", new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("registerAd", model);
            }

            using (var db = new RecruitmentDbContext())
            {
                if (db.Accounts.Any(a => a.Username == model.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                    return View("registerAd", model);
                }
                var email = (model.Email ?? string.Empty).Trim();
                if (db.Accounts.Any(a => a.Email.ToLower() == email.ToLower()))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
                    return View("registerAd", model);
                }

                var password = model.Password ?? string.Empty;
                bool hasLower = password.Any(char.IsLower);
                bool hasUpper = password.Any(char.IsUpper);
                bool hasDigit = password.Any(char.IsDigit);
                if (password.Length < 6 || !hasLower || !hasUpper || !hasDigit)
                {
                    ModelState.AddModelError("Password", "Mật khẩu phải tối thiểu 6 ký tự gồm chữ hoa, chữ thường và số.");
                    return View("registerAd", model);
                }

                var newAccount = new Account
                {
                    Username = model.TenDangNhap,
                    Email = email,
                    Phone = model.SoDienThoai,
                    Role = "Admin",
                    PasswordHash = PasswordHelper.HashPassword(model.Password),
                    CreatedAt = DateTime.Now,
                    ActiveFlag = 1
                };
                db.Accounts.Add(newAccount);
                db.SaveChanges();

                // create Admin profile
                db.Admins.Add(new Project_Recruiment_Huce.Models.Admin
                {
                    AccountId = newAccount.AccountId,
                    FullName = model.TenDangNhap,
                    ContactEmail = email,
                    CreatedAt = DateTime.Now,
                    Permission = "Super"
                });
                db.SaveChanges();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, newAccount.AccountId.ToString()),
                    new Claim(ClaimTypes.Name, newAccount.Username),
                    new Claim(ClaimTypes.Email, newAccount.Email ?? string.Empty),
                    new Claim("VaiTro", newAccount.Role),
                    new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Local")
                };
                var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                AuthenticationManager.SignIn(identity);

                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", new { area = "Admin" });
        }

        [AllowAnonymous]
        public ActionResult LogOffGet()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", new { area = "Admin" });
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }
    }
}



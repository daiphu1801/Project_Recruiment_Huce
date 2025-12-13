using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Configuration;
using System.Globalization;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    [AllowAnonymous] // Cho phép truy cập AuthController mà không cần đăng nhập
    public class AuthController : Controller
    {
        //
        // GET: /Admin/Auth/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("loginAd", new LoginViewModel());
        }

        //
        // POST: /Admin/Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            System.Diagnostics.Debug.WriteLine($"Login attempt - Username: {model.EmailOrUsername}");
            if (!ModelState.IsValid)
            {
                return View("loginAd", model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Normalize input (allow username OR email)
                var input = (model.EmailOrUsername ?? string.Empty).Trim();
                bool isEmail = input.Contains("@");
                var lower = input.ToLower();
                var user = db.Accounts.FirstOrDefault(a =>
                    (isEmail ? a.Email.ToLower() == lower : a.Username == input) && a.ActiveFlag == 1);

                // Kiểm tra quyền Admin
                if (user == null || user.Role != "Admin")
                {
                    ModelState.AddModelError("", "Tài khoản không có quyền quản trị.");
                    return View("loginAd", model);
                }

                // Xác thực mật khẩu sử dụng PBKDF2
                if (!PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    return View("loginAd", model);
                }

                // Create claims identity for OWIN authentication (Admin Cookie)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.AccountID.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Local")
                };

                var identity = new ClaimsIdentity(claims, "AdminCookie");
                AuthenticationManager.SignIn(new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                }, identity);
                System.Diagnostics.Debug.WriteLine($"User authenticated: {User.Identity.IsAuthenticated}");
                System.Diagnostics.Debug.WriteLine($"Redirecting to: {returnUrl ?? "/Admin/Dashboard/Index"}");
                return RedirectToLocal(returnUrl);
            }
        }

        //
        // GET: /Admin/Auth/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View("registerAd", new RegisterViewModel());
        }

        //
        // POST: /Admin/Auth/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            // Đảm bảo model không null
            if (model == null)
            {
                model = new RegisterViewModel();
            }
            
            // Set VaiTro = "Admin" mặc định cho admin area
            model.VaiTro = "Admin";
            
            // Bỏ qua hoàn toàn validation cho VaiTro trong admin area
            // Clear errors và set Value để ModelState.IsValid pass
            ModelState.Remove("VaiTro");
            var vaiTroState = new ModelState();
            vaiTroState.Value = new ValueProviderResult("Admin", "Admin", CultureInfo.CurrentCulture);
            ModelState.Add("VaiTro", vaiTroState);
            
            // Kiểm tra ModelState sau khi đã fix VaiTro
            if (!ModelState.IsValid)
            {
                // Log các lỗi để debug
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}");
                System.Diagnostics.Debug.WriteLine("ModelState Errors: " + string.Join("; ", errors));
                return View("registerAd", model);
            }

            try
            {
                using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    // Check if Username already exists
                    if (db.Accounts.Any(a => a.Username == model.TenDangNhap))
                    {
                        ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                        return View("registerAd", model);
                    }

                    // Check if Email already exists
                    var email = (model.Email ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        ModelState.AddModelError("Email", "Email không được để trống.");
                        return View("registerAd", model);
                    }
                    
                    if (db.Accounts.Any(a => a.Email.ToLower() == email.ToLower()))
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng.");
                        return View("registerAd", model);
                    }

                    // Validate phone number format and uniqueness (if provided)
                    var phone = (model.SoDienThoai ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(phone))
                    {
                        // Validate phone format
                        if (!ValidationHelper.IsValidVietnamesePhone(phone))
                        {
                            ModelState.AddModelError("SoDienThoai", ValidationHelper.GetPhoneErrorMessage());
                            return View("registerAd", model);
                        }

                        // Normalize phone number
                        phone = ValidationHelper.NormalizePhone(phone);

                        // Check if phone already exists
                        if (!ValidationHelper.IsAccountPhoneUnique(phone))
                        {
                            ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã được sử dụng. Mỗi số điện thoại chỉ có thể đăng ký một tài khoản.");
                            return View("registerAd", model);
                        }
                    }
                    else
                    {
                        phone = null; // Set to null if empty
                    }

                    // Extra server-side password complexity validation
                    var password = model.Password ?? string.Empty;
                    bool hasLower = password.Any(char.IsLower);
                    bool hasUpper = password.Any(char.IsUpper);
                    bool hasDigit = password.Any(char.IsDigit);
                    if (password.Length < 6 || !hasLower || !hasUpper || !hasDigit)
                    {
                        ModelState.AddModelError("Password", "Mật khẩu phải tối thiểu 6 ký tự gồm chữ hoa, chữ thường và số.");
                        return View("registerAd", model);
                    }

                    // Hash password sử dụng PBKDF2 (không cần salt riêng)
                    string passwordHash = PasswordHelper.HashPassword(model.Password);

                    // Create new Admin Account
                    // FORCE Role = "Admin" - không phụ thuộc vào model.VaiTro
                    var newAccount = new Account
                    {
                        Username = model.TenDangNhap,
                        Email = email,
                        Phone = phone, // Use normalized phone
                        Role = "Admin", // HARDCODE: Luôn là Admin cho admin area
                        PasswordHash = passwordHash,
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };

                    // Đảm bảo Role không bị thay đổi
                    System.Diagnostics.Debug.WriteLine($"Creating account with Role: {newAccount.Role}");
                    
                    db.Accounts.InsertOnSubmit(newAccount);
                    db.SubmitChanges();
                    
                    // Refresh để lấy giá trị từ database
                    db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, newAccount);
                    
                    // Verify Role đã được lưu đúng trong database
                    var savedAccount = db.Accounts.FirstOrDefault(a => a.AccountID == newAccount.AccountID);
                    if (savedAccount == null || savedAccount.Role != "Admin")
                    {
                        var actualRole = savedAccount != null ? savedAccount.Role : "NULL";
                        throw new Exception($"Role không được lưu đúng trong database. Expected: Admin, Actual: {actualRole}");
                    }

                    // Create Admin profile
                    db.Admins.InsertOnSubmit(new Project_Recruiment_Huce.Models.Admin
                    {
                        AccountID = newAccount.AccountID,
                        FullName = model.TenDangNhap,
                        ContactEmail = email,
                        CreatedAt = DateTime.Now,
                        Permission = "Super"
                    });
                    db.SubmitChanges();

                    // Auto login after registration (Admin Cookie)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, newAccount.AccountID.ToString()),
                        new Claim(ClaimTypes.Name, newAccount.Username),
                        new Claim(ClaimTypes.Email, newAccount.Email ?? string.Empty),
                        new Claim(ClaimTypes.Role, newAccount.Role),
                        new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Local")
                    };
                    var identity = new ClaimsIdentity(claims, "AdminCookie");
                    AuthenticationManager.SignIn(identity);

                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
            }
            catch (Exception ex)
            {
                // Log exception và hiển thị lỗi cho user
                ModelState.AddModelError("", "Đã có lỗi xảy ra khi đăng ký. Vui lòng thử lại sau. Chi tiết: " + ex.Message);
                return View("registerAd", model);
            }
        }

        //
        // POST: /Admin/Auth/LogOff
        [HttpPost]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut("AdminCookie");
            return RedirectToAction("Login", "Auth", new { area = "Admin" });
        }

        //
        // GET: /Admin/Auth/LogOff (for easy testing)
        [AllowAnonymous]
        public ActionResult LogOffGet()
        {
            AuthenticationManager.SignOut("AdminCookie");
            return RedirectToAction("Login", "Auth", new { area = "Admin" });
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
            if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }
        #endregion
    }
}



using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Controllers.MyAccount
{
    [Authorize]
    public class MyAccountController : Controller
    {
        private int? GetCurrentAccountId()
        {
            var idClaim = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) return null;
            int accountId;
            return int.TryParse(idClaim.Value, out accountId) ? (int?)accountId : null;
        }

        // GET: MyAccount
        public ActionResult Index()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == accountId.Value);
                if (account == null) return HttpNotFound();

                // Lấy email liên lạc từ profile
                string contactEmail = null;
                if (account.Role == "Candidate")
                {
                    var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                    contactEmail = candidate?.Email;
                }
                else if (account.Role == "Recruiter")
                {
                    var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                    contactEmail = recruiter?.CompanyEmail;
                }

                var vm = new MyAccountViewModel
                {
                    AccountId = account.AccountID,
                    Username = account.Username,
                    LoginEmail = account.Email, // Email đăng nhập (read-only)
                    ContactEmail = contactEmail, // Email liên lạc từ profile
                    Phone = account.Phone,
                    Role = account.Role,
                    CreatedAt = account.CreatedAt
                };

                return View(vm);
            }
        }

        // POST: MyAccount/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload account info
                var accountId = GetCurrentAccountId();
                if (accountId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    var account = db.Accounts.FirstOrDefault(a => a.AccountID == accountId.Value);
                    if (account == null) return HttpNotFound();

                    string contactEmail = null;
                    if (account.Role == "Candidate")
                    {
                        var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                        contactEmail = candidate?.Email;
                    }
                    else if (account.Role == "Recruiter")
                    {
                        var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                        contactEmail = recruiter?.CompanyEmail;
                    }

                    var vm = new MyAccountViewModel
                    {
                        AccountId = account.AccountID,
                        Username = account.Username,
                        LoginEmail = account.Email,
                        ContactEmail = contactEmail,
                        Phone = account.Phone,
                        Role = account.Role,
                        CreatedAt = account.CreatedAt
                    };

                    ViewBag.ChangePasswordModel = model;
                    return View("Index", vm);
                }
            }

            var currentAccountId = GetCurrentAccountId();
            if (currentAccountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountID == currentAccountId.Value);
                if (account == null) return HttpNotFound();

                // Verify current password
                bool isPasswordValid = string.IsNullOrEmpty(account.Salt)
                    ? PasswordHelper.VerifyPassword(model.CurrentPassword, account.PasswordHash)
                    : PasswordHelper.VerifyPassword(model.CurrentPassword, account.PasswordHash, account.Salt);

                if (!isPasswordValid)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                    
                    // Reload account info
                    string contactEmail = null;
                    if (account.Role == "Candidate")
                    {
                        var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == currentAccountId.Value);
                        contactEmail = candidate?.Email;
                    }
                    else if (account.Role == "Recruiter")
                    {
                        var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == currentAccountId.Value);
                        contactEmail = recruiter?.CompanyEmail;
                    }

                    var vm = new MyAccountViewModel
                    {
                        AccountId = account.AccountID,
                        Username = account.Username,
                        LoginEmail = account.Email,
                        ContactEmail = contactEmail,
                        Phone = account.Phone,
                        Role = account.Role,
                        CreatedAt = account.CreatedAt
                    };

                    ViewBag.ChangePasswordModel = model;
                    return View("Index", vm);
                }

                // Validate new password complexity
                var newPassword = model.NewPassword ?? string.Empty;
                bool hasLower = newPassword.Any(char.IsLower);
                bool hasUpper = newPassword.Any(char.IsUpper);
                bool hasDigit = newPassword.Any(char.IsDigit);
                if (newPassword.Length < 6 || !hasLower || !hasUpper || !hasDigit)
                {
                    ModelState.AddModelError("NewPassword", "Mật khẩu mới phải tối thiểu 6 ký tự gồm chữ hoa, chữ thường và số.");
                    
                    // Reload account info
                    string contactEmail = null;
                    if (account.Role == "Candidate")
                    {
                        var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == currentAccountId.Value);
                        contactEmail = candidate?.Email;
                    }
                    else if (account.Role == "Recruiter")
                    {
                        var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == currentAccountId.Value);
                        contactEmail = recruiter?.CompanyEmail;
                    }

                    var vm = new MyAccountViewModel
                    {
                        AccountId = account.AccountID,
                        Username = account.Username,
                        LoginEmail = account.Email,
                        ContactEmail = contactEmail,
                        Phone = account.Phone,
                        Role = account.Role,
                        CreatedAt = account.CreatedAt
                    };

                    ViewBag.ChangePasswordModel = model;
                    return View("Index", vm);
                }

                // Update password
                string salt = PasswordHelper.GenerateSalt();
                string passwordHash = PasswordHelper.HashPassword(model.NewPassword, salt);
                account.PasswordHash = passwordHash;
                account.Salt = salt;

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }
        }
    }

    public class MyAccountViewModel
    {
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string LoginEmail { get; set; } // Email đăng nhập (read-only)
        public string ContactEmail { get; set; } // Email liên lạc từ profile
        public string Phone { get; set; }
        public string Role { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string CurrentPassword { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [System.ComponentModel.DataAnnotations.StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string NewPassword { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}


using System;
using System.Web.Mvc;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Services;
using Project_Recruiment_Huce.Repositories;

namespace Project_Recruiment_Huce.Controllers.MyAccount
{
    /// <summary>
    /// Controller quản lý tài khoản cá nhân - xem thông tin, đổi mật khẩu
    /// Sử dụng MyAccountService cho business logic
    /// </summary>
    [Authorize]
    public class MyAccountController : BaseController
    {
        /// <summary>
        /// Tạo MyAccountService instance với database context
        /// </summary>
        private MyAccountService GetMyAccountService(JOBPORTAL_ENDataContext db)
        {
            var repo = new MyAccountRepository(db);
            return new MyAccountService(repo);
        }

        /// <summary>
        /// Hiển thị trang thông tin tài khoản
        /// GET: MyAccount/Index
        /// </summary>
        public ActionResult Index()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
                return RedirectToAction("Login", "Account");

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var myAccountService = GetMyAccountService(db);
                var vm = myAccountService.GetAccountInfo(accountId.Value);
                if (vm == null) return HttpNotFound();
                return View(vm);
            }
        }

        /// <summary>
        /// Xử lý đổi mật khẩu
        /// POST: MyAccount/ChangePassword
        /// Validate: current password, new password strength, confirmation match
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var currentAccountId = GetCurrentAccountId();
            if (currentAccountId == null)
                return RedirectToAction("Login", "Account");

            Func<ActionResult> returnViewWithErrors = () =>
            {
                using (var db = DbContextFactory.Create())
                {
                    var myAccountService = GetMyAccountService(db);
                    var vm = myAccountService.GetAccountInfo(currentAccountId.Value);
                    if (vm == null) return HttpNotFound();
                    ViewBag.ChangePasswordModel = model;
                    return View("Index", vm);
                }
            };

            // Sử dụng Service layer cho validation
            using (var db = DbContextFactory.Create())
            {
                var myAccountService = GetMyAccountService(db);
                var validationResult = myAccountService.ValidateChangePassword(model, currentAccountId.Value);

                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }
                    return returnViewWithErrors();
                }

                // Tất cả validation passed, cập nhật mật khẩu
                myAccountService.ChangePassword(currentAccountId.Value, model.NewPassword);

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }
        }
    }
}

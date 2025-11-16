using System;
using System.Web.Mvc;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Services;
using Project_Recruiment_Huce.Repositories;

namespace Project_Recruiment_Huce.Controllers.MyAccount
{
    [Authorize]
    public class MyAccountController : BaseController
    {
        private MyAccountService GetMyAccountService(JOBPORTAL_ENDataContext db)
        {
            var accountRepository = new AccountRepository(db);
            return new MyAccountService(accountRepository, db);
        }

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

            // Use Service layer for validation
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

                // All validations passed, update password
                myAccountService.ChangePassword(currentAccountId.Value, model.NewPassword);

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }
        }
    }
}

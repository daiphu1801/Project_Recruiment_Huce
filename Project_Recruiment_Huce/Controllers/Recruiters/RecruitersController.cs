using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Repositories.RecruiterRepo;
using Project_Recruiment_Huce.Services.RecruiterService;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller quản lý hồ sơ recruiter - thông tin cá nhân, vị trí, company, avatar
    /// Chỉ dành cho người dùng đã đăng nhập với vai trò Recruiter
    /// </summary>
    [Authorize]
    public class RecruitersController : BaseController
    {
        private readonly IRecruiterService _recruiterService;
        private readonly IRecruiterRepository _repository;

        public RecruitersController()
        {
            var db = DbContextFactory.Create();
            _repository = new RecruiterRepository(db);
            _recruiterService = new RecruiterService(_repository);
        }
        /// <summary>
        /// Hiển thị trang quản lý hồ sơ recruiter
        /// GET: Recruiters/RecruitersManage
        /// Tự động tạo profile nếu chưa có
        /// </summary>
        [HttpGet]
        public ActionResult RecruitersManage()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null) 
                return RedirectToAction("Login", "Account");

            // Get or create profile through service
            var result = _recruiterService.GetOrCreateProfile(accountId.Value, User.Identity.Name);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Recruiter);
        }

        /// <summary>
        /// Cập nhật hồ sơ recruiter và upload avatar
        /// POST: Recruiters/RecruitersManage
        /// Validate: phone format/uniqueness, company email format/uniqueness
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecruitersManage(Recruiter recruiter, HttpPostedFileBase avatar)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Update profile through service
            var result = _recruiterService.UpdateProfile(
                accountId.Value, 
                recruiter, 
                avatar, 
                path => Server.MapPath(path));

            if (!result.Success)
            {
                // Add errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }

                TempData["ErrorMessage"] = result.ErrorMessage;
                
                // Reload profile for view
                var profileResult = _recruiterService.GetOrCreateProfile(accountId.Value, User.Identity.Name);
                return View(profileResult.Recruiter);
            }

            TempData["SuccessMessage"] = result.SuccessMessage;
            return RedirectToAction("RecruitersManage");
        }
    }
}


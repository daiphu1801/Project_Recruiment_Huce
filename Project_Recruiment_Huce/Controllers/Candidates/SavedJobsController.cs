using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Configuration;
using System.Collections.Generic;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Candidates;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Mappers;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Services.SavedJobService;
using Project_Recruiment_Huce.Repositories.SavedJobRepo;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class SavedJobsController : BaseController
    {
        private readonly ISavedJobService _savedJobService;

        public SavedJobsController()
        {
            var repository = new SavedJobRepository(readOnly: false);
            _savedJobService = new SavedJobService(repository);
        }

        /// <summary>
        /// GET: Candidates/SavedJobs/MySavedJobs
        /// Hiển thị danh sách công việc đã lưu với search, filter, sort
        /// </summary>
        [HttpGet]
        public ActionResult MySavedJobs(string keyword = null, string location = null, string sortBy = "savedDate", int? page = null)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;

            var result = _savedJobService.GetSavedJobs(accountId.Value, keyword, location, sortBy, pageNumber, pageSize);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("CandidatesManage", "Candidates");
            }

            ViewBag.Keyword = result.Keyword;
            ViewBag.Location = result.Location;
            ViewBag.SortBy = result.SortBy;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.Locations = result.Locations;

            return View(result.SavedJobs);
        }

        /// <summary>
        /// POST: Candidates/SavedJobs/SaveJob
        /// Lưu công việc
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveJob(int? jobPostId)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để lưu công việc." });
            }

            if (!jobPostId.HasValue)
            {
                return Json(new { success = false, message = "Không tìm thấy công việc." });
            }

            var result = _savedJobService.SaveJob(accountId.Value, jobPostId.Value);

            if (result.IsValid)
            {
                return Json(new { success = true, message = "Đã lưu công việc thành công!" });
            }
            else
            {
                var errorMessage = result.Errors.ContainsKey("General") ? result.Errors["General"] : "Lỗi khi lưu công việc.";
                return Json(new { success = false, message = errorMessage });
            }
        }

        /// <summary>
        /// POST: Candidates/SavedJobs/UnsaveJob
        /// Bỏ lưu công việc
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnsaveJob(int? jobPostId)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            if (!jobPostId.HasValue)
            {
                return Json(new { success = false, message = "Không tìm thấy công việc." });
            }

            var result = _savedJobService.UnsaveJob(accountId.Value, jobPostId.Value);

            if (result.IsValid)
            {
                return Json(new { success = true, message = "Đã bỏ lưu công việc thành công!" });
            }
            else
            {
                var errorMessage = result.Errors.ContainsKey("General") ? result.Errors["General"] : "Lỗi khi bỏ lưu công việc.";
                return Json(new { success = false, message = errorMessage });
            }
        }

        /// <summary>
        /// Check if a job is saved by current candidate
        /// </summary>
        [HttpGet]
        public ActionResult IsJobSaved(int? jobPostId)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null || !jobPostId.HasValue)
            {
                return Json(new { isSaved = false }, JsonRequestBehavior.AllowGet);
            }

            var isSaved = _savedJobService.IsJobSaved(accountId.Value, jobPostId.Value);
            return Json(new { isSaved = isSaved }, JsonRequestBehavior.AllowGet);
        }
    }
}


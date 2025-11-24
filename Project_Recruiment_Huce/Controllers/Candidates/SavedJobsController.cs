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

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class SavedJobsController : BaseController
    {

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

            using (var db = DbContextFactory.CreateReadOnly())
            {
                db.ObjectTrackingEnabled = false;
                JobStatusHelper.NormalizeStatuses(db);

                // Eager load related entities - MUST be set BEFORE any queries
                var loadOptions = new System.Data.Linq.DataLoadOptions();
                loadOptions.LoadWith<SavedJob>(sj => sj.JobPost);
                loadOptions.LoadWith<JobPost>(j => j.Company);
                db.LoadOptions = loadOptions;

                // Get candidate
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng hoàn thiện hồ sơ trước khi xem công việc đã lưu.";
                    return RedirectToAction("CandidatesManage", "Candidates");
                }

                // Get saved jobs - only get jobs that are still published (not closed/expired)
                var savedJobs = db.SavedJobs
                    .Where(sj => sj.CandidateID == candidate.CandidateID)
                    .ToList()
                    .Where(sj => sj.JobPost != null && 
                                JobStatusHelper.IsPublished(JobStatusHelper.NormalizeStatus(sj.JobPost.Status)))
                    .ToList();

                // Get distinct locations for filter dropdown (from already loaded data)
                var allLocations = savedJobs
                    .Where(sj => sj.JobPost != null && sj.JobPost.Location != null && !string.IsNullOrEmpty(sj.JobPost.Location))
                    .Select(sj => sj.JobPost.Location)
                    .Distinct()
                    .OrderBy(l => l)
                    .ToList();

                // Convert to enumerable for filtering
                var savedJobsEnumerable = savedJobs.AsEnumerable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    string keywordLower = keyword.ToLower();
                    savedJobsEnumerable = savedJobsEnumerable.Where(sj => 
                        (sj.JobPost.Title != null && sj.JobPost.Title.ToLower().Contains(keywordLower)) ||
                        (sj.JobPost.Company != null && sj.JobPost.Company.CompanyName != null && sj.JobPost.Company.CompanyName.ToLower().Contains(keywordLower)) ||
                        (sj.JobPost.Description != null && sj.JobPost.Description.ToLower().Contains(keywordLower))
                    );
                }

                // Apply location filter
                if (!string.IsNullOrWhiteSpace(location) && location != "Tất cả")
                {
                    string locationLower = location.ToLower();
                    savedJobsEnumerable = savedJobsEnumerable.Where(sj => 
                        sj.JobPost.Location != null && 
                        sj.JobPost.Location.ToLower().Contains(locationLower)
                    );
                }

                // Apply sorting
                switch (sortBy.ToLower())
                {
                    case "title":
                        savedJobsEnumerable = savedJobsEnumerable.OrderBy(sj => sj.JobPost.Title);
                        break;
                    case "company":
                        savedJobsEnumerable = savedJobsEnumerable.OrderBy(sj => sj.JobPost.Company != null ? sj.JobPost.Company.CompanyName : "");
                        break;
                    case "deadline":
                        savedJobsEnumerable = savedJobsEnumerable.OrderBy(sj => sj.JobPost.ApplicationDeadline ?? DateTime.MaxValue);
                        break;
                    case "savedDate":
                    default:
                        savedJobsEnumerable = savedJobsEnumerable.OrderByDescending(sj => sj.SavedAt);
                        break;
                }

                // Pagination
                int pageSize = 10;
                int pageNumber = page ?? 1;
                int totalItems = savedJobsEnumerable.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedJobs = savedJobsEnumerable
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Map to ViewModels
                var viewModels = pagedJobs.Select(sj => JobMapper.MapToSavedJob(sj)).ToList();

                ViewBag.Keyword = keyword;
                ViewBag.Location = location;
                ViewBag.SortBy = sortBy;
                ViewBag.TotalItems = totalItems;
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.Locations = allLocations;

                return View(viewModels);
            }
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

            using (var db = DbContextFactory.Create())
            {
                JobStatusHelper.NormalizeStatuses(db);
                // Get candidate
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    return Json(new { success = false, message = "Vui lòng hoàn thiện hồ sơ trước." });
                }

                // Check if job exists and is published
                var job = db.JobPosts.FirstOrDefault(j => j.JobPostID == jobPostId.Value);
                if (job == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy công việc." });
                }

                job.Status = JobStatusHelper.NormalizeStatus(job.Status);
                if (!JobStatusHelper.IsPublished(job.Status))
                {
                    return Json(new { success = false, message = "Công việc này không còn nhận ứng viên." });
                }

                // Check if already saved (unique constraint will prevent duplicate, but we check here for better UX)
                var existingSaved = db.SavedJobs
                    .FirstOrDefault(sj => sj.CandidateID == candidate.CandidateID && 
                                         sj.JobPostID == jobPostId.Value);
                
                if (existingSaved != null)
                {
                    return Json(new { success = false, message = "Bạn đã lưu công việc này rồi." });
                }

                try
                {
                    // Create new saved job
                    var savedJob = new SavedJob
                    {
                        CandidateID = candidate.CandidateID,
                        JobPostID = jobPostId.Value,
                        SavedAt = DateTime.Now
                    };

                    db.SavedJobs.InsertOnSubmit(savedJob);
                    db.SubmitChanges();

                    return Json(new { success = true, message = "Đã lưu công việc thành công!" });
                }
                catch (Exception ex)
                {
                    // Handle unique constraint violation
                    if (ex.Message.Contains("UNIQUE") || ex.Message.Contains("duplicate"))
                    {
                        return Json(new { success = false, message = "Bạn đã lưu công việc này rồi." });
                    }
                    return Json(new { success = false, message = "Lỗi khi lưu công việc: " + ex.Message });
                }
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

            using (var db = DbContextFactory.Create())
            {
                // Get candidate
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    return Json(new { success = false, message = "Vui lòng hoàn thiện hồ sơ trước." });
                }

                // Find saved job
                var savedJob = db.SavedJobs
                    .FirstOrDefault(sj => sj.CandidateID == candidate.CandidateID && 
                                         sj.JobPostID == jobPostId.Value);

                if (savedJob == null)
                {
                    return Json(new { success = false, message = "Bạn chưa lưu công việc này." });
                }

                try
                {
                    db.SavedJobs.DeleteOnSubmit(savedJob);
                    db.SubmitChanges();

                    return Json(new { success = true, message = "Đã bỏ lưu công việc thành công!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi khi bỏ lưu công việc: " + ex.Message });
                }
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

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                if (candidate == null)
                {
                    return Json(new { isSaved = false }, JsonRequestBehavior.AllowGet);
                }

                var isSaved = db.SavedJobs
                    .Any(sj => sj.CandidateID == candidate.CandidateID && 
                              sj.JobPostID == jobPostId.Value);

                return Json(new { isSaved = isSaved }, JsonRequestBehavior.AllowGet);
            }
        }

        // Removed duplicate mapping methods - now using JobMapper class
        // Removed duplicate GetEmploymentTypeDisplay - now using EmploymentTypeHelper
        // Removed duplicate FormatSalaryRange - now using SalaryHelper
    }
}


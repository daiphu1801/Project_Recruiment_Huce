using System;
using System.Linq;
using System.Web.Mvc;
using System.Security.Claims;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Recruiters;
using Project_Recruiment_Huce.Services;
using Project_Recruiment_Huce.Infrastructure;

namespace Project_Recruiment_Huce.Controllers.Recruiters
{
    /// <summary>
    /// Controller for recruiter analytics dashboard
    /// Handles viewing analytics and metrics for job posts
    /// Keeps thin by delegating business logic to RecruiterAnalyticsService
    /// </summary>
    [Authorize]
    public class RecruiterAnalyticsController : Controller
    {
        /// <summary>
        /// Analytics dashboard - summary and job breakdown
        /// GET: /RecruiterAnalytics/Index
        /// </summary>
        public ActionResult Index(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var recruiterId = GetCurrentRecruiterId();
            if (!recruiterId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhà tuyển dụng.";
                return RedirectToAction("Index", "Home");
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var service = new RecruiterAnalyticsService(db);
                var dashboardData = service.GetDashboardData(recruiterId.Value, fromDate, toDate);

                // Pass filter values to view
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                return View(dashboardData);
            }
        }

        /// <summary>
        /// Per-job analytics details
        /// GET: /RecruiterAnalytics/Job/123
        /// </summary>
        public ActionResult Job(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng.";
                return RedirectToAction("Index");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (!recruiterId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhà tuyển dụng.";
                return RedirectToAction("Index", "Home");
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var service = new RecruiterAnalyticsService(db);
                var jobAnalytics = service.GetJobAnalytics(id.Value, recruiterId.Value);

                if (jobAnalytics == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền xem.";
                    return RedirectToAction("Index");
                }

                return View(jobAnalytics);
            }
        }

        /// <summary>
        /// Helper: Get current logged-in recruiter ID from claims
        /// </summary>
        private int? GetCurrentRecruiterId()
        {
            if (!User.Identity.IsAuthenticated)
                return null;

            var identity = (ClaimsIdentity)User.Identity;
            
            // Check role
            var roleClaim = identity.FindFirst("VaiTro");
            if (roleClaim == null || roleClaim.Value != "Recruiter")
                return null;

            // Get AccountID
            var accountIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return null;

            // Get RecruiterID from Account
            using (var db = DbContextFactory.CreateReadOnly())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId);
                return recruiter?.RecruiterID;
            }
        }
    }
}

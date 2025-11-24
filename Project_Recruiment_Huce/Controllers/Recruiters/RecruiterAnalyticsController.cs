using System;
using System.Linq;
using System.Web.Mvc;
using System.Security.Claims;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Recruiters;
using Project_Recruiment_Huce.Repositories.RecruiterAnalyticsRepo;
using IRecruiterAnalyticsService = Project_Recruiment_Huce.Services.RecruiterAnalyticsService.IRecruiterAnalyticsService;
using NewRecruiterAnalyticsService = Project_Recruiment_Huce.Services.RecruiterAnalyticsService.RecruiterAnalyticsService;

namespace Project_Recruiment_Huce.Controllers.Recruiters
{
    /// <summary>
    /// Controller for recruiter analytics dashboard
    /// Handles viewing analytics and metrics for job posts
    /// Uses layered architecture with Repository and Service patterns
    /// </summary>
    [Authorize(Roles="Recruiter")]
    public class RecruiterAnalyticsController : Controller
    {
        private readonly IRecruiterAnalyticsService _analyticsService;
        private readonly IRecruiterAnalyticsRepository _repository;

        public RecruiterAnalyticsController()
        {
            _repository = new RecruiterAnalyticsRepository(readOnly: true);
            _analyticsService = new NewRecruiterAnalyticsService(_repository);
        }
        /// <summary>
        /// Analytics dashboard - summary and job breakdown
        /// GET: /RecruiterAnalytics/Index
        /// </summary>
        public ActionResult Index(DateTime? fromDate = null, DateTime? toDate = null, int page = 1)
        {
            var recruiterId = GetCurrentRecruiterId();
            if (!recruiterId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhà tuyển dụng.";
                return RedirectToAction("Index", "Home");
            }

            const int pageSize = 10;
            
            // Call service to get dashboard data
            var dashboardData = _analyticsService.GetDashboardData(recruiterId.Value, fromDate, toDate, page, pageSize);

            // Pass filter values and pagination to view
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(dashboardData);
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

            // Call service to get job analytics
            var result = _analyticsService.GetJobAnalytics(id.Value, recruiterId.Value);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Index");
            }

            return View(result.Analytics);
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

            // Get RecruiterID from repository
            return _repository.GetRecruiterIdByAccountId(accountId);
        }
    }
}

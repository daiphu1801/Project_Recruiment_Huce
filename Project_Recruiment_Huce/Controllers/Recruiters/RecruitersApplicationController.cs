using System;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Models.Recruiters;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Repositories.RecruiterApplicationRepo;
using Project_Recruiment_Huce.Services.RecruiterApplicationService;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller quản lý đơn ứng tuyển cho Recruiter
    /// Thin Controller - chỉ xử lý HTTP request/response
    /// </summary>
    [Authorize(Roles = "Recruiter")]
    public class RecruitersApplicationController : BaseController
    {
        private readonly IRecruiterApplicationService _applicationService;

        public RecruitersApplicationController()
        {
            var db = DbContextFactory.Create();
            var repository = new RecruiterApplicationRepository(db);
            _applicationService = new RecruiterApplicationService(repository);
        }

        #region Actions

        /// <summary>
        /// GET: RecruitersApplication/MyApplications
        /// </summary>
        [HttpGet]
        public ActionResult MyApplications(int? jobId, string status = null, int? page = null)
        {
            var recruiterId = GetCurrentRecruiterId();
            if (!recruiterId.HasValue) return RedirectToRecruiterSetup();

            var result = _applicationService.GetApplicationsList(recruiterId.Value, jobId, status, page ?? 1, 10);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Index", "Home");
            }

            var jobs = _applicationService.GetJobsForFilter(recruiterId.Value);

            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.JobId = result.JobId;
            ViewBag.Status = result.Status;
            ViewBag.Jobs = new SelectList(jobs, "JobPostID", "Title", jobId);
            ViewBag.StatusOptions = new SelectList(_applicationService.GetStatusOptions(), "Value", "Text", status);

            return View(result.Applications);
        }

        /// <summary>
        /// GET: RecruitersApplication/ApplicationDetails
        /// </summary>
        [HttpGet]
        public ActionResult ApplicationDetails(int? id)
        {
            if (!id.HasValue) return RedirectWithError("Không tìm thấy đơn ứng tuyển.", "MyApplications");

            var recruiterId = GetCurrentRecruiterId();
            if (!recruiterId.HasValue) return RedirectToRecruiterSetup();

            var result = _applicationService.GetApplicationDetails(id.Value, recruiterId.Value);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("MyApplications");
            }

            return View(result.Application);
        }

        /// <summary>
        /// GET: RecruitersApplication/UpdateApplicationStatus
        /// </summary>
        [HttpGet]
        public ActionResult UpdateApplicationStatus(int? id)
        {
            if (!id.HasValue) return RedirectWithError("Không tìm thấy đơn ứng tuyển.", "MyApplications");

            var recruiterId = GetCurrentRecruiterId();
            if (!recruiterId.HasValue) return RedirectToRecruiterSetup();

            var result = _applicationService.GetApplicationForStatusUpdate(id.Value, recruiterId.Value);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("MyApplications");
            }

            ViewBag.StatusOptions = new SelectList(
                _applicationService.GetStatusOptions().Skip(1), // Bỏ "Tất cả"
                "Value", "Text", result.ViewModel.Status);
            return View(result.ViewModel);
        }

        /// <summary>
        /// POST: RecruitersApplication/UpdateApplicationStatus
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateApplicationStatus(UpdateApplicationStatusViewModel viewModel)
        {
            var recruiterId = GetCurrentRecruiterId();
            if (!recruiterId.HasValue) return RedirectToRecruiterSetup();

            if (!ModelState.IsValid)
            {
                ViewBag.StatusOptions = new SelectList(
                    _applicationService.GetStatusOptions().Skip(1),
                    "Value", "Text", viewModel.Status);
                return View(viewModel);
            }

            var result = _applicationService.UpdateApplicationStatus(
                viewModel.ApplicationID, recruiterId.Value, viewModel.Status, viewModel.Note);

            if (!result.Success)
            {
                ModelState.AddModelError("Status", result.ErrorMessage);
                ViewBag.StatusOptions = new SelectList(
                    _applicationService.GetStatusOptions().Skip(1),
                    "Value", "Text", viewModel.Status);
                return View(viewModel);
            }

            TempData["SuccessMessage"] = result.SuccessMessage;
            return RedirectToAction("ApplicationDetails", new { id = result.ApplicationId });
        }

        /// <summary>
        /// GET: RecruitersApplication/ScheduleInterview
        /// </summary>
        [HttpGet]
        public ActionResult ScheduleInterview(int? id)
        {
            if (!id.HasValue) return RedirectWithError("Không tìm thấy đơn ứng tuyển.", "MyApplications");

            var recruiterId = GetCurrentRecruiterId();
            if (!recruiterId.HasValue) return RedirectToRecruiterSetup();

            // Service xử lý toàn bộ business logic (subscription check, status check)
            var result = _applicationService.GetScheduleInterviewForm(id.Value, recruiterId.Value);

            if (result.RequiresSubscription)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Index", "Subscription");
            }

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("ApplicationDetails", new { id });
            }

            ViewBag.InterviewTypes = new SelectList(
                _applicationService.GetInterviewTypeOptions(), "Value", "Text");
            return View(result.ViewModel);
        }

        /// <summary>
        /// POST: RecruitersApplication/ScheduleInterview
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ScheduleInterview(InterviewScheduleViewModel viewModel)
        {
            var recruiterId = GetCurrentRecruiterId();
            if (!recruiterId.HasValue) return RedirectToRecruiterSetup();

            if (!ModelState.IsValid)
            {
                ViewBag.InterviewTypes = new SelectList(
                    _applicationService.GetInterviewTypeOptions(),
                    "Value", "Text", viewModel.InterviewType);
                return View(viewModel);
            }

            var result = _applicationService.ScheduleInterview(viewModel, recruiterId.Value);

            if (result.ErrorMessage == "SUBSCRIPTION_REQUIRED")
            {
                TempData["ErrorMessage"] = "Tính năng này yêu cầu gói đăng ký Premium.";
                return RedirectToAction("Index", "Subscription");
            }

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                ViewBag.InterviewTypes = new SelectList(
                    _applicationService.GetInterviewTypeOptions(),
                    "Value", "Text", viewModel.InterviewType);
                return View(viewModel);
            }

            TempData["SuccessMessage"] = result.SuccessMessage;
            return RedirectToAction("ApplicationDetails", new { id = viewModel.ApplicationID });
        }

        #endregion

        #region Helper Methods

        private ActionResult RedirectToRecruiterSetup()
        {
            TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để thực hiện thao tác này.";
            return RedirectToAction("RecruitersManage", "Recruiters");
        }

        private ActionResult RedirectWithError(string message, string action)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(action);
        }

        #endregion
    }
}

using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Recruiters;
using Project_Recruiment_Huce.Repositories;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Repositories.RecruiterApplicationRepo;
using Project_Recruiment_Huce.Services.RecruiterApplicationService;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller quản lý đơn ứng tuyển cho Recruiter
    /// </summary>
    [Authorize(Roles = "Recruiter")]
    public class RecruitersApplicationController : BaseController
    {
        private readonly IRecruiterApplicationService _applicationService;
        private readonly IRecruiterApplicationRepository _repository;

        public RecruitersApplicationController()
        {
            var db = DbContextFactory.CreateReadOnly();
            _repository = new RecruiterApplicationRepository(db);
            _applicationService = new RecruiterApplicationService(_repository);
        }

        /// <summary>
        /// GET: RecruitersApplication/MyApplications
        /// Xem danh sách đơn ứng tuyển cho Recruiter
        /// </summary>
        [HttpGet]
        public ActionResult MyApplications(int? jobId, string status = null, int? page = null)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để xem đơn ứng tuyển.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            // Get applications list from service
            var result = _applicationService.GetApplicationsList(
                recruiterId.Value, 
                jobId, 
                status, 
                page ?? 1, 
                10);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Index", "Home");
            }

            // Get jobs for filter dropdown
            var jobs = _applicationService.GetJobsForFilter(recruiterId.Value);

            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.JobId = result.JobId;
            ViewBag.Status = result.Status;
            ViewBag.Jobs = new SelectList(jobs, "JobPostID", "Title", jobId);
            ViewBag.StatusOptions = new SelectList(new[] {
                new { Value = "", Text = "Tất cả trạng thái" },
                new { Value = "Under review", Text = "Đang xem xét" },
                new { Value = "Interview", Text = "Phỏng vấn" },
                new { Value = "Offered", Text = "Đã đề xuất" },
                new { Value = "Hired", Text = "Đã tuyển" },
                new { Value = "Rejected", Text = "Đã từ chối" }
            }, "Value", "Text", status);

            return View(result.Applications);
        }

        /// <summary>
        /// GET: RecruitersApplication/ApplicationDetails
        /// Xem chi tiết đơn ứng tuyển
        /// </summary>
        [HttpGet]
        public ActionResult ApplicationDetails(int? id)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để xem đơn ứng tuyển.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn ứng tuyển.";
                return RedirectToAction("MyApplications", "RecruitersApplication");
            }

            // Get application details from service
            var result = _applicationService.GetApplicationDetails(id.Value, recruiterId.Value);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("MyApplications", "RecruitersApplication");
            }

            return View(result.Application);
        }

        /// <summary>
        /// GET: RecruitersApplication/UpdateApplicationStatus
        /// Form cập nhật trạng thái đơn ứng tuyển
        /// </summary>
        [HttpGet]
        public ActionResult UpdateApplicationStatus(int? id)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để cập nhật trạng thái.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn ứng tuyển.";
                return RedirectToAction("MyApplications", "RecruitersApplication");
            }

            // Get application for status update from service
            var result = _applicationService.GetApplicationForStatusUpdate(id.Value, recruiterId.Value);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("MyApplications", "RecruitersApplication");
            }

            ViewBag.StatusOptions = new SelectList(new[] {
                new { Value = "Under review", Text = "Đang xem xét" },
                new { Value = "Interview", Text = "Phỏng vấn" },
                new { Value = "Offered", Text = "Đã đề xuất" },
                new { Value = "Hired", Text = "Đã tuyển" },
                new { Value = "Rejected", Text = "Đã từ chối" }
            }, "Value", "Text", result.ViewModel.Status);

            return View(result.ViewModel);
        }

        /// <summary>
        /// POST: RecruitersApplication/UpdateApplicationStatus
        /// Cập nhật trạng thái đơn ứng tuyển
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateApplicationStatus(UpdateApplicationStatusViewModel viewModel)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để cập nhật trạng thái.";
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.StatusOptions = new SelectList(new[] {
                    new { Value = "Under review", Text = "Đang xem xét" },
                    new { Value = "Interview", Text = "Phỏng vấn" },
                    new { Value = "Offered", Text = "Đã đề xuất" },
                    new { Value = "Hired", Text = "Đã tuyển" },
                    new { Value = "Rejected", Text = "Đã từ chối" }
                }, "Value", "Text", viewModel.Status);
                return View(viewModel);
            }

            // Update status through service
            var result = _applicationService.UpdateApplicationStatus(
                viewModel.ApplicationID,
                recruiterId.Value,
                viewModel.Status,
                viewModel.Note);

            if (!result.Success)
            {
                ModelState.AddModelError("Status", result.ErrorMessage);
                ViewBag.StatusOptions = new SelectList(new[] {
                    new { Value = "Under review", Text = "Đang xem xét" },
                    new { Value = "Interview", Text = "Phỏng vấn" },
                    new { Value = "Offered", Text = "Đã đề xuất" },
                    new { Value = "Hired", Text = "Đã tuyển" },
                    new { Value = "Rejected", Text = "Đã từ chối" }
                }, "Value", "Text", viewModel.Status);
                return View(viewModel);
            }

            TempData["SuccessMessage"] = result.SuccessMessage;
            return RedirectToAction("ApplicationDetails", "RecruitersApplication", new { id = result.ApplicationId });
        }
    }
}


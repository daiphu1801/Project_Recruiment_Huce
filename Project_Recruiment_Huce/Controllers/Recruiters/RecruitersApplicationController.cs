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
            var db = DbContextFactory.Create();
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

        /// <summary>
        /// GET: RecruitersApplication/ScheduleInterview
        /// Form đặt lịch phỏng vấn
        /// </summary>
        [HttpGet]
        public ActionResult ScheduleInterview(int? id)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có hồ sơ Recruiter để đặt lịch phỏng vấn.";
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

            // Check if status is Interview
            if (result.Application.Status != "Interview")
            {
                TempData["ErrorMessage"] = "Chỉ có thể đặt lịch phỏng vấn cho đơn ứng tuyển có trạng thái 'Phỏng vấn'.";
                return RedirectToAction("ApplicationDetails", "RecruitersApplication", new { id = id.Value });
            }

            // Create ViewModel with pre-filled data
            var viewModel = new InterviewScheduleViewModel
            {
                ApplicationID = result.Application.ApplicationID,
                CandidateName = result.Application.CandidateName,
                CandidateEmail = result.Application.CandidateEmail,
                JobTitle = result.Application.JobTitle,
                InterviewDate = DateTime.Today.AddDays(3), // Default to 3 days from now
                Duration = 60 // Default 60 minutes
            };

            // Prepare interview type options
            ViewBag.InterviewTypes = new SelectList(new[] {
                new { Value = "Trực tiếp", Text = "Phỏng vấn trực tiếp tại văn phòng" },
                new { Value = "Trực tuyến", Text = "Phỏng vấn trực tuyến (Online)" },
                new { Value = "Điện thoại", Text = "Phỏng vấn qua điện thoại" }
            }, "Value", "Text");

            return View(viewModel);
        }

        /// <summary>
        /// POST: RecruitersApplication/ScheduleInterview
        /// Xử lý đặt lịch phỏng vấn (sẽ implement backend sau)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]

            public ActionResult ScheduleInterview(InterviewScheduleViewModel viewModel)
        {
            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 1. Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                // Nếu lỗi, load lại Dropdown để không bị crash View
                ViewBag.InterviewTypes = new SelectList(new[] {
                    new { Value = "Trực tiếp", Text = "Phỏng vấn trực tiếp tại văn phòng" },
                    new { Value = "Trực tuyến", Text = "Phỏng vấn trực tuyến (Online)" },
                    new { Value = "Điện thoại", Text = "Phỏng vấn qua điện thoại" }
                }, "Value", "Text", viewModel.InterviewType);

                return View(viewModel);
            }

            // 2. GỌI SERVICE (Đây là dòng quan trọng kích hoạt Hangfire)
            var result = _applicationService.ScheduleInterview(viewModel, recruiterId.Value);

            // 3. Kiểm tra kết quả
            if (result.Success)
            {
                // THÀNH CÔNG: Hiện thông báo xanh, quay về trang chi tiết
                TempData["SuccessMessage"] = result.SuccessMessage;
                return RedirectToAction("ApplicationDetails", "RecruitersApplication", new { id = viewModel.ApplicationID });
            }
            else
            {
                // THẤT BẠI: Hiện thông báo đỏ, ở lại trang cũ để sửa
                TempData["ErrorMessage"] = result.ErrorMessage;

                // Load lại Dropdown
                ViewBag.InterviewTypes = new SelectList(new[] {
                    new { Value = "Trực tiếp", Text = "Phỏng vấn trực tiếp tại văn phòng" },
                    new { Value = "Trực tuyến", Text = "Phỏng vấn trực tuyến (Online)" },
                    new { Value = "Điện thoại", Text = "Phỏng vấn qua điện thoại" }
                }, "Value", "Text", viewModel.InterviewType);

                return View(viewModel);
            }
        }
    }
    }



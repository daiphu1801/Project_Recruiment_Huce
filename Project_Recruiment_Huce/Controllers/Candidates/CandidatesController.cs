using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Candidates;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Services.CandidateService;
using Project_Recruiment_Huce.Repositories.CandidateRepo;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller quản lý hồ sơ candidate, đơn ứng tuyển, tin đã lưu, upload resume
    /// Chỉ dành cho người dùng đã đăng nhập
    /// </summary>
    [Authorize(Roles = "Candidate")]
    public class CandidatesController : BaseController
    {
        private readonly ICandidateService _candidateService;

        public CandidatesController()
        {
            var repository = new CandidateRepository();
            _candidateService = new CandidateService(repository);
        }

        /// <summary>
        /// Quản lý hồ sơ candidate - hiển thị form
        /// GET: Candidates/CandidatesManage
        /// </summary>
        [HttpGet]
        public ActionResult CandidatesManage()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = _candidateService.GetOrCreateCandidate(accountId.Value, User.Identity.Name);
            return View(viewModel);
        }

        /// <summary>
        /// Cập nhật hồ sơ candidate và upload avatar
        /// POST: Candidates/CandidatesManage
        /// Validate: phone format/uniqueness, email format, sanitize HTML summary
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CandidatesManage(CandidateManageViewModel viewModel, HttpPostedFileBase avatar)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = _candidateService.ValidateAndUpdateProfile(viewModel, accountId.Value, avatar, Server);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin. Có lỗi trong form.";
                return View(viewModel);
            }

            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công.";
            return RedirectToAction("CandidatesManage");
        }

        [HttpGet]
        public ActionResult MyApplications(int? page)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;

            var result = _candidateService.GetApplicationsList(accountId.Value, pageNumber, pageSize);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("CandidatesManage");
            }

            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;

            return View(result.Applications);
        }


        /// <summary>
        /// GET: Candidates/Apply
        /// Hiển thị form ứng tuyển
        /// </summary>
        [HttpGet]
        public ActionResult Apply(int? jobId)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!jobId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy công việc.";
                return RedirectToAction("JobsListing", "Jobs");
            }

            var result = _candidateService.GetApplyFormData(accountId.Value, jobId.Value, Server);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                
                // Redirect based on error type
                if (result.ErrorMessage.Contains("hoàn thiện hồ sơ"))
                {
                    return RedirectToAction("CandidatesManage");
                }
                else if (result.ErrorMessage.Contains("đã ứng tuyển"))
                {
                    TempData["WarningMessage"] = result.ErrorMessage;
                    TempData.Remove("ErrorMessage");
                    return RedirectToAction("JobDetails", "Jobs", new { id = jobId.Value });
                }
                else
                {
                    return RedirectToAction("JobDetails", "Jobs", new { id = jobId.Value });
                }
            }

            return View(result.ViewModel);
        }

        /// <summary>
        /// POST: Candidates/Apply
        /// Xử lý form ứng tuyển
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Apply(ApplicationApplyViewModel viewModel, HttpPostedFileBase newResumeFile)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }


            //if (!ModelState.IsValid)
            //{
            //    // Reload data for view
            //    return Apply(viewModel.JobPostID);
            //}

            if (!ModelState.IsValid)
            {
                return Apply(viewModel.JobPostID);
            }


            var result = _candidateService.SubmitApplication(viewModel, accountId.Value, newResumeFile, Server);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Key == "General")
                    {
                        // General errors that require redirect
                        TempData["ErrorMessage"] = error.Value;
                        
                        if (error.Value.Contains("hoàn thiện hồ sơ"))
                        {
                            return RedirectToAction("CandidatesManage");
                        }
                        else if (error.Value.Contains("đã ứng tuyển"))
                        {
                            TempData["WarningMessage"] = error.Value;
                            TempData.Remove("ErrorMessage");
                            return RedirectToAction("JobDetails", "Jobs", new { id = viewModel.JobPostID });
                        }
                        else
                        {
                            return RedirectToAction("JobsListing", "Jobs");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }
                }
                return Apply(viewModel.JobPostID);
            }

            TempData["SuccessMessage"] = "Bạn đã gửi đơn ứng tuyển thành công! Chúng tôi sẽ xem xét và liên hệ với bạn sớm nhất có thể.";
            return RedirectToAction("JobDetails", "Jobs", new { id = viewModel.JobPostID });
        }
    }
}



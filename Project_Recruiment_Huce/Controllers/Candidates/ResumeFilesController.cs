using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Candidates;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Services.ResumeFileService;
using Project_Recruiment_Huce.Repositories.ResumeFileRepo;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class ResumeFilesController : BaseController
    {
        private readonly IResumeFileService _resumeFileService;

        public ResumeFilesController()
        {
            var repository = new ResumeFileRepository();
            _resumeFileService = new ResumeFileService(repository);
        }

        /// <summary>
        /// GET: Candidates/ResumeFiles/MyResumes
        /// Hiển thị danh sách CV files của candidate
        /// </summary>
        [HttpGet]
        public ActionResult MyResumes()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = _resumeFileService.GetResumeFiles(accountId.Value);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("CandidatesManage", "Candidates");
            }

            return View(result.ResumeFiles);
        }

        /// <summary>
        /// POST: Candidates/ResumeFiles/UploadResume
        /// Upload CV file mới
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadResume(HttpPostedFileBase resumeFile, string title)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = _resumeFileService.UploadResume(accountId.Value, resumeFile, title, Server);

            if (!result.IsValid)
            {
                var errorMessage = result.Errors.ContainsKey("General") ? result.Errors["General"] :
                                  result.Errors.ContainsKey("ResumeFile") ? result.Errors["ResumeFile"] :
                                  "Có lỗi xảy ra khi upload CV.";
                TempData["ErrorMessage"] = errorMessage;

                if (result.Errors.ContainsKey("General") && errorMessage.Contains("hoàn thiện hồ sơ"))
                {
                    return RedirectToAction("CandidatesManage", "Candidates");
                }
            }
            else
            {
                TempData["SuccessMessage"] = "Upload CV thành công!";
            }

            return RedirectToAction("MyResumes");
        }

        /// <summary>
        /// POST: Candidates/ResumeFiles/DeleteResume
        /// Xóa CV file
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteResume(int? id)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy CV cần xóa.";
                return RedirectToAction("MyResumes");
            }

            var result = _resumeFileService.DeleteResume(accountId.Value, id.Value, Server);

            if (!result.IsValid)
            {
                var errorMessage = result.Errors.ContainsKey("General") ? result.Errors["General"] : "Có lỗi xảy ra khi xóa CV.";
                TempData["ErrorMessage"] = errorMessage;

                if (errorMessage.Contains("hoàn thiện hồ sơ"))
                {
                    return RedirectToAction("CandidatesManage", "Candidates");
                }
            }
            else
            {
                TempData["SuccessMessage"] = "Xóa CV thành công!";
            }

            return RedirectToAction("MyResumes");
        }

        /// <summary>
        /// POST: Candidates/ResumeFiles/EditResume
        /// Chỉnh sửa tên CV
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditResume(int? id, string title)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy CV cần chỉnh sửa.";
                return RedirectToAction("MyResumes");
            }

            var result = _resumeFileService.EditResume(accountId.Value, id.Value, title);

            if (!result.IsValid)
            {
                var errorMessage = result.Errors.ContainsKey("General") ? result.Errors["General"] :
                                  result.Errors.ContainsKey("Title") ? result.Errors["Title"] :
                                  "Có lỗi xảy ra khi cập nhật CV.";
                TempData["ErrorMessage"] = errorMessage;

                if (result.Errors.ContainsKey("General") && errorMessage.Contains("hoàn thiện hồ sơ"))
                {
                    return RedirectToAction("CandidatesManage", "Candidates");
                }
            }
            else
            {
                TempData["SuccessMessage"] = "Cập nhật tên CV thành công!";
            }

            return RedirectToAction("MyResumes");
        }
    }
}


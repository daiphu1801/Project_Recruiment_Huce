using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class RecruitersController : Controller
    {
        private int? GetCurrentAccountId()
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
                return null;

            var idClaim = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) return null;
            int accountId;
            return int.TryParse(idClaim.Value, out accountId) ? (int?)accountId : null;
        }

        [HttpGet]
        public ActionResult RecruitersManage()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null) return RedirectToAction("Login", "Account");

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                if (recruiter == null)
                {
                    // Create new recruiter profile
                    recruiter = new Recruiter
                    {
                        AccountID = accountId.Value,
                        FullName = User.Identity.Name,
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    db.Recruiters.InsertOnSubmit(recruiter);
                    db.SubmitChanges();
                }

                return View(recruiter);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecruitersManage(Recruiter recruiter, HttpPostedFileBase avatar)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Load entity from database
                var existingRecruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                if (existingRecruiter == null)
                {
                    // Create new recruiter if doesn't exist
                    existingRecruiter = new Recruiter
                    {
                        AccountID = accountId.Value,
                        FullName = recruiter.FullName ?? User.Identity.Name,
                        PositionTitle = recruiter.PositionTitle,
                        CompanyEmail = recruiter.CompanyEmail,
                        Phone = recruiter.Phone,
                        CompanyID = recruiter.CompanyID,
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    db.Recruiters.InsertOnSubmit(existingRecruiter);
                }
                else
                {
                    // Update existing recruiter using LINQ to SQL (same approach as CandidatesController)
                    // Always update FullName (required field)
                    if (!string.IsNullOrWhiteSpace(recruiter.FullName))
                    {
                        existingRecruiter.FullName = recruiter.FullName;
                    }
                    
                    // Update nullable fields - allow setting to null/empty if user clears them
                    existingRecruiter.PositionTitle = recruiter.PositionTitle;
                    existingRecruiter.CompanyEmail = recruiter.CompanyEmail;
                    existingRecruiter.Phone = recruiter.Phone;
                    
                    // Update CompanyID if provided (including null to clear it)
                    existingRecruiter.CompanyID = recruiter.CompanyID;
                }

                // Handle avatar upload if provided
                if (avatar != null && avatar.ContentLength > 0)
                {
                    // TODO: Implement avatar upload logic similar to CandidatesController
                    // For now, just save the file info
                }

                // Submit changes using LINQ to SQL
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("RecruitersManage");
            }
        }
    }
}


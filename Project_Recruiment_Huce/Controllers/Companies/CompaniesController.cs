using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Companies;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class CompaniesController : Controller
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

        private int? GetCurrentRecruiterId()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null) return null;

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                return recruiter?.RecruiterID;
            }
        }

        [HttpGet]
        public ActionResult CompaniesManage()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null) return RedirectToAction("Login", "Account");

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Get recruiter and their company
                var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                if (recruiter == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng tạo hồ sơ nhà tuyển dụng trước.";
                    return RedirectToAction("RecruitersManage", "Recruiters");
                }

                Company company = null;
                if (recruiter.CompanyID.HasValue)
                {
                    company = db.Companies.FirstOrDefault(c => c.CompanyID == recruiter.CompanyID.Value);
                }

                // Map to ViewModel
                var viewModel = new CompanyManageViewModel
                {
                    CompanyID = company?.CompanyID,
                    CompanyName = company?.CompanyName,
                    TaxCode = company?.TaxCode,
                    Industry = company?.Industry,
                    Address = company?.Address,
                    Phone = company?.Phone,
                    CompanyEmail = company?.CompanyEmail,
                    Website = company?.Website,
                    Description = company?.Description
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompaniesManage(CompanyManageViewModel viewModel)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Get recruiter
                var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                if (recruiter == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng tạo hồ sơ nhà tuyển dụng trước.";
                    return RedirectToAction("RecruitersManage", "Recruiters");
                }

                Company company = null;
                if (recruiter.CompanyID.HasValue)
                {
                    company = db.Companies.FirstOrDefault(c => c.CompanyID == recruiter.CompanyID.Value);
                }

                if (company == null)
                {
                    // Create new company
                    company = new Company
                    {
                        CompanyName = viewModel.CompanyName,
                        TaxCode = viewModel.TaxCode,
                        Industry = viewModel.Industry,
                        Address = viewModel.Address,
                        Phone = viewModel.Phone,
                        CompanyEmail = viewModel.CompanyEmail,
                        Website = viewModel.Website,
                        Description = !string.IsNullOrWhiteSpace(viewModel.Description) 
                            ? HtmlSanitizerHelper.Sanitize(viewModel.Description) 
                            : null,
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    db.Companies.InsertOnSubmit(company);
                    db.SubmitChanges();

                    // Link company to recruiter
                    recruiter.CompanyID = company.CompanyID;
                    db.SubmitChanges();
                }
                else
                {
                    // Update existing company
                    company.CompanyName = viewModel.CompanyName;
                    company.TaxCode = viewModel.TaxCode;
                    company.Industry = viewModel.Industry;
                    company.Address = viewModel.Address;
                    company.Phone = viewModel.Phone;
                    company.CompanyEmail = viewModel.CompanyEmail;
                    company.Website = viewModel.Website;
                    company.Description = !string.IsNullOrWhiteSpace(viewModel.Description)
                        ? HtmlSanitizerHelper.Sanitize(viewModel.Description)
                        : null;

                    db.SubmitChanges();
                }

                TempData["SuccessMessage"] = "Cập nhật thông tin công ty thành công!";
                return RedirectToAction("CompaniesManage");
            }
        }
    }
}


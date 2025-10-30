using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using Project_Recruiment_Huce.DbContext;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class CandidatesController : Controller
    {
        private int? GetCurrentAccountId()
        {
            var idClaim = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) return null;
            int accountId;
            return int.TryParse(idClaim.Value, out accountId) ? (int?)accountId : null;
        }

        [HttpGet]
        public ActionResult CandidatesManage()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new RecruitmentDbContext())
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountId == accountId.Value);
                if (candidate == null)
                {
                    candidate = new Candidate
                    {
                        AccountId = accountId.Value,
                        FullName = User.Identity.Name,
                        Gender = "Nam",
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    db.Candidates.Add(candidate);
                    db.SaveChanges();
                }

                return View(candidate);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CandidatesManage(Candidate form)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(form);
            }

            using (var db = new RecruitmentDbContext())
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountId == accountId.Value);
                if (candidate == null)
                {
                    candidate = new Candidate { AccountId = accountId.Value, CreatedAt = DateTime.Now, ActiveFlag = 1 };
                    db.Candidates.Add(candidate);
                }

                candidate.FullName = form.FullName;
                candidate.BirthDate = form.BirthDate;
                candidate.Gender = string.IsNullOrWhiteSpace(form.Gender) ? "Nam" : form.Gender;
                candidate.Phone = form.Phone;
                candidate.Email = form.Email;
                candidate.Address = form.Address;
                if (!string.IsNullOrEmpty(form.About) && form.About.Length > 500)
                {
                    candidate.About = form.About.Substring(0, 500);
                }
                else
                {
                    candidate.About = form.About;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công.";
                return RedirectToAction("CandidatesManage");
            }
        }
    }
}



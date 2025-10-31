using System;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        public ActionResult Manage()
        {
            ViewBag.Title = "Tài khoản";
            ViewBag.Breadcrumbs = new System.Collections.Generic.List<Tuple<string, string>>
            {
                new Tuple<string, string>("Profile", null)
            };

            var idClaim = ((System.Security.Claims.ClaimsIdentity)User.Identity).FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int accountId = idClaim != null ? int.Parse(idClaim.Value) : 0;
            using (var db = new Project_Recruiment_Huce.DbContext.RecruitmentDbContext())
            {
                var acc = db.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (acc == null) return HttpNotFound();
                var vm = new ProfileVm
                {
                    AccountId = acc.AccountId,
                    Username = acc.Username,
                    Email = acc.Email,
                    Phone = acc.Phone,
                    Role = acc.Role,
                    CreatedAt = acc.CreatedAt
                };
                return View(vm);
            }
        }
    }
}



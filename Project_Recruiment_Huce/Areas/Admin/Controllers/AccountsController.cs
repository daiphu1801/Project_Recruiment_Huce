using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class AccountsController : Controller
    {
        public ActionResult Index(string q, string role = null, int page = 1)
        {
            ViewBag.Title = "Tài khoản";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Accounts", null)
            };

            var data = MockData.Accounts.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(x => (x.Username ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.Email ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.Phone ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (!string.IsNullOrWhiteSpace(role))
            {
                data = data.Where(x => string.Equals(x.Role, role, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.RoleOptions = new SelectList(new[] { "Admin", "Company", "Recruiter", "Candidate" });

            return View(data.ToList());
        }

        public ActionResult Details(int id)
        {
            var item = MockData.Accounts.FirstOrDefault(x => x.AccountId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết tài khoản";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Accounts", Url.Action("Index")),
                new Tuple<string, string>($"#{item.AccountId}", null)
            };
            return View(item);
        }
    }
}



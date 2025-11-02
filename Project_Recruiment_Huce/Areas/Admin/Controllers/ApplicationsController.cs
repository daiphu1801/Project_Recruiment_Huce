using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class ApplicationsController : AdminBaseController
    {
        public ActionResult Index(string q, string status = null, int page = 1)
        {
            ViewBag.Title = "Hồ sơ ứng tuyển";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Applications", null) };
            var data = MockData.Applications.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(x => (x.CandidateName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.JobTitle ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (!string.IsNullOrWhiteSpace(status)) data = data.Where(x => string.Equals(x.AppStatus, status, StringComparison.OrdinalIgnoreCase));
            ViewBag.StatusOptions = new SelectList(new[] { "Under review", "Interview", "Offered", "Hired", "Rejected" });
            return View(data.ToList());
        }

        public ActionResult Details(int id)
        {
            var item = MockData.Applications.FirstOrDefault(x => x.ApplicationId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết hồ sơ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("Applications", Url.Action("Index")),
                new Tuple<string, string>($"#{item.ApplicationId}", null)
            };
            return View(item);
        }
    }
}



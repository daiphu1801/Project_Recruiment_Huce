using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class CandidatesController : AdminBaseController
    {
        public ActionResult Index(string q, int page = 1)
        {
            ViewBag.Title = "Ứng viên";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Candidates", null) };
            var data = MockData.Candidates.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(x => (x.FullName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.Email ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.Phone ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return View(data.ToList());
        }

        public ActionResult Details(int id)
        {
            var item = MockData.Candidates.FirstOrDefault(x => x.CandidateId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết ứng viên";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("Candidates", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CandidateId}", null)
            };
            return View(item);
        }
    }
}



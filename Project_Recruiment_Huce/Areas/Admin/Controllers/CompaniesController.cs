using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class CompaniesController : Controller
    {
        public ActionResult Index(string q, int page = 1)
        {
            ViewBag.Title = "Công ty";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Companies", null) };
            var data = MockData.Companies.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(x => (x.CompanyName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.CompanyEmail ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.Industry ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return View(data.ToList());
        }

        public ActionResult Details(int id)
        {
            var item = MockData.Companies.FirstOrDefault(x => x.CompanyId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết công ty";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("Companies", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CompanyId}", null)
            };
            return View(item);
        }
    }
}



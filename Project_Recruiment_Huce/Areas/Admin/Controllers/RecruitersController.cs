using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class RecruitersController : AdminBaseController
    {
        public ActionResult Index(string q, int? companyId, int page = 1)
        {
            ViewBag.Title = "Nhà tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Recruiters", null) };
            var data = MockData.Recruiters.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(x => (x.FullName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.CompanyName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.WorkEmail ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (companyId.HasValue)
            {
                var company = MockData.Companies.FirstOrDefault(c => c.CompanyId == companyId.Value)?.CompanyName;
                if (!string.IsNullOrEmpty(company)) data = data.Where(x => string.Equals(x.CompanyName, company, StringComparison.OrdinalIgnoreCase));
            }
            ViewBag.CompanyOptions = new SelectList(MockData.Companies.Select(c => new { Id = c.CompanyId, Name = c.CompanyName }), "Id", "Name");
            return View(data.ToList());
        }

        public ActionResult Details(int id)
        {
            var item = MockData.Recruiters.FirstOrDefault(x => x.RecruiterId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết nhà tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("Recruiters", Url.Action("Index")),
                new Tuple<string, string>($"#{item.RecruiterId}", null)
            };
            return View(item);
        }
    }
}



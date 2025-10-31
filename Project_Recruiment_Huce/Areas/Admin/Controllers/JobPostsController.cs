using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class JobPostsController : Controller
    {
        public ActionResult Index(string q, string status = null, int? companyId = null, int? recruiterId = null, int page = 1)
        {
            ViewBag.Title = "Tin tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("JobPosts", null) };
            var data = MockData.JobPosts.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(x => (x.Title ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.JobCode ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.CompanyName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (!string.IsNullOrWhiteSpace(status)) data = data.Where(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase));
            if (companyId.HasValue)
            {
                var comp = MockData.Companies.FirstOrDefault(c => c.CompanyId == companyId.Value)?.CompanyName;
                if (!string.IsNullOrEmpty(comp)) data = data.Where(x => string.Equals(x.CompanyName, comp, StringComparison.OrdinalIgnoreCase));
            }
            if (recruiterId.HasValue)
            {
                var rec = MockData.Recruiters.FirstOrDefault(r => r.RecruiterId == recruiterId.Value)?.RecruiterId; // not used by name here
            }
            ViewBag.StatusOptions = new SelectList(new[] { "Visible", "Hidden", "Closed", "Draft" });
            ViewBag.CompanyOptions = new SelectList(MockData.Companies.Select(c => new { Id = c.CompanyId, Name = c.CompanyName }), "Id", "Name");
            ViewBag.RecruiterOptions = new SelectList(MockData.Recruiters.Select(r => new { Id = r.RecruiterId, Name = r.FullName }), "Id", "Name");
            return View(data.ToList());
        }

        public ActionResult Details(int id)
        {
            var item = MockData.JobPosts.FirstOrDefault(x => x.JobId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết tin tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("JobPosts", Url.Action("Index")),
                new Tuple<string, string>($"#{item.JobId}", null)
            };
            return View(item);
        }
    }
}



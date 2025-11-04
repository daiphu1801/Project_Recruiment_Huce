using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class JobPostsController : AdminBaseController
    {
        // GET: Admin/JobPosts
        // NOTE: This controller uses MockData as a template/base.
        // Team members should follow AccountsController pattern to implement CRUD with database.
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

        // GET: Admin/JobPosts/Details/5
        // NOTE: This action uses MockData as a template/base.
        // Team members should follow AccountsController pattern to implement CRUD with database.
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

        // GET: Admin/JobPosts/Create
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm tin tuyển dụng mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("JobPosts", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };
            ViewBag.CompanyOptions = new SelectList(MockData.Companies.Select(c => new { Id = c.CompanyId, Name = c.CompanyName }), "Id", "Name");
            ViewBag.RecruiterOptions = new SelectList(MockData.Recruiters.Select(r => new { Id = r.RecruiterId, Name = r.FullName }), "Id", "Name");
            ViewBag.StatusOptions = new SelectList(new[] { "Visible", "Hidden", "Closed", "Draft" });
            ViewBag.EmploymentOptions = new SelectList(new[] { "Full-time", "Part-time", "Internship", "Contract", "Remote" });
            return View();
        }

        // POST: Admin/JobPosts/Create
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JobPostListVm model)
        {
            // Mock implementation - in real scenario, save to database
            TempData["SuccessMessage"] = "Tạo tin tuyển dụng thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/JobPosts/Edit/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Edit(int id)
        {
            var item = MockData.JobPosts.FirstOrDefault(x => x.JobId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Sửa tin tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("JobPosts", Url.Action("Index")),
                new Tuple<string, string>($"#{item.JobId}", null)
            };
            ViewBag.CompanyOptions = new SelectList(MockData.Companies.Select(c => new { Id = c.CompanyId, Name = c.CompanyName }), "Id", "Name", item.CompanyId);
            ViewBag.RecruiterOptions = new SelectList(MockData.Recruiters.Select(r => new { Id = r.RecruiterId, Name = r.FullName }), "Id", "Name", item.RecruiterId);
            ViewBag.StatusOptions = new SelectList(new[] { "Visible", "Hidden", "Closed", "Draft" }, item.Status);
            ViewBag.EmploymentOptions = new SelectList(new[] { "Full-time", "Part-time", "Internship", "Contract", "Remote" }, item.Employment);
            return View(item);
        }

        // POST: Admin/JobPosts/Edit/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JobPostListVm model)
        {
            // Mock implementation - in real scenario, update database
            TempData["SuccessMessage"] = "Cập nhật tin tuyển dụng thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/JobPosts/Delete/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Delete(int id)
        {
            var item = MockData.JobPosts.FirstOrDefault(x => x.JobId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Xóa tin tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("JobPosts", Url.Action("Index")),
                new Tuple<string, string>($"#{item.JobId}", null)
            };
            return View(item);
        }

        // POST: Admin/JobPosts/Delete/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Mock implementation - in real scenario, delete from database
            TempData["SuccessMessage"] = "Xóa tin tuyển dụng thành công! (MockData)";
            return RedirectToAction("Index");
        }
    }
}



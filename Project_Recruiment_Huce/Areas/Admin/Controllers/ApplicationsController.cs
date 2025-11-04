using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class ApplicationsController : AdminBaseController
    {
        // GET: Admin/Applications
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

        // GET: Admin/Applications/Details/5
        // NOTE: This action uses MockData as a template/base.
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

        // GET: Admin/Applications/Create
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm hồ sơ ứng tuyển mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Applications", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };
            ViewBag.CandidateOptions = new SelectList(MockData.Candidates.Select(c => new { Id = c.CandidateId, Name = c.FullName }), "Id", "Name");
            ViewBag.JobOptions = new SelectList(MockData.JobPosts.Select(j => new { Id = j.JobId, Name = j.Title }), "Id", "Name");
            ViewBag.StatusOptions = new SelectList(new[] { "Under review", "Interview", "Offered", "Hired", "Rejected" });
            return View();
        }

        // POST: Admin/Applications/Create
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ApplicationListVm model)
        {
            // Mock implementation - in real scenario, save to database
            TempData["SuccessMessage"] = "Tạo hồ sơ ứng tuyển thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/Applications/Edit/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Edit(int id)
        {
            var item = MockData.Applications.FirstOrDefault(x => x.ApplicationId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Sửa hồ sơ ứng tuyển";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Applications", Url.Action("Index")),
                new Tuple<string, string>($"#{item.ApplicationId}", null)
            };
            ViewBag.CandidateOptions = new SelectList(MockData.Candidates.Select(c => new { Id = c.CandidateId, Name = c.FullName }), "Id", "Name", item.CandidateId);
            ViewBag.JobOptions = new SelectList(MockData.JobPosts.Select(j => new { Id = j.JobId, Name = j.Title }), "Id", "Name", item.JobId);
            ViewBag.StatusOptions = new SelectList(new[] { "Under review", "Interview", "Offered", "Hired", "Rejected" }, item.AppStatus);
            return View(item);
        }

        // POST: Admin/Applications/Edit/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationListVm model)
        {
            // Mock implementation - in real scenario, update database
            TempData["SuccessMessage"] = "Cập nhật hồ sơ ứng tuyển thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/Applications/Delete/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Delete(int id)
        {
            var item = MockData.Applications.FirstOrDefault(x => x.ApplicationId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Xóa hồ sơ ứng tuyển";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Applications", Url.Action("Index")),
                new Tuple<string, string>($"#{item.ApplicationId}", null)
            };
            return View(item);
        }

        // POST: Admin/Applications/Delete/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Mock implementation - in real scenario, delete from database
            TempData["SuccessMessage"] = "Xóa hồ sơ ứng tuyển thành công! (MockData)";
            return RedirectToAction("Index");
        }
    }
}



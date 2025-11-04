using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class CandidatesController : AdminBaseController
    {
        // GET: Admin/Candidates
        public ActionResult Index(string q, int page = 1)
        {
            ViewBag.Title = "Quản lý ứng viên";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Ứng viên", null) };
            
            var data = MockData.Candidates.AsEnumerable();
            
            // Search
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(c =>
                    (c.FullName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (c.Email ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (c.Phone ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                );
            }

            return View(data.ToList());
        }

        // GET: Admin/Candidates/Details/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Details(int id)
        {
            var item = MockData.Candidates.FirstOrDefault(x => x.CandidateId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết ứng viên";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("Ứng viên", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CandidateId}", null)
            };
            return View(item);
        }

        // GET: Admin/Candidates/Create
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm ứng viên mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Ứng viên", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };
            ViewBag.AccountOptions = new SelectList(MockData.Accounts.Where(a => a.Role == "Candidate").Select(a => new { Id = a.AccountId, Name = a.Username }), "Id", "Name");
            return View();
        }

        // POST: Admin/Candidates/Create
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CandidateListVm model)
        {
            // Mock implementation - in real scenario, save to database
            TempData["SuccessMessage"] = "Tạo ứng viên thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/Candidates/Edit/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Edit(int id)
        {
            var item = MockData.Candidates.FirstOrDefault(x => x.CandidateId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Sửa ứng viên";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Ứng viên", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CandidateId}", null)
            };
            ViewBag.AccountOptions = new SelectList(MockData.Accounts.Where(a => a.Role == "Candidate").Select(a => new { Id = a.AccountId, Name = a.Username }), "Id", "Name");
            return View(item);
        }

        // POST: Admin/Candidates/Edit/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CandidateListVm model)
        {
            // Mock implementation - in real scenario, update database
            TempData["SuccessMessage"] = "Cập nhật ứng viên thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/Candidates/Delete/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Delete(int id)
        {
            var item = MockData.Candidates.FirstOrDefault(x => x.CandidateId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Xóa ứng viên";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Ứng viên", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CandidateId}", null)
            };
            return View(item);
        }

        // POST: Admin/Candidates/Delete/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Mock implementation - in real scenario, delete from database
            TempData["SuccessMessage"] = "Xóa ứng viên thành công! (MockData)";
            return RedirectToAction("Index");
        }
    }
}

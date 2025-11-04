using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class RecruitersController : AdminBaseController
    {
        // GET: Admin/Recruiters
        public ActionResult Index(string q, int? companyId, int page = 1)
        {
            ViewBag.Title = "Quản lý nhà tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Nhà tuyển dụng", null) };
            
            var data = MockData.Recruiters.AsEnumerable();
            
            // Search
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(r =>
                    (r.FullName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.WorkEmail ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Phone ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                );
            }

            // Filter by company
            if (companyId.HasValue)
            {
                data = data.Where(r => r.CompanyId == companyId.Value);
            }

            ViewBag.CompanyOptions = new SelectList(MockData.Companies.Select(c => new { Id = c.CompanyId, Name = c.CompanyName }), "Id", "Name");
            return View(data.ToList());
        }

        // GET: Admin/Recruiters/Details/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Details(int id)
        {
            var item = MockData.Recruiters.FirstOrDefault(x => x.RecruiterId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết nhà tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                new Tuple<string, string>($"#{item.RecruiterId}", null)
            };
            return View(item);
        }

        // GET: Admin/Recruiters/Create
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm nhà tuyển dụng mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };
            ViewBag.AccountOptions = new SelectList(MockData.Accounts.Where(a => a.Role == "Recruiter").Select(a => new { Id = a.AccountId, Name = a.Username }), "Id", "Name");
            ViewBag.CompanyOptions = new SelectList(MockData.Companies.Select(c => new { Id = c.CompanyId, Name = c.CompanyName }), "Id", "Name");
            return View();
        }

        // POST: Admin/Recruiters/Create
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RecruiterListVm model)
        {
            // Mock implementation - in real scenario, save to database
            TempData["SuccessMessage"] = "Tạo nhà tuyển dụng thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/Recruiters/Edit/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Edit(int id)
        {
            var item = MockData.Recruiters.FirstOrDefault(x => x.RecruiterId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Sửa nhà tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                new Tuple<string, string>($"#{item.RecruiterId}", null)
            };
            ViewBag.AccountOptions = new SelectList(MockData.Accounts.Where(a => a.Role == "Recruiter").Select(a => new { Id = a.AccountId, Name = a.Username }), "Id", "Name");
            ViewBag.CompanyOptions = new SelectList(MockData.Companies.Select(c => new { Id = c.CompanyId, Name = c.CompanyName }), "Id", "Name");
            return View(item);
        }

        // POST: Admin/Recruiters/Edit/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RecruiterListVm model)
        {
            // Mock implementation - in real scenario, update database
            TempData["SuccessMessage"] = "Cập nhật nhà tuyển dụng thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/Recruiters/Delete/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Delete(int id)
        {
            var item = MockData.Recruiters.FirstOrDefault(x => x.RecruiterId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Xóa nhà tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                new Tuple<string, string>($"#{item.RecruiterId}", null)
            };
            return View(item);
        }

        // POST: Admin/Recruiters/Delete/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Mock implementation - in real scenario, delete from database
            TempData["SuccessMessage"] = "Xóa nhà tuyển dụng thành công! (MockData)";
            return RedirectToAction("Index");
        }
    }
}

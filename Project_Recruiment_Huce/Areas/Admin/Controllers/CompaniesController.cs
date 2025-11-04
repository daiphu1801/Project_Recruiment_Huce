using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class CompaniesController : AdminBaseController
    {
        public ActionResult Index(string q, int page = 1)
        {
            ViewBag.Title = "Quản lý công ty";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Công ty", null) };
            
            // Get data from MockData
            var companies = MockData.Companies;
            
            // Apply search filter if provided
            var data = companies.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(x => 
                    (x.CompanyName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (x.TaxCode ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (x.Website ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (x.Address ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                );
            }
            
            var result = data.ToList();
            return View(result);
        }

        // GET: Admin/Companies/Details/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Details(int id)
        {
            var item = MockData.Companies.FirstOrDefault(x => x.CompanyId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết công ty";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("Công ty", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CompanyId}", null)
            };
            return View(item);
        }

        // GET: Admin/Companies/Create
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm công ty mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Công ty", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };
            return View();
        }

        // POST: Admin/Companies/Create
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompanyListVm model)
        {
            // Mock implementation - in real scenario, save to database
            TempData["SuccessMessage"] = "Tạo công ty thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/Companies/Edit/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Edit(int id)
        {
            var item = MockData.Companies.FirstOrDefault(x => x.CompanyId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Sửa công ty";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Công ty", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CompanyId}", null)
            };
            return View(item);
        }

        // POST: Admin/Companies/Edit/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CompanyListVm model)
        {
            // Mock implementation - in real scenario, update database
            TempData["SuccessMessage"] = "Cập nhật công ty thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/Companies/Delete/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Delete(int id)
        {
            var item = MockData.Companies.FirstOrDefault(x => x.CompanyId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Xóa công ty";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Công ty", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CompanyId}", null)
            };
            return View(item);
        }

        // POST: Admin/Companies/Delete/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Mock implementation - in real scenario, delete from database
            TempData["SuccessMessage"] = "Xóa công ty thành công! (MockData)";
            return RedirectToAction("Index");
        }
    }
}



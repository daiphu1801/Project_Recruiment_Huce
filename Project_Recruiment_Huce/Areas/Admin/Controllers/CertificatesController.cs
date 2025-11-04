using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class CertificatesController : AdminBaseController
    {
        // GET: Admin/Certificates
        public ActionResult Index(string q)
        {
            ViewBag.Title = "Chứng chỉ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Certificates", null) };
            var data = MockData.Certificates.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(x => (x.CertificateName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.Issuer ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.Industry ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return View(data.ToList());
        }

        // GET: Admin/Certificates/Details/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Details(int id)
        {
            var item = MockData.Certificates.FirstOrDefault(x => x.CertificateId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết chứng chỉ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("Certificates", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CertificateId}", null)
            };
            return View(item);
        }

        // GET: Admin/Certificates/Create
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm chứng chỉ mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Certificates", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };
            return View();
        }

        // POST: Admin/Certificates/Create
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CertificateListVm model)
        {
            // Mock implementation - in real scenario, save to database
            TempData["SuccessMessage"] = "Tạo chứng chỉ thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/Certificates/Edit/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Edit(int id)
        {
            var item = MockData.Certificates.FirstOrDefault(x => x.CertificateId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Sửa chứng chỉ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Certificates", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CertificateId}", null)
            };
            return View(item);
        }

        // POST: Admin/Certificates/Edit/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CertificateListVm model)
        {
            // Mock implementation - in real scenario, update database
            TempData["SuccessMessage"] = "Cập nhật chứng chỉ thành công! (MockData)";
            return RedirectToAction("Index");
        }

        // GET: Admin/Certificates/Delete/5
        // NOTE: This action uses MockData as a template/base.
        public ActionResult Delete(int id)
        {
            var item = MockData.Certificates.FirstOrDefault(x => x.CertificateId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Xóa chứng chỉ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Certificates", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CertificateId}", null)
            };
            return View(item);
        }

        // POST: Admin/Certificates/Delete/5
        // NOTE: This action uses MockData as a template/base.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Mock implementation - in real scenario, delete from database
            TempData["SuccessMessage"] = "Xóa chứng chỉ thành công! (MockData)";
            return RedirectToAction("Index");
        }
    }
}



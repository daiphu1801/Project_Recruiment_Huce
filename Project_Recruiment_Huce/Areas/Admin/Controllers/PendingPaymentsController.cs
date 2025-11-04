using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class PendingPaymentsController : AdminBaseController
    {
        public ActionResult Index(string status = null)
        {
            ViewBag.Title = "Công nợ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("PendingPayments", null) };
            var data = MockData.PendingPayments.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(status)) data = data.Where(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase));
            ViewBag.StatusOptions = new SelectList(new[] { "Waiting", "Overdue" });
            return View(data.ToList());
        }

        public ActionResult Details(int id)
        {
            var item = MockData.PendingPayments.FirstOrDefault(x => x.PendingId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết công nợ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("PendingPayments", Url.Action("Index")), new Tuple<string, string>($"#{item.PendingId}", null) };
            return View(item);
        }
    }
}



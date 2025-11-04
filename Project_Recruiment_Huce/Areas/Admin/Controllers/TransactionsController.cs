using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class TransactionsController : AdminBaseController
    {
        public ActionResult Index(string q, string method = null, string status = null, int page = 1)
        {
            ViewBag.Title = "Giao dịch";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Transactions", null) };
            var data = MockData.Transactions.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(x => (x.TransactionNo ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.AccountEmail ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            // Note: Method property may not exist in TransactionListVm - filter removed
            // if (!string.IsNullOrWhiteSpace(method)) data = data.Where(x => string.Equals(x.Method, method, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(status)) data = data.Where(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase));
            ViewBag.StatusOptions = new SelectList(new[] { "Processing", "Completed", "Failed" });
            ViewBag.MethodOptions = new SelectList(new[] { "Bank", "Card" });
            return View(data.ToList());
        }

        public ActionResult Details(int id)
        {
            var item = MockData.Transactions.FirstOrDefault(x => x.TransactionId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết giao dịch";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("Transactions", Url.Action("Index")),
                new Tuple<string, string>($"#{item.TransactionId}", null)
            };
            return View(item);
        }
    }
}



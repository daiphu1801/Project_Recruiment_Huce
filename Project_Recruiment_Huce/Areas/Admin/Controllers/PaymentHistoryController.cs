using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class PaymentHistoryController : AdminBaseController
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Lịch sử thanh toán";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("PaymentHistory", null) };
            return View(MockData.PaymentHistory.ToList());
        }

        public ActionResult Details(int id)
        {
            var item = MockData.PaymentHistory.FirstOrDefault(x => x.PaymentId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết thanh toán";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("PaymentHistory", Url.Action("Index")), new Tuple<string, string>($"#{item.PaymentId}", null) };
            return View(item);
        }
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class BankCardsController : AdminBaseController
    {
        public ActionResult Index(int? companyId)
        {
            ViewBag.Title = "Thẻ ngân hàng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("BankCards", null) };
            var data = MockData.BankCards.AsEnumerable();
            if (companyId.HasValue)
            {
                var comp = MockData.Companies.FirstOrDefault(c => c.CompanyId == companyId.Value)?.CompanyName;
                if (!string.IsNullOrEmpty(comp)) data = data.Where(x => string.Equals(x.CompanyName, comp, StringComparison.OrdinalIgnoreCase));
            }
            ViewBag.CompanyOptions = new SelectList(MockData.Companies.Select(c => new { Id = c.CompanyId, Name = c.CompanyName }), "Id", "Name");
            return View(data.ToList());
        }

        public ActionResult Details(int id)
        {
            var item = MockData.BankCards.FirstOrDefault(x => x.CardId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết thẻ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("BankCards", Url.Action("Index")),
                new Tuple<string, string>($"#{item.CardId}", null)
            };
            return View(item);
        }
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class PhotosController : AdminBaseController
    {
        public ActionResult Index(string q)
        {
            ViewBag.Title = "Thư viện";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Photos", null) };
            var data = MockData.Photos.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(x => (x.FileName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.MimeType ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return View(data.ToList());
        }

        public ActionResult Details(int id)
        {
            var item = MockData.Photos.FirstOrDefault(x => x.PhotoId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Ảnh";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Photos", Url.Action("Index")), new Tuple<string, string>($"#{item.PhotoId}", null) };
            return View(item);
        }
    }
}



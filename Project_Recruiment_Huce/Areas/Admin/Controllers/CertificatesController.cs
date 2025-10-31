using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class CertificatesController : Controller
    {
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
    }
}



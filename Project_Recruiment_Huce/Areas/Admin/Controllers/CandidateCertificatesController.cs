using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    // NOTE: This controller uses MockData as a template/base.
    // Team members should follow AccountsController pattern to implement CRUD with database.
    public class CandidateCertificatesController : AdminBaseController
    {
        public ActionResult Index(int candidateId)
        {
            var candidate = MockData.Candidates.FirstOrDefault(c => c.CandidateId == candidateId);
            ViewBag.Title = $"Chứng chỉ - {candidate?.FullName}";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("Candidates", Url.Action("Index","Candidates")),
                new Tuple<string, string>($"#{candidateId}", Url.Action("Details","Candidates", new { id = candidateId })),
                new Tuple<string, string>("Certificates", null)
            };
            var data = MockData.CandidateCertificates.Where(x => string.Equals(x.CandidateName, candidate?.FullName, StringComparison.OrdinalIgnoreCase)).ToList();
            return View(data);
        }

        public ActionResult Details(int id)
        {
            var item = MockData.CandidateCertificates.FirstOrDefault(x => x.CandidateCertificateId == id);
            if (item == null) return HttpNotFound();
            ViewBag.Title = "Chi tiết chứng chỉ ứng viên";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Certificates", null) };
            return View(item);
        }
    }
}



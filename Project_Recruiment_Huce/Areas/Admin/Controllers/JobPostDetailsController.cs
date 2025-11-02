using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class JobPostDetailsController : AdminBaseController
    {
        public ActionResult Index(int jobId)
        {
            var job = MockData.JobPosts.FirstOrDefault(j => j.JobId == jobId);
            ViewBag.Title = $"Yêu cầu - {job?.Title}";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("JobPosts", Url.Action("Index","JobPosts")),
                new Tuple<string, string>($"#{jobId}", Url.Action("Details","JobPosts", new { id = jobId })),
                new Tuple<string, string>("Details", null)
            };
            var data = MockData.JobPostDetails.Where(x => x.JobId == jobId).ToList();
            return View(data);
        }
    }
}



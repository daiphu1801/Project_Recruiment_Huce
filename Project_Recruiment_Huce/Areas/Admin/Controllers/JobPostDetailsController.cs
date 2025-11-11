//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web.Mvc;
//using Project_Recruiment_Huce.Areas.Admin.Models;

//namespace Project_Recruiment_Huce.Areas.Admin.Controllers
//{
//    // NOTE: This controller uses MockData as a template/base.
//    // Team members should follow AccountsController pattern to implement CRUD with database.
//    public class JobPostDetailsController : AdminBaseController
//    {
//        public ActionResult Index(int jobId)
//        {
           
//            ViewBag.Title = $"Yêu cầu - {job?.Title}";
//            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
//                new Tuple<string, string>("JobPosts", Url.Action("Index","JobPosts")),
//                new Tuple<string, string>($"#{jobId}", Url.Action("Details","JobPosts", new { id = jobId })),
//                new Tuple<string, string>("Details", null)
//            };
           
//            return View(data);
//        }
//    }
//}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class DashboardController : AdminBaseController
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Tổng quan";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Dashboard", null)
            };

            // Mock time-series for last 7 days based on PostedAt/AppliedAt
            var dates = new List<string>();
            var jobSeries = new List<int>();
            var appSeries = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var d = DateTime.Today.AddDays(-i);
                dates.Add(d.ToString("yyyy-MM-dd"));
                jobSeries.Add(MockData.JobPosts.Count(j => j.PostedAt.Date == d));
                appSeries.Add(MockData.Applications.Count(a => a.AppliedAt.Date == d));
            }

            var vm = new DashboardVm
            {
                Accounts = MockData.Accounts.Count,
                Companies = MockData.Companies.Count,
                Recruiters = MockData.Recruiters.Count,
                Candidates = MockData.Candidates.Count,
                JobPosts = MockData.JobPosts.Count,
                Applications = MockData.Applications.Count,
                Transactions = MockData.Transactions.Count,
                Dates7 = dates,
                JobPostsWeekly = jobSeries,
                ApplicationsWeekly = appSeries
            };
            return View(vm);
        }
    }
}



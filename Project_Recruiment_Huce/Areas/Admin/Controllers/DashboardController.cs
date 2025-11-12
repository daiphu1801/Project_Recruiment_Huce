using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Models;

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

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Get counts from database
                var accountsCount = db.Accounts.Count();
                var companiesCount = db.Companies.Count();
                var recruitersCount = db.Recruiters.Count();
                var candidatesCount = db.Candidates.Count();
                var jobPostsCount = db.JobPosts.Count();
                var applicationsCount = db.Applications.Count();
                var transactionsCount = db.Transactions.Count();

                // Time-series for last 7 days
                var dates = new List<string>();
                var jobSeries = new List<int>();
                var appSeries = new List<int>();
                for (int i = 6; i >= 0; i--)
                {
                    var d = DateTime.Today.AddDays(-i);
                    dates.Add(d.ToString("yyyy-MM-dd"));
                    
                    // Count job posts posted on this date
                    jobSeries.Add(db.JobPosts.Count(j => j.PostedAt.HasValue && j.PostedAt.Value.Date == d));
                    
                    // Count applications applied on this date
                    appSeries.Add(db.Applications.Count(a => a.AppliedAt.HasValue && a.AppliedAt.Value.Date == d));
                }

                // Phân bố theo loại hình công việc
                // Lấy tất cả JobPosts về memory trước, sau đó filter và group
                var allJobPosts = db.JobPosts.ToList();
                
                var employmentTypeDistribution = allJobPosts
                    .Where(j => j.EmploymentType != null)
                    .GroupBy(j => j.EmploymentType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var employmentLabels = new List<string>();
                var employmentCounts = new List<int>();
                
                // Map EmploymentType values to Vietnamese labels
                var typeMapping = new Dictionary<string, string>
                {
                    { "Full-time", "Toàn thời gian" },
                    { "Part-time", "Bán thời gian" },
                    { "Contract", "Hợp đồng" },
                    { "Internship", "Thực tập" },
                    { "Freelance", "Tự do" }
                };

                foreach (var item in employmentTypeDistribution)
                {
                    var label = typeMapping.ContainsKey(item.Type) ? typeMapping[item.Type] : item.Type;
                    employmentLabels.Add(label);
                    employmentCounts.Add(item.Count);
                }

                var vm = new DashboardVm
                {
                    Accounts = accountsCount,
                    Companies = companiesCount,
                    Recruiters = recruitersCount,
                    Candidates = candidatesCount,
                    JobPosts = jobPostsCount,
                    Applications = applicationsCount,
                    Transactions = transactionsCount,
                    Dates7 = dates,
                    JobPostsWeekly = jobSeries,
                    ApplicationsWeekly = appSeries,
                    EmploymentTypeLabels = employmentLabels,
                    EmploymentTypeCounts = employmentCounts
                };
                return View(vm);
            }
        }
    }
}



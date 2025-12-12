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
                
                // Thống kê thanh toán từ SePayTransactions
                var transactionsCount = db.SePayTransactions.Count();
                var totalAmountIn = db.SePayTransactions.Sum(t => (decimal?)t.AmountIn) ?? 0;
                var totalAmountOut = db.SePayTransactions.Sum(t => (decimal?)t.AmountOut) ?? 0;
                var netAmount = totalAmountIn - totalAmountOut;
                
                var today = DateTime.Today;
                var transactionsToday = db.SePayTransactions.Count(t => t.TransactionDate.Date == today);
                
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                var transactionsThisMonth = db.SePayTransactions.Count(t => t.TransactionDate >= firstDayOfMonth);
                
                var avgAmount = transactionsCount > 0 ? (totalAmountIn + totalAmountOut) / transactionsCount : 0;

                // Time-series for last 7 days
                var dates = new List<string>();
                var jobSeries = new List<int>();
                var appSeries = new List<int>();
                var paymentSeries = new List<decimal>();
                for (int i = 6; i >= 0; i--)
                {
                    var d = DateTime.Today.AddDays(-i);
                    dates.Add(d.ToString("yyyy-MM-dd"));
                    
                    // Count job posts posted on this date
                    jobSeries.Add(db.JobPosts.Count(j => j.PostedAt.Date == d));
                    
                    // Count applications applied on this date
                    appSeries.Add(db.Applications.Count(a => a.AppliedAt.Date == d));
                    
                    // Sum payments (AmountIn) on this date
                    var dailyPayment = db.SePayTransactions
                        .Where(t => t.TransactionDate.Date == d)
                        .Sum(t => (decimal?)t.AmountIn) ?? 0;
                    paymentSeries.Add(dailyPayment);
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
                    EmploymentTypeCounts = employmentCounts,
                    // Thanh toán
                    TotalAmountIn = totalAmountIn,
                    TotalAmountOut = totalAmountOut,
                    NetAmount = netAmount,
                    TransactionsToday = transactionsToday,
                    TransactionsThisMonth = transactionsThisMonth,
                    PaymentWeekly = paymentSeries,
                    AverageTransactionAmount = avgAmount
                };
                return View(vm);
            }
        }
    }
}



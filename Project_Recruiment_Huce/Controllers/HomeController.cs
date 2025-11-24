using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Home;
using Project_Recruiment_Huce.Models.Jobs;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Mappers;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Services;
using LegacyJobRepository = Project_Recruiment_Huce.Repositories.JobRepository;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller trang chủ hiển thị thống kê tổng quan và danh sách tin tuyển dụng mới
    /// Sử dụng JobService cho business logic, CreateReadOnly() để tối ưu hiệu suất read
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Trang chủ - hiển thị statistics, recent jobs, locations và popular keywords
        /// </summary>
        public ActionResult Index()
        {
            using (var db = DbContextFactory.CreateReadOnly())
            {
                // Sử dụng JobService cho business logic
                var jobRepository = new LegacyJobRepository(db);
                var jobService = new LegacyJobService(jobRepository, db);

                // Lấy thống kê tổng quan
                var totalCandidates = db.Candidates.Count(c => c.ActiveFlag == 1);
                var totalJobPosts = db.JobPosts.Count(j => j.Status == JobStatusHelper.Published);
                var totalHiredJobs = db.Applications.Count(a => a.Status == "Hired" || a.Status == "Accepted");
                var totalCompanies = db.Companies.Count(c => c.ActiveFlag == 1);

                // Lấy danh sách tin tuyển dụng mới nhất (7 tin)
                var jobViewModels = jobService.GetRecentPublishedJobs(7);

                // Tổng số tin đang published
                var totalJobsCount = db.JobPosts.Count(j => j.Status == JobStatusHelper.Published);

                // Lấy danh sách locations duy nhất cho filter dropdown
                var locations = db.JobPosts
                    .Where(j => j.Status == JobStatusHelper.Published && j.Location != null)
                    .Select(j => j.Location)
                    .ToList()
                    .Where(l => !string.IsNullOrEmpty(l))
                    .Distinct()
                    .OrderBy(l => l)
                    .ToList();

                // Lấy từ khóa phổ biến (các từ xuất hiện nhiều trong job titles)
                var popularKeywords = new List<string>();
                try
                {
                    var jobTitles = db.JobPosts
                        .Where(j => j.Status == JobStatusHelper.Published && j.Title != null)
                        .Select(j => j.Title)
                        .ToList()
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                    
                    var keywords = jobTitles
                        .SelectMany(title => title.Split(new[] { ' ', ',', '.', '-', '_', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries))
                        .Where(word => word.Length > 3 && !word.All(char.IsDigit))
                        .GroupBy(word => word.ToLower())
                        .OrderByDescending(g => g.Count())
                        .Take(10)
                        .Select(g => g.Key)
                        .ToList();
                    
                    popularKeywords = keywords;
                }
                catch
                {
                    // Fallback to default keywords if extraction fails
                    popularKeywords = new List<string> { "Developer", "Designer", "Manager", "Engineer", "Analyst" };
                }

                var viewModel = new HomeIndexViewModel
                {
                    TotalCandidates = totalCandidates,
                    TotalJobPosts = totalJobPosts,
                    TotalHiredJobs = totalHiredJobs,
                    TotalCompanies = totalCompanies,
                    RecentJobs = jobViewModels,
                    TotalJobsCount = totalJobsCount
                };

                ViewBag.Locations = locations;
                ViewBag.PopularKeywords = popularKeywords;

                return View(viewModel);
            }
        }

        // Đã loại bỏ các phương thức trùng lặp - hiện sử dụng JobMapper class
        // Đã loại bỏ GetEmploymentTypeDisplay trùng lặp - hiện sử dụng EmploymentTypeHelper
        // Đã loại bỏ FormatSalaryRange trùng lặp - hiện sử dụng SalaryHelper

        [AllowAnonymous]
        public ActionResult Login()
        {
            return RedirectToAction("Login", "Account");
        }

        public ActionResult About()
        {
            using (var db = DbContextFactory.CreateReadOnly())
            {
                db.ObjectTrackingEnabled = false;

                // Get statistics for About page
                var totalCandidates = db.Candidates.Count(c => c.ActiveFlag == 1);
                JobStatusHelper.NormalizeStatuses(db);
                var totalJobPosts = db.JobPosts.Count(j => j.Status == JobStatusHelper.Published);
                var totalHiredJobs = db.Applications.Count(a => a.Status == "Hired" || a.Status == "Accepted");
                var totalCompanies = db.Companies.Count(c => c.ActiveFlag == 1);

                ViewBag.TotalCandidates = totalCandidates;
                ViewBag.TotalJobPosts = totalJobPosts;
                ViewBag.TotalHiredJobs = totalHiredJobs;
                ViewBag.TotalCompanies = totalCompanies;

                return View();
            }
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
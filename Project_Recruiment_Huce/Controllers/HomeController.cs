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

namespace Project_Recruiment_Huce.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString))
            {
                // Eager load related entities for better performance
                var loadOptions = new System.Data.Linq.DataLoadOptions();
                loadOptions.LoadWith<JobPost>(j => j.Company);
                loadOptions.LoadWith<JobPost>(j => j.Recruiter);
                db.LoadOptions = loadOptions;
                db.ObjectTrackingEnabled = false;

                // Get statistics
                var totalCandidates = db.Candidates.Count(c => c.ActiveFlag == 1);
                var totalJobPosts = db.JobPosts.Count(j => j.Status == "Published");
                var totalHiredJobs = db.Applications.Count(a => a.Status == "Hired" || a.Status == "Accepted");
                var totalCompanies = db.Companies.Count(c => c.ActiveFlag == 1);

                // Get recent published jobs (limit to 6-7 for homepage)
                var recentJobs = db.JobPosts
                    .Where(j => j.Status == "Published")
                    .OrderByDescending(j => j.PostedAt ?? j.UpdatedAt ?? (DateTime?)SqlDateTime.MinValue.Value)
                    .Take(7)
                    .ToList();

                // Map to ViewModels
                var jobViewModels = recentJobs.Select(j => MapToJobListingItem(j, db)).ToList();

                // Get total jobs count
                var totalJobsCount = db.JobPosts.Count(j => j.Status == "Published");

                var viewModel = new HomeIndexViewModel
                {
                    TotalCandidates = totalCandidates,
                    TotalJobPosts = totalJobPosts,
                    TotalHiredJobs = totalHiredJobs,
                    TotalCompanies = totalCompanies,
                    RecentJobs = jobViewModels,
                    TotalJobsCount = totalJobsCount
                };

                return View(viewModel);
            }
        }

        /// <summary>
        /// Map JobPost entity to JobListingItemViewModel (helper method)
        /// </summary>
        private JobListingItemViewModel MapToJobListingItem(JobPost job, JOBPORTAL_ENDataContext db)
        {
            string companyName = job.Company != null ? job.Company.CompanyName : 
                                (job.Recruiter != null ? job.Recruiter.FullName : "N/A");
            
            // Get company logo URL
            string logoUrl = "/Content/images/job_logo_1.jpg"; // Default logo
            if (job.Company?.PhotoID.HasValue == true)
            {
                var photo = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == job.Company.PhotoID.Value);
                if (photo != null && !string.IsNullOrEmpty(photo.FilePath))
                {
                    logoUrl = photo.FilePath;
                }
            }
            
            // Get employment type display
            string employmentTypeDisplay = GetEmploymentTypeDisplay(job.EmploymentType);
            
            return new JobListingItemViewModel
            {
                JobPostID = job.JobPostID,
                JobCode = job.JobCode,
                Title = job.Title,
                Description = job.Description,
                CompanyName = companyName,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                EmploymentTypeDisplay = employmentTypeDisplay,
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                SalaryCurrency = job.SalaryCurrency,
                SalaryRange = FormatSalaryRange(job.SalaryFrom, job.SalaryTo, job.SalaryCurrency),
                PostedAt = job.PostedAt,
                ApplicationDeadline = job.ApplicationDeadline,
                Status = job.Status,
                LogoUrl = logoUrl
            };
        }

        /// <summary>
        /// Get display text for employment type
        /// </summary>
        private string GetEmploymentTypeDisplay(string employmentType)
        {
            if (string.IsNullOrEmpty(employmentType))
                return "";

            string empType = employmentType.ToLower();
            if (empType == "part-time" || empType == "part time")
                return "Bán thời gian";
            else if (empType == "full-time" || empType == "full time")
                return "Toàn thời gian";
            else
                return employmentType;
        }

        /// <summary>
        /// Format salary range for display
        /// </summary>
        private string FormatSalaryRange(decimal? salaryFrom, decimal? salaryTo, string currency)
        {
            if (!salaryFrom.HasValue && !salaryTo.HasValue)
                return "Thỏa thuận";

            string currencyDisplay = currency == "VND" ? "VNĐ" : currency ?? "VNĐ";

            if (salaryFrom.HasValue && salaryTo.HasValue)
            {
                return $"{salaryFrom.Value:N0} - {salaryTo.Value:N0} {currencyDisplay}";
            }
            else if (salaryFrom.HasValue)
            {
                return $"Từ {salaryFrom.Value:N0} {currencyDisplay}";
            }
            else if (salaryTo.HasValue)
            {
                return $"Đến {salaryTo.Value:N0} {currencyDisplay}";
            }

            return "Thỏa thuận";
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return RedirectToAction("Login", "Account");
        }

        public ActionResult About()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString))
            {
                db.ObjectTrackingEnabled = false;

                // Get statistics for About page
                var totalCandidates = db.Candidates.Count(c => c.ActiveFlag == 1);
                var totalJobPosts = db.JobPosts.Count(j => j.Status == "Published");
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
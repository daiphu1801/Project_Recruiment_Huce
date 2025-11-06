using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Jobs;

namespace Project_Recruiment_Huce.Controllers
{
    public class JobDetailsController : Controller
    {
        /// <summary>
        /// Map JobPost entity to JobDetailsViewModel
        /// </summary>
        private JobDetailsViewModel MapToJobDetails(JobPost job)
        {
            string companyName = job.Company != null ? job.Company.CompanyName : 
                                (job.Recruiter != null ? job.Recruiter.FullName : "N/A");
            
            return new JobDetailsViewModel
            {
                JobPostID = job.JobPostID,
                JobCode = job.JobCode,
                Title = job.Title,
                Description = job.Description,
                Requirements = job.Requirements,
                CompanyName = companyName,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                EmploymentTypeDisplay = GetEmploymentTypeDisplay(job.EmploymentType),
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                SalaryCurrency = job.SalaryCurrency,
                SalaryRange = FormatSalaryRange(job.SalaryFrom, job.SalaryTo, job.SalaryCurrency),
                PostedAt = job.PostedAt,
                UpdatedAt = job.UpdatedAt,
                ApplicationDeadline = job.ApplicationDeadline,
                LogoUrl = "/Content/images/job_logo_1.jpg"
            };
        }

        /// <summary>
        /// Map JobPost entity to RelatedJobViewModel
        /// </summary>
        private RelatedJobViewModel MapToRelatedJob(JobPost job)
        {
            string companyName = job.Company != null ? job.Company.CompanyName : 
                                (job.Recruiter != null ? job.Recruiter.FullName : "N/A");
            
            return new RelatedJobViewModel
            {
                JobPostID = job.JobPostID,
                Title = job.Title,
                CompanyName = companyName,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                EmploymentTypeDisplay = GetEmploymentTypeDisplay(job.EmploymentType),
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                SalaryCurrency = job.SalaryCurrency,
                SalaryRange = FormatSalaryRange(job.SalaryFrom, job.SalaryTo, job.SalaryCurrency),
                PostedAt = job.PostedAt,
                LogoUrl = "/Content/images/job_logo_1.jpg"
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

        /// <summary>
        /// Display job details
        /// </summary>
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("JobsListing", "Jobs");
            }

            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            using (var db = new JOBPORTAL_ENDataContext(connectionString))
            {
                db.ObjectTrackingEnabled = false;

                // Eager load related entities for better performance
                var loadOptions = new System.Data.Linq.DataLoadOptions();
                loadOptions.LoadWith<JobPost>(j => j.Company);
                loadOptions.LoadWith<JobPost>(j => j.Recruiter);
                db.LoadOptions = loadOptions;

                var job = db.JobPosts.FirstOrDefault(j => j.JobPostID == id.Value && j.Status == "Published");

                if (job == null)
                {
                    return RedirectToAction("JobsListing", "Jobs");
                }

                // Get related jobs (same company or similar location)
                // Use SqlDateTime.MinValue.Value (1753-01-01) instead of DateTime.MinValue to avoid SqlDateTime overflow
                var relatedJobs = db.JobPosts
                    .Where(j => j.JobPostID != id.Value && 
                               j.Status == "Published" &&
                               (j.CompanyID == job.CompanyID || 
                                (j.Location != null && job.Location != null && j.Location == job.Location)))
                    .OrderByDescending(j => j.PostedAt ?? j.UpdatedAt ?? (DateTime?)SqlDateTime.MinValue.Value)
                    .Take(5)
                    .ToList();

                // Map to ViewModels
                var jobDetailsViewModel = MapToJobDetails(job);
                var relatedJobsViewModels = relatedJobs.Select(j => MapToRelatedJob(j)).ToList();

                ViewBag.RelatedJobs = relatedJobsViewModels;

                return View(jobDetailsViewModel);
            }
        }
    }
}


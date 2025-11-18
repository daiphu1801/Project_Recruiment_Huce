using System;
using System.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Jobs;
using Project_Recruiment_Huce.Models.Candidates;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Mappers
{
    /// <summary>
    /// Mapper class for JobPost entity to various ViewModels
    /// Centralizes mapping logic to avoid duplication across controllers
    /// </summary>
    public static class JobMapper
    {
        /// <summary>
        /// Map JobPost entity to JobListingItemViewModel
        /// </summary>
        /// <param name="job">JobPost entity</param>
        /// <param name="db">Optional database context for calculating pending applications</param>
        /// <returns>JobListingItemViewModel</returns>
        public static JobListingItemViewModel MapToListingItem(JobPost job, JOBPORTAL_ENDataContext db = null)
        {
            if (job == null) return null;

            // Normalize status
            job.Status = JobStatusHelper.NormalizeStatus(job.Status);

            // Get company name - prioritize Company, then Recruiter's company, then Recruiter name
            string companyName = job.Company != null ? job.Company.CompanyName :
                                (job.Recruiter?.Company != null ? job.Recruiter.Company.CompanyName :
                                (job.Recruiter != null ? job.Recruiter.FullName : string.Empty));

            // Get logo URL using helper
            string logoUrl = CompanyLogoHelper.GetLogoUrl(job);

            // Calculate pending applications count if db context is provided
            int pendingCount = 0;
            if (db != null)
            {
                var pendingStatuses = new[] { "Under review", "Interview", "Offered" };
                pendingCount = db.Applications
                    .Where(a => a.JobPostID == job.JobPostID &&
                               a.Status != null &&
                               pendingStatuses.Contains(a.Status))
                    .Count();
            }

            return new JobListingItemViewModel
            {
                JobPostID = job.JobPostID,
                JobCode = job.JobCode,
                Title = job.Title,
                Description = job.Description,
                CompanyName = companyName,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                EmploymentTypeDisplay = EmploymentTypeHelper.GetDisplay(job.EmploymentType),
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                SalaryCurrency = job.SalaryCurrency,
                SalaryRange = SalaryHelper.FormatSalaryRange(job.SalaryFrom, job.SalaryTo, job.SalaryCurrency),
                PostedAt = job.PostedAt,
                ApplicationDeadline = job.ApplicationDeadline,
                Status = job.Status,
                LogoUrl = logoUrl,
                PendingApplicationsCount = pendingCount,
                ViewCount = job.ViewCount
            };
        }

        /// <summary>
        /// Map JobPost entity to JobDetailsViewModel
        /// </summary>
        /// <param name="job">JobPost entity</param>
        /// <returns>JobDetailsViewModel</returns>
        public static JobDetailsViewModel MapToDetails(JobPost job)
        {
            if (job == null) return null;

            // Normalize status
            job.Status = JobStatusHelper.NormalizeStatus(job.Status);

            // Get company name - prioritize Company, then Recruiter's company, then Recruiter name
            string companyName = job.Company != null ? job.Company.CompanyName :
                                (job.Recruiter?.Company != null ? job.Recruiter.Company.CompanyName :
                                (job.Recruiter != null ? job.Recruiter.FullName : string.Empty));

            // Get logo URL using helper
            string logoUrl = CompanyLogoHelper.GetLogoUrl(job);

            // Get job detail
            var jobDetail = job.JobPostDetails?.FirstOrDefault();

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
                EmploymentTypeDisplay = EmploymentTypeHelper.GetDisplay(job.EmploymentType),
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                SalaryCurrency = job.SalaryCurrency,
                SalaryRange = SalaryHelper.FormatSalaryRange(job.SalaryFrom, job.SalaryTo, job.SalaryCurrency),
                PostedAt = job.PostedAt,
                UpdatedAt = job.UpdatedAt,
                ApplicationDeadline = job.ApplicationDeadline,
                Status = job.Status,
                LogoUrl = logoUrl,
                Industry = jobDetail?.Industry,
                Major = jobDetail?.Major,
                YearsExperience = jobDetail?.YearsExperience,
                DegreeRequired = jobDetail?.DegreeRequired,
                Skills = jobDetail?.Skills,
                Headcount = jobDetail?.Headcount,
                GenderRequirement = jobDetail?.GenderRequirement,
                AgeFrom = jobDetail?.AgeFrom,
                AgeTo = jobDetail?.AgeTo,
                DetailStatus = jobDetail?.Status,
                ViewCount = job.ViewCount
            };
        }

        /// <summary>
        /// Map JobPost entity to RelatedJobViewModel
        /// </summary>
        /// <param name="job">JobPost entity</param>
        /// <returns>RelatedJobViewModel</returns>
        public static RelatedJobViewModel MapToRelatedJob(JobPost job)
        {
            if (job == null) return null;

            // Normalize status
            job.Status = JobStatusHelper.NormalizeStatus(job.Status);

            // Get company name - prioritize Company, then Recruiter's company, then Recruiter name
            string companyName = job.Company != null ? job.Company.CompanyName :
                                (job.Recruiter?.Company != null ? job.Recruiter.Company.CompanyName :
                                (job.Recruiter != null ? job.Recruiter.FullName : string.Empty));

            // Get logo URL using helper
            string logoUrl = CompanyLogoHelper.GetLogoUrl(job);

            return new RelatedJobViewModel
            {
                JobPostID = job.JobPostID,
                Title = job.Title,
                CompanyName = companyName,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                EmploymentTypeDisplay = EmploymentTypeHelper.GetDisplay(job.EmploymentType),
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                SalaryCurrency = job.SalaryCurrency,
                SalaryRange = SalaryHelper.FormatSalaryRange(job.SalaryFrom, job.SalaryTo, job.SalaryCurrency),
                PostedAt = job.PostedAt,
                LogoUrl = logoUrl
            };
        }

        /// <summary>
        /// Map SavedJob entity to SavedJobViewModel
        /// </summary>
        /// <param name="savedJob">SavedJob entity</param>
        /// <returns>SavedJobViewModel</returns>
        public static SavedJobViewModel MapToSavedJob(SavedJob savedJob)
        {
            if (savedJob == null) return null;

            var job = savedJob.JobPost;
            if (job == null) return null;

            // Get company name - prioritize Company, then Recruiter's company, then Recruiter name
            string companyName = job.Company != null ? job.Company.CompanyName :
                                (job.Recruiter?.Company != null ? job.Recruiter.Company.CompanyName :
                                (job.Recruiter != null ? job.Recruiter.FullName : string.Empty));

            // Get logo URL using helper
            string logoUrl = CompanyLogoHelper.GetLogoUrl(job);

            return new SavedJobViewModel
            {
                SavedJobID = savedJob.SavedJobID,
                JobPostID = job.JobPostID,
                JobCode = job.JobCode,
                Title = job.Title,
                CompanyName = companyName,
                Location = job.Location,
                EmploymentTypeDisplay = EmploymentTypeHelper.GetDisplay(job.EmploymentType),
                SalaryRange = SalaryHelper.FormatSalaryRange(job.SalaryFrom, job.SalaryTo, job.SalaryCurrency),
                SavedAt = savedJob.SavedAt,
                ApplicationDeadline = job.ApplicationDeadline,
                LogoUrl = logoUrl
            };
        }
    }
}


using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Jobs;

namespace Project_Recruiment_Huce.Services.JobService
{
    /// <summary>
    /// Interface định nghĩa business logic cho tin tuyển dụng
    /// Xử lý validation, sanitization và orchestration giữa các repositories
    /// </summary>
    public interface IJobService
    {
        // Create job operations
        CreateJobResult ValidateAndCreateJob(JobCreateViewModel viewModel, int recruiterId);
        
        // Edit job operations
        EditJobResult GetJobForEdit(int jobPostId, int recruiterId);
        UpdateJobResult ValidateAndUpdateJob(int jobPostId, JobCreateViewModel viewModel, int recruiterId);
        
        // Status operations
        ServiceResult CloseJob(int jobPostId, int recruiterId, string status);
        ServiceResult ReopenJob(int jobPostId, int recruiterId);
    }

    /// <summary>
    /// Result object cho create job operation
    /// </summary>
    public class CreateJobResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> Errors { get; set; }
        public int? JobPostId { get; set; }

        public CreateJobResult()
        {
            Errors = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Result object cho edit job operation
    /// </summary>
    public class EditJobResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public JobCreateViewModel ViewModel { get; set; }
        public List<SelectListItem> Companies { get; set; }
        public int JobPostId { get; set; }
    }

    /// <summary>
    /// Result object cho update job operation
    /// </summary>
    public class UpdateJobResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> Errors { get; set; }

        public UpdateJobResult()
        {
            Errors = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Generic service result object
    /// </summary>
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string WarningMessage { get; set; }
        public string RedirectAction { get; set; }
        public object RedirectRouteValues { get; set; }
    }
}

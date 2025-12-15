using System.Collections.Generic;
using Project_Recruiment_Huce.Models.Recruiters;

namespace Project_Recruiment_Huce.Services.RecruiterApplicationService
{
    /// <summary>
    /// Service interface for Recruiter Application operations
    /// Handles business logic for application management by recruiters
    /// </summary>
    public interface IRecruiterApplicationService
    {
        /// <summary>
        /// Get paginated list of applications for recruiter with filters
        /// </summary>
        ApplicationListResult GetApplicationsList(int recruiterId, int? jobId, string status, int page, int pageSize);

        /// <summary>
        /// Get detailed information about a specific application
        /// </summary>
        ApplicationDetailsResult GetApplicationDetails(int applicationId, int recruiterId);

        /// <summary>
        /// Get application for status update form
        /// </summary>
        ApplicationStatusFormResult GetApplicationForStatusUpdate(int applicationId, int recruiterId);

        /// <summary>
        /// Update application status with validation
        /// </summary>
        UpdateStatusResult UpdateApplicationStatus(int applicationId, int recruiterId, string status, string note);

        /// <summary>
        /// Get list of jobs for filter dropdown
        /// </summary>
        List<JobFilterItem> GetJobsForFilter(int recruiterId);
<<<<<<< HEAD
        ServiceResult ScheduleInterview(InterviewScheduleViewModel viewModel, int recruiterId);
=======
>>>>>>> b5687619104f46f9178da37581c63d949fa94225
    }

    #region Result Classes

    /// <summary>
    /// Result for paginated application list
    /// </summary>
    public class ApplicationListResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<RecruiterApplicationViewModel> Applications { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int? JobId { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Result for application details
    /// </summary>
    public class ApplicationDetailsResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public RecruiterApplicationViewModel Application { get; set; }
    }

    /// <summary>
    /// Result for application status form
    /// </summary>
    public class ApplicationStatusFormResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public UpdateApplicationStatusViewModel ViewModel { get; set; }
    }

    /// <summary>
    /// Result for status update operation
    /// </summary>
    public class UpdateStatusResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public int ApplicationId { get; set; }
    }

    /// <summary>
    /// Job filter item for dropdown
    /// </summary>
    public class JobFilterItem
    {
        public int JobPostID { get; set; }
        public string Title { get; set; }
        public string JobCode { get; set; }
    }

    #endregion
}

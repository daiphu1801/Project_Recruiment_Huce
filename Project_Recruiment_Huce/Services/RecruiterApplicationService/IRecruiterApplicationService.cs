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

        /// <summary>
        /// Schedule interview and send email
        /// </summary>
        ServiceResult ScheduleInterview(InterviewScheduleViewModel viewModel, int recruiterId);

        /// <summary>
        /// Get schedule interview form with subscription and status validation
        /// </summary>
        ScheduleInterviewFormResult GetScheduleInterviewForm(int applicationId, int recruiterId);

        /// <summary>
        /// Get status options for dropdown
        /// </summary>
        List<StatusOption> GetStatusOptions();

        /// <summary>
        /// Get interview type options for dropdown
        /// </summary>
        List<InterviewTypeOption> GetInterviewTypeOptions();
    }
}

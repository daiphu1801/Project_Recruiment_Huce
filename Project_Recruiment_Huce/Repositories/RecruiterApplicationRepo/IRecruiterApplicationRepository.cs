using System.Collections.Generic;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.RecruiterApplicationRepo
{
    /// <summary>
    /// Repository interface for Recruiter Application operations
    /// Handles data access for recruiter-specific application management
    /// </summary>
    public interface IRecruiterApplicationRepository
    {
        /// <summary>
        /// Get recruiter ID by account ID
        /// </summary>
        int? GetRecruiterIdByAccountId(int accountId);

        /// <summary>
        /// Get application with all related entities loaded (JobPost, Candidate, Company)
        /// </summary>
        Application GetApplicationByIdWithDetails(int applicationId);

        /// <summary>
        /// Get all applications for jobs posted by a specific recruiter
        /// Includes eager loading of related entities
        /// </summary>
        IEnumerable<Application> GetApplicationsByRecruiter(int recruiterId);

        /// <summary>
        /// Get list of jobs posted by a specific recruiter for filter dropdown
        /// </summary>
        List<JobPost> GetJobsByRecruiter(int recruiterId);

        /// <summary>
        /// Check if application belongs to recruiter's job
        /// </summary>
        bool IsApplicationOwnedByRecruiter(int applicationId, int recruiterId);

        /// <summary>
        /// Update application status and note
        /// </summary>
        void UpdateApplicationStatus(int applicationId, string status, string note);

        /// <summary>
        /// Save changes to database
        /// </summary>
        void SaveChanges();
        // Các hàm lấy dữ liệu
        Application GetApplicationById(int id);
    }
}

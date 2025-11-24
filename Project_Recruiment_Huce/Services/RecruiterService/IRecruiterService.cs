using System;
using System.Web;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Services.RecruiterService
{
    /// <summary>
    /// Service interface for Recruiter profile operations
    /// </summary>
    public interface IRecruiterService
    {
        /// <summary>
        /// Get or create recruiter profile for account
        /// </summary>
        RecruiterProfileResult GetOrCreateProfile(int accountId, string userName);

        /// <summary>
        /// Update recruiter profile with validation
        /// </summary>
        UpdateRecruiterResult UpdateProfile(int accountId, Recruiter recruiter, HttpPostedFileBase avatar, Func<string, string> serverMapPath);

        /// <summary>
        /// Validate recruiter data
        /// </summary>
        ValidationResult ValidateRecruiter(Recruiter recruiter, int accountId);
    }

    #region Result Classes

    /// <summary>
    /// Result for get/create profile operation
    /// </summary>
    public class RecruiterProfileResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Recruiter Recruiter { get; set; }
    }

    /// <summary>
    /// Result for update profile operation
    /// </summary>
    public class UpdateRecruiterResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public System.Collections.Generic.Dictionary<string, string> Errors { get; set; }

        public UpdateRecruiterResult()
        {
            Errors = new System.Collections.Generic.Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Result for validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public System.Collections.Generic.Dictionary<string, string> Errors { get; set; }
        public string NormalizedPhone { get; set; }
        public string NormalizedEmail { get; set; }

        public ValidationResult()
        {
            Errors = new System.Collections.Generic.Dictionary<string, string>();
        }
    }

    #endregion
}

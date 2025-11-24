using System;
using System.Linq;
using System.Configuration;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class for company logo URL retrieval
    /// Centralized logic to avoid duplication across controllers
    /// </summary>
    public static class CompanyLogoHelper
    {
        private const string DefaultLogoUrl = "/Content/images/job_logo_1.jpg";

        /// <summary>
        /// Get company logo URL from JobPost
        /// </summary>
        /// <param name="job">JobPost entity</param>
        /// <returns>Logo URL or default logo</returns>
        public static string GetLogoUrl(JobPost job)
        {
            // Try to get logo from Company first
            if (job?.Company?.PhotoID != null && job.Company.PhotoID.HasValue)
            {
                return GetLogoUrlByPhotoId(job.Company.PhotoID.Value);
            }

            // If no Company, try to get from Recruiter's Company
            if (job?.Recruiter?.Company?.PhotoID != null && job.Recruiter.Company.PhotoID.HasValue)
            {
                return GetLogoUrlByPhotoId(job.Recruiter.Company.PhotoID.Value);
            }

            return DefaultLogoUrl;
        }

        /// <summary>
        /// Get company logo URL from Company entity
        /// </summary>
        /// <param name="company">Company entity</param>
        /// <returns>Logo URL or default logo</returns>
        public static string GetLogoUrl(Company company)
        {
            if (company?.PhotoID == null || !company.PhotoID.HasValue)
            {
                return DefaultLogoUrl;
            }

            return GetLogoUrlByPhotoId(company.PhotoID.Value);
        }

        /// <summary>
        /// Get company logo URL from PhotoID
        /// </summary>
        /// <param name="photoId">Photo ID</param>
        /// <returns>Logo URL or default logo</returns>
        public static string GetLogoUrlByPhotoId(int photoId)
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
                using (var db = new JOBPORTAL_ENDataContext(connectionString))
                {
                    db.ObjectTrackingEnabled = false;
                    var photo = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId);
                    if (photo != null && !string.IsNullOrEmpty(photo.FilePath))
                    {
                        return photo.FilePath;
                    }
                }
            }
            catch (Exception)
            {
                // If any error occurs, return default logo
                return DefaultLogoUrl;
            }

            return DefaultLogoUrl;
        }

        /// <summary>
        /// Get company logo URL with existing database context (more efficient)
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="photoId">Photo ID</param>
        /// <returns>Logo URL or default logo</returns>
        public static string GetLogoUrlWithContext(JOBPORTAL_ENDataContext db, int? photoId)
        {
            if (!photoId.HasValue)
            {
                return DefaultLogoUrl;
            }

            try
            {
                var photo = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value);
                if (photo != null && !string.IsNullOrEmpty(photo.FilePath))
                {
                    return photo.FilePath;
                }
            }
            catch (Exception)
            {
                return DefaultLogoUrl;
            }

            return DefaultLogoUrl;
        }

        /// <summary>
        /// Get default logo URL
        /// </summary>
        /// <returns>Default logo URL</returns>
        public static string GetDefaultLogoUrl()
        {
            return DefaultLogoUrl;
        }
    }
}


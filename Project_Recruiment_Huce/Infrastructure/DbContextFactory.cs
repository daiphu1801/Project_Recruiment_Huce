using System;
using System.Configuration;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Infrastructure
{
    /// <summary>
    /// Factory class for creating database context instances
    /// Centralizes connection string management and reduces code duplication
    /// </summary>
    public static class DbContextFactory
    {
        private static readonly string ConnectionString;

        static DbContextFactory()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"]?.ConnectionString;
            
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new InvalidOperationException("JOBPORTAL_ENConnectionString not found in configuration");
            }
        }

        /// <summary>
        /// Create a new database context instance
        /// </summary>
        /// <returns>New JOBPORTAL_ENDataContext instance</returns>
        public static JOBPORTAL_ENDataContext Create()
        {
            return new JOBPORTAL_ENDataContext(ConnectionString);
        }

        /// <summary>
        /// Create a new database context instance with object tracking disabled
        /// Use this for read-only operations for better performance
        /// </summary>
        /// <returns>New JOBPORTAL_ENDataContext instance with tracking disabled</returns>
        public static JOBPORTAL_ENDataContext CreateReadOnly()
        {
            var db = new JOBPORTAL_ENDataContext(ConnectionString);
            db.ObjectTrackingEnabled = false;
            return db;
        }

        /// <summary>
        /// Get the connection string
        /// </summary>
        /// <returns>Connection string</returns>
        public static string GetConnectionString()
        {
            return ConnectionString;
        }
    }
}


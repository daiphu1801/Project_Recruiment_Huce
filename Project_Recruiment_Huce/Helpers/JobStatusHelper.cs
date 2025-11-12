using System;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Helpers
{
    public static class JobStatusHelper
    {
        public const string Published = "Published";
        public const string Closed = "Closed";
        public const string Hidden = "Hidden";
        public const string Draft = "Draft";

        /// <summary>
        /// Convert legacy status values (e.g. Visible) to current equivalents.
        /// </summary>
        public static void NormalizeStatuses(JOBPORTAL_ENDataContext db)
        {
            if (db == null) return;

            if (db.JobPosts.Any(j => j.Status == "Visible"))
            {
                db.ExecuteCommand("UPDATE [dbo].[JobPosts] SET [Status] = {0} WHERE [Status] = {1}",
                    Published, "Visible");
            }
        }

        /// <summary>
        /// Returns the normalized status string without persisting it.
        /// </summary>
        public static string NormalizeStatus(string status)
        {
            if (string.Equals(status, "Visible", StringComparison.OrdinalIgnoreCase))
            {
                return Published;
            }
            return status;
        }

        public static bool IsPublished(string status)
        {
            return string.Equals(status, Published, StringComparison.OrdinalIgnoreCase);
        }
    }
}


using System.Web.Mvc;

namespace Project_Recruiment_Huce.Areas.Admin.Helpers
{
    /// <summary>
    /// Helper class for managing notification messages in Admin area
    /// Provides methods to set success, error, warning, and info messages
    /// </summary>
    public static class NotificationHelper
    {
        /// <summary>
        /// Set success message to display to user
        /// </summary>
        public static void SetSuccessMessage(this Controller controller, string message)
        {
            controller.TempData["SuccessMessage"] = message;
        }

        /// <summary>
        /// Set error message to display to user
        /// </summary>
        public static void SetErrorMessage(this Controller controller, string message)
        {
            controller.TempData["ErrorMessage"] = message;
        }

        /// <summary>
        /// Set warning message to display to user
        /// </summary>
        public static void SetWarningMessage(this Controller controller, string message)
        {
            controller.TempData["WarningMessage"] = message;
        }

        /// <summary>
        /// Set info message to display to user
        /// </summary>
        public static void SetInfoMessage(this Controller controller, string message)
        {
            controller.TempData["InfoMessage"] = message;
        }

        /// <summary>
        /// Set error message for foreign key constraint violation
        /// </summary>
        public static void SetForeignKeyError(this Controller controller, string entityName, string relatedEntity, int count)
        {
            controller.TempData["ErrorMessage"] = $"Không thể xóa {entityName} này vì có {count} {relatedEntity} đang được liên kết. Vui lòng xóa hoặc chuyển {relatedEntity} trước.";
        }

        /// <summary>
        /// Set error message for account deletion with job posts
        /// </summary>
        public static void SetAccountDeleteError(this Controller controller, string accountType, int jobPostCount, int applicationCount)
        {
            if (jobPostCount > 0)
            {
                controller.TempData["ErrorMessage"] = $"Không thể xóa tài khoản {accountType} này vì có {jobPostCount} tin tuyển dụng đang được liên kết. Vui lòng xóa hoặc chuyển các tin tuyển dụng trước.";
            }
            else if (applicationCount > 0)
            {
                controller.TempData["ErrorMessage"] = $"Không thể xóa tài khoản {accountType} này vì có {applicationCount} đơn ứng tuyển liên quan. Vui lòng xử lý các đơn ứng tuyển trước.";
            }
        }
    }
}

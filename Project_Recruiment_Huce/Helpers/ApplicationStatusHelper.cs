using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để quản lý trạng thái của đơn ứng tuyển
    /// Cung cấp các phương thức chuyển đổi giữa giá trị tiếng Anh (database) và tiếng Việt (hiển thị)
    /// </summary>
    public static class ApplicationStatusHelper
    {
        // Giá trị lưu trong database (tiếng Anh)
        public const string UnderReview = "Under review"; // Đang xem xét
        public const string Interview = "Interview";       // Phỏng vấn
        public const string Offered = "Offered";           // Đã đề nghị
        public const string Hired = "Hired";               // Đã nhận việc
        public const string Rejected = "Rejected";         // Từ chối

        /// <summary>
        /// Lấy SelectList để hiển thị trong dropdown với text tiếng Việt
        /// Value vẫn là tiếng Anh để tương thích với database
        /// </summary>
        /// <param name="selectedValue">Giá trị được chọn mặc định</param>
        /// <returns>SelectList cho dropdown</returns>
        public static SelectList GetStatusSelectList(string selectedValue = null)
        {
            var statusList = new List<SelectListItem>
            {
                new SelectListItem { Value = UnderReview, Text = "Đang xem xét" },
                new SelectListItem { Value = Interview, Text = "Phỏng vấn" },
                new SelectListItem { Value = Offered, Text = "Đã đề nghị" },
                new SelectListItem { Value = Hired, Text = "Đã nhận" },
                new SelectListItem { Value = Rejected, Text = "Từ chối" }
            };

            return new SelectList(statusList, "Value", "Text", selectedValue);
        }

        /// <summary>
        /// Chuyển đổi giá trị status từ database (tiếng Anh) sang text hiển thị (tiếng Việt)
        /// </summary>
        /// <param name="status">Giá trị status từ database</param>
        /// <returns>Text hiển thị tiếng Việt</returns>
        public static string GetStatusDisplayText(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "Đang xem xét";

            switch (status.ToLower())
            {
                case "under review":
                    return "Đang xem xét";
                case "interview":
                    return "Phỏng vấn";
                case "offered":
                    return "Đã đề nghị";
                case "hired":
                    return "Đã nhận";
                case "rejected":
                    return "Từ chối";
                default:
                    return status;
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái có hợp lệ hay không
        /// </summary>
        /// <param name="status">Trạng thái cần kiểm tra</param>
        /// <returns>True nếu hợp lệ, false nếu không</returns>
        public static bool IsValidStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                return false;

            var validStatuses = new[] { UnderReview, Interview, Offered, Hired, Rejected };
            return Array.Exists(validStatuses, s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
        }
    }
}


using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Project_Recruiment_Huce.Helpers
{
    public static class ApplicationStatusHelper
    {
        // Giá trị lưu trong database (tiếng Anh)
        public const string UnderReview = "Under review";
        public const string Interview = "Interview";
        public const string Offered = "Offered";
        public const string Hired = "Hired";
        public const string Rejected = "Rejected";

        /// <summary>
        /// Lấy SelectList với text tiếng Việt nhưng value vẫn là tiếng Anh (để tương thích với database)
        /// </summary>
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
        /// Kiểm tra status có hợp lệ không
        /// </summary>
        public static bool IsValidStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                return false;

            var validStatuses = new[] { UnderReview, Interview, Offered, Hired, Rejected };
            return Array.Exists(validStatuses, s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
        }
    }
}


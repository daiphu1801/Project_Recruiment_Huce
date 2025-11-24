using System;
using System.Collections.Generic;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để hiển thị và chuyển đổi loại hình công việc
    /// Tập trung logic để tránh trùng lặp giữa các controllers
    /// </summary>
    public static class EmploymentTypeHelper
    {
        // Bảng ánh xạ giữa giá trị database (tiếng Anh) và text hiển thị (tiếng Việt)
        private static readonly Dictionary<string, string> _employmentTypeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "part-time", "Bán thời gian" },
            { "full-time", "Toàn thời gian" },
            { "internship", "Thực tập" },
            { "contract", "Hợp đồng" },
            { "remote", "Làm việc từ xa" }
        };

        /// <summary>
        /// Lấy text hiển thị tiếng Việt cho loại hình công việc
        /// </summary>
        /// <param name="employmentType">Loại hình công việc từ database</param>
        /// <returns>Text hiển thị tiếng Việt</returns>
        public static string GetDisplay(string employmentType)
        {
            if (string.IsNullOrWhiteSpace(employmentType))
                return string.Empty;

            var normalized = employmentType.Trim();
            if (_employmentTypeMapping.TryGetValue(normalized, out var display))
            {
                return display;
            }

            return employmentType;
        }

        /// <summary>
        /// Lấy tất cả các ánh xạ loại hình công việc
        /// </summary>
        /// <returns>Dictionary chứa các ánh xạ loại hình công việc</returns>
        public static Dictionary<string, string> GetMappings()
        {
            return new Dictionary<string, string>(_employmentTypeMapping);
        }

        /// <summary>
        /// Tìm ngược: Lấy giá trị database từ text hiển thị tiếng Việt
        /// Sử dụng khi cần chuyển từ tiếng Việt về giá trị database
        /// </summary>
        /// <param name="displayText">Text hiển thị tiếng Việt</param>
        /// <returns>Giá trị loại hình công việc trong database</returns>
        public static string GetDatabaseValue(string displayText)
        {
            if (string.IsNullOrWhiteSpace(displayText))
                return null;

            foreach (var kvp in _employmentTypeMapping)
            {
                if (kvp.Value.Equals(displayText, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Key;
                }
            }
            return displayText;
        }
    }
}


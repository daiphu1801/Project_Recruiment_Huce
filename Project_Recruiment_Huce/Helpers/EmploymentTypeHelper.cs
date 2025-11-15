using System;
using System.Collections.Generic;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class for employment type display and mapping
    /// Centralized logic to avoid duplication across controllers
    /// </summary>
    public static class EmploymentTypeHelper
    {
        private static readonly Dictionary<string, string> _employmentTypeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "part-time", "Bán thời gian" },
            { "part time", "Bán thời gian" },
            { "full-time", "Toàn thời gian" },
            { "full time", "Toàn thời gian" },
            { "internship", "Thực tập" },
            { "contract", "Hợp đồng" },
            { "remote", "Làm việc từ xa" }
        };

        /// <summary>
        /// Get Vietnamese display text for employment type
        /// </summary>
        /// <param name="employmentType">Employment type from database</param>
        /// <returns>Vietnamese display text</returns>
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
        /// Get all employment type mappings
        /// </summary>
        /// <returns>Dictionary of employment type mappings</returns>
        public static Dictionary<string, string> GetMappings()
        {
            return new Dictionary<string, string>(_employmentTypeMapping);
        }

        /// <summary>
        /// Reverse lookup: Get database value from Vietnamese display text
        /// </summary>
        /// <param name="displayText">Vietnamese display text</param>
        /// <returns>Database employment type value</returns>
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


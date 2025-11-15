using System;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để format và hiển thị mức lương
    /// </summary>
    public static class SalaryHelper
    {
        /// <summary>
        /// Format salary range cho hiển thị
        /// </summary>
        /// <param name="salaryFrom">Lương từ</param>
        /// <param name="salaryTo">Lương đến</param>
        /// <param name="currency">Loại tiền tệ (VND, USD, etc.)</param>
        /// <returns>Chuỗi hiển thị mức lương</returns>
        public static string FormatSalaryRange(decimal? salaryFrom, decimal? salaryTo, string currency)
        {
            if (!salaryFrom.HasValue && !salaryTo.HasValue)
                return "Thỏa thuận";

            string currencyDisplay = currency == "VND" ? "VNĐ" : currency ?? "VNĐ";

            if (salaryFrom.HasValue && salaryTo.HasValue)
            {
                return $"{salaryFrom.Value:N0} - {salaryTo.Value:N0} {currencyDisplay}";
            }
            else if (salaryFrom.HasValue)
            {
                return $"Từ {salaryFrom.Value:N0} {currencyDisplay}";
            }
            else if (salaryTo.HasValue)
            {
                return $"Đến {salaryTo.Value:N0} {currencyDisplay}";
            }

            return "Thỏa thuận";
        }

        /// <summary>
        /// Format salary range với format ngắn gọn
        /// </summary>
        public static string FormatSalaryRangeShort(decimal? salaryFrom, decimal? salaryTo, string currency)
        {
            if (!salaryFrom.HasValue && !salaryTo.HasValue)
                return "Thỏa thuận";

            string currencySymbol = currency == "VND" ? "VNĐ" : currency ?? "VNĐ";

            if (salaryFrom.HasValue && salaryTo.HasValue)
            {
                return $"{salaryFrom.Value:N0}-{salaryTo.Value:N0} {currencySymbol}";
            }
            else if (salaryFrom.HasValue)
            {
                return $"{salaryFrom.Value:N0}+ {currencySymbol}";
            }
            else if (salaryTo.HasValue)
            {
                return $"≤{salaryTo.Value:N0} {currencySymbol}";
            }

            return "Thỏa thuận";
        }
    }
}


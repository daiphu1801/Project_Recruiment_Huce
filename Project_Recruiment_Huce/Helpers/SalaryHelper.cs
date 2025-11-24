using System;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để format và hiển thị mức lương
    /// Cung cấp các phương thức để hiển thị lương theo nhiều định dạng khác nhau
    /// </summary>
    public static class SalaryHelper
    {
        /// <summary>
        /// Format khoảng lương cho hiển thị dạng đầy đủ (ví dụ: "10,000,000 - 15,000,000 VNĐ")
        /// Nếu không có thông tin lương sẽ hiển thị "Thỏa thuận"
        /// </summary>
        /// <param name="salaryFrom">Mức lương tối thiểu</param>
        /// <param name="salaryTo">Mức lương tối đa</param>
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
        /// Format khoảng lương với định dạng ngắn gọn (ví dụ: "10,000,000-15,000,000 VNĐ")
        /// Sử dụng cho các giao diện có giới hạn không gian hiển thị
        /// </summary>
        /// <param name="salaryFrom">Mức lương tối thiểu</param>
        /// <param name="salaryTo">Mức lương tối đa</param>
        /// <param name="currency">Loại tiền tệ (VND, USD, etc.)</param>
        /// <returns>Chuỗi hiển thị mức lương dạng ngắn</returns>
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


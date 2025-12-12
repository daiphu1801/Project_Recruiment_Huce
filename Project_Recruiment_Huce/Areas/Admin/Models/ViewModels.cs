using System;
using System.Collections.Generic;
using System.Globalization;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// Helper utilities shared by Admin pages
    /// </summary>
    public static class AdminUiHelpers
    {
        public static string FormatMoney(decimal? amount)
        {
            if (!amount.HasValue) return "";
            return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:n0}", amount.Value);
        }

        public static string Mask(string number)
        {
            if (string.IsNullOrEmpty(number) || number.Length <= 4) return number ?? string.Empty;
            return new string('*', number.Length - 4) + number.Substring(number.Length - 4);
        }
    }

    /// <summary>
    /// ViewModel cho Dashboard Admin
    /// </summary>
    public class DashboardVm
    {
        public int Accounts { get; set; }
        public int Companies { get; set; }
        public int Recruiters { get; set; }
        public int Candidates { get; set; }
        public int JobPosts { get; set; }
        public int Applications { get; set; }
        public int Transactions { get; set; }
        public List<string> Dates7 { get; set; }
        public List<int> JobPostsWeekly { get; set; }
        public List<int> ApplicationsWeekly { get; set; }
        // Phân bố theo loại hình công việc
        public List<string> EmploymentTypeLabels { get; set; }
        public List<int> EmploymentTypeCounts { get; set; }
        
        // Thống kê thanh toán SePay
        public decimal TotalAmountIn { get; set; }
        public decimal TotalAmountOut { get; set; }
        public decimal NetAmount { get; set; }
        public int TransactionsToday { get; set; }
        public int TransactionsThisMonth { get; set; }
        public List<decimal> PaymentWeekly { get; set; }
        public decimal AverageTransactionAmount { get; set; }
    }

    /// <summary>
    /// ViewModel cho Profile Admin
    /// </summary>
    public class ProfileVm
    {
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

using System;

namespace Project_Recruiment_Huce.Models.Accounts
{
    /// <summary>
    /// ViewModel chứa thông tin user từ Google OAuth
    /// </summary>
    public class GoogleUserInfoViewModel
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string GoogleId { get; set; }
        public string AvatarUrl { get; set; }
    }

    /// <summary>
    /// ViewModel kết quả xử lý Google Login (giữ nguyên trong Service)
    /// </summary>
    public class GoogleLoginResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Account Account { get; set; }
        public bool IsNewAccount { get; set; }
    }
}

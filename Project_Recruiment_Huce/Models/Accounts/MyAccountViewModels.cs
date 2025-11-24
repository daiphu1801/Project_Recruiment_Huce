using System;
using System.ComponentModel.DataAnnotations;

namespace Project_Recruiment_Huce.Models.Accounts
{
    /// <summary>
    /// ViewModel cho trang quản lý tài khoản của tôi
    /// </summary>
    public class MyAccountViewModel
    {
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string LoginEmail { get; set; } // Email đăng nhập (read-only)
        public string ContactEmail { get; set; } // Email liên lạc từ profile
        public string Phone { get; set; }
        public string Role { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel cho form đổi mật khẩu
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}


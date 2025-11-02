using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách tài khoản
    /// </summary>
    public class AccountListVm
    {
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PhotoUrl { get; set; }
    }

    /// <summary>
    /// ViewModel cho tạo tài khoản mới
    /// </summary>
    public class CreateAccountVm
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(100, ErrorMessage = "Tên đăng nhập tối đa 100 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; }

        public HttpPostedFileBase PhotoFile { get; set; }
    }

    /// <summary>
    /// ViewModel cho sửa tài khoản
    /// </summary>
    public class EditAccountVm
    {
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(100, ErrorMessage = "Tên đăng nhập tối đa 100 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string Role { get; set; }

        public bool Active { get; set; }
        public string CurrentPhotoUrl { get; set; }
        public HttpPostedFileBase PhotoFile { get; set; }
    }
}


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
        public byte? ActiveFlag { get; set; } // 1 = active, 0 = inactive
        public DateTime? CreatedAt { get; set; }
        public int? PhotoId { get; set; }
        public string PhotoUrl { get; set; } // From ProfilePhotos.FilePath

        // Helper property for display
        public bool Active => ActiveFlag == 1;
    }

    /// <summary>
    /// ViewModel cho tạo tài khoản mới
    /// </summary>
    public class CreateAccountVm
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(255, ErrorMessage = "Họ và tên tối đa 255 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

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

        // [REQ 6] Thêm trường mật khẩu mới
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới (để trống nếu không đổi)")]
        public string Password { get; set; }

        public byte? ActiveFlag { get; set; } // 1 = active, 0 = inactive
        public int? CurrentPhotoId { get; set; }
        public HttpPostedFileBase PhotoFile { get; set; }

        // Helper property for display
        public bool Active
        {
            get => ActiveFlag == 1;
            set => ActiveFlag = value ? (byte?)1 : (byte?)0;
        }
    }
}
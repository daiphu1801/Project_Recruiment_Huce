using System;
using System.ComponentModel.DataAnnotations;

namespace Project_Recruiment_Huce.Models.Home
{
    /// <summary>
    /// ViewModel cho form liên hệ từ user
    /// </summary>
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ")]
        [StringLength(50, ErrorMessage = "Họ không được quá 50 ký tự")]
        [Display(Name = "Họ")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [StringLength(50, ErrorMessage = "Tên không được quá 50 ký tự")]
        [Display(Name = "Tên")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chủ đề")]
        [StringLength(200, ErrorMessage = "Chủ đề không được quá 200 ký tự")]
        [Display(Name = "Chủ đề")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn")]
        [StringLength(5000, ErrorMessage = "Nội dung không được quá 5000 ký tự")]
        [Display(Name = "Nội dung")]
        public string Message { get; set; }
    }

    /// <summary>
    /// ViewModel hiển thị thông tin liên hệ của công ty
    /// </summary>
    public class ContactInfoViewModel
    {
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string WorkingHours { get; set; }
    }

    /// <summary>
    /// ViewModel kết hợp cho trang Contact
    /// </summary>
    public class ContactPageViewModel
    {
        public ContactViewModel ContactForm { get; set; }
        public ContactInfoViewModel ContactInfo { get; set; }
        public bool IsSubmitted { get; set; }
        public string SuccessMessage { get; set; }
    }
}

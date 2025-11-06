using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Project_Recruiment_Huce.Models.Candidates
{
    public class CandidateManageViewModel
    {
        public int? CandidateID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Giới tính")]
        public string Gender { get; set; }

        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [AllowHtml] // Cho phép HTML từ Quill editor
        [StringLength(500, ErrorMessage = "Giới thiệu không được vượt quá 500 ký tự")]
        [Display(Name = "Giới thiệu")]
        public string Summary { get; set; }
    }
}


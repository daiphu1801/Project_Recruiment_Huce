using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Project_Recruiment_Huce.Models.Companies
{
    /// <summary>
    /// ViewModel hiển thị chi tiết công ty cho ứng viên
    /// </summary>
    public class CompanyDetailsViewModel
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string TaxCode { get; set; }
        public string Industry { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string CompanyEmail { get; set; }
        public string Website { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public int ActiveJobCount { get; set; }
    }

    public class CompanyManageViewModel
    {
        public int? CompanyID { get; set; }

        [Required(ErrorMessage = "Tên công ty là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên công ty không được vượt quá 255 ký tự")]
        [Display(Name = "Tên công ty")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Mã số thuế là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã số thuế không được vượt quá 50 ký tự")]
        [Display(Name = "Mã số thuế")]
        public string TaxCode { get; set; }

        [Required(ErrorMessage = "Lĩnh vực hoạt động là bắt buộc")]
        [StringLength(150, ErrorMessage = "Lĩnh vực hoạt động không được vượt quá 150 ký tự")]
        [Display(Name = "Lĩnh vực hoạt động")]
        public string Industry { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Số fax là bắt buộc")]
        [StringLength(20, ErrorMessage = "Số fax không được vượt quá 20 ký tự")]
        [Display(Name = "Số fax")]
        public string Fax { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự")]
        [Display(Name = "Email công ty")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string CompanyEmail { get; set; }

        [Required(ErrorMessage = "Website là bắt buộc")]
        [StringLength(200, ErrorMessage = "Website không được vượt quá 200 ký tự")]
        [Display(Name = "Website")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        public string Website { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        [Display(Name = "Mô tả công ty")]
        [AllowHtml]
        public string Description { get; set; }

        [Display(Name = "Logo công ty")]
        public HttpPostedFileBase Logo { get; set; }

        public int? PhotoID { get; set; }
        public string LogoUrl { get; set; }
    }
}


using System;
using System.ComponentModel.DataAnnotations;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách công ty
    /// </summary>
    public class CompanyListVm
    {
        [Display(Name = "ID công ty")]
        public int CompanyId { get; set; }

        [Display(Name = "Tên công ty")]
        public string CompanyName { get; set; }

        [Display(Name = "Mã số thuế")]
        public string TaxCode { get; set; }

        [Display(Name = "Lĩnh vực")]
        public string Industry { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        public string CompanyEmail { get; set; }

        [Display(Name = "Website")]
        public string Website { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedAt { get; set; }
        public byte? ActiveFlag { get; set; } // 1 = active, 0 = inactive

        // Helper property for display
        [Display(Name = "Hoạt động")]
        public bool Active
        {
            get => ActiveFlag == 1;
            set => ActiveFlag = value ? (byte?)1 : (byte?)0;
        }
    }
    public class CreateCompanyVm
    {
        [Display(Name = "Tên công ty")]
        [Required(ErrorMessage = "Vui lòng nhập tên công ty")]
        [StringLength(255, ErrorMessage = "Tên công ty tối đa 255 ký tự")]
        public string CompanyName { get; set; }

        [Display(Name = "Mã số thuế")]
        [Required(ErrorMessage = "Vui lòng nhập mã số thuế")]
        [StringLength(20, ErrorMessage = "Mã số thuế không hợp lệ")]
        public string TaxCode { get; set; }

        [Display(Name = "Lĩnh vực")]
        [Required(ErrorMessage = "Vui lòng nhập lĩnh vực")]
        [StringLength(50, ErrorMessage = "Lĩnh vực tối đa 50 ký tự")]
        public string Industry { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(255, ErrorMessage = "Địa chỉ không hợp lệ")]
        public string Address { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string CompanyEmail { get; set; }

        [Display(Name = "Website")]
        [Required(ErrorMessage = "Vui lòng nhập wesite")]
        [StringLength(200, ErrorMessage = "Website không hợp lệ")]
        public string Website { get; set; }

        [Display(Name = "Mô tả")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [StringLength(1000, ErrorMessage = "Mô tả không hợp lệ")]
        public string Description { get; set; }

        public byte? ActiveFlag { get; set; }

        [Display(Name = "Hoạt động")]
        public bool Active
        {
            get => ActiveFlag == 1;
            set => ActiveFlag = value ? (byte?)1 : (byte?)0;
        }
    }

    public class EditCompanyVm
    {
        [Display(Name = "ID công ty")]
        public int CompanyId { get; set; }

        [Display(Name = "Tên công ty")]
        [Required(ErrorMessage = "Vui lòng nhập tên công ty")]
        [StringLength(255, ErrorMessage = "Tên công ty tối đa 200 ký tự")]
        public string CompanyName { get; set; }

        [Display(Name = "Mã số thuế")]
        [Required(ErrorMessage = "Vui lòng nhập mã số thuế")]
        [StringLength(50, ErrorMessage = "Mã số thuế tối đa 50 ký tự")]
        public string TaxCode { get; set; }

        [Display(Name = "Lĩnh vực")]
        [Required(ErrorMessage = "Vui lòng nhập lĩnh vực")]
        [StringLength(150, ErrorMessage = "Lĩnh vực tối đa 150 ký tự")]
        public string Industry { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(255, ErrorMessage = "Đại chỉ tối đa 255 ký tự")]
        public string Address { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email công ty")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string CompanyEmail { get; set; }

        [Display(Name = "Website")]
        [Required(ErrorMessage = "Vui lòng nhập wesite")]
        [StringLength(200, ErrorMessage = "Website tối đa 200 ký tự")]
        public string Website { get; set; }

        [Display(Name = "Mô tả")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        public string Description { get; set; }

        public byte? ActiveFlag { get; set; }

        [Display(Name = "Hoạt động")]
        public bool Active
        {
            get => ActiveFlag == 1;
            set => ActiveFlag = value ? (byte?)1 : (byte?)0;
        }
    }
}

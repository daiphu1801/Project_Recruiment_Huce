using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Xml.Linq;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách nhà tuyển dụng
    /// </summary>
    public class RecruiterListVm
    {
        [Display(Name = "ID nhà tuyển dụng")]
        public int RecruiterId { get; set; }

        [Display(Name = "Tài khoản")]
        public int AccountId { get; set; }

        [Display(Name = "Công ty")]
        public int? CompanyId { get; set; }

        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Display(Name = "Chức danh")]
        public string PositionTitle { get; set; }

        [Display(Name = "Email")]
        public string CompanyEmail { get; set; }

        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedAt { get; set; }

        public byte? ActiveFlag { get; set; } // 1 = active, 0 = inactive

        // Helper properties for display (from related tables)
        public string CompanyName { get; set; }

        [Display(Name = "Hoạt động")]
        // Helper property for display
        public bool Active
        {
            get => ActiveFlag == 1;
            set => ActiveFlag = value ? (byte?)1 : (byte?)0;
        }
        public int? PhotoId { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class CreateRecruiterVm
    {
        // Added AccountId and CompanyId so the Create view's dropdowns bind correctly.
        [Display(Name = "Tài khoản")]
        public int AccountId { get; set; }

        [Display(Name = "Công ty")]
        public int? CompanyId { get; set; }

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Vui lòng nhập tên nhà tuyển dụng")]
        [StringLength(100, ErrorMessage = "Tên nhà tuyển dụng tối đa 100 ký tự")]
        public string FullName { get; set; }

        [Display(Name = "Chức danh")]
        [Required(ErrorMessage = "Vui lòng nhập chức vụ")]
        [StringLength(100, ErrorMessage = "Chức vụ không hợp lệ")]
        public string PositionTitle { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string CompanyEmail { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(20)]
        public string Phone { get; set; }

        public byte? ActiveFlag { get; set; }

        [Display(Name = "Hoạt động")]
        public bool Active
        {
            get => ActiveFlag == 1;
            set => ActiveFlag = value ? (byte?)1 : (byte?)0;
        }
        public HttpPostedFileBase PhotoFile { get; set; }
    }

    public class EditRecruiterVm
    {
        [Display(Name = "ID Nhà tuyển dụng")]
        public int RecruiterId { get; set; }

        [Display(Name = "Tài khoản")]
        public int AccountId { get; set; }

        [Display(Name = "Công ty")]
        public int? CompanyId { get; set; }

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Vui lòng nhập tên nhà tuyển dụng")]
        [StringLength(100, ErrorMessage = "Tên nhà tuyển dụng tối đa 100 ký tự")]
        public string FullName { get; set; }

        [Display(Name = "Chức danh")]
        [Required(ErrorMessage = "Vui lòng nhập chức vụ")]
        [StringLength(100, ErrorMessage = "Chức vụ không hợp lệ")]
        public string PositionTitle { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string CompanyEmail { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(20)]
        public string Phone { get; set; }

        public byte? ActiveFlag { get; set; }

        [Display(Name = "Hoạt động")]
        public bool Active
        {
            get => ActiveFlag == 1;
            set => ActiveFlag = value ? (byte?)1 : (byte?)0;
        }
        public int? CurrentPhotoId { get; set; }
        public string CurrentPhotoUrl { get; set; }
        public HttpPostedFileBase PhotoFile { get; set; }
    }
}
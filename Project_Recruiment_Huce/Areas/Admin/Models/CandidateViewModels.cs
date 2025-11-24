using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách ứng viên
    /// </summary>
    public class CandidatesListVm
    {
        [Display(Name = "ID ứng viên")]
        public int CandidateId { get; set; }
        [Display(Name = "ID tài khoản")]
        public int AccountId { get; set; }
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }
        [Display(Name = "Ngày sinh")]
        public DateTime? DateOfBirth { get; set; } 
        [Display(Name = "Giới tính")]
        public string Gender { get; set; }
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }
        public int? PhotoId { get; set; }
        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedAt { get; set; }
        [Display(Name = "Trạng thái")]
        public byte? ActiveFlag { get; set; } // 1 = active, 0 = inactive

        // Helper properties for display (from related Account table)
        [Display(Name = "Email")]
        public string Email { get; set; }

        // Helper property for display
        [Display(Name = "Trạng thái hoạt động")]
        public bool Active => ActiveFlag == 1;

        // Alias for compatibility
        public DateTime? BirthDate => DateOfBirth;

        public string PhotoUrl { get;   set; }
        public string Address { get;   set; }
        public string Summary { get;   set; }
        public HttpPostedFileBase PhotoFile { get; set; }
        public string ApplicationEmail { get;   set; }
        public string CurrentPhotoUrl { get; set; }
    }
    public class CreateCandidatesListVm
    {
        [Display(Name = "ID tài khoản")]
        [Required(ErrorMessage = "Vui lòng nhập ID tài khoản")]
        public int AccountId { get; set; }
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string FullName { get; set; }
        [Display(Name = "Ngày sinh")]
        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        public DateTime? DateOfBirth { get; set; }
        [Display(Name = "Giới tính")]
        [Required(ErrorMessage = "Vui lòng nhập giới tính")]
        public string Gender { get; set; }
        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }
        public int? PhotoId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte? ActiveFlag { get; set; }
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; }
        public bool Active
        {
            get => ActiveFlag == 1;
            set => ActiveFlag = value ? (byte?)1 : (byte?)0; // Thêm setter
        }
        public DateTime? BirthDate => DateOfBirth;
        public HttpPostedFileBase PhotoFile { get; set; }


        public string PhotoUrl
        {
            get; set;
        }
        public string Address { get; set; }
        public string Summary { get; set; }
        public string CandidateID { get; set; }
        [Display(Name = "Email ứng tuyển")]
        [Required(ErrorMessage = "Vui lòng nhập email ứng tuyển")]
        public string ApplicationEmail { get; set; }
        public string CurrentPhotoUrl { get; set; }
        public string Username { get;  set; }
        public string Password { get; internal set; }
    }
    public class EditCandidatesListVm
    {
        [Display(Name = "ID ứng viên")]
        [Required(ErrorMessage = "Vui lòng chon ID ứng viên")]
        public int CandidateId { get; set; }
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string FullName { get; set; }
        
        public string Email { get; set; }
        [Display(Name = "Tài khoản")]
        [Required(ErrorMessage = "Vui lòng chọn tài khoản")]
        public int AccountId { get; set; }
        [Display(Name = "Ngày sinh")]
        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        public DateTime? DateOfBirth { get; set; }
        [Display(Name = "Giới tính")]
        [Required(ErrorMessage = "Vui lòng nhập giới tính")]
        public string Gender { get; set; }
        [Display(Name = "Số điện thoại")]

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
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
        public int? PhotoId { get;   set; }     
        public DateTime? CreatedAt { get; set; }



        public string Address { get; set; }
        public string PhotoUrl { get; set; }
        public string Summary { get;   set; }
        public string ApplicationEmail { get; set; }
    }
}



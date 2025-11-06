using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Project_Recruiment_Huce.Models.Jobs
{
    /// <summary>
    /// ViewModel cho form đăng tin tuyển dụng
    /// Bao gồm cả JobPost và JobPostDetails
    /// </summary>
    public class JobCreateViewModel
    {
        // JobPost fields
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề công việc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        [Display(Name = "Tiêu đề công việc")]
        public string Title { get; set; }

        [AllowHtml]
        [Display(Name = "Mô tả công việc")]
        public string Description { get; set; }

        [AllowHtml]
        [Display(Name = "Yêu cầu")]
        public string Requirements { get; set; }

        [Display(Name = "Lương từ")]
        [Range(0, double.MaxValue, ErrorMessage = "Lương từ phải lớn hơn 0")]
        public decimal? SalaryFrom { get; set; }

        [Display(Name = "Lương đến")]
        [Range(0, double.MaxValue, ErrorMessage = "Lương đến phải lớn hơn 0")]
        public decimal? SalaryTo { get; set; }

        [Display(Name = "Đơn vị tiền tệ")]
        public string SalaryCurrency { get; set; } = "VND";

        [Display(Name = "Địa điểm")]
        [StringLength(255, ErrorMessage = "Địa điểm không được vượt quá 255 ký tự")]
        public string Location { get; set; }

        [Display(Name = "Hình thức làm việc")]
        public string EmploymentType { get; set; }

        [Display(Name = "Hạn nộp hồ sơ")]
        [DataType(DataType.Date)]
        public DateTime? ApplicationDeadline { get; set; }

        [Display(Name = "Mã công việc")]
        [StringLength(20, ErrorMessage = "Mã công việc không được vượt quá 20 ký tự")]
        public string JobCode { get; set; }

        [Display(Name = "Công ty")]
        public int? CompanyID { get; set; }

        // JobPostDetails fields
        [Required(ErrorMessage = "Vui lòng chọn ngành nghề")]
        [StringLength(150, ErrorMessage = "Ngành nghề không được vượt quá 150 ký tự")]
        [Display(Name = "Ngành nghề")]
        public string Industry { get; set; }

        [StringLength(150, ErrorMessage = "Chuyên ngành không được vượt quá 150 ký tự")]
        [Display(Name = "Chuyên ngành")]
        public string Major { get; set; }

        [Display(Name = "Số năm kinh nghiệm")]
        [Range(0, 50, ErrorMessage = "Số năm kinh nghiệm phải từ 0 đến 50")]
        public int? YearsExperience { get; set; }

        [StringLength(100, ErrorMessage = "Bằng cấp yêu cầu không được vượt quá 100 ký tự")]
        [Display(Name = "Bằng cấp yêu cầu")]
        public string DegreeRequired { get; set; }

        [AllowHtml]
        [Display(Name = "Kỹ năng")]
        public string Skills { get; set; }

        [Display(Name = "Số lượng cần tuyển")]
        [Range(1, 1000, ErrorMessage = "Số lượng cần tuyển phải từ 1 đến 1000")]
        public int? Headcount { get; set; } = 1;

        [Display(Name = "Yêu cầu giới tính")]
        public string GenderRequirement { get; set; } = "Not required";

        [Display(Name = "Độ tuổi từ")]
        [Range(16, 100, ErrorMessage = "Độ tuổi từ phải từ 16 đến 100")]
        public int? AgeFrom { get; set; }

        [Display(Name = "Độ tuổi đến")]
        [Range(16, 100, ErrorMessage = "Độ tuổi đến phải từ 16 đến 100")]
        public int? AgeTo { get; set; }
    }
}



using Project_Recruiment_Huce.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách tin tuyển dụng
    /// Maps to JobPost table in database
    /// </summary>
    public class JobPostListVm
    {
        public string Description { get; set; }
        public int JobPostID { get; set; }
        public string JobCode { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string EmploymentType { get; set; }
        public string Industry { get; set; }
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public DateTime PostedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public string Status { get; set; }

        // Thông tin công ty
        public int? CompanyID { get; set; }
        public string CompanyName { get; set; }
        public int RecruiterID { get; set; }
        public string FullName { get; set; }
        public string PositionTitle { get; set; }
        public string Phone { get; set; }


    }


    // ---------------------------------------------------------------------
    // File: JobPostDetailVm.cs
    // Mục đích: View Model để hiển thị chi tiết một bài đăng
    // ---------------------------------------------------------------------


    // File: JobPostDetailVm.cs (hoặc trong file Models chung)
    public class JobPostDetailVm
    {
        // Thông tin JobPost cơ bản
        public int JobPostID { get; set; }
        public string JobCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public string SalaryCurrency { get; set; }
        public string Location { get; set; }
        public string EmploymentType { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public string Status { get; set; }
        public DateTime PostedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Thông tin chi tiết (từ JobPostDetails)
        public int DetailID { get; set; } // Khóa chính của bảng Detail
        public string Industry { get; set; }
        public string Major { get; set; }
        public int YearsExperience { get; set; }
        public string DegreeRequired { get; set; }
        public string Skills { get; set; }
        public int Headcount { get; set; }
        public string GenderRequirement { get; set; }
        public int? AgeFrom { get; set; }
        public int? AgeTo { get; set; }

        // Thông tin Công ty (từ Companies)
        public int? CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public int? CompanyPhotoID { get; set; } // Nếu cần hiển thị logo

        // Thông tin Người đăng (từ Recruiters)
        public int RecruiterID { get; set; }
        public string FullName { get; set; }
        public string PositionTitle { get; set; }
        public string Phone { get; set; }
        public int? RecruiterPhotoID { get; set; } // Nếu cần hiển thị ảnh đại diện
    }

    // ---------------------------------------------------------------------
    // File: JobPostCreateVm.cs
    // Mục đích: View Model để tạo bài đăng mới (dùng cho form nhập liệu)
    // ---------------------------------------------------------------------


    public class JobPostCreateVm
    {
        public string FullName { get; set; }
        public int JobPostID { get; set; }

        [Required(ErrorMessage = "Mã công việc là bắt buộc")]
        public string JobCode { get; set; }

        [Required(ErrorMessage = "Nhà tuyển dụng là bắt buộc")]
        public int RecruiterID { get; set; }

        // CompanyID được tự động lấy từ RecruiterID, không cần Required
        // Dùng int? để tránh validation tự động khi giá trị là 0
        public int? CompanyID { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Yêu cầu là bắt buộc")]
        public string Requirements { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "Mức lương tối thiểu không hợp lệ")]
        public decimal SalaryFrom { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "Mức lương tối đa không hợp lệ")]
        public decimal SalaryTo { get; set; }

        [Required(ErrorMessage = "Loại tiền lương là bắt buộc")]
        public string SalaryCurrency { get; set; } // ví dụ: VND, USD

        [Required(ErrorMessage = "Địa điểm là bắt buộc")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Loại hình công việc là bắt buộc")]
        public string EmploymentType { get; set; } // ví dụ: Full-time, Part-time

        [Required(ErrorMessage = "Hạn nộp hồ sơ là bắt buộc")]
        public DateTime ApplicationDeadline { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; } // Visible, Hidden, Closed, Draft

        public DateTime PostedAt { get; set; } = DateTime.Now;
        public DateTime? UpdateAt { get; set; }

        [Required(ErrorMessage = "Ngành nghề là bắt buộc")]
        public string Industry { get; set; }

        public string Major { get; set; }

        [Required(ErrorMessage = "Số năm kinh nghiệm là bắt buộc")]
        [Range(0, 50, ErrorMessage = "Số năm kinh nghiệm không hợp lệ (0-50)")]
        public int YearsExperience { get; set; } = 0; // Default 0

        public string DegreeRequired { get; set; }

        public string Skills { get; set; } // NVARCHAR(MAX)

        [Required(ErrorMessage = "Số lượng tuyển là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Số lượng tuyển tối thiểu là 1")]
        public int Headcount { get; set; } = 1; // Default 1

        public string GenderRequirement { get; set; } // Not required, Male, Female

        [Range(18, 100, ErrorMessage = "Tuổi tối thiểu không hợp lệ (18-100)")]
        public int? AgeFrom { get; set; }

        [Range(18, 100, ErrorMessage = "Tuổi tối đa không hợp lệ (18-100)")]
        public int? AgeTo { get; set; }
    }

   

        
        
    



    //// ---------------------------------------------------------------------
    //// File: JobPostEditVm.cs
    //// Mục đích: View Model để chỉnh sửa bài đăng (dùng cho form nhập liệu)
    //// ---------------------------------------------------------------------


    public class JobPostEditVm
    {
        [Required(ErrorMessage = "Công ty là bắt buộc")]
        public int? CompanyID { get; set; }

        public int JobPostID { get; set; }

        [Required(ErrorMessage = "Mã công việc là bắt buộc")]
        public string JobCode { get; set; }

        [Required(ErrorMessage = "Nhà tuyển dụng là bắt buộc")]
        public int RecruiterID { get; set; }




        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Yêu cầu là bắt buộc")]
        public string Requirements { get; set; }

        [Required(ErrorMessage = "Mức lương tối thiểu là bắt buộc")]
        public decimal? SalaryFrom { get; set; } 

        [Required(ErrorMessage = "Mức lương tối đa là bắt buộc")]
        public decimal? SalaryTo { get; set; }  

        [Required(ErrorMessage = "Loại tiền là bắt buộc")]
        public string SalaryCurrency { get; set; }

        [Required(ErrorMessage = "Địa điểm là bắt buộc")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Loại hình công việc là bắt buộc")]
        public string EmploymentType { get; set; }

        [Required(ErrorMessage = "Hạn nộp hồ sơ là bắt buộc")]
        public DateTime? ApplicationDeadline { get; set; }  

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; }

        public DateTime? PostedAt { get; set; }  // ✅ Hoặc DateTime? nếu DB nullable
        public string Industry { get; set; }

        public string Major { get; set; }

        [Required(ErrorMessage = "Số năm kinh nghiệm là bắt buộc")]
        [Range(0, 50, ErrorMessage = "Số năm kinh nghiệm không hợp lệ (0-50)")]
        public int YearsExperience { get; set; } = 0; // Default 0

        public string DegreeRequired { get; set; }

        public string Skills { get; set; } // NVARCHAR(MAX)

        [Required(ErrorMessage = "Số lượng tuyển là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Số lượng tuyển tối thiểu là 1")]
        public int Headcount { get; set; } = 1; // Default 1

        public string GenderRequirement { get; set; } // Not required, Male, Female

        [Range(18, 100, ErrorMessage = "Tuổi tối thiểu không hợp lệ (18-100)")]
        public int? AgeFrom { get; set; }

        [Range(18, 100, ErrorMessage = "Tuổi tối đa không hợp lệ (18-100)")]
        public int? AgeTo { get; set; }
    }
}



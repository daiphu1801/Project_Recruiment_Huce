using System;
using System.ComponentModel.DataAnnotations;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách tin nhắn liên hệ
    /// </summary>
    public class ContactMessageListVm
    {
        public int ContactMessageID { get; set; }
        
        [Display(Name = "Họ")]
        public string FirstName { get; set; }
        
        [Display(Name = "Tên")]
        public string LastName { get; set; }
        
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Chủ đề")]
        public string Subject { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
        
        [Display(Name = "Ngày gửi")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Ngày đọc")]
        public DateTime? ReadAt { get; set; }
        
        [Display(Name = "Ngày trả lời")]
        public DateTime? RepliedAt { get; set; }

        /// <summary>
        /// Tên đầy đủ của người gửi
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Lấy class CSS cho badge trạng thái
        /// </summary>
        public string StatusBadgeClass
        {
            get
            {
                if (Status == "Pending") return "badge bg-warning text-dark";
                if (Status == "Read") return "badge bg-info";
                if (Status == "Replied") return "badge bg-success";
                if (Status == "Closed") return "badge bg-secondary";
                return "badge bg-light text-dark";
            }
        }

        /// <summary>
        /// Lấy text tiếng Việt cho trạng thái
        /// </summary>
        public string StatusText
        {
            get
            {
                if (Status == "Pending") return "Chờ xử lý";
                if (Status == "Read") return "Đã đọc";
                if (Status == "Replied") return "Đã trả lời";
                if (Status == "Closed") return "Đã đóng";
                return Status;
            }
        }
    }

    /// <summary>
    /// ViewModel cho chi tiết tin nhắn liên hệ
    /// </summary>
    public class ContactMessageDetailsVm
    {
        public int ContactMessageID { get; set; }

        [Display(Name = "Họ")]
        public string FirstName { get; set; }

        [Display(Name = "Tên")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Chủ đề")]
        public string Subject { get; set; }

        [Display(Name = "Nội dung")]
        public string Message { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Ghi chú của Admin")]
        public string AdminNotes { get; set; }

        [Display(Name = "Ngày gửi")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày đọc")]
        public DateTime? ReadAt { get; set; }

        [Display(Name = "Ngày trả lời")]
        public DateTime? RepliedAt { get; set; }

        /// <summary>
        /// Tên đầy đủ của người gửi
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Lấy text tiếng Việt cho trạng thái
        /// </summary>
        public string StatusText
        {
            get
            {
                if (Status == "Pending") return "Chờ xử lý";
                if (Status == "Read") return "Đã đọc";
                if (Status == "Replied") return "Đã trả lời";
                if (Status == "Closed") return "Đã đóng";
                return Status;
            }
        }

        /// <summary>
        /// Lấy class CSS cho badge trạng thái
        /// </summary>
        public string StatusBadgeClass
        {
            get
            {
                if (Status == "Pending") return "badge bg-warning text-dark";
                if (Status == "Read") return "badge bg-info";
                if (Status == "Replied") return "badge bg-success";
                if (Status == "Closed") return "badge bg-secondary";
                return "badge bg-light text-dark";
            }
        }
    }
}

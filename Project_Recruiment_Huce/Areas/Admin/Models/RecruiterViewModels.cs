using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách nhà tuyển dụng
    /// </summary>
    public class RecruiterListVm
    {
        public int RecruiterId { get; set; }
        public int AccountId { get; set; }
        public int? CompanyId { get; set; }
        public string FullName { get; set; }
        public string PositionTitle { get; set; }
        public string WorkEmail { get; set; }
        public string Phone { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte? ActiveFlag { get; set; } // 1 = active, 0 = inactive
        
        // Helper properties for display (from related tables)
        public string CompanyName { get; set; }
        
        // Helper property for display
        public bool Active
        {
            get => ActiveFlag == 1;
            set => ActiveFlag = value ? (byte?)1 : (byte?)0;
        }
    }
}


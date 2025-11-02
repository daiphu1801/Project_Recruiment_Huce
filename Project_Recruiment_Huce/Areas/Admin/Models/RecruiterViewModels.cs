using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách nhà tuyển dụng
    /// </summary>
    public class RecruiterListVm
    {
        public int RecruiterId { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string PositionTitle { get; set; }
        public string WorkEmail { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
    }
}


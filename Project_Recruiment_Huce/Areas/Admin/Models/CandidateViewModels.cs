using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách ứng viên
    /// </summary>
    public class CandidateListVm
    {
        public int CandidateId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
    }
}


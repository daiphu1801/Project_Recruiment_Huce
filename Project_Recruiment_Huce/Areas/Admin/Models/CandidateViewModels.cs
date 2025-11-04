using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách ứng viên
    /// </summary>
    public class CandidateListVm
    {
        public int CandidateId { get; set; }
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; } // DateOfBirth in database
        public string Gender { get; set; }
        public string Phone { get; set; }
        public int? PhotoId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte? ActiveFlag { get; set; } // 1 = active, 0 = inactive
        
        // Helper properties for display (from related Account table)
        public string Email { get; set; }
        
        // Helper property for display
        public bool Active => ActiveFlag == 1;
        
        // Alias for compatibility
        public DateTime? BirthDate => DateOfBirth;
    }
}


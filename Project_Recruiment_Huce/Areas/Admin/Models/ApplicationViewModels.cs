using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách đơn ứng tuyển
    /// </summary>
    public class ApplicationListVm
    {
        public int ApplicationId { get; set; }
        public int CandidateId { get; set; }
        public int JobId { get; set; }
        public DateTime? AppliedAt { get; set; }
        public string AppStatus { get; set; }
        public string CvPath { get; set; }
        public string CertificatePath { get; set; }
        public string Note { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Helper properties for display (from related tables)
        public string CandidateName { get; set; }
        public string JobTitle { get; set; }
    }
}


using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách đơn ứng tuyển
    /// </summary>
    public class ApplicationListVm
    {
        public int ApplicationId { get; set; }
        public string CandidateName { get; set; }
        public string JobTitle { get; set; }
        public DateTime AppliedAt { get; set; }
        public string AppStatus { get; set; }
    }
}


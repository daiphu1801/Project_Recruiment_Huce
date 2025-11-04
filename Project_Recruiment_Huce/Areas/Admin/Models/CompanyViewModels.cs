using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách công ty
    /// </summary>
    public class CompanyListVm
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string TaxCode { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public int? LogoPhotoId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte? ActiveFlag { get; set; } // 1 = active, 0 = inactive
        
        // Helper property for display
        public bool Active
        {
            get => ActiveFlag == 1;
            set => ActiveFlag = value ? (byte?)1 : (byte?)0;
        }
    }
}


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
        public string Industry { get; set; }
        public string Phone { get; set; }
        public string CompanyEmail { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


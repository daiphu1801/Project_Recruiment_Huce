using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("Companies")]
    public class Company
    {
        [Key]
        [Column("CompanyId")]
        public int CompanyId { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required, StringLength(255)] public string CompanyName { get; set; }
        [StringLength(20)] public string TaxCode { get; set; }
        [StringLength(150)] public string Industry { get; set; }
        [StringLength(255)] public string Address { get; set; }
        [StringLength(20)] public string Phone { get; set; }
        [StringLength(150)] public string CompanyEmail { get; set; }
        [StringLength(200)] public string Website { get; set; }
        [StringLength(1000)] public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte? ActiveFlag { get; set; }
    }
}



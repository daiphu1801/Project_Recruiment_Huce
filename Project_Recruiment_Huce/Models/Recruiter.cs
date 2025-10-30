using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("Recruiters")]
    public class Recruiter
    {
        [Key]
        [Column("RecruiterId")]
        public int RecruiterId { get; set; }
        [Required]
        public int AccountId { get; set; }
        public int? CompanyId { get; set; }
        [Required, StringLength(100)] public string FullName { get; set; }
        [StringLength(100)] public string PositionTitle { get; set; }
        [StringLength(150)] public string WorkEmail { get; set; }
        [StringLength(20)] public string Phone { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte? ActiveFlag { get; set; }
    }
}



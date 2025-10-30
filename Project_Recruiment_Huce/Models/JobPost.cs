using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("JobPosts")]
    public class JobPost
    {
        [Key]
        [Column("JobId")]
        public int JobId { get; set; }
        [StringLength(20)] public string JobCode { get; set; }
        [Required] public int RecruiterId { get; set; }
        public int? CompanyId { get; set; }
        [Required, StringLength(200)] public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        [StringLength(50)] public string SalaryUnit { get; set; }
        [StringLength(255)] public string LocationText { get; set; }
        [StringLength(100)] public string Employment { get; set; }
        public DateTime? Deadline { get; set; }
        [StringLength(50)] public string Status { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}



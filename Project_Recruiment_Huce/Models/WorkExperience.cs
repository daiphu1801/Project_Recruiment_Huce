using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("WorkExperiences")]
    public class WorkExperience
    {
        [Key]
        [Column("ExperienceId")]
        public int ExperienceId { get; set; }
        [Required] public int CandidateId { get; set; }
        [Required, StringLength(255)] public string CompanyName { get; set; }
        [Required, StringLength(150)] public string JobTitle { get; set; }
        [StringLength(150)] public string Industry { get; set; }
        [StringLength(150)] public string Major { get; set; }
        public string JobDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? SalaryAmount { get; set; }
        [StringLength(20)] public string SalaryCurrency { get; set; }
        [StringLength(500)] public string Achievements { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}



using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("Applications")]
    public class Application
    {
        [Key]
        [Column("ApplicationId")]
        public int ApplicationId { get; set; }
        [Required] public int CandidateId { get; set; }
        [Required] public int JobId { get; set; }
        public DateTime? AppliedAt { get; set; }
        [StringLength(50)] public string AppStatus { get; set; }
        [StringLength(500)] public string CvPath { get; set; }
        [StringLength(500)] public string CertificatePath { get; set; }
        [StringLength(500)] public string Note { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}



using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("CandidateCertificates")]
    public class CandidateCertificate
    {
        [Key]
        [Column("CandidateCertificateId")]
        public int CandidateCertificateId { get; set; }
        [Required] public int CandidateId { get; set; }
        [Required] public int CertificateId { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        [StringLength(500)] public string FilePath { get; set; }
        [StringLength(50)] public string ScoreText { get; set; }
        [StringLength(255)] public string Note { get; set; }
        public DateTime? UploadedAt { get; set; }
    }
}



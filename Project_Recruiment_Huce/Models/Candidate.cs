using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Project_Recruiment_Huce.Models
{
    [Table("Candidates")]
    public class Candidate
    {
        [Key]
        [Column("CandidateId")]
        public int CandidateId { get; set; }
        [Required] public int AccountId { get; set; }
        [Required, StringLength(100)] public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        [Required, StringLength(5)] public string Gender { get; set; }
        [StringLength(15)] public string Phone { get; set; }
        [StringLength(100)] public string Email { get; set; }
        [StringLength(255)] public string Address { get; set; }
        [AllowHtml]
        [StringLength(500)] public string About { get; set; }
        public int? PhotoId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte? ActiveFlag { get; set; }
    }
}



using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("Admins")]
    public class Admin
    {
        [Key]
        [Column("AdminId")]
        public int AdminId { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required, StringLength(100)] public string FullName { get; set; }
        [StringLength(150)] public string ContactEmail { get; set; }
        [StringLength(100)] public string Permission { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}



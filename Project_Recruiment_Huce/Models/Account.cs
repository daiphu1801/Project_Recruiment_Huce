using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("Accounts")]
    public class Account
    {
        [Key]
        [Column("AccountId")]
        public int AccountId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Username")]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(150)]
        [Column("Email")]
        public string Email { get; set; }

        [StringLength(20)]
        [Column("Phone")]
        public string Phone { get; set; }

        [Required]
        [StringLength(30)]
        [Column("Role")]
        public string Role { get; set; }

        [Column("CreatedAt")]
        public DateTime? CreatedAt { get; set; }

        [Column("ActiveFlag")]
        public byte ActiveFlag { get; set; }

        [Column("PhotoId")]
        public int? PhotoId { get; set; }
    }
}



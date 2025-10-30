using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("Transactions")]
    public class Transaction
    {
        [Key]
        [Column("TransactionId")]
        public int TransactionId { get; set; }
        [StringLength(20)] public string TransactionNo { get; set; }
        [Required] public int AccountId { get; set; }
        [Required, StringLength(100)] public string Type { get; set; }
        [Required] public decimal Amount { get; set; }
        [Required, StringLength(50)] public string Method { get; set; }
        [StringLength(50)] public string Status { get; set; }
        public DateTime? TransactedAt { get; set; }
        [StringLength(500)] public string Description { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}



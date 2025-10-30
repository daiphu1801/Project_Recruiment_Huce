using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("Certificates")]
    public class Certificate
    {
        [Key]
        [Column("CertificateId")]
        public int CertificateId { get; set; }
        [Required, StringLength(100)] public string CertificateName { get; set; }
        [StringLength(100)] public string Issuer { get; set; }
        [Required, StringLength(100)] public string Industry { get; set; }
        [StringLength(150)] public string Major { get; set; }
        [StringLength(255)] public string Summary { get; set; }
    }
}



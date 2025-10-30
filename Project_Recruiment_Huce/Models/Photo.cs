using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("Photos")]
    public class Photo
    {
        [Key]
        [Column("PhotoId")]
        public int PhotoId { get; set; }
        [Required, StringLength(255)]
        public string FileName { get; set; }
        [Required, StringLength(500)]
        public string FilePath { get; set; }
        public int? SizeKB { get; set; }
        [StringLength(50)] public string MimeType { get; set; }
        public DateTime? UploadedAt { get; set; }
    }
}



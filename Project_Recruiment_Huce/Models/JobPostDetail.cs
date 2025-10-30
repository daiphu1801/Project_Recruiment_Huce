using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("JobPostDetails")]
    public class JobPostDetail
    {
        [Key]
        [Column("DetailId")]
        public int DetailId { get; set; }
        [Required] public int JobId { get; set; }
        [Required, StringLength(150)] public string Industry { get; set; }
        [StringLength(150)] public string Major { get; set; }
        public int? YearsExperience { get; set; }
        [StringLength(100)] public string EducationLevel { get; set; }
        public string Skills { get; set; }
        public int? Headcount { get; set; }
        [StringLength(10)] public string Gender { get; set; }
        public int? AgeFrom { get; set; }
        public int? AgeTo { get; set; }
        [StringLength(50)] public string Status { get; set; }
    }
}



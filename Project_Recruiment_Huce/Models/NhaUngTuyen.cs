using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("NhaUngTuyen")]
    public class NhaUngTuyen
    {
        [Key]
        public int Id { get; set; }

        [StringLength(200)]
        public string CompanyName { get; set; }
    }
}



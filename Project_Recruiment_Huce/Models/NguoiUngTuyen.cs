using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("NguoiUngTuyen")]
    public class NguoiUngTuyen
    {
        [Key]
        public int Id { get; set; }

        [StringLength(150)]
        public string FullName { get; set; }
    }
}



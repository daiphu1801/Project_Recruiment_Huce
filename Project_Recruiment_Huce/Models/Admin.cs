using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("Admin")]
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [StringLength(150)]
        public string FullName { get; set; }
    }
}



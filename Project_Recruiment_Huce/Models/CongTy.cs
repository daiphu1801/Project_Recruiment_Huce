using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("CongTy")]
    public class CongTy
    {
        [Key]
        public int Id { get; set; }

        [StringLength(200)]
        public string Name { get; set; }
    }
}



using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Recruiment_Huce.Models
{
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        [Column("TaiKhoanID")]
        public int TaiKhoanID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("TenDangNhap")]
        public string TenDangNhap { get; set; }

        [Required]
        [StringLength(255)]
        [Column("MatKhau")]
        public string MatKhau { get; set; }

        [Required]
        [StringLength(150)]
        [Column("Email")]
        public string Email { get; set; }

        [StringLength(20)]
        [Column("SoDienThoai")]
        public string SoDienThoai { get; set; }

        [Required]
        [StringLength(30)]
        [Column("VaiTro")]
        public string VaiTro { get; set; }

        [Column("NgayTao")]
        public DateTime? NgayTao { get; set; }

        [Column("TrangThai")]
        public byte TrangThai { get; set; }

        [Column("AnhID")]
        public int? AnhID { get; set; }
    }
}



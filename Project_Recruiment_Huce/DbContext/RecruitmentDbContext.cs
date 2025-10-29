using System.Data.Entity;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.DbContext
{
    public class RecruitmentDbContext : System.Data.Entity.DbContext
    {
        public RecruitmentDbContext() : base("RecruitmentDb")
        {
            // Disable initializer to map to existing SQL database schema without EF creating/modifying tables
            Database.SetInitializer<RecruitmentDbContext>(null);
        }

        public virtual DbSet<Job> Jobs { get; set; }

        // Domain tables
        public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<NguoiUngTuyen> NguoiUngTuyens { get; set; }
        public virtual DbSet<NhaUngTuyen> NhaUngTuyens { get; set; }
        public virtual DbSet<CongTy> CongTys { get; set; }

        public static RecruitmentDbContext Create()
        {
            return new RecruitmentDbContext();
        }
    }
}



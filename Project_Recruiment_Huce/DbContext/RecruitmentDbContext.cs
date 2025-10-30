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

        // New schema DbSets
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Photo> Photos { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Recruiter> Recruiters { get; set; }
        public virtual DbSet<Candidate> Candidates { get; set; }
        public virtual DbSet<WorkExperience> WorkExperiences { get; set; }
        public virtual DbSet<Certificate> Certificates { get; set; }
        public virtual DbSet<CandidateCertificate> CandidateCertificates { get; set; }
        public virtual DbSet<JobPost> JobPosts { get; set; }
        public virtual DbSet<JobPostDetail> JobPostDetails { get; set; }
        public virtual DbSet<Application> Applications { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }

        public static RecruitmentDbContext Create()
        {
            return new RecruitmentDbContext();
        }
    }
}



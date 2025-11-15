using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Base controller cho tất cả controllers trong User area
    /// Cung cấp các helper methods chung
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Lấy AccountID của user hiện tại từ Claims
        /// </summary>
        /// <returns>AccountID hoặc null nếu không tìm thấy</returns>
        protected int? GetCurrentAccountId()
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
                return null;

            var idClaim = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) return null;
            
            int accountId;
            return int.TryParse(idClaim.Value, out accountId) ? (int?)accountId : null;
        }

        /// <summary>
        /// Tạo database context với connection string từ config
        /// </summary>
        /// <returns>JOBPORTAL_ENDataContext instance</returns>
        protected JOBPORTAL_ENDataContext CreateDbContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            return new JOBPORTAL_ENDataContext(connectionString);
        }

        /// <summary>
        /// Lấy RecruiterID của user hiện tại
        /// </summary>
        /// <returns>RecruiterID hoặc null nếu không tìm thấy</returns>
        protected int? GetCurrentRecruiterId()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null) return null;

            using (var db = CreateDbContext())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                return recruiter?.RecruiterID;
            }
        }

        /// <summary>
        /// Lấy CandidateID của user hiện tại
        /// </summary>
        /// <returns>CandidateID hoặc null nếu không tìm thấy</returns>
        protected int? GetCurrentCandidateId()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null) return null;

            using (var db = CreateDbContext())
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                return candidate?.CandidateID;
            }
        }
    }
}


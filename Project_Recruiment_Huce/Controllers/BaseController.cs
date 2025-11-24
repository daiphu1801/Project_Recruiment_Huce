using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Infrastructure;

namespace Project_Recruiment_Huce.Controllers
{
    /// <summary>
    /// Controller cơ sở cho tất cả controllers trong User area (không phải Admin)
    /// Cung cấp các phương thức trợ giúp chung để truy xuất thông tin người dùng hiện tại
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Lấy AccountID của người dùng hiện tại từ Claims Identity
        /// Được sử dụng để xác định người dùng đã đăng nhập
        /// </summary>
        /// <returns>AccountID nếu người dùng đã đăng nhập, null nếu chưa đăng nhập hoặc không tìm thấy claim</returns>
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
        /// Sử dụng DbContextFactory.Create() hoặc DbContextFactory.CreateReadOnly() thay thế
        /// </summary>
        /// <returns>Instance của JOBPORTAL_ENDataContext</returns>
        [Obsolete("Sử dụng DbContextFactory.Create() hoặc DbContextFactory.CreateReadOnly() thay thế")]
        protected JOBPORTAL_ENDataContext CreateDbContext()
        {
            return DbContextFactory.Create();
        }

        /// <summary>
        /// Lấy RecruiterID của người dùng hiện tại dựa trên AccountID
        /// Sử dụng cho các chức năng dành cho nhà tuyển dụng
        /// </summary>
        /// <returns>RecruiterID nếu tìm thấy, null nếu người dùng không phải recruiter hoặc chưa đăng nhập</returns>
        protected int? GetCurrentRecruiterId()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null) return null;

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId.Value);
                return recruiter?.RecruiterID;
            }
        }

        /// <summary>
        /// Lấy CandidateID của người dùng hiện tại dựa trên AccountID
        /// Sử dụng cho các chức năng dành cho ứng viên
        /// </summary>
        /// <returns>CandidateID nếu tìm thấy, null nếu người dùng không phải candidate hoặc chưa đăng nhập</returns>
        protected int? GetCurrentCandidateId()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null) return null;

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId.Value);
                return candidate?.CandidateID;
            }
        }
    }
}


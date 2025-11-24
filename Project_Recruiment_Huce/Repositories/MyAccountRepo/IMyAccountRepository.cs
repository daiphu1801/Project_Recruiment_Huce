using System;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Giao diện cho các thao tác dữ liệu liên quan đến quản lý tài khoản cá nhân
    /// Cung cấp các phương thức truy vấn và cập nhật cho MyAccount features
    /// </summary>
    public interface IMyAccountRepository
    {
        Account GetAccountById(int accountId);
        Candidate GetCandidateByAccountId(int accountId);
        Recruiter GetRecruiterByAccountId(int accountId);
        void UpdatePassword(int accountId, string passwordHash, string salt);
        void SaveChanges();
    }
}

using System;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    public class AccountRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public AccountRepository(JOBPORTAL_ENDataContext db)
        {
            _db = db;
        }

        public Account FindByUsernameOrEmail(string emailOrUsername)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername)) return null;
            return _db.Accounts.FirstOrDefault(a =>
                (a.Username == emailOrUsername || a.Email == emailOrUsername));
        }

        public bool ExistsUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return _db.Accounts.Any(a => a.Username == username);
        }

        public bool ExistsEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return _db.Accounts.Any(a => a.Email == email);
        }

        public Account Create(string username, string email, string phone, string role, string passwordHash)
        {
            var account = new Account
            {
                Username = username,
                Email = email,
                Phone = phone,
                Role = role,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.Now,
                ActiveFlag = 1
            };
            _db.Accounts.InsertOnSubmit(account);
            _db.SubmitChanges();
            return account;
        }
    }
}

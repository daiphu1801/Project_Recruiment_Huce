using System;
using System.Collections.Generic;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Repository triển khai các thao tác với bảng ContactMessages
    /// </summary>
    public class ContactRepository : IContactRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public ContactRepository(JOBPORTAL_ENDataContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public ContactMessage Create(string firstName, string lastName, string email, string subject, string message)
        {
            var contactMessage = new ContactMessage
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Subject = subject,
                Message = message,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _db.ContactMessages.InsertOnSubmit(contactMessage);
            _db.SubmitChanges();

            return contactMessage;
        }

        /// <inheritdoc/>
        public ContactMessage GetById(int contactMessageId)
        {
            return _db.ContactMessages.FirstOrDefault(c => c.ContactMessageID == contactMessageId);
        }

        /// <inheritdoc/>
        public IEnumerable<ContactMessage> GetAll()
        {
            return _db.ContactMessages.OrderByDescending(c => c.CreatedAt).ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<ContactMessage> GetByStatus(string status)
        {
            return _db.ContactMessages
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        /// <inheritdoc/>
        public bool EmailExistsInSystem(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var normalizedEmail = email.Trim().ToLowerInvariant();
            return _db.Accounts.Any(a => a.Email.ToLower() == normalizedEmail);
        }

        /// <inheritdoc/>
        public void UpdateStatus(int contactMessageId, string status)
        {
            var contactMessage = GetById(contactMessageId);
            if (contactMessage != null)
            {
                contactMessage.Status = status;
                if (status == "Read" && !contactMessage.ReadAt.HasValue)
                {
                    contactMessage.ReadAt = DateTime.Now;
                }
                else if (status == "Replied" && !contactMessage.RepliedAt.HasValue)
                {
                    contactMessage.RepliedAt = DateTime.Now;
                }
            }
        }

        /// <inheritdoc/>
        public void AddAdminNotes(int contactMessageId, string notes)
        {
            var contactMessage = GetById(contactMessageId);
            if (contactMessage != null)
            {
                contactMessage.AdminNotes = notes;
            }
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            _db.SubmitChanges();
        }
    }
}

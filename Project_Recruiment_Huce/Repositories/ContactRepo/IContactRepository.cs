using System;
using System.Collections.Generic;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Giao diện cho các thao tác truy xuất dữ liệu liên quan tới ContactMessage
    /// </summary>
    public interface IContactRepository
    {
        /// <summary>
        /// Tạo tin nhắn liên hệ mới
        /// </summary>
        ContactMessage Create(string firstName, string lastName, string email, string subject, string message);

        /// <summary>
        /// Lấy tin nhắn theo ID
        /// </summary>
        ContactMessage GetById(int contactMessageId);

        /// <summary>
        /// Lấy tất cả tin nhắn liên hệ (cho admin)
        /// </summary>
        IEnumerable<ContactMessage> GetAll();

        /// <summary>
        /// Lấy tin nhắn theo trạng thái
        /// </summary>
        IEnumerable<ContactMessage> GetByStatus(string status);

        /// <summary>
        /// Kiểm tra email có tồn tại trong hệ thống (Accounts table)
        /// </summary>
        bool EmailExistsInSystem(string email);

        /// <summary>
        /// Cập nhật trạng thái tin nhắn
        /// </summary>
        void UpdateStatus(int contactMessageId, string status);

        /// <summary>
        /// Thêm ghi chú admin
        /// </summary>
        void AddAdminNotes(int contactMessageId, string notes);

        /// <summary>
        /// Lưu thay đổi
        /// </summary>
        void SaveChanges();
    }
}

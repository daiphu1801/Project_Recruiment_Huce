using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý tin nhắn liên hệ từ người dùng
    /// Cho phép admin xem, cập nhật trạng thái, thêm ghi chú và xóa tin nhắn
    /// </summary>
    public class ContactMessagesController : AdminBaseController
    {
        // GET: Admin/ContactMessages
        public ActionResult Index(string q, string status = null, int page = 1)
        {
            ViewBag.Title = "Quản lý liên hệ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Liên hệ", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var query = db.ContactMessages.AsQueryable();

                // Search by name, email, or subject
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.Where(c =>
                        c.FirstName.Contains(q) ||
                        c.LastName.Contains(q) ||
                        c.Email.Contains(q) ||
                        c.Subject.Contains(q)
                    );
                }

                // Filter by status
                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(c => c.Status == status);
                }

                // Pagination
                int pageSize = 15;
                int totalRecords = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var messages = query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new ContactMessageListVm
                    {
                        ContactMessageID = c.ContactMessageID,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Email = c.Email,
                        Subject = c.Subject,
                        Status = c.Status,
                        CreatedAt = c.CreatedAt,
                        ReadAt = c.ReadAt,
                        RepliedAt = c.RepliedAt
                    }).ToList();

                // Status options for filter
                ViewBag.StatusOptions = new SelectList(new[] 
                { 
                    new { Value = "Pending", Text = "Chờ xử lý" },
                    new { Value = "Read", Text = "Đã đọc" },
                    new { Value = "Replied", Text = "Đã trả lời" },
                    new { Value = "Closed", Text = "Đã đóng" }
                }, "Value", "Text");

                // Statistics
                ViewBag.TotalPending = db.ContactMessages.Count(c => c.Status == "Pending");
                ViewBag.TotalRead = db.ContactMessages.Count(c => c.Status == "Read");
                ViewBag.TotalReplied = db.ContactMessages.Count(c => c.Status == "Replied");
                ViewBag.TotalClosed = db.ContactMessages.Count(c => c.Status == "Closed");

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalRecords;
                ViewBag.PageSize = pageSize;
                ViewBag.SearchQuery = q;
                ViewBag.StatusFilter = status;

                return View(messages);
            }
        }

        // GET: Admin/ContactMessages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Title = "Chi tiết liên hệ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Liên hệ", Url.Action("Index")),
                new Tuple<string, string>("Chi tiết", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var message = db.ContactMessages.FirstOrDefault(c => c.ContactMessageID == id.Value);

                if (message == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin nhắn liên hệ.";
                    return RedirectToAction("Index");
                }

                // Auto mark as Read if status is Pending
                if (message.Status == "Pending")
                {
                    message.Status = "Read";
                    message.ReadAt = DateTime.Now;
                    db.SubmitChanges();
                }

                var viewModel = new ContactMessageDetailsVm
                {
                    ContactMessageID = message.ContactMessageID,
                    FirstName = message.FirstName,
                    LastName = message.LastName,
                    Email = message.Email,
                    Subject = message.Subject,
                    Message = message.Message,
                    Status = message.Status,
                    AdminNotes = message.AdminNotes,
                    CreatedAt = message.CreatedAt,
                    ReadAt = message.ReadAt,
                    RepliedAt = message.RepliedAt
                };

                return View(viewModel);
            }
        }

        // GET: Admin/ContactMessages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Title = "Sửa liên hệ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Liên hệ", Url.Action("Index")),
                new Tuple<string, string>("Sửa", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var message = db.ContactMessages.FirstOrDefault(c => c.ContactMessageID == id.Value);

                if (message == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin nhắn liên hệ.";
                    return RedirectToAction("Index");
                }

                var viewModel = new ContactMessageDetailsVm
                {
                    ContactMessageID = message.ContactMessageID,
                    FirstName = message.FirstName,
                    LastName = message.LastName,
                    Email = message.Email,
                    Subject = message.Subject,
                    Message = message.Message,
                    Status = message.Status,
                    AdminNotes = message.AdminNotes,
                    CreatedAt = message.CreatedAt,
                    ReadAt = message.ReadAt,
                    RepliedAt = message.RepliedAt
                };

                // Status options for dropdown
                ViewBag.StatusOptions = new SelectList(new[] 
                { 
                    new { Value = "Pending", Text = "Chờ xử lý" },
                    new { Value = "Read", Text = "Đã đọc" },
                    new { Value = "Replied", Text = "Đã trả lời" },
                    new { Value = "Closed", Text = "Đã đóng" }
                }, "Value", "Text", message.Status);

                return View(viewModel);
            }
        }

        // POST: Admin/ContactMessages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ContactMessageDetailsVm model)
        {
            if (!ModelState.IsValid)
            {
                // Reload status options if validation fails
                ViewBag.StatusOptions = new SelectList(new[] 
                { 
                    new { Value = "Pending", Text = "Chờ xử lý" },
                    new { Value = "Read", Text = "Đã đọc" },
                    new { Value = "Replied", Text = "Đã trả lời" },
                    new { Value = "Closed", Text = "Đã đóng" }
                }, "Value", "Text", model.Status);

                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var message = db.ContactMessages.FirstOrDefault(c => c.ContactMessageID == model.ContactMessageID);

                if (message == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin nhắn liên hệ.";
                    return RedirectToAction("Index");
                }

                // Update status
                message.Status = model.Status;
                message.AdminNotes = model.AdminNotes;

                // Update timestamps
                if (model.Status == "Read" && !message.ReadAt.HasValue)
                {
                    message.ReadAt = DateTime.Now;
                }
                else if (model.Status == "Replied" && !message.RepliedAt.HasValue)
                {
                    message.RepliedAt = DateTime.Now;
                }

                try
                {
                    db.SubmitChanges();
                    TempData["SuccessMessage"] = "Cập nhật tin nhắn liên hệ thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật: " + ex.Message;
                    
                    ViewBag.StatusOptions = new SelectList(new[] 
                    { 
                        new { Value = "Pending", Text = "Chờ xử lý" },
                        new { Value = "Read", Text = "Đã đọc" },
                        new { Value = "Replied", Text = "Đã trả lời" },
                        new { Value = "Closed", Text = "Đã đóng" }
                    }, "Value", "Text", model.Status);

                    return View(model);
                }
            }
        }

        // GET: Admin/ContactMessages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Title = "Xóa liên hệ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Liên hệ", Url.Action("Index")),
                new Tuple<string, string>("Xóa", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var message = db.ContactMessages.FirstOrDefault(c => c.ContactMessageID == id.Value);

                if (message == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin nhắn liên hệ.";
                    return RedirectToAction("Index");
                }

                var viewModel = new ContactMessageDetailsVm
                {
                    ContactMessageID = message.ContactMessageID,
                    FirstName = message.FirstName,
                    LastName = message.LastName,
                    Email = message.Email,
                    Subject = message.Subject,
                    Message = message.Message,
                    Status = message.Status,
                    CreatedAt = message.CreatedAt
                };

                return View(viewModel);
            }
        }

        // POST: Admin/ContactMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var message = db.ContactMessages.FirstOrDefault(c => c.ContactMessageID == id);

                if (message == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin nhắn liên hệ.";
                    return RedirectToAction("Index");
                }

                try
                {
                    db.ContactMessages.DeleteOnSubmit(message);
                    db.SubmitChanges();
                    TempData["SuccessMessage"] = "Đã xóa tin nhắn liên hệ thành công!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa: " + ex.Message;
                }

                return RedirectToAction("Index");
            }
        }
    }
}

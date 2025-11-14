using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class ApplicationsController : AdminBaseController
    {
        private JOBPORTAL_ENDataContext GetDataContext()
        {
            return new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString);
        }

        // Phương thức Index không thay đổi nhiều
        public ActionResult Index(string q, string status = null, int page = 1)
        {
            ViewBag.Title = "Hồ sơ ứng tuyển";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Applications", null) };

            using (var db = GetDataContext())
            {
                // **Lưu ý:** Bạn nên bỏ comment và sử dụng join JobPosts để lấy JobTitle thực tế
                var query = from app in db.Applications
                            join candidate in db.Candidates on app.CandidateID equals candidate.CandidateID
                            join job in db.JobPosts on app.JobPostID equals job.JobPostID
                            join company in db.Companies on job.CompanyID equals company.CompanyID
                            // Nếu bạn có JobPosts, hãy join:
                            // join job in db.JobPosts on app.JobPostID equals job.JobPostID
                            select new ApplicationListVm
                            {
                                ApplicationId = app.ApplicationID,
                                CandidateId = app.CandidateID,
                                JobPostId = app.JobPostID,
                                CandidateName = candidate.FullName,
                                JobTitle = "Tên công việc (Cần JobPost)", // Thay bằng job.Title nếu đã join
                                AppliedAt = app.AppliedAt,
                                AppStatus = app.Status,
                                CompanyName = company.CompanyName,
                            };

                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.ToLower();
                    query = query.Where(x => (x.CandidateName ?? "").ToLower().Contains(q)
                                          || (x.JobTitle ?? "").ToLower().Contains(q));
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(x => string.Equals(x.AppStatus, status, StringComparison.OrdinalIgnoreCase));
                }

                int pageSize = 10;
                int skip = (page - 1) * pageSize;

                ViewBag.TotalRecords = query.Count();
                ViewBag.PageSize = pageSize;
                ViewBag.CurrentPage = page;

                var data = query.OrderByDescending(x => x.AppliedAt)
                                .Skip(skip)
                                .Take(pageSize)
                                .ToList();

                ViewBag.StatusOptions = ApplicationStatusHelper.GetStatusSelectList(status);
                return View(data);
            }
        }

        // Action Details: OK
        public ActionResult Details(int id)
        {
            using (var db = GetDataContext())
            {
                var query = from app in db.Applications
                            join candidate in db.Candidates on app.CandidateID equals candidate.CandidateID
                            join job in db.JobPosts on app.JobPostID equals job.JobPostID

                            join company in db.Companies on job.CompanyID equals company.CompanyID
                            where app.ApplicationID == id
                            select new ApplicationListVm
                            {
                                ApplicationId = app.ApplicationID,
                                CandidateId = app.CandidateID,
                                JobPostId = app.JobPostID,
                                CandidateName = candidate.FullName,
                                JobTitle = "Tên công việc (Cần JobPost)",
                                AppliedAt = app.AppliedAt,
                                AppStatus = app.Status,
                                ResumeFilePath = app.ResumeFilePath,
                                CertificateFilePath = app.CertificateFilePath,
                                Note = app.Note,
                                CompanyName = company.CompanyName,
                            };

                var item = query.FirstOrDefault();
                if (item == null) return HttpNotFound();
                var companyList = db.Companies.Select(c => new { Id = c.CompanyID, Name = c.CompanyName })
               .ToList();
                ViewBag.CompanyName = new SelectList(companyList, "Id", "Name");

                ViewBag.Title = "Chi tiết hồ sơ";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                    new Tuple<string, string>("Applications", Url.Action("Index")),
                    new Tuple<string, string>($"#{item.ApplicationId}", null)
                };
                return View(item);
            }
        }

        // GET: Admin/Applications/Create - FIX: Thêm .ToList() và JobOptions
        public ActionResult Create()
        {
            using (var db = GetDataContext())
            {
                ViewBag.Title = "Thêm hồ sơ ứng tuyển mới";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("Applications", Url.Action("Index")),
            new Tuple<string, string>("Thêm mới", null)
        };

                // Lấy danh sách Ứng viên (Candidates)
                var candidateList = db.Candidates
                                      .Select(c => new { Id = c.CandidateID, Name = c.FullName })
                                      .ToList();
                ViewBag.CandidateOptions = new SelectList(candidateList, "Id", "Name");

                // Lấy danh sách Công việc (JobPosts) và Tên Công ty (CompanyName)
                // FIX: Thêm JOIN Company và chỉ hiển thị CompanyName
                var jobList = (from j in db.JobPosts
                               join company in db.Companies on j.CompanyID equals company.CompanyID
                               select new
                               {
                                   Id = j.JobPostID,
                                   // Chỉ hiển thị Tên Công ty
                                   Title = company.CompanyName
                               }).ToList();

                ViewBag.JobOptions = new SelectList(jobList, "Id", "Title");
                var companyList = db.Companies
            .Select(c => new { Id = c.CompanyID, Name = c.CompanyName })
            .ToList();
                ViewBag.CompanyName = new SelectList(companyList, "Id", "Name");

                ViewBag.StatusOptions = ApplicationStatusHelper.GetStatusSelectList();
                return View(new CreateApplicationListVm());
            }
        }

        // POST: Admin/Applications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateApplicationListVm model)
        {
            using (var db = GetDataContext())
            {
                if (ModelState.IsValid)
                {
                    var newApp = new Application
                    {
                        CandidateID = model.CandidateId,
                        JobPostID = model.JobPostId,
                        AppliedAt = DateTime.Now,
                        Status = model.AppStatus ?? ApplicationStatusHelper.UnderReview,
                        ResumeFilePath = model.ResumeFilePath,
                        CertificateFilePath = model.CertificateFilePath,
                        Note = model.Note,
                        UpdatedAt = DateTime.Now
                    };

                    db.Applications.InsertOnSubmit(newApp);
                    db.SubmitChanges();

                    TempData["SuccessMessage"] = "Tạo hồ sơ ứng tuyển thành công!";
                    return RedirectToAction("Index");
                }

                // --- Logic load lại View khi ModelState không hợp lệ ---

                // Lấy danh sách Ứng viên (Candidates)
                var candidateList = db.Candidates
                                      .Select(c => new { Id = c.CandidateID, Name = c.FullName })
                                      .ToList();
                ViewBag.CandidateOptions = new SelectList(candidateList, "Id", "Name", model.CandidateId);

                // Lấy danh sách Công việc (JobPosts) và Tên Công ty (CompanyName)
                // FIX: Thêm JOIN Company và chỉ hiển thị CompanyName
                var jobList = (from j in db.JobPosts
                               join company in db.Companies on j.CompanyID equals company.CompanyID
                               select new
                               {
                                   Id = j.JobPostID,
                                   // Chỉ hiển thị Tên Công ty
                                   Title = company.CompanyName
                               }).ToList();

                ViewBag.JobOptions = new SelectList(jobList, "Id", "Title", model.JobPostId);
                var companyList = db.Companies
                            .Select(c => new { Id = c.CompanyID, Name = c.CompanyName })
                            .ToList();
                ViewBag.CompanyName = new SelectList(companyList, "Id", "Name");
                ViewBag.StatusOptions = ApplicationStatusHelper.GetStatusSelectList(model.AppStatus);
                return View(model);
            }
        }
        public ActionResult Edit(int id)
        {
            using (var db = GetDataContext())
            {
                var app = db.Applications.FirstOrDefault(x => x.ApplicationID == id);
                if (app == null) return HttpNotFound();

                var item = new EditApplicationListVm
                {
                    ApplicationId = app.ApplicationID,
                    CandidateId = app.CandidateID,
                    JobPostId = app.JobPostID,
                    AppStatus = app.Status,
                    ResumeFilePath = app.ResumeFilePath,
                    CertificateFilePath = app.CertificateFilePath,
                    Note = app.Note,
                };

                ViewBag.Title = "Sửa hồ sơ ứng tuyển";


                var candidateList = db.Candidates
                                      .Select(c => new { Id = c.CandidateID, Name = c.FullName })
                                      .ToList();
                ViewBag.CandidateOptions = new SelectList(candidateList, "Id", "Name", item.CandidateId);
                var companyList = db.Companies
                            .Select(c => new { Id = c.CompanyID, Name = c.CompanyName })
                            .ToList();
                ViewBag.CompanyName = new SelectList(companyList, "Id", "Name");
                var jobList = db.JobPosts
                                .Select(j => new { Id = j.JobPostID, Title = j.Title })
                                .ToList();
                ViewBag.JobOptions = new SelectList(jobList, "Id", "Title", item.JobPostId);

                ViewBag.StatusOptions = ApplicationStatusHelper.GetStatusSelectList(item.AppStatus);
                ViewBag.CompanyName = new SelectList(companyList, "Id", "Name");

                return View(item);
            }
        }

        // POST: Admin/Applications/Edit/5 - FIX: Loại bỏ ép kiểu object nếu VM đã chuẩn hóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditApplicationListVm model)
        {
            using (var db = GetDataContext())
            {
                if (ModelState.IsValid)
                {
                    var appToUpdate = db.Applications.FirstOrDefault(x => x.ApplicationID == model.ApplicationId);

                    if (appToUpdate == null) return HttpNotFound();

                    // FIX: Nếu VM (EditApplicationListVm) đã được sửa kiểu dữ liệu thành int/string, KHÔNG CẦN ép kiểu (int), (string)
                    appToUpdate.CandidateID = model.CandidateId;
                    appToUpdate.JobPostID = model.JobPostId;
                    appToUpdate.Status = model.AppStatus ?? ApplicationStatusHelper.UnderReview;
                    appToUpdate.ResumeFilePath = model.ResumeFilePath;
                    appToUpdate.CertificateFilePath = model.CertificateFilePath;
                    appToUpdate.Note = model.Note;
                    appToUpdate.UpdatedAt = DateTime.Now;

                    db.SubmitChanges();

                    TempData["SuccessMessage"] = "Cập nhật hồ sơ ứng tuyển thành công!";
                    return RedirectToAction("Index");
                }

                // FIX: Buộc truy vấn thực thi bằng .ToList() khi ModelState không hợp lệ
                var candidateList = db.Candidates
                                      .Select(c => new { Id = c.CandidateID, Name = c.FullName })
                                      .ToList();
                ViewBag.CandidateOptions = new SelectList(candidateList, "Id", "Name", model.CandidateId);

                var jobList = db.JobPosts
                                .Select(j => new { Id = j.JobPostID, Title = j.Title })
                                .ToList();
                ViewBag.JobOptions = new SelectList(jobList, "Id", "Title", model.JobPostId);

                ViewBag.StatusOptions = ApplicationStatusHelper.GetStatusSelectList(model.AppStatus);
                var companyList = db.Companies
                            .Select(c => new { Id = c.CompanyID, Name = c.CompanyName })
                            .ToList();
                ViewBag.CompanyName = new SelectList(companyList, "Id", "Name");
                return View(model);
            }
        }

        // Các phương thức Delete không cần sửa nhiều
        public ActionResult Delete(int id)
        {
            using (var db = GetDataContext())
            {
                var query = from app in db.Applications
                            join candidate in db.Candidates on app.CandidateID equals candidate.CandidateID
                            where app.ApplicationID == id
                            select new ApplicationListVm
                            {
                                ApplicationId = app.ApplicationID,
                                CandidateName = candidate.FullName,
                                JobTitle = "Tên công việc (Cần JobPost)",
                                AppliedAt = app.AppliedAt,
                                AppStatus = app.Status
                            };

                var item = query.FirstOrDefault();
                if (item == null) return HttpNotFound();

                ViewBag.Title = "Xóa hồ sơ ứng tuyển";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Applications", Url.Action("Index")),
                    new Tuple<string, string>($"#{item.ApplicationId}", null)
                };
                return View(item);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = GetDataContext())
            {
                var appToDelete = db.Applications.FirstOrDefault(x => x.ApplicationID == id);
                if (appToDelete == null) return HttpNotFound();

                db.Applications.DeleteOnSubmit(appToDelete);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Xóa hồ sơ ứng tuyển thành công!";
                return RedirectToAction("Index");
            }
        }
    }
}
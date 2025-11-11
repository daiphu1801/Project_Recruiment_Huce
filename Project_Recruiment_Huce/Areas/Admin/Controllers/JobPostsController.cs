using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
using System.Web.Mvc;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class JobPostsController : AdminBaseController
    {
        // GET: Admin/JobPosts
        // NOTE: This controller uses MockData as a template/base.
        // Team members should follow AccountsController pattern to implement CRUD with database.
        public ActionResult Index(string q, string status = null, int? companyId = null, int? recruiterId = null, int page = 1)
        {
            ViewBag.Title = "Tin tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
            new Tuple<string, string>("JobPosts", null),
            new Tuple<string, string>("Tin Tuyển Dụng", null)
             };

            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var query = from job in db.JobPosts
                            join company in db.Companies on job.CompanyID equals company.CompanyID
                            join recruiter in db.Recruiters on job.RecruiterID equals recruiter.RecruiterID
                            select new
                            {
                                job.JobCode,
                                job.Title,
                                company.CompanyName,
                                company.Address,
                                job.Status,
                                job.RecruiterID,
                                recruiter.FullName,
                                job.SalaryFrom,
                                job.SalaryTo,
                                job.PostedAt,
                                job.UpdatedAt,
                                job.EmploymentType,
                                job.Description,
                                job.JobPostID
                            };

                // Tìm kiếm
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.Where(r =>
                       (r.Title ?? "").Contains(q) ||
                       (r.JobCode ?? "").Contains(q) ||
                       (r.CompanyName ?? "").Contains(q));
                }

                // Lọc trạng thái
                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase));

                // Lọc theo công ty
                if (companyId.HasValue)
                {
                    var comp = db.Companies
                                 .Where(c => c.CompanyID == companyId.Value)
                                 .Select(c => c.CompanyName)
                                 .FirstOrDefault();
                    if (!string.IsNullOrEmpty(comp))
                        query = query.Where(x => string.Equals(x.CompanyName, comp, StringComparison.OrdinalIgnoreCase));
                }

                // Lọc theo nhà tuyển dụng
                if (recruiterId.HasValue)
                {
                    query = query.Where(x => x.RecruiterID == recruiterId.Value);
                }

                // ViewBag dropdowns
                ViewBag.StatusOptions = new SelectList(new[] { "Visible", "Hidden", "Closed", "Draft" });
                ViewBag.CompanyOptions = new SelectList(
                    db.Companies.Select(c => new { Id = c.CompanyID, Name = c.CompanyName }).ToList(),
                    "Id", "Name"
                );
                ViewBag.RecruiterOptions = new SelectList(
                    db.Recruiters.Select(r => new { Id = r.RecruiterID, Name = r.FullName }).ToList(),
                    "Id", "Name"
                );

                // Map sang ViewModel
                var JobPosts = query.ToList().Select(r => new JobPostListVm
                {
                    JobCode = r.JobCode,
                    Title = r.Title,
                    CompanyName = r.CompanyName,
                    FullName = r.FullName,
                    SalaryFrom = r.SalaryFrom,
                    SalaryTo = r.SalaryTo,
                    EmploymentType = r.EmploymentType,
                    PostedAt = (DateTime)r.PostedAt,
                    UpdatedAt = (DateTime)r.UpdatedAt,
                    Status = r.Status,
                    JobPostID=r.JobPostID

                }).ToList();

                return View(JobPosts);
            }

        }











        //GET: Admin/JobPosts/Details/5
        // NOTE: This action uses MockData as a template/base.
        // Team members should follow AccountsController pattern to implement CRUD with database.
        // public ActionResult Details(int id)
        //{
        // using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
        // {
        //     var JobPost = db.JobPosts.FirstOrDefault(r => r.JobPostID == id);
        //     if (JobPost == null) return HttpNotFound();
        //     var JobPostDetail = db.JobPostDetails.FirstOrDefault(a => a.DetailID = JobPost.DetailID);
        //     var company = db.Companies.FirstOrDefault(c => c.CompanyID == JobPost.CompanyID);
        //     var vm = new JobPostListVm
        //     {
        //         JobCode = JobPost.JobCode,
        //         Title = JobPost.Title,
        //         CompanyID = JobPost.CompanyID,
        //         CompanyName = company != null ? company.CompanyName : null,
        //         RecruiterID = JobPost.RecruiterID,
        //         FullName = Recruiter


        //     }

        //     }
        //     ;
        // return View(item);
        // }


        // GET: Admin/JobPosts/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm tin tuyển dụng mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
    {
        new Tuple<string, string>("Tin tuyển dụng", Url.Action("Index")),
        new Tuple<string, string>("Thêm mới", null)
    };

            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                ViewBag.CompanyOptions = new SelectList(
                    db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(),
                    "CompanyID",
                    "CompanyName"
                );

                ViewBag.RecruiterOptions = new SelectList(
                    db.Recruiters.Select(r => new { r.RecruiterID, r.FullName }).ToList(),
                    "RecruiterID",
                    "FullName"
                );

                ViewBag.StatusOptions = new SelectList(new[] { "Visible", "Hidden", "Closed", "Draft" });
            }

            return View();
        }

        // POST: Admin/JobPosts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JobPostCreateVm model)
        {
            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // ✅ Kiểm tra tiêu đề trùng
                if (!string.IsNullOrWhiteSpace(model.Title) && db.JobPosts.Any(j => j.Title == model.Title))
                {
                    ModelState.AddModelError("Title", "Tiêu đề công việc đã tồn tại");
                }

                // ✅ Kiểm tra SalaryFrom & SalaryTo
                if (model.SalaryFrom < 0 || model.SalaryFrom > 999999999.99m)
                    ModelState.AddModelError("SalaryFrom", "Mức lương tối thiểu không hợp lệ");

                if (model.SalaryTo < 0 || model.SalaryTo > 999999999.99m)
                    ModelState.AddModelError("SalaryTo", "Mức lương tối đa không hợp lệ");

                if (model.SalaryFrom > model.SalaryTo)
                    ModelState.AddModelError("", "Mức lương tối thiểu không được lớn hơn mức lương tối đa");

                // ✅ Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(model.JobCode))
                    ModelState.AddModelError("JobCode", "Mã công việc là bắt buộc");

                if (model.RecruiterID <= 0)
                    ModelState.AddModelError("RecruiterID", "Nhà tuyển dụng là bắt buộc");

                if (model.CompanyID <= 0)
                    ModelState.AddModelError("CompanyID", "Công ty là bắt buộc");

                if (string.IsNullOrWhiteSpace(model.Description))
                    ModelState.AddModelError("Description", "Mô tả là bắt buộc");

                if (string.IsNullOrWhiteSpace(model.Requirements))
                    ModelState.AddModelError("Requirements", "Yêu cầu là bắt buộc");

                if (string.IsNullOrWhiteSpace(model.SalaryCurrency))
                    ModelState.AddModelError("SalaryCurrency", "Loại tiền lương là bắt buộc");

                if (string.IsNullOrWhiteSpace(model.Location))
                    ModelState.AddModelError("Location", "Địa điểm là bắt buộc");

                if (string.IsNullOrWhiteSpace(model.EmploymentType))
                    ModelState.AddModelError("EmploymentType", "Loại hình công việc là bắt buộc");

                if (model.ApplicationDeadline == default)
                    ModelState.AddModelError("ApplicationDeadline", "Hạn nộp hồ sơ là bắt buộc");

                if (string.IsNullOrWhiteSpace(model.Status))
                    ModelState.AddModelError("Status", "Trạng thái là bắt buộc");

                // Nếu có lỗi, load lại dropdown
                if (!ModelState.IsValid)
                {
                    ViewBag.CompanyOptions = new SelectList(
                        db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(),
                        "CompanyID",
                        "CompanyName",
                        model.CompanyID
                    );

                    ViewBag.RecruiterOptions = new SelectList(
                        db.Recruiters.Select(r => new { r.RecruiterID, r.FullName }).ToList(),
                        "RecruiterID",
                        "FullName",
                        model.RecruiterID
                    );

                    ViewBag.StatusOptions = new SelectList(new[] { "Visible", "Hidden", "Closed", "Draft" }, model.Status);

                    return View(model);
                }

                // Tạo đối tượng JobPost mới
                var jobPost = new JobPost
                {
                    JobCode = model.JobCode,
                    RecruiterID = model.RecruiterID,
                    CompanyID = model.CompanyID,
                    Title = model.Title,
                    Description = model.Description,
                    Requirements = model.Requirements,
                    SalaryFrom = model.SalaryFrom,
                    SalaryTo = model.SalaryTo,
                    SalaryCurrency = model.SalaryCurrency,
                    Location = model.Location,
                    EmploymentType = model.EmploymentType,
                    ApplicationDeadline = model.ApplicationDeadline,
                    Status = model.Status,
                    PostedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now

                };

                db.JobPosts.InsertOnSubmit(jobPost);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Tạo tin tuyển dụng thành công!";
                return RedirectToAction("Index");
            }
        }








        // GET: Admin/JobPosts/Edit/5
        // GET: Admin/JobPosts/Edit/5
        public ActionResult Edit(int id)
        {
            if (id == 0) // Lỗi này có thể do ID = 0 không hợp lệ
            {
                // Xử lý lỗi: ID không hợp lệ hoặc không tìm thấy
                return HttpNotFound();
            }
            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var jobPost = db.JobPosts.FirstOrDefault(j => j.JobPostID == id);

                if (jobPost == null)
                {
                    return HttpNotFound();
                }

                // Load dropdown options with current selected values
                ViewBag.CompanyOptions = new SelectList(
                    db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(),
                    "CompanyID",
                    "CompanyName",
                    jobPost.CompanyID
                );

                ViewBag.RecruiterOptions = new SelectList(
                    db.Recruiters.Select(r => new { r.RecruiterID, r.FullName }).ToList(),
                    "RecruiterID",
                    "FullName",
                    jobPost.RecruiterID
                );

                ViewBag.StatusOptions = new SelectList(
                    new[] { "Visible", "Hidden", "Closed", "Draft" },
                    jobPost.Status
                );

                // Map entity to view model
                var vm = new JobPostEditVm
                {
                    JobPostID = jobPost.JobPostID,
                    JobCode = jobPost.JobCode ?? string.Empty,
                    RecruiterID = jobPost.RecruiterID,
                    CompanyID = jobPost.CompanyID,
                    Title = jobPost.Title ?? string.Empty,
                    Description = jobPost.Description ?? string.Empty,
                    Requirements = jobPost.Requirements ?? string.Empty,
                    SalaryFrom = jobPost.SalaryFrom,
                    SalaryTo = jobPost.SalaryTo,
                    SalaryCurrency = jobPost.SalaryCurrency ?? string.Empty,
                    Location = jobPost.Location ?? string.Empty,
                    EmploymentType = jobPost.EmploymentType ?? string.Empty,
                    ApplicationDeadline = jobPost.ApplicationDeadline,
                    Status = jobPost.Status ?? string.Empty,
                    PostedAt = jobPost.PostedAt
                };

                ViewBag.Title = "Chỉnh sửa tin tuyển dụng";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("Tin tuyển dụng", Url.Action("Index")),
            new Tuple<string, string>($"#{jobPost.JobPostID}", null)
        };

                return View(vm);
            }
        }

        // POST: Admin/JobPosts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JobPostEditVm model)
        {
            // Nếu ModelState không hợp lệ, load lại dropdown
            if (!ModelState.IsValid)
            {
                using (var db = new JOBPORTAL_ENDataContext(
                    ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    ViewBag.CompanyOptions = new SelectList(
                        db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(),
                        "CompanyID",
                        "CompanyName",
                        model.CompanyID
                    );

                    ViewBag.RecruiterOptions = new SelectList(
                        db.Recruiters.Select(r => new { r.RecruiterID, r.FullName }).ToList(),
                        "RecruiterID",
                        "FullName",
                        model.RecruiterID
                    );

                    ViewBag.StatusOptions = new SelectList(
                        new[] { "Visible", "Hidden", "Closed", "Draft" },
                        model.Status
                    );
                }
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var jobPost = db.JobPosts.FirstOrDefault(j => j.JobPostID == model.JobPostID);

                if (jobPost == null)
                {
                    return HttpNotFound();
                }

                //  Kiểm tra tiêu đề trùng (trừ chính nó)
                

                //  Kiểm tra JobCode trùng (trừ chính nó)
                if (!string.IsNullOrWhiteSpace(model.JobCode) &&
                    db.JobPosts.Any(j => j.JobCode == model.JobCode && j.JobPostID != model.JobPostID))
                {
                    ModelState.AddModelError("JobCode", "Mã công việc đã tồn tại");
                    ViewBag.CompanyOptions = new SelectList(
                        db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(),
                        "CompanyID",
                        "CompanyName",
                        model.CompanyID
                    );
                    ViewBag.RecruiterOptions = new SelectList(
                        db.Recruiters.Select(r => new { r.RecruiterID, r.FullName }).ToList(),
                        "RecruiterID",
                        "FullName",
                        model.RecruiterID
                    );
                    ViewBag.StatusOptions = new SelectList(
                        new[] { "Visible", "Hidden", "Closed", "Draft" },
                        model.Status
                    );
                    return View(model);
                }

                // Kiểm tra SalaryFrom & SalaryTo
                if (model.SalaryFrom < 0 || model.SalaryFrom > 999999999.99m)
                {
                    ModelState.AddModelError("SalaryFrom", "Mức lương tối thiểu không hợp lệ");
                    ViewBag.CompanyOptions = new SelectList(
                        db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(),
                        "CompanyID",
                        "CompanyName",
                        model.CompanyID
                    );
                    ViewBag.RecruiterOptions = new SelectList(
                        db.Recruiters.Select(r => new { r.RecruiterID, r.FullName }).ToList(),
                        "RecruiterID",
                        "FullName",
                        model.RecruiterID
                    );
                    ViewBag.StatusOptions = new SelectList(
                        new[] { "Visible", "Hidden", "Closed", "Draft" },
                        model.Status
                    );
                    return View(model);
                }

                if (model.SalaryTo < 0 || model.SalaryTo > 999999999.99m)
                {
                    ModelState.AddModelError("SalaryTo", "Mức lương tối đa không hợp lệ");
                    ViewBag.CompanyOptions = new SelectList(
                        db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(),
                        "CompanyID",
                        "CompanyName",
                        model.CompanyID
                    );
                    ViewBag.RecruiterOptions = new SelectList(
                        db.Recruiters.Select(r => new { r.RecruiterID, r.FullName }).ToList(),
                        "RecruiterID",
                        "FullName",
                        model.RecruiterID
                    );
                    ViewBag.StatusOptions = new SelectList(
                        new[] { "Visible", "Hidden", "Closed", "Draft" },
                        model.Status
                    );
                    return View(model);
                }

                if (model.SalaryFrom > model.SalaryTo)
                {
                    ModelState.AddModelError("", "Mức lương tối thiểu không được lớn hơn mức lương tối đa");
                    ViewBag.CompanyOptions = new SelectList(
                        db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(),
                        "CompanyID",
                        "CompanyName",
                        model.CompanyID
                    );
                    ViewBag.RecruiterOptions = new SelectList(
                        db.Recruiters.Select(r => new { r.RecruiterID, r.FullName }).ToList(),
                        "RecruiterID",
                        "FullName",
                        model.RecruiterID
                    );
                    ViewBag.StatusOptions = new SelectList(
                        new[] { "Visible", "Hidden", "Closed", "Draft" },
                        model.Status
                    );
                    return View(model);
                }

                // ✅ Cập nhật thông tin JobPost
                jobPost.JobCode = model.JobCode;
                jobPost.RecruiterID = model.RecruiterID;
                jobPost.CompanyID = model.CompanyID;
                jobPost.Title = model.Title;
                jobPost.Description = model.Description;
                jobPost.Requirements = model.Requirements;
                jobPost.SalaryFrom = model.SalaryFrom;
                jobPost.SalaryTo = model.SalaryTo;
                jobPost.SalaryCurrency = model.SalaryCurrency;
                jobPost.Location = model.Location;
                jobPost.EmploymentType = model.EmploymentType;
                jobPost.ApplicationDeadline = model.ApplicationDeadline;
                jobPost.Status = model.Status;
                jobPost.UpdatedAt = DateTime.Now;  

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật tin tuyển dụng thành công!";
                return RedirectToAction("Index");
            }
        }
        // GET: Admin/JobPosts/Delete/5
        // GET: Admin/JobPosts/Delete/5
        public ActionResult Delete(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var job = (from j in db.JobPosts
                           join c in db.Companies on j.CompanyID equals c.CompanyID
                           join r in db.Recruiters on j.RecruiterID equals r.RecruiterID
                           where j.JobPostID == id
                           select new JobPostListVm
                           {
                               JobPostID = j.JobPostID,
                               JobCode = j.JobCode,
                               Title = j.Title,
                               CompanyName = c.CompanyName,
                               FullName = r.FullName,
                               SalaryFrom = j.SalaryFrom,
                               SalaryTo = j.SalaryTo,
                               EmploymentType = j.EmploymentType,
                               PostedAt = (DateTime)j.PostedAt,
                               UpdatedAt = (DateTime)j.UpdatedAt,
                               Status = j.Status
                           }).FirstOrDefault();

                if (job == null)
                {
                    return HttpNotFound("Không tìm thấy tin tuyển dụng cần xóa.");
                }

                return View(job);
            }
        }
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int JobPostID)
        {
            try
            {
                using (var db = new JOBPORTAL_ENDataContext(
                    ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    var job = db.JobPosts.SingleOrDefault(j => j.JobPostID == JobPostID);
                    if (job == null)
                    {
                        return HttpNotFound("Không tìm thấy tin tuyển dụng để xóa.");
                    }

                    db.JobPosts.DeleteOnSubmit(job);
                    db.SubmitChanges();

                    TempData["SuccessMessage"] = "Xóa tin tuyển dụng thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


    }
}



//POST: Admin / JobPosts / Delete / 5
//// NOTE: This action uses MockData as a template/base.
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//        public ActionResult DeleteConfirmed(int id)
//{
//    // Mock implementation - in real scenario, delete from database
//    TempData["SuccessMessage"] = "Xóa tin tuyển dụng thành công! (MockData)";
//    return RedirectToAction("Index");
//}
//    }
//}



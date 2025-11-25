using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models; 
using Project_Recruiment_Huce.Models; 
using Project_Recruiment_Huce.Helpers; 

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class ApplicationsController : AdminBaseController
    {
        
        public ActionResult Index(string q, string status = null, int? companyId = null, int? jobId = null, int page = 1)
        {
            ViewBag.Title = "Quản lý hồ sơ ứng tuyển";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Hồ sơ ứng tuyển", null) };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {

                
                var query = from app in db.Applications
                            join candidate in db.Candidates on app.CandidateID equals candidate.CandidateID
                            join job in db.JobPosts on app.JobPostID equals job.JobPostID
                            join company in db.Companies on job.CompanyID equals company.CompanyID
                            select new ApplicationListVm
                            {
                                ApplicationId = app.ApplicationID,
                                CandidateId = app.CandidateID,
                                CandidateName = candidate.FullName,
                                JobPostId = app.JobPostID,
                                JobTitle = job.Title,
                                CompanyId = company.CompanyID,      
                                CompanyName = company.CompanyName,  
                                AppliedAt = app.AppliedAt,
                                AppStatus = app.Status,
                                ResumeFilePath = app.ResumeFilePath
                            };

                

                // 1. Tìm kiếm từ khóa
                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.ToLower().Trim();
                    query = query.Where(x => x.CandidateName.ToLower().Contains(q) || x.JobTitle.ToLower().Contains(q));
                }

                // 2. Lọc theo Trạng thái
                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(x => x.AppStatus == status);
                }

                // 3. Lọc theo Công ty
                if (companyId.HasValue)
                {
                    query = query.Where(x => x.CompanyId == companyId.Value);
                }

                // 4. Lọc theo Công việc
                if (jobId.HasValue)
                {
                    query = query.Where(x => x.JobPostId == jobId.Value);
                }

                // C. PHÂN TRANG & SẮP XẾP
                int pageSize = 10;
                int totalRecords = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var data = query.OrderByDescending(x => x.AppliedAt) // Mới nhất lên đầu
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

                

                // Dropdown Công ty (Lấy tất cả)
                var companies = db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList();
                ViewBag.CompanyId = new SelectList(companies, "CompanyID", "CompanyName", companyId);

                // Dropdown Công việc 
                
                var jobQuery = db.JobPosts.AsQueryable();
                if (companyId.HasValue)
                {
                    jobQuery = jobQuery.Where(j => j.CompanyID == companyId.Value);
                }
                var jobs = jobQuery.Select(j => new { j.JobPostID, j.Title }).ToList();
                ViewBag.JobId = new SelectList(jobs, "JobPostID", "Title", jobId);

                // Dropdown Trạng thái
                ViewBag.StatusOptions = ApplicationStatusHelper.GetStatusSelectList(status);

                
                ViewBag.TotalRecords = totalRecords;
                ViewBag.PageSize = pageSize;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                ViewBag.CurrentQ = q;
                ViewBag.CurrentStatus = status;
                ViewBag.CurrentCompanyId = companyId;
                ViewBag.CurrentJobId = jobId;

                return View(data);
            }
        }

        
        public ActionResult Details(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var appVm = (from app in db.Applications
                             join candidate in db.Candidates on app.CandidateID equals candidate.CandidateID
                             join job in db.JobPosts on app.JobPostID equals job.JobPostID
                             join company in db.Companies on job.CompanyID equals company.CompanyID
                             where app.ApplicationID == id
                             select new ApplicationListVm
                             {
                                 ApplicationId = app.ApplicationID,
                                 CandidateId = app.CandidateID,
                                 CandidateName = candidate.FullName,
                                 JobPostId = app.JobPostID,
                                 JobTitle = job.Title,
                                 CompanyName = company.CompanyName,
                                 AppliedAt = app.AppliedAt,
                                 AppStatus = app.Status,
                                 ResumeFilePath = app.ResumeFilePath,
                                 CertificateFilePath = app.CertificateFilePath,
                                 Note = app.Note
                             }).FirstOrDefault();

                if (appVm == null) return HttpNotFound();

                ViewBag.Title = "Chi tiết hồ sơ";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                    new Tuple<string, string>("Hồ sơ ứng tuyển", Url.Action("Index")),
                    new Tuple<string, string>($"#{appVm.ApplicationId}", null)
                };

                return View(appVm);
            }
        }

        
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm hồ sơ mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                new Tuple<string, string>("Hồ sơ ứng tuyển", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };

            LoadCreateEditDropdowns(); 
            return View(new CreateApplicationListVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateApplicationListVm model)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var newApp = new Application
                        {
                            CandidateID = model.CandidateId,
                            JobPostID = model.JobPostId,
                            AppliedAt = DateTime.Now,
                            Status = model.AppStatus ?? ApplicationStatusHelper.UnderReview,
                            Note = model.Note,
                            UpdatedAt = DateTime.Now
                        };

                        db.Applications.InsertOnSubmit(newApp);
                        db.SubmitChanges();

                        TempData["SuccessMessage"] = "Tạo hồ sơ thành công!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                    }
                }

                LoadCreateEditDropdowns(model.CandidateId, model.JobPostId, model.AppStatus);
                return View(model);
            }
        }

        
        public ActionResult Edit(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var app = db.Applications.FirstOrDefault(x => x.ApplicationID == id);
                if (app == null) return HttpNotFound();

                var vm = new EditApplicationListVm
                {
                    ApplicationId = app.ApplicationID,
                    CandidateId = app.CandidateID,
                    JobPostId = app.JobPostID,
                    AppStatus = app.Status,
                    ResumeFilePath = app.ResumeFilePath,
                    Note = app.Note
                };

                ViewBag.Title = "Cập nhật hồ sơ";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                    new Tuple<string, string>("Hồ sơ ứng tuyển", Url.Action("Index")),
                    new Tuple<string, string>($"#{id}", Url.Action("Details", new { id = id })),
                    new Tuple<string, string>("Sửa", null)
                };

                LoadCreateEditDropdowns(vm.CandidateId, vm.JobPostId, vm.AppStatus);
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditApplicationListVm model)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                if (ModelState.IsValid)
                {
                    var appToUpdate = db.Applications.FirstOrDefault(x => x.ApplicationID == model.ApplicationId);
                    if (appToUpdate == null) return HttpNotFound();

                    try
                    {
                        appToUpdate.CandidateID = model.CandidateId;
                        appToUpdate.JobPostID = model.JobPostId;
                        appToUpdate.Status = model.AppStatus;
                        appToUpdate.Note = model.Note;
                        appToUpdate.UpdatedAt = DateTime.Now;

                        db.SubmitChanges();
                        TempData["SuccessMessage"] = "Cập nhật thành công!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
                    }
                }

                LoadCreateEditDropdowns(model.CandidateId, model.JobPostId, model.AppStatus);
                return View(model);
            }
        }

        
        public ActionResult Delete(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                
                var appVm = (from app in db.Applications
                             join candidate in db.Candidates on app.CandidateID equals candidate.CandidateID
                             join job in db.JobPosts on app.JobPostID equals job.JobPostID
                             where app.ApplicationID == id
                             select new ApplicationListVm
                             {
                                 ApplicationId = app.ApplicationID,
                                 CandidateName = candidate.FullName,
                                 JobTitle = job.Title,
                                 AppStatus = app.Status,
                                 AppliedAt = app.AppliedAt,
                                 Note = app.Note
                             }).FirstOrDefault();

                if (appVm == null) return HttpNotFound();

                ViewBag.Title = "Xóa hồ sơ";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                    new Tuple<string, string>("Hồ sơ ứng tuyển", Url.Action("Index")),
                    new Tuple<string, string>($"#{id}", null)
                };

                return View(appVm);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var app = db.Applications.FirstOrDefault(x => x.ApplicationID == id);
                if (app != null)
                {
                    try
                    {
                        db.Applications.DeleteOnSubmit(app);
                        db.SubmitChanges();
                        TempData["SuccessMessage"] = "Đã xóa hồ sơ thành công!";
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Không thể xóa: " + ex.Message;
                    }
                }
                return RedirectToAction("Index");
            }
        }

        
        private void LoadCreateEditDropdowns(int? selectedCandidate = null, int? selectedJob = null, string selectedStatus = null)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // 1. Dropdown Ứng viên
                var candidates = db.Candidates
                                   .Select(c => new { Id = c.CandidateID, Name = c.FullName + " (#" + c.CandidateID + ")" })
                                   .ToList();
                ViewBag.CandidateOptions = new SelectList(candidates, "Id", "Name", selectedCandidate);

                // 2. Dropdown Công việc (Hiện kèm tên công ty cho dễ chọn)
                var jobs = (from j in db.JobPosts
                            join c in db.Companies on j.CompanyID equals c.CompanyID
                            select new
                            {
                                Id = j.JobPostID,
                                // Hiển thị: "Tuyển Java - FPT Software"
                                Title = j.Title + " - " + c.CompanyName
                            }).ToList();
                ViewBag.JobOptions = new SelectList(jobs, "Id", "Title", selectedJob);

                // 3. Dropdown Trạng thái
                ViewBag.StatusOptions = ApplicationStatusHelper.GetStatusSelectList(selectedStatus);
            }
        }
    }
}
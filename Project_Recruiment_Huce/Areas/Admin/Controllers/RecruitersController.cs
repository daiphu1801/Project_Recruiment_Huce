using Microsoft.Owin.BuilderProperties;
using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    /// <summary>
    /// CRUD controller for recruiters.
    /// </summary>
    public class RecruitersController : AdminBaseController
    {
        // GET: Admin/Recruiters
        public ActionResult Index(string q, string role = null, int page = 1)
        {
            ViewBag.Title = "Quản lý nhà tuyển dụng";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Nhà tuyển dụng", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var query = db.Recruiters.AsQueryable();

                // Search
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.Where(r =>
                        (r.FullName != null && r.FullName.Contains(q)) ||
                        (r.CompanyEmail != null && r.CompanyEmail.Contains(q)) ||
                        (r.Phone != null && r.Phone.Contains(q))
                    );
                }

                // Left join recruiters with companies to include CompanyName for display
                var recruiters = (from r in query
                                  join c in db.Companies on r.CompanyID equals c.CompanyID into gj
                                  from c in gj.DefaultIfEmpty()
                                  select new RecruiterListVm
                                  {
                                      RecruiterId = r.RecruiterID,
                                      AccountId = r.AccountID,
                                      CompanyId = r.CompanyID,
                                      FullName = r.FullName,
                                      PositionTitle = r.PositionTitle,
                                      CompanyEmail = r.CompanyEmail,
                                      Phone = r.Phone,
                                      CreatedAt = r.CreatedAt,
                                      ActiveFlag = r.ActiveFlag,
                                      CompanyName = c != null ? c.CompanyName : null
                                  }).ToList();

                return View(recruiters);
            }
        }

        // GET: Admin/Recruiters/Details/5
        public ActionResult Details(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == id);
                if (recruiter == null) return HttpNotFound();

                var company = db.Companies.FirstOrDefault(c => c.CompanyID == recruiter.CompanyID);

                var vm = new RecruiterListVm
                {
                    RecruiterId = recruiter.RecruiterID,
                    AccountId = recruiter.AccountID,
                    CompanyId = recruiter.CompanyID,
                    FullName = recruiter.FullName,
                    PositionTitle = recruiter.PositionTitle,
                    Phone = recruiter.Phone,
                    CompanyEmail = recruiter.CompanyEmail,
                    CreatedAt = recruiter.CreatedAt,
                    ActiveFlag = recruiter.ActiveFlag,
                    CompanyName = company != null ? company.CompanyName : null
                };

                ViewBag.Title = "Chi tiết nhà tuyển dụng";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                    new Tuple<string, string>($"#{recruiter.RecruiterID}", null)
                };

                return View(vm);
            }
        }

        // GET: Admin/Recruiters/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm nhà tuyển dụng mới";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                new Tuple<string, string>("Thêm mới", null)
            };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Only show accounts that have role 'Recruiter' and are active
                ViewBag.AccountOptions = new SelectList(
                    db.Accounts
                      .Where(a => a.ActiveFlag == 1 && a.Role == "Recruiter")
                      .Select(a => new { a.AccountID, a.Username })
                      .ToList(),
                    "AccountID",
                    "Username"
                );
                ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName");
            }

            return View();
        }

        // POST: Admin/Recruiters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateRecruiterVm model)
        {
            if (!ModelState.IsValid)
            {
                using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && a.Role == "Recruiter")
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                }
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                if (!string.IsNullOrWhiteSpace(model.FullName) && db.Recruiters.Any(r => r.FullName == model.FullName))
                {
                    ModelState.AddModelError("FullName", "Nhà tuyển dụng đã tồn tại");
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && a.Role == "Recruiter")
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                    return View(model);
                }

                // Validate AccountId uniqueness (DB likely enforces a unique constraint on AccountID)
                if (model.AccountId > 0 && db.Recruiters.Any(r => r.AccountID == model.AccountId))
                {
                    ModelState.AddModelError("AccountId", "Tài khoản này đã được liên kết với nhà tuyển dụng khác");
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && a.Role == "Recruiter")
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.CompanyEmail))
                {
                    var emailLower = model.CompanyEmail.ToLowerInvariant();
                    if (db.Recruiters.Any(r => r.CompanyEmail != null && r.CompanyEmail.ToLower() == emailLower))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email đã được sử dụng");
                        ViewBag.AccountOptions = new SelectList(
                            db.Accounts
                              .Where(a => a.ActiveFlag == 1 && a.Role == "Recruiter")
                              .Select(a => new { a.AccountID, a.Username })
                              .ToList(),
                            "AccountID",
                            "Username",
                            model.AccountId
                        );
                        ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                        return View(model);
                    }
                }

                var recruiter = new Recruiter
                {
                    AccountID = model.AccountId,
                    CompanyID = model.CompanyId,
                    FullName = model.FullName,
                    PositionTitle = model.PositionTitle,
                    CompanyEmail = model.CompanyEmail,
                    Phone = model.Phone,
                    CreatedAt = DateTime.Now,
                    ActiveFlag = model.Active ? (byte?)1 : (byte?)0
                };

                db.Recruiters.InsertOnSubmit(recruiter);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Tạo nhà tuyển dụng thành công!";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/Recruiters/Edit/5
        public ActionResult Edit(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == id);
                if (recruiter == null) return HttpNotFound();

                // Only show accounts that have role 'Recruiter' and are active,
                // but include the currently assigned account so the selection isn't lost if its role differs.
                ViewBag.AccountOptions = new SelectList(
                    db.Accounts
                      .Where(a => a.ActiveFlag == 1 && (a.Role == "Recruiter" || a.AccountID == recruiter.AccountID))
                      .Select(a => new { a.AccountID, a.Username })
                      .ToList(),
                    "AccountID",
                    "Username",
                    recruiter.AccountID
                );
                ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", recruiter.CompanyID);

                var vm = new EditRecruiterVm
                {
                    RecruiterId = recruiter.RecruiterID,
                    AccountId = recruiter.AccountID,
                    CompanyId = recruiter.CompanyID,
                    FullName = recruiter.FullName ?? string.Empty,
                    PositionTitle = recruiter.PositionTitle ?? string.Empty,
                    Phone = recruiter.Phone ?? string.Empty,
                    CompanyEmail = recruiter.CompanyEmail ?? string.Empty,
                    ActiveFlag = recruiter.ActiveFlag,
                    Active = recruiter.ActiveFlag == 1
                };

                ViewBag.Title = "Sửa nhà tuyển dụng";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                    new Tuple<string, string>($"#{recruiter.RecruiterID}", null)
                };
                return View(vm);
            }
        }

        // POST: Admin/Recruiters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditRecruiterVm model)
        {
            if (!ModelState.IsValid)
            {
                using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
                {
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && (a.Role == "Recruiter" || a.AccountID == model.AccountId))
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                }
                return View(model);
            }

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == model.RecruiterId);
                if (recruiter == null) return HttpNotFound();

                if (!string.IsNullOrWhiteSpace(model.FullName) && db.Recruiters.Any(r => r.FullName == model.FullName && r.RecruiterID != model.RecruiterId))
                {
                    ModelState.AddModelError("FullName", "Tên nhà tuyển dụng đã tồn tại");
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && (a.Role == "Recruiter" || a.AccountID == model.AccountId))
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                    return View(model);
                }

                // Prevent assigning an account that is already linked to another recruiter
                if (model.AccountId > 0 && db.Recruiters.Any(r => r.AccountID == model.AccountId && r.RecruiterID != model.RecruiterId))
                {
                    ModelState.AddModelError("AccountId", "Tài khoản này đã được liên kết với nhà tuyển dụng khác");
                    ViewBag.AccountOptions = new SelectList(
                        db.Accounts
                          .Where(a => a.ActiveFlag == 1 && (a.Role == "Recruiter" || a.AccountID == model.AccountId))
                          .Select(a => new { a.AccountID, a.Username })
                          .ToList(),
                        "AccountID",
                        "Username",
                        model.AccountId
                    );
                    ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.CompanyEmail))
                {
                    var emailLower = model.CompanyEmail.ToLowerInvariant();
                    if (db.Recruiters.Any(c => c.CompanyEmail != null && c.CompanyEmail.ToLower() == emailLower && c.RecruiterID != model.RecruiterId))
                    {
                        ModelState.AddModelError("CompanyEmail", "Email đã được sử dụng");
                        ViewBag.AccountOptions = new SelectList(
                            db.Accounts
                              .Where(a => a.ActiveFlag == 1 && (a.Role == "Recruiter" || a.AccountID == model.AccountId))
                              .Select(a => new { a.AccountID, a.Username })
                              .ToList(),
                            "AccountID",
                            "Username",
                            model.AccountId
                        );
                        ViewBag.CompanyOptions = new SelectList(db.Companies.Select(c => new { c.CompanyID, c.CompanyName }).ToList(), "CompanyID", "CompanyName", model.CompanyId);
                        return View(model);
                    }
                }

                recruiter.AccountID = model.AccountId;
                recruiter.CompanyID = model.CompanyId;
                recruiter.FullName = model.FullName;
                recruiter.PositionTitle = model.PositionTitle;
                recruiter.CompanyEmail = model.CompanyEmail;
                recruiter.Phone = model.Phone;
                recruiter.ActiveFlag = model.Active ? (byte?)1 : (byte?)0;

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật nhà tuyển dụng thành công!";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/Recruiters/Delete/5
        public ActionResult Delete(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == id);
                if (recruiter == null) return HttpNotFound();

                var company = db.Companies.FirstOrDefault(c => c.CompanyID == recruiter.CompanyID);

                var vm = new RecruiterListVm
                {
                    RecruiterId = recruiter.RecruiterID,
                    AccountId = recruiter.AccountID,
                    CompanyId = recruiter.CompanyID,
                    FullName = recruiter.FullName ?? string.Empty,
                    PositionTitle = recruiter.PositionTitle ?? string.Empty,
                    Phone = recruiter.Phone ?? string.Empty,
                    CompanyEmail = recruiter.CompanyEmail ?? string.Empty,
                    ActiveFlag = recruiter.ActiveFlag,
                    CompanyName = company != null ? company.CompanyName : null
                };

                ViewBag.Title = "Xóa nhà tuyển dụng";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("Nhà tuyển dụng", Url.Action("Index")),
                    new Tuple<string, string>($"#{recruiter.RecruiterID}", null)
                };

                return View(vm);
            }
        }

        // POST: Admin/Recruiters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == id);
                if (recruiter == null) return HttpNotFound();

                db.Recruiters.DeleteOnSubmit(recruiter);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Xóa nhà tuyển dụng thành công!";
                return RedirectToAction("Index");
            }
        }
    }
}

using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
using System.Web.Mvc;
using Project_Recruiment_Huce.Helpers;

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
                JobStatusHelper.NormalizeStatuses(db);
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
                                job.JobPostID,
                                job.CompanyID
                            };
                if (!string.IsNullOrWhiteSpace(status))
                {
                    // Chuyển cả hai vế sang chữ thường. Phương thức .ToLower() được LINQ to SQL dịch thành SQL.
                    string lowerStatus = status.ToLower();
                    query = query.Where(x => x.Status.ToLower() == lowerStatus);
                }



                // Tìm kiếm
                if (!string.IsNullOrWhiteSpace(q))
                {
                    string lowerQ = q.Trim().ToLower();
                    query = query.Where(r =>
                        (r.Title != null && r.Title.ToLower().Contains(lowerQ)) ||
                        (r.JobCode != null && r.JobCode.ToLower().Contains(lowerQ)) ||
                        (r.CompanyName != null && r.CompanyName.ToLower().Contains(lowerQ)));
                }



                // Lọc trạng thái
                if (!string.IsNullOrWhiteSpace(status))
                {
                    string lowerStatus = status.ToLower();
                    query = query.Where(x => x.Status.ToLower() == lowerStatus); // FIX: Sử dụng ToLower() ==
                }

                // Lọc theo công ty
                if (companyId.HasValue)
                {
                    var comp = db.Companies
                                 .Where(c => c.CompanyID == companyId.Value)
                                 .Select(c => c.CompanyName)
                                 .FirstOrDefault();
                    if (!string.IsNullOrEmpty(comp))
                    {
                        string lowerComp = comp.ToLower();
                        query = query.Where(x => x.CompanyName.ToLower() == lowerComp); // FIX: Sử dụng ToLower() ==
                    }
                }


                // ViewBag dropdowns
                ViewBag.StatusOptions = BuildStatusSelectList(status);
                ViewBag.CompanyOptions = new SelectList(
                    db.Companies.Select(c => new { Id = c.CompanyID, Name = c.CompanyName }).ToList(),
                    "Id", "Name"
                );
                ViewBag.RecruiterOptions = new SelectList(
                    db.Recruiters.Select(r => new { Id = r.RecruiterID, Name = r.FullName }).ToList(),
                    "Id", "Name"
                );

                // Map sang ViewModel
                var JobPosts = query.OrderByDescending(j => j.PostedAt)
                             .ToList() // Thực thi query tại đây
                             .Select(r => new JobPostListVm
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
                                 JobPostID = r.JobPostID

                             }).ToList();

                return View(JobPosts);
            }

        }
        //GET: Admin/JobPosts/Details/5
        // NOTE: This action uses MockData as a template/base.
        // Team members should follow AccountsController pattern to implement CRUD with database.
        // GET: Admin/JobPosts/Details/5
        // GET: Admin/JobPosts/Details/5
public ActionResult Details(int id)
{
    ViewBag.Title = "Chi tiết tin tuyển dụng";
    ViewBag.Breadcrumbs = new List<Tuple<string, string>>
    {
        new Tuple<string, string>("Tin tuyển dụng", Url.Action("Index")),
        new Tuple<string, string>($"#{id}", null)
    };

    using (var db = new JOBPORTAL_ENDataContext(
        ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
    {
        // 1. Lấy JobPost theo JobPostID
        var jobPost = db.JobPosts.FirstOrDefault(j => j.JobPostID == id);

        if (jobPost == null)
            return HttpNotFound();

        // 2. ✅ SỬA LỖI: Lấy các thực thể liên quan qua FOREIGN KEY từ jobPost
        var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == jobPost.RecruiterID);
        var company = db.Companies.FirstOrDefault(c => c.CompanyID == jobPost.CompanyID);
        var jobPostDetail = db.JobPostDetails.FirstOrDefault(jpd => jpd.JobPostID == jobPost.JobPostID);

        // 3. Ánh xạ sang ViewModel
        var vm = new JobPostDetailVm
        {
            // JobPost fields
            JobPostID = jobPost.JobPostID,
            JobCode = jobPost.JobCode,
            Title = jobPost.Title,
            Description = jobPost.Description,
            Requirements = jobPost.Requirements,
            Location = jobPost.Location,
            EmploymentType = jobPost.EmploymentType,
            SalaryFrom = jobPost.SalaryFrom,
            SalaryTo = jobPost.SalaryTo,
            SalaryCurrency = jobPost.SalaryCurrency,
            ApplicationDeadline = jobPost.ApplicationDeadline,
            Status = jobPost.Status,
            PostedAt = jobPost.PostedAt ,
            UpdatedAt = jobPost.UpdatedAt ,

            // Company fields (null-safe)
            CompanyID = company?.CompanyID,
            CompanyName = company?.CompanyName,
            Address = company?.Address,
            Website = company?.Website,
            CompanyPhotoID = company?.PhotoID,

            // Recruiter fields (null-safe)
            RecruiterID = recruiter?.RecruiterID ?? 0,
            FullName = recruiter?.FullName ?? "N/A",
            PositionTitle = recruiter?.PositionTitle,
            Phone = recruiter?.Phone,
            RecruiterPhotoID = recruiter?.PhotoID,

            // JobPostDetail fields (null-safe)
            DetailID = jobPostDetail?.DetailID ?? 0,
            Industry = jobPostDetail?.Industry,
            Major = jobPostDetail?.Major,
            YearsExperience = jobPostDetail?.YearsExperience ?? 0,
            DegreeRequired = jobPostDetail?.DegreeRequired,
            Skills = jobPostDetail?.Skills,
            Headcount = jobPostDetail?.Headcount ?? 0,
            GenderRequirement = jobPostDetail?.GenderRequirement ?? "Not required",
            AgeFrom = jobPostDetail?.AgeFrom,
            AgeTo = jobPostDetail?.AgeTo
        };

        // 4. Cập nhật Breadcrumbs với tiêu đề thực tế
        ViewBag.Breadcrumbs = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("Tin tuyển dụng", Url.Action("Index")),
            new Tuple<string, string>($"#{vm.JobPostID} - {vm.Title}", null)
        };

        return View(vm);
    }
}



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
                JobStatusHelper.NormalizeStatuses(db);

                ViewBag.RecruiterOptions = new SelectList(
                    db.Recruiters.Select(r => new { r.RecruiterID, r.FullName }).ToList(),
                    "RecruiterID",
                    "FullName"
                );

                ViewBag.StatusOptions = BuildStatusSelectList(null);
            }

            return View();
        }

        // GET: Admin/JobPosts/GetCompanyByRecruiter
        // Action để lấy CompanyID từ RecruiterID qua AJAX
        public JsonResult GetCompanyByRecruiter(int recruiterId)
        {
            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId);
                if (recruiter != null && recruiter.CompanyID.HasValue)
                {
                    return Json(new { companyId = recruiter.CompanyID.Value }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { companyId = (int?)null }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/JobPosts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JobPostCreateVm model)
        {
            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                JobStatusHelper.NormalizeStatuses(db);
                
                // Xóa validation error cho CompanyID vì nó được tự động lấy từ RecruiterID
                ModelState.Remove("CompanyID");
                
                // Tự động lấy CompanyID từ RecruiterID trước khi validate
                if (model.RecruiterID > 0)
                {
                    var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == model.RecruiterID);
                    if (recruiter != null && recruiter.CompanyID.HasValue)
                    {
                        model.CompanyID = recruiter.CompanyID.Value;
                    }
                }
                
                //  Kiểm tra tiêu đề trùng
                if (!string.IsNullOrWhiteSpace(model.Title) && db.JobPosts.Any(j => j.Title == model.Title))
                {
                    ModelState.AddModelError("Title", "Tiêu đề công việc đã tồn tại");
                }

                //  Kiểm tra SalaryFrom & SalaryTo
                if (model.SalaryFrom < 0 || model.SalaryFrom > 999999999.99m)
                    ModelState.AddModelError("SalaryFrom", "Mức lương tối thiểu không hợp lệ");

                if (model.SalaryTo < 0 || model.SalaryTo > 999999999.99m)
                    ModelState.AddModelError("SalaryTo", "Mức lương tối đa không hợp lệ");

                if (model.SalaryFrom > model.SalaryTo)
                    ModelState.AddModelError("", "Mức lương tối thiểu không được lớn hơn mức lương tối đa");

                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(model.JobCode))
                    ModelState.AddModelError("JobCode", "Mã công việc là bắt buộc");

                if (model.RecruiterID <= 0)
                    ModelState.AddModelError("RecruiterID", "Nhà tuyển dụng là bắt buộc");
                else if (!model.CompanyID.HasValue || model.CompanyID.Value <= 0)
                {
                    // Nếu CompanyID vẫn chưa được set (sau khi đã lấy từ RecruiterID ở trên)
                    ModelState.AddModelError("RecruiterID", "Nhà tuyển dụng này chưa được gán cho công ty nào");
                }

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
                    ViewBag.RecruiterOptions = new SelectList(
                        db.Recruiters.Select(r => new { r.RecruiterID, r.FullName }).ToList(),
                        "RecruiterID",
                        "FullName",
                        model.RecruiterID
                    );

                    ViewBag.StatusOptions = BuildStatusSelectList(model.Status);

                    return View(model);
                }

                // Tạo đối tượng JobPost mới
                var jobPost = new JobPost
                {
                    JobCode = model.JobCode,
                    RecruiterID = model.RecruiterID,
                    CompanyID = model.CompanyID, // CompanyID đã được validate và set ở trên
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
                //TẠO VÀ LƯU JOBPOSTDETAIL(Lần SubmitChanges 2)
        var jobPostDetail = new JobPostDetail
        {
            JobPostID = jobPost.JobPostID, // Dùng ID vừa tạo
            Industry = model.Industry,
            Major = model.Major,
            YearsExperience = model.YearsExperience,
            DegreeRequired = model.DegreeRequired,
            Skills = model.Skills,
            Headcount = model.Headcount,
            // Xử lý GenderRequirement: nếu null, dùng giá trị default của DB
            GenderRequirement = string.IsNullOrWhiteSpace(model.GenderRequirement) ? "Not required" : model.GenderRequirement,
            AgeFrom = model.AgeFrom,
            AgeTo = model.AgeTo,
            // Sử dụng Status của JobPost, JobPostDetails trong DB của bạn có default là 'Published'
            Status = model.Status ?? "Published"
        };

                db.JobPostDetails.InsertOnSubmit(jobPostDetail);
                db.SubmitChanges(); // LƯU LẦN 2: Lưu chi tiết bài đăng

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
                JobStatusHelper.NormalizeStatuses(db);
                var jobPost = db.JobPosts.FirstOrDefault(j => j.JobPostID == id);

                if (jobPost == null)
                {
                    return HttpNotFound();
                }

                jobPost.Status = JobStatusHelper.NormalizeStatus(jobPost.Status);
                // --- BẮT ĐẦU PHẦN SỬA LỖI ---

                // 1. Lấy danh sách recruiters VỚI TÊN CÔNG TY (chuẩn bị 1 lần)
                // (Giả định có quan hệ Recruiter -> Company)
                var recruiters = db.Recruiters
                                   .Select(r => new {
                                       r.RecruiterID,
                                       r.FullName,
                                       CompanyName = r.Company.CompanyName
                                   }).ToList();

                // 2. Tạo SelectList cho dropdown
                ViewBag.RecruiterOptions = new SelectList(
                    recruiters, // Dùng list đã lấy ở trên
                    "RecruiterID",
                    "FullName",
                    jobPost.RecruiterID
                );

                // 3. TẠO MAP cho JavaScript (Key: RecruiterID, Value: CompanyName)
                // Đây là dữ liệu chúng ta sẽ truyền sang View để JS sử dụng
                ViewBag.RecruiterCompanyMap = recruiters.ToDictionary(
                    r => r.RecruiterID.ToString(), // Key (dạng string để JS dễ map)
                    r => r.CompanyName ?? "Chưa gán công ty" // Value
                );

                // --- KẾT THÚC PHẦN SỬA LỖI ---

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

                ViewBag.StatusOptions = BuildStatusSelectList(jobPost.Status);

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

        // GET: Admin/JobPosts/Edit/5
        // POST: Admin/JobPosts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JobPostEditVm model) // Giả định VM này đã có đủ trường
        {
            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                JobStatusHelper.NormalizeStatuses(db);

                // --- BẮT ĐẦU KHỐI VALIDATION (Tương tự Create) ---

                // Xóa validation error cho CompanyID vì nó được tự động lấy từ RecruiterID
                ModelState.Remove("CompanyID");

                // Tự động lấy CompanyID từ RecruiterID trước khi validate
                if (model.RecruiterID > 0)
                {
                    var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == model.RecruiterID);
                    if (recruiter != null && recruiter.CompanyID.HasValue)
                    {
                        model.CompanyID = recruiter.CompanyID.Value;
                    }
                }

                // Kiểm tra tiêu đề trùng (trừ chính nó)
                if (!string.IsNullOrWhiteSpace(model.Title) &&
                    db.JobPosts.Any(j => j.Title == model.Title && j.JobPostID != model.JobPostID))
                {
                    ModelState.AddModelError("Title", "Tiêu đề công việc đã tồn tại");
                }

                // Kiểm tra JobCode trùng (trừ chính nó)
                if (!string.IsNullOrWhiteSpace(model.JobCode) &&
                    db.JobPosts.Any(j => j.JobCode == model.JobCode && j.JobPostID != model.JobPostID))
                {
                    ModelState.AddModelError("JobCode", "Mã công việc đã tồn tại");
                }

                // Kiểm tra SalaryFrom & SalaryTo
                if (model.SalaryFrom < 0 || model.SalaryFrom > 999999999.99m)
                    ModelState.AddModelError("SalaryFrom", "Mức lương tối thiểu không hợp lệ");

                if (model.SalaryTo < 0 || model.SalaryTo > 999999999.99m)
                    ModelState.AddModelError("SalaryTo", "Mức lương tối đa không hợp lệ");

                if (model.SalaryFrom > model.SalaryTo)
                    ModelState.AddModelError("", "Mức lương tối thiểu không được lớn hơn mức lương tối đa");

                // Kiểm tra các trường bắt buộc (tương tự Create)
                if (string.IsNullOrWhiteSpace(model.JobCode))
                    ModelState.AddModelError("JobCode", "Mã công việc là bắt buộc");

                if (model.RecruiterID <= 0)
                    ModelState.AddModelError("RecruiterID", "Nhà tuyển dụng là bắt buộc");
                else if (!model.CompanyID.HasValue || model.CompanyID.Value <= 0)
                {
                    ModelState.AddModelError("RecruiterID", "Nhà tuyển dụng này chưa được gán cho công ty nào");
                }

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

                // --- KẾT THÚC KHỐI VALIDATION ---

                // Nếu có lỗi, load lại dropdown và trả về view
                if (!ModelState.IsValid)
                {
                    ViewBag.RecruiterOptions = new SelectList(
                        db.Recruiters.Select(r => new { r.RecruiterID, r.FullName }).ToList(),
                        "RecruiterID",
                        "FullName",
                        model.RecruiterID
                    );

                    ViewBag.StatusOptions = BuildStatusSelectList(model.Status);

                    // Nạp lại Title và Breadcrumbs
                    ViewBag.Title = "Chỉnh sửa tin tuyển dụng (Lỗi)";
                    ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Tin tuyển dụng", Url.Action("Index")),
                new Tuple<string, string>($"Chỉnh sửa #{model.JobCode}", null)
            };

                    return View(model);
                }

                // Nếu ModelState Hợp Lệ -> Tiến hành cập nhật

                // Lấy các bản ghi hiện tại từ DB
                var jobPost = db.JobPosts.FirstOrDefault(j => j.JobPostID == model.JobPostID);
                var jobPostDetail = db.JobPostDetails.FirstOrDefault(d => d.JobPostID == model.JobPostID);

                if (jobPost == null || jobPostDetail == null)
                {
                    return HttpNotFound("Không tìm thấy bản ghi để cập nhật.");
                }

                // Cập nhật thông tin JobPost
                jobPost.JobCode = model.JobCode;
                jobPost.RecruiterID = model.RecruiterID;
                jobPost.CompanyID = model.CompanyID; // Đã được validate và set ở trên
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

                // Cập nhật thông tin JobPostDetail
                jobPostDetail.Industry = model.Industry;
                jobPostDetail.Major = model.Major;
                jobPostDetail.YearsExperience = model.YearsExperience;
                jobPostDetail.DegreeRequired = model.DegreeRequired;
                jobPostDetail.Skills = model.Skills;
                jobPostDetail.Headcount = model.Headcount;
                jobPostDetail.GenderRequirement = string.IsNullOrWhiteSpace(model.GenderRequirement) ? "Not required" : model.GenderRequirement;
                jobPostDetail.AgeFrom = model.AgeFrom;
                jobPostDetail.AgeTo = model.AgeTo;
                jobPostDetail.Status = model.Status ?? "Published"; // Đồng bộ status

                // Lưu thay đổi cho cả hai đối tượng (chỉ cần 1 lần SubmitChanges)
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
                JobStatusHelper.NormalizeStatuses(db);
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

        private SelectList BuildStatusSelectList(string selectedStatus)
        {
            var statusItems = new[]
            {
                new { Value = JobStatusHelper.Published, Text = "Hiển thị" },
                new { Value = JobStatusHelper.Hidden, Text = "Đã ẩn" },
                new { Value = JobStatusHelper.Closed, Text = "Đã đóng" },
                new { Value = JobStatusHelper.Draft, Text = "Nháp" }
            };

            var value = string.IsNullOrWhiteSpace(selectedStatus) ? JobStatusHelper.Published : selectedStatus;
            return new SelectList(statusItems, "Value", "Text", value);
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



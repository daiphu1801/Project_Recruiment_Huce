using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
// Đảm bảo JobPostDetailVm và JobPostDetailListVm nằm trong namespace này
// Đảm bảo bạn có Model JobPostDetail (Entity) từ DataContext

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    public class JobPostDetailsController : Controller
    {
        // GET: Admin/JobPostDetails?jobId=5
        // Hiển thị danh sách chi tiết của một JobPost cụ thể (thường chỉ có 1 row)
        public ActionResult Index(int? jobId)
        {
            ViewBag.Title = "Chi tiết tin tuyển dụng ";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>>
            {
                new Tuple<string,string>("Đéo biết viết j  ",null)
            };
            if (!jobId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng cung cấp JobPostID.";
                return RedirectToAction("Index", "JobPosts");
            }

            ViewBag.Title = "Yêu cầu chi tiết công việc";
            ViewBag.JobPostID = jobId.Value;

            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Lấy JobPost để hiển thị tiêu đề trong View
                var jobPost = db.JobPosts.FirstOrDefault(j => j.JobPostID == jobId.Value);
                if (jobPost == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy Tin tuyển dụng này.";
                    return RedirectToAction("Index", "JobPosts");
                }
                ViewBag.JobTitle = jobPost.Title;

                // Lấy JobPostDetail Entity
                var detailEntity = db.JobPostDetails
                                   .Where(jpd => jpd.JobPostID == jobId.Value)
                                   .FirstOrDefault();

                // Khởi tạo List ViewModel để trả về View
                var modelList = new List<JobPostDetailVm>();

                // 1. Mapping Entity sang ViewModel và thêm vào List
                if (detailEntity != null)
                {
                    var detailVm = new JobPostDetailVm
                    {
                        // Thông tin JobPost
                        JobPostID = jobPost.JobPostID,
                        Title = jobPost.Title,

                        //Thông tin JobPostDetail (Mapping đầy đủ các trường chi tiết)
                        DetailID = detailEntity.DetailID,
                        Industry = detailEntity.Industry,
                        Major = detailEntity.Major,
                        YearsExperience = detailEntity.YearsExperience,
                        DegreeRequired = detailEntity.DegreeRequired,
                        Skills = detailEntity.Skills,
                        Headcount = detailEntity.Headcount,
                        GenderRequirement = detailEntity.GenderRequirement,
                        AgeFrom = detailEntity.AgeFrom,
                        AgeTo = detailEntity.AgeTo,
                        Status = detailEntity.Status

                        // Lưu ý: Bổ sung thêm các trường JobPost liên quan (CompanyName, FullName) nếu cần
                        // Nhưng chúng ta sẽ chỉ tập trung vào các trường Detail ở đây
                    };

                    // 2. Thêm ViewModel (duy nhất) vào List
                    modelList.Add(detailVm);
                }

                // 3. Trả về View với List ViewModel đã được đóng gói
                // Điều này khắc phục lỗi Model Mismatch (The model item passed... requires a model item of type List<...>)
                return View(modelList);
            }
        }

        // GET: Admin/JobPostDetails/Details/5 (ID là DetailID, không phải JobPostID)
        public ActionResult Details(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(
                ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                // Lấy JobPostDetail bằng DetailID
                var detail = (from jpd in db.JobPostDetails
                              join j in db.JobPosts on jpd.JobPostID equals j.JobPostID
                              where jpd.DetailID == id
                              select new JobPostDetailVm // Sử dụng ViewModel đầy đủ
                              {
                                  // Thông tin JobPostDetail
                                  DetailID = jpd.DetailID,
                                  Industry = jpd.Industry,
                                  Major = jpd.Major,
                                  YearsExperience = jpd.YearsExperience,
                                  DegreeRequired = jpd.DegreeRequired,
                                  Skills = jpd.Skills,
                                  Headcount = jpd.Headcount,
                                  GenderRequirement = jpd.GenderRequirement,
                                  AgeFrom = jpd.AgeFrom,
                                  AgeTo = jpd.AgeTo,

                                  // Thông tin JobPost liên quan
                                  JobPostID = j.JobPostID,
                                  Title = j.Title,
                                  JobCode = j.JobCode
                                  // ...
                              }).FirstOrDefault();

                if (detail == null)
                {
                    TempData["ErrorMessage"] = "Chi tiết yêu cầu không tồn tại.";
                    return RedirectToAction("Index", "JobPosts");
                }

                ViewBag.Title = $"Chi tiết yêu cầu cho: {detail.Title}";
                return View(detail);
            }
        }
    }
}


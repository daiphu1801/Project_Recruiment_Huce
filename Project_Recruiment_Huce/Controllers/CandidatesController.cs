using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Project_Recruiment_Huce.DbContext;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize]
    public class CandidatesController : Controller
    {
        private int? GetCurrentAccountId()
        {
            var idClaim = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) return null;
            int accountId;
            return int.TryParse(idClaim.Value, out accountId) ? (int?)accountId : null;
        }

        [HttpGet]
        public ActionResult CandidatesManage()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new RecruitmentDbContext())
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountId == accountId.Value);
                if (candidate == null)
                {
                    candidate = new Candidate
                    {
                        AccountId = accountId.Value,
                        FullName = User.Identity.Name,
                        Gender = "Nam",
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    db.Candidates.Add(candidate);
                    db.SaveChanges();
                }

                return View(candidate);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CandidatesManage(Candidate form, HttpPostedFileBase avatar)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Do not return early on invalid model; we still allow saving avatar

            using (var db = new RecruitmentDbContext())
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.AccountId == accountId.Value);
                if (candidate == null)
                {
                    candidate = new Candidate { AccountId = accountId.Value, FullName = User.Identity.Name, Gender = "Nam", CreatedAt = DateTime.Now, ActiveFlag = 1 };
                    db.Candidates.Add(candidate);
                }

                // Update profile fields only when model is valid
                if (ModelState.IsValid)
                {
                    candidate.FullName = form.FullName;
                    candidate.BirthDate = form.BirthDate;
                    candidate.Gender = string.IsNullOrWhiteSpace(form.Gender) ? "Nam" : form.Gender;
                    candidate.Phone = form.Phone;
                    candidate.Email = form.Email;
                    candidate.Address = form.Address;
                    if (!string.IsNullOrEmpty(form.About) && form.About.Length > 500)
                    {
                        candidate.About = form.About.Substring(0, 500);
                    }
                    else
                    {
                        candidate.About = form.About;
                    }
                }

                // Handle avatar upload
                if (avatar != null && avatar.ContentLength > 0)
                {
                    var contentType = (avatar.ContentType ?? string.Empty).ToLowerInvariant();
                    var allowed = new[] { "image/jpeg", "image/jpg", "image/pjpeg", "image/png", "image/x-png", "image/gif", "image/webp" };
                    const int maxBytes = 2 * 1024 * 1024; // 2MB
                    if (avatar.ContentLength > maxBytes)
                    {
                        ModelState.AddModelError("", "Image must be 2MB or smaller.");
                        return View(candidate);
                    }
                    if (allowed.Contains(contentType))
                    {
                        var uploadsRoot = Server.MapPath("~/Content/uploads/candidate/");
                        if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                        var ext = Path.GetExtension(avatar.FileName);
                        if (string.IsNullOrEmpty(ext))
                        {
                            // basic fallback by mime
                            ext = contentType.Contains("png") ? ".png" : contentType.Contains("gif") ? ".gif" : contentType.Contains("webp") ? ".webp" : ".jpg";
                        }
                        var safeFileName = $"avatar_{accountId.Value}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
                        var physicalPath = Path.Combine(uploadsRoot, safeFileName);
                        avatar.SaveAs(physicalPath);

                        var relativePath = $"~/Content/uploads/candidate/{safeFileName}";
                        var photo = new Photo
                        {
                            FileName = safeFileName,
                            FilePath = relativePath,
                            SizeKB = (int)Math.Round(avatar.ContentLength / 1024.0),
                            MimeType = avatar.ContentType,
                            UploadedAt = DateTime.UtcNow
                        };
                        db.Photos.Add(photo);
                        db.SaveChanges();

                        // Link to both candidate and account
                        candidate.PhotoId = photo.PhotoId;
                        var account = db.Accounts.FirstOrDefault(a => a.AccountId == accountId.Value);
                        if (account != null)
                        {
                            account.PhotoId = photo.PhotoId;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Only JPG, PNG, GIF or WEBP images are allowed.");
                        return View(candidate);
                    }
                }

                db.SaveChanges();
                if (ModelState.IsValid)
                {
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công.";
                    return RedirectToAction("CandidatesManage");
                }
                // If model invalid, stay on page but keep newly uploaded avatar
                return View(candidate);
            }
        }
    }
}



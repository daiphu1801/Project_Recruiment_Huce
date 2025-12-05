using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Services;
using System.Threading.Tasks;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize(Roles = "Recruiter")]
    public class SubscriptionController : BaseController
    {
        private readonly VietQRService _vietQRService;

        public SubscriptionController()
        {
            _vietQRService = new VietQRService();
        }

        // Plans
        private readonly List<SubscriptionPlan> _plans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan { Id = "Monthly", Name = "Gói Tháng", Price = 25000, Duration = "1 Tháng", Description = "Đăng tin không giới hạn trong 30 ngày." },
            new SubscriptionPlan { Id = "Lifetime", Name = "Gói Trọn Đời", Price = 250000, Duration = "Vĩnh viễn", Description = "Đăng tin không giới hạn trọn đời." }
        };

        // GET: Subscription
        public ActionResult Index()
        {
            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId.Value);
                if (recruiter == null) return HttpNotFound();

                var model = new SubscriptionViewModel
                {
                    CurrentPlan = recruiter.SubscriptionType ?? "Free",
                    ExpiryDate = recruiter.SubscriptionExpiryDate,
                    FreeJobPostCount = recruiter.FreeJobPostCount,
                    Plans = _plans
                };

                return View(model);
            }
        }

        // GET: Subscription/Upgrade
        public async Task<ActionResult> Upgrade(string planId)
        {
            var plan = _plans.FirstOrDefault(p => p.Id == planId);
            if (plan == null)
            {
                TempData["ErrorMessage"] = "Gói dịch vụ không tồn tại.";
                return RedirectToAction("Index");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null) return RedirectToAction("Login", "Account");

            // Generate payment content (max 25 characters for VietQR)
            // Format: UPGRADE {RecruiterID} {PlanID}
            string content = $"UP{recruiterId} {planId}";
            
            // Ensure content is within VietQR limit (25 chars)
            if (content.Length > 25)
            {
                content = content.Substring(0, 25);
            }

            PaymentLogger.Info($"Đang tạo mã QR cho RecruiterID: {recruiterId}, Gói: {planId}, Số tiền: {plan.Price}");

            // Try to generate QR using VietQR.io API with timeout
            VietQRResponse vietQRResponse = null;
            try
            {
                var qrTask = _vietQRService.GenerateQRAsync(plan.Price, content, "compact2");
                if (await Task.WhenAny(qrTask, Task.Delay(5000)) == qrTask)
                {
                    vietQRResponse = await qrTask;
                }
                else
                {
                    PaymentLogger.Warning($"VietQR API quá thời gian chờ cho RecruiterID: {recruiterId}");
                }
            }
            catch (Exception ex)
            {
                PaymentLogger.Error($"Lỗi VietQR API cho RecruiterID: {recruiterId}", ex);
            }
            
            string qrUrl;
            if (vietQRResponse != null && vietQRResponse.code == "00" && vietQRResponse.data != null)
            {
                // Use API-generated QR (base64 data URL)
                qrUrl = vietQRResponse.data.qrDataURL;
                PaymentLogger.Info($"Tạo mã QR thành công qua VietQR API cho RecruiterID: {recruiterId}");
            }
            else
            {
                // Fallback to simple image URL (no API call)
                qrUrl = _vietQRService.GenerateFallbackQRUrl(plan.Price, content);
                PaymentLogger.Warning($"Sử dụng QR dự phòng cho RecruiterID: {recruiterId}");
            }

            ViewBag.Plan = plan;
            ViewBag.QrUrl = qrUrl;
            ViewBag.RecruiterID = recruiterId;
            ViewBag.TransferContent = $"UPGRADE {recruiterId} {planId}"; // Full content for display

            return View();
        }

        // GET: Subscription/CheckPaymentStatus
        [HttpGet]
        public JsonResult CheckPaymentStatus()
        {
            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" }, JsonRequestBehavior.AllowGet);
            }

            using (var db = DbContextFactory.CreateReadOnly())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId.Value);
                if (recruiter == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhà tuyển dụng" }, JsonRequestBehavior.AllowGet);
                }

                var subscriptionType = recruiter.SubscriptionType ?? "Free";
                var expiryDate = recruiter.SubscriptionExpiryDate;

                // Check if subscription is active (not Free)
                if (subscriptionType != "Free")
                {
                    var planName = subscriptionType == "Monthly" ? "Gói Tháng" : "Gói Trọn Đời";
                    var expiryText = expiryDate.HasValue ? expiryDate.Value.ToString("dd/MM/yyyy") : "Vĩnh viễn";
                    
                    return Json(new 
                    { 
                        success = true, 
                        upgraded = true,
                        planName = planName,
                        subscriptionType = subscriptionType,
                        expiryDate = expiryText,
                        message = $"Chúc mừng! Bạn đã nâng cấp thành công lên {planName}!"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, upgraded = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Subscription/SePayWebhook
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SePayWebhook(SePayWebhookModel model)
        {
            var ipAddress = Request.UserHostAddress;
            var userAgent = Request.UserAgent;
            
            // Log incoming webhook request
            PaymentLogger.LogWebhookRequest(ipAddress, userAgent, 
                Newtonsoft.Json.JsonConvert.SerializeObject(model));

            if (model == null)
            {
                PaymentLogger.Warning("Webhook nhận dữ liệu null");
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            // Security Check 1: API Key Validation (SePay requirement)
            var apiKey = Request.Headers["authorization"] ?? Request.Headers["x-key-api-v3a"];
            var expectedKey = ConfigurationManager.AppSettings["SePay:WebhookSecret"];
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                // Remove common prefixes: "Bearer ", "Apikey ", "ApiKey ", etc.
                var prefixes = new[] { "Bearer ", "Apikey ", "ApiKey ", "API-Key " };
                foreach (var prefix in prefixes)
                {
                    if (apiKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        apiKey = apiKey.Substring(prefix.Length).Trim();
                        break;
                    }
                }
                
                if (apiKey != expectedKey)
                {
                    PaymentLogger.Warning($"Webhook bị từ chối: API Key không hợp lệ từ IP {ipAddress}. Received: {apiKey?.Substring(0, Math.Min(10, apiKey.Length))}..., Expected: {expectedKey?.Substring(0, Math.Min(10, expectedKey.Length))}...");
                    return Json(new { success = false, message = "API Key không hợp lệ" });
                }
                PaymentLogger.Info($"API Key xác thực thành công từ IP {ipAddress}");
            }
            else
            {
                PaymentLogger.Warning($"Webhook bị từ chối: Thiếu API Key từ IP {ipAddress}");
                return Json(new { success = false, message = "Thiếu API Key" });
            }

            // Security Check 2: IP Whitelist Validation
            if (!SePaySecurityHelper.IsValidIP(ipAddress))
            {
                PaymentLogger.Warning($"Webhook bị từ chối: IP không hợp lệ {ipAddress}");
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            // Security Check 3: Signature Validation (if provided in header - optional)
            var signature = Request.Headers["X-SePay-Signature"];
            if (!string.IsNullOrEmpty(signature))
            {
                var payload = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                if (!SePaySecurityHelper.ValidateSignature(payload, signature))
                {
                    PaymentLogger.Warning($"Webhook bị từ chối: Chữ ký không hợp lệ từ IP {ipAddress}");
                    return Json(new { success = false, message = "Chữ ký không hợp lệ" });
                }
            }

            // Security Check 4: Timestamp validation (prevent replay attacks)
            DateTime? timestamp = null;
            if (DateTime.TryParse(model.transactionDate, out var parsedDate))
            {
                timestamp = parsedDate;
            }
            if (!SePaySecurityHelper.IsRequestTimestampValid(timestamp, maxAgeMinutes: 10))
            {
                PaymentLogger.Warning($"Webhook bị từ chối: Yêu cầu quá cũ. Thời gian: {model.transactionDate}");
                return Json(new { success = false, message = "Yêu cầu đã hết hạn" });
            }

            try
            {
                using (var db = DbContextFactory.Create())
                {
                    // Security Check 5: Idempotency - Check if transaction already processed
                    var existingTx = db.SePayTransactions
                        .FirstOrDefault(t => t.ReferenceCode == model.referenceCode && !string.IsNullOrEmpty(model.referenceCode));
                    
                    if (existingTx != null)
                    {
                        PaymentLogger.Info($"Phát hiện webhook trùng lặp. ReferenceCode: {model.referenceCode} đã được xử lý.");
                        return Json(new { success = true, message = "Đã xử lý trước đó" });
                    }

                    PaymentLogger.Info($"Đang xử lý webhook mới. Mã tham chiếu: {model.referenceCode}, Số tiền: {model.amountIn}");

                    // Log transaction
                    var transaction = new SePayTransaction
                    {
                        Gateway = model.gateway,
                        TransactionDate = DateTime.TryParse(model.transactionDate, out var tDate) ? tDate : DateTime.Now,
                        AccountNumber = model.accountNumber,
                        SubAccount = model.subAccount,
                        AmountIn = model.amountIn,
                        AmountOut = model.amountOut,
                        Accumulated = model.accumulated,
                        Code = model.code,
                        TransactionContent = model.transactionContent,
                        ReferenceCode = model.referenceCode,
                        Description = model.description,
                        CreatedAt = DateTime.Now
                    };
                    db.SePayTransactions.InsertOnSubmit(transaction);
                    db.SubmitChanges();
                    
                    PaymentLogger.Info($"Đã ghi nhận giao dịch. ID: {transaction.Id}, Số tiền: {transaction.AmountIn}");

                    // Process Upgrade
                    // Expected Content Formats:
                    // - Full: UPGRADE {RecruiterID} {PlanID} (e.g., UPGRADE 123 Monthly)
                    // - Short: UP{RecruiterID} {PlanID} (e.g., UP123 Monthly) - for VietQR 25 char limit
                    string content = model.transactionContent ?? model.description;
                    PaymentLogger.Info($"Đang xử lý nội dung thanh toán: {content}");
                    
                    if (!string.IsNullOrEmpty(content) && (content.Contains("UPGRADE") || content.Contains("UP")))
                    {
                        var parts = content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        
                        // Try to find "UPGRADE" or "UP" pattern
                        int upgradeIndex = Array.FindIndex(parts, p => 
                            p.Equals("UPGRADE", StringComparison.OrdinalIgnoreCase) || 
                            p.StartsWith("UP", StringComparison.OrdinalIgnoreCase));
                        
                        if (upgradeIndex >= 0 && parts.Length > upgradeIndex + 1)
                        {
                            string firstPart = parts[upgradeIndex];
                            string recruiterIdStr = "";
                            string planId = "";
                            
                            // Check if format is "UP123" (short format)
                            if (firstPart.StartsWith("UP", StringComparison.OrdinalIgnoreCase) && firstPart.Length > 2)
                            {
                                recruiterIdStr = firstPart.Substring(2); // Extract ID from "UP123"
                                planId = parts.Length > upgradeIndex + 1 ? parts[upgradeIndex + 1] : "";
                            }
                            // Format is "UPGRADE 123" (full format)
                            else
                            {
                                recruiterIdStr = parts.Length > upgradeIndex + 1 ? parts[upgradeIndex + 1] : "";
                                planId = parts.Length > upgradeIndex + 2 ? parts[upgradeIndex + 2] : "";
                            }

                            if (int.TryParse(recruiterIdStr, out int recruiterId))
                            {
                                PaymentLogger.Info($"Đã phân tích RecruiterID: {recruiterId}, Gói: {planId}");
                                
                                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId);
                                if (recruiter == null)
                                {
                                    PaymentLogger.Warning($"Không tìm thấy nhà tuyển dụng: {recruiterId}");
                                }
                                else
                                {
                                    // Verify Amount (Optional but recommended)
                                    var plan = _plans.FirstOrDefault(p => p.Id.Equals(planId, StringComparison.OrdinalIgnoreCase));
                                    if (plan == null)
                                    {
                                        PaymentLogger.Warning($"Không tìm thấy gói: {planId}");
                                    }
                                    else if (model.amountIn < plan.Price)
                                    {
                                        PaymentLogger.Warning($"Số tiền không khớp. Yêu cầu: {plan.Price}, Nhận được: {model.amountIn}");
                                    }
                                    else
                                    {
                                        var oldSubscription = recruiter.SubscriptionType;
                                        var oldExpiry = recruiter.SubscriptionExpiryDate;
                                        
                                        recruiter.SubscriptionType = plan.Id;
                                        
                                        if (plan.Id == "Monthly")
                                        {
                                            // Extend if already monthly, else set new
                                            if (recruiter.SubscriptionExpiryDate.HasValue && recruiter.SubscriptionExpiryDate > DateTime.Now)
                                            {
                                                recruiter.SubscriptionExpiryDate = recruiter.SubscriptionExpiryDate.Value.AddDays(30);
                                            }
                                            else
                                            {
                                                recruiter.SubscriptionExpiryDate = DateTime.Now.AddDays(30);
                                            }
                                        }
                                        else if (plan.Id == "Lifetime")
                                        {
                                            recruiter.SubscriptionExpiryDate = null; // No expiry
                                        }

                                        // Reset free count
                                        recruiter.FreeJobPostCount = 0; 

                                        db.SubmitChanges();
                                        
                                        PaymentLogger.Info($"Nâng cấp gói thành công. RecruiterID: {recruiterId}, "
                                            + $"Gói cũ: {oldSubscription} (Hết hạn: {oldExpiry}), "
                                            + $"Gói mới: {recruiter.SubscriptionType} (Hết hạn: {recruiter.SubscriptionExpiryDate})");
                                    }
                                }
                            }
                            else
                            {
                                PaymentLogger.Warning($"Không thể phân tích RecruiterID: {recruiterIdStr}");
                            }
                        }
                    }
                    else
                    {
                        PaymentLogger.Info($"Giao dịch không chứa từ khóa UPGRADE. Nội dung: {content}");
                    }
                }

                PaymentLogger.Info("Xử lý webhook thành công");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                PaymentLogger.Error($"Xử lý webhook thất bại. Mã tham chiếu: {model?.referenceCode}", ex);
                return Json(new { success = false, message = "Lỗi hệ thống" });
            }
        }
    }
}

using System;
using System.Web.Mvc;
using System.Threading.Tasks;
using Project_Recruiment_Huce.Models;
using SePayModel = Project_Recruiment_Huce.Models.Payment.SePayWebhookModel;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Services.VietQRService;
using Project_Recruiment_Huce.Repositories.SubscriptionRepo;
using Project_Recruiment_Huce.Services.SubscriptionService;
using Project_Recruiment_Huce.Services.WebhookService;

namespace Project_Recruiment_Huce.Controllers
{
    [Authorize(Roles = "Recruiter")]
    public class SubscriptionController : BaseController
    {
        private readonly IVietQRService _vietQRService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISePayWebhookService _webhookService;

        // Constructor with dependency injection
        public SubscriptionController(
            IVietQRService vietQRService,
            ISubscriptionService subscriptionService,
            ISePayWebhookService webhookService)
        {
            _vietQRService = vietQRService ?? throw new ArgumentNullException(nameof(vietQRService));
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
            _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        }

        // Parameterless constructor for MVC (uses default implementations)
        public SubscriptionController() : this(
            new VietQRService(),
            new SubscriptionService(),
            new SePayWebhookService())
        {
        }

        // GET: Subscription
        public ActionResult Index()
        {
            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                return RedirectToAction("RecruitersManage", "Recruiters");
            }

            var status = _subscriptionService.GetSubscriptionStatus(recruiterId.Value);
            if (status == null)
            {
                return HttpNotFound();
            }

            var model = new SubscriptionViewModel
            {
                CurrentPlan = status.CurrentPlan,
                ExpiryDate = status.ExpiryDate,
                FreeJobPostCount = status.FreeJobPostCount,
                Plans = _subscriptionService.GetAvailablePlans()
            };

            return View(model);
        }

        // GET: Subscription/Upgrade
        public async Task<ActionResult> Upgrade(string planId)
        {
            var plan = _subscriptionService.GetPlanById(planId);
            if (plan == null)
            {
                TempData["ErrorMessage"] = "Gói dịch vụ không tồn tại.";
                return RedirectToAction("Index");
            }

            var recruiterId = GetCurrentRecruiterId();
            if (recruiterId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Generate payment content (max 25 characters for VietQR)
            // Format: UP{RecruiterID} {PlanID}
            string content = $"UP{recruiterId} {planId}";

            // Ensure content is within VietQR limit (25 chars)
            if (content.Length > 25)
            {
                content = content.Substring(0, 25);
            }

            PaymentLogger.Info($"Generating QR for RecruiterID: {recruiterId}, Plan: {planId}, Amount: {plan.Price}");

            // Generate QR using VietQR service (handles timeout & fallback internally)
            string qrUrl = await _vietQRService.GenerateQRAsync((int)plan.Price, content, timeoutSeconds: 5);

            PaymentLogger.Info($"QR generated successfully for RecruiterID: {recruiterId}");

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

            var status = _subscriptionService.GetSubscriptionStatus(recruiterId.Value);
            if (status == null)
            {
                return Json(new { success = false, message = "Không tìm thấy nhà tuyển dụng" }, JsonRequestBehavior.AllowGet);
            }

            // Check if subscription is active (not Free)
            if (status.IsUpgraded)
            {
                var planName = status.CurrentPlan == "Monthly" ? "Gói Tháng" : "Gói Trọn Đời";
                var expiryText = status.ExpiryDate.HasValue ? status.ExpiryDate.Value.ToString("dd/MM/yyyy") : "Vĩnh viễn";

                return Json(new
                {
                    success = true,
                    upgraded = true,
                    planName = planName,
                    subscriptionType = status.CurrentPlan,
                    expiryDate = expiryText,
                    message = $"Chúc mừng! Bạn đã nâng cấp thành công lên {planName}!"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, upgraded = false }, JsonRequestBehavior.AllowGet);
        }

        // POST: Subscription/SePayWebhook
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SePayWebhook(SePayModel model)
        {
            var ipAddress = Request.UserHostAddress;
            var userAgent = Request.UserAgent;

            // Log incoming webhook request
            PaymentLogger.LogWebhookRequest(ipAddress, userAgent,
                Newtonsoft.Json.JsonConvert.SerializeObject(model));

            if (model == null)
            {
                PaymentLogger.Warning("Webhook received null data");
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            // Extract security headers
            var apiKey = Request.Headers["authorization"] ?? Request.Headers["x-key-api-v3a"];
            var signature = Request.Headers["X-SePay-Signature"];
            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(model);

            DateTime? timestamp = null;
            if (DateTime.TryParse(model.transactionDate, out var parsedDate))
            {
                timestamp = parsedDate;
            }

            // Validate webhook security
            var (isValid, errorMessage) = _webhookService.ValidateWebhookSecurity(
                apiKey, ipAddress, signature, payload, timestamp);

            if (!isValid)
            {
                return Json(new { success = false, message = errorMessage });
            }

            // Process webhook
            var result = _webhookService.ProcessWebhook(model);

            if (result.IsDuplicate)
            {
                return Json(new { success = true, message = result.Message });
            }

            if (result.Success)
            {
                PaymentLogger.Info($"Webhook processed successfully. RecruiterID: {result.RecruiterID}, Plan: {result.PlanId}");
                return Json(new { success = true, message = result.Message });
            }
            else
            {
                PaymentLogger.Warning($"Webhook processing failed: {result.Message}");
                return Json(new { success = false, message = result.Message });
            }
        }
    }
}

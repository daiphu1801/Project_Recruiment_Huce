using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Payment;
using Project_Recruiment_Huce.Repositories.SubscriptionRepo;

namespace Project_Recruiment_Huce.Services.SubscriptionService
{
    /// <summary>
    /// Service xử lý logic nghiệp vụ cho gói đăng ký (subscription)
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repository;

        public SubscriptionService() : this(new SubscriptionRepository())
        {
        }
        private readonly List<SubscriptionPlan> _plans;

        public SubscriptionService(ISubscriptionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            
            // Định nghĩa các gói đăng ký
            _plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan 
                { 
                    Id = "Monthly", 
                    Name = "Gói Tháng", 
                    Price = 25000, 
                    Duration = "1 Tháng", 
                    Description = "Đăng tin không giới hạn trong 30 ngày." 
                },
                new SubscriptionPlan 
                { 
                    Id = "Lifetime", 
                    Name = "Gói Trọn Đời", 
                    Price = 250000, 
                    Duration = "Vĩnh viễn", 
                    Description = "Đăng tin không giới hạn trọn đời." 
                }
            };
        }

        /// <summary>
        /// Lấy danh sách tất cả các gói đăng ký khả dụng
        /// </summary>
        public List<SubscriptionPlan> GetAvailablePlans()
        {
            return _plans;
        }

        /// <summary>
        /// Lấy thông tin gói đăng ký theo ID
        /// </summary>
        public SubscriptionPlan GetPlanById(string planId)
        {
            return _plans.FirstOrDefault(p => p.Id.Equals(planId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Xử lý nâng cấp gói đăng ký
        /// </summary>
        public bool ProcessUpgrade(SubscriptionUpgradeRequest request)
        {
            if (request == null)
            {
                PaymentLogger.Warning("ProcessUpgrade: Yêu cầu nâng cấp rỗng (null)");
                return false;
            }

            var recruiter = _repository.GetRecruiterById(request.RecruiterID);
            if (recruiter == null)
            {
                PaymentLogger.Warning($"ProcessUpgrade: Không tìm thấy nhà tuyển dụng - ID: {request.RecruiterID}");
                return false;
            }

            var plan = GetPlanById(request.PlanId);
            if (plan == null)
            {
                PaymentLogger.Warning($"ProcessUpgrade: Không tìm thấy gói đăng ký - {request.PlanId}");
                return false;
            }

            // Kiểm tra số tiền (cho phép amountIn=0 nếu nội dung hợp lệ)
            if (request.AmountPaid > 0 && request.AmountPaid < plan.Price)
            {
                PaymentLogger.Warning($"ProcessUpgrade: Số tiền không khớp. Yêu cầu: {plan.Price}, Đã trả: {request.AmountPaid}");
                return false;
            }

            var oldSubscription = recruiter.SubscriptionType;
            var oldExpiry = recruiter.SubscriptionExpiryDate;

            // Cập nhật thông tin gói đăng ký
            recruiter.SubscriptionType = plan.Id;

            if (plan.Id == "Monthly")
            {
                // Nếu còn hạn thì gia hạn thêm, nếu hết hạn thì đặt mới
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
                recruiter.SubscriptionExpiryDate = null; // Không hết hạn
            }

            // Đặt lại số lượt đăng tin miễn phí
            recruiter.FreeJobPostCount = 0;

            _repository.UpdateRecruiterSubscription(recruiter);

            PaymentLogger.Info($"Nâng cấp thành công. RecruiterID: {request.RecruiterID}, "
                + $"Cũ: {oldSubscription} (Hết hạn: {oldExpiry}), "
                + $"Mới: {recruiter.SubscriptionType} (Hết hạn: {recruiter.SubscriptionExpiryDate})");

            return true;
        }

        /// <summary>
        /// Lấy trạng thái gói đăng ký của nhà tuyển dụng
        /// </summary>
        public SubscriptionStatusDto GetSubscriptionStatus(int recruiterId)
        {
            return _repository.GetSubscriptionStatus(recruiterId);
        }

        /// <summary>
        /// Kiểm tra nhà tuyển dụng có gói đăng ký đang hoạt động không
        /// </summary>
        public bool HasActiveSubscription(int recruiterId)
        {
            return _repository.HasActiveSubscription(recruiterId);
        }

        /// <summary>
        /// Phân tích nội dung thanh toán để trích xuất RecruiterID và PlanID
        /// Hỗ trợ các định dạng:
        /// - "UPGRADE 123 Monthly"
        /// - "UP123 Monthly"
        /// - "BankAPINotify 109808362433-UP37 Monthly-CHUYEN TIEN-..."
        /// </summary>
        public (int? recruiterId, string planId) ParsePaymentContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return (null, null);

            PaymentLogger.Info($"Đang phân tích nội dung thanh toán: {content}");

            // Thử regex để trích xuất "UP{ID} {Plan}" từ bất kỳ vị trí nào trong chuỗi
            var match = Regex.Match(content, @"UP(\d+)\s+(\w+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int recruiterId))
                {
                    string planId = match.Groups[2].Value;
                    PaymentLogger.Info($"Phân tích thành công qua regex: RecruiterID={recruiterId}, PlanId={planId}");
                    return (recruiterId, planId);
                }
            }

            // Dự phòng: Tách theo khoảng trắng và tìm từ khóa UPGRADE/UP
            var parts = content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int upgradeIndex = Array.FindIndex(parts, p =>
                p.Equals("UPGRADE", StringComparison.OrdinalIgnoreCase) ||
                p.StartsWith("UP", StringComparison.OrdinalIgnoreCase));

            if (upgradeIndex >= 0 && parts.Length > upgradeIndex + 1)
            {
                string firstPart = parts[upgradeIndex];
                string recruiterIdStr = "";
                string planId = "";

                // Kiểm tra định dạng "UP123" (dạng ngắn)
                if (firstPart.StartsWith("UP", StringComparison.OrdinalIgnoreCase) && firstPart.Length > 2)
                {
                    recruiterIdStr = firstPart.Substring(2);
                    planId = parts.Length > upgradeIndex + 1 ? parts[upgradeIndex + 1] : "";
                }
                // Định dạng "UPGRADE 123" (dạng đầy đủ)
                else
                {
                    recruiterIdStr = parts.Length > upgradeIndex + 1 ? parts[upgradeIndex + 1] : "";
                    planId = parts.Length > upgradeIndex + 2 ? parts[upgradeIndex + 2] : "";
                }

                if (int.TryParse(recruiterIdStr, out int recruiterId))
                {
                    PaymentLogger.Info($"Phân tích thành công qua split: RecruiterID={recruiterId}, PlanId={planId}");
                    return (recruiterId, planId);
                }
            }

            PaymentLogger.Warning($"Không thể phân tích nội dung thanh toán: {content}");
            return (null, null);
        }

        /// <summary>
        /// Kiểm tra số tiền thanh toán có khớp với giá gói không
        /// </summary>
        public bool ValidatePaymentAmount(string planId, decimal amountPaid)
        {
            var plan = GetPlanById(planId);
            if (plan == null)
                return false;

            // Cho phép amountIn=0 (hạn chế của SePay API) hoặc số tiền bằng/lớn hơn giá gói
            return amountPaid == 0 || amountPaid >= plan.Price;
        }
    }
}

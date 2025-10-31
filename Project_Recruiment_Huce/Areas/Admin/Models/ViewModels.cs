using System;
using System.Collections.Generic;
using System.Globalization;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    // Helper utilities shared by Admin pages
    public static class AdminUiHelpers
    {
        public static string FormatMoney(decimal? amount)
        {
            if (!amount.HasValue) return "";
            return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:n0}", amount.Value);
        }

        public static string Mask(string number)
        {
            if (string.IsNullOrEmpty(number) || number.Length <= 4) return number ?? string.Empty;
            return new string('*', number.Length - 4) + number.Substring(number.Length - 4);
        }
    }

    // ViewModels
    public class AccountListVm { public int AccountId { get; set; } public string Username { get; set; } public string Email { get; set; } public string Phone { get; set; } public string Role { get; set; } public bool Active { get; set; } public DateTime CreatedAt { get; set; } }
    public class CompanyListVm { public int CompanyId { get; set; } public string CompanyName { get; set; } public string TaxCode { get; set; } public string Industry { get; set; } public string Phone { get; set; } public string CompanyEmail { get; set; } public bool Active { get; set; } public DateTime CreatedAt { get; set; } }
    public class RecruiterListVm { public int RecruiterId { get; set; } public string FullName { get; set; } public string CompanyName { get; set; } public string PositionTitle { get; set; } public string WorkEmail { get; set; } public string Phone { get; set; } public DateTime CreatedAt { get; set; } public bool Active { get; set; } }
    public class CandidateListVm { public int CandidateId { get; set; } public string FullName { get; set; } public string Email { get; set; } public string Phone { get; set; } public DateTime? BirthDate { get; set; } public string Gender { get; set; } public DateTime CreatedAt { get; set; } public bool Active { get; set; } }
    public class WorkExperienceVm { public int ExperienceId { get; set; } public string CandidateName { get; set; } public string CompanyName { get; set; } public string JobTitle { get; set; } public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } }
    public class CertificateListVm { public int CertificateId { get; set; } public string CertificateName { get; set; } public string Issuer { get; set; } public string Industry { get; set; } public string Major { get; set; } }
    public class CandidateCertificateVm { public int CandidateCertificateId { get; set; } public string CandidateName { get; set; } public string CertificateName { get; set; } public DateTime? IssuedDate { get; set; } public DateTime? ExpiredDate { get; set; } public string ScoreText { get; set; } }
    public class JobPostListVm { public int JobId { get; set; } public string JobCode { get; set; } public string Title { get; set; } public string CompanyName { get; set; } public string RecruiterName { get; set; } public decimal? SalaryMin { get; set; } public decimal? SalaryMax { get; set; } public string SalaryUnit { get; set; } public string Employment { get; set; } public DateTime? Deadline { get; set; } public string Status { get; set; } public DateTime PostedAt { get; set; } }
    public class JobPostDetailVm { public int DetailId { get; set; } public int JobId { get; set; } public string Industry { get; set; } public string Major { get; set; } public int YearsExperience { get; set; } public string EducationLevel { get; set; } public string Gender { get; set; } public int? AgeFrom { get; set; } public int? AgeTo { get; set; } }
    public class ApplicationListVm { public int ApplicationId { get; set; } public string CandidateName { get; set; } public string JobTitle { get; set; } public DateTime AppliedAt { get; set; } public string AppStatus { get; set; } }
    public class TransactionListVm { public int TransactionId { get; set; } public string TransactionNo { get; set; } public string AccountEmail { get; set; } public decimal Amount { get; set; } public string Method { get; set; } public string Status { get; set; } public DateTime TransactedAt { get; set; } }
    public class BankCardListVm { public int CardId { get; set; } public string CompanyName { get; set; } public string MaskedNumber { get; set; } public string BankName { get; set; } public bool Active { get; set; } }
    public class PendingPaymentVm { public int PendingId { get; set; } public string CompanyName { get; set; } public decimal AmountDue { get; set; } public DateTime? DueDate { get; set; } public string Status { get; set; } }
    public class PaymentHistoryVm { public int PaymentId { get; set; } public string CompanyName { get; set; } public decimal Amount { get; set; } public string PaymentMethod { get; set; } public DateTime PaymentDate { get; set; } public string Status { get; set; } }
    public class PhotoVm { public int PhotoId { get; set; } public string FileName { get; set; } public string FilePath { get; set; } public int? SizeKB { get; set; } public string MimeType { get; set; } public DateTime UploadedAt { get; set; } }

    // Dashboard
    public class DashboardVm
    {
        public int Accounts { get; set; }
        public int Companies { get; set; }
        public int Recruiters { get; set; }
        public int Candidates { get; set; }
        public int JobPosts { get; set; }
        public int Applications { get; set; }
        public int Transactions { get; set; }
        public List<string> Dates7 { get; set; }
        public List<int> JobPostsWeekly { get; set; }
        public List<int> ApplicationsWeekly { get; set; }
    }

    public class ProfileVm
    {
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}



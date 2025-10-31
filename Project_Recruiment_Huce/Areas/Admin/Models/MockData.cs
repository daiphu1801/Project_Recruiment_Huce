using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    public static class MockData
    {
        private static readonly Lazy<List<AccountListVm>> _accounts = new Lazy<List<AccountListVm>>(() =>
        {
            return new List<AccountListVm>
            {
                new AccountListVm{ AccountId=1, Username="admin", Email="admin@site.local", Phone="0900000001", Role="Admin", Active=true, CreatedAt=DateTime.Today.AddDays(-30)},
                new AccountListVm{ AccountId=2, Username="alpha", Email="alpha@company.vn", Phone="0900000002", Role="Company", Active=true, CreatedAt=DateTime.Today.AddDays(-10)},
                new AccountListVm{ AccountId=3, Username="recruit1", Email="recruit1@company.vn", Phone="0900000003", Role="Recruiter", Active=true, CreatedAt=DateTime.Today.AddDays(-7)},
                new AccountListVm{ AccountId=4, Username="candyn", Email="candy@example.com", Phone="0900000004", Role="Candidate", Active=true, CreatedAt=DateTime.Today.AddDays(-5)},
                new AccountListVm{ AccountId=5, Username="hungnv", Email="hung@example.com", Phone="0900000005", Role="Candidate", Active=false, CreatedAt=DateTime.Today.AddDays(-2)},
            };
        });

        private static readonly Lazy<List<CompanyListVm>> _companies = new Lazy<List<CompanyListVm>>(() => new List<CompanyListVm>
        {
            new CompanyListVm{ CompanyId=1, CompanyName="Alpha Co.", TaxCode="01010101", Industry="IT", Phone="0241111111", CompanyEmail="contact@alpha.vn", Active=true, CreatedAt=DateTime.Today.AddMonths(-3)},
            new CompanyListVm{ CompanyId=2, CompanyName="Beta Ltd", TaxCode="02020202", Industry="Finance", Phone="0242222222", CompanyEmail="hello@beta.vn", Active=true, CreatedAt=DateTime.Today.AddMonths(-2)},
        });

        private static readonly Lazy<List<RecruiterListVm>> _recruiters = new Lazy<List<RecruiterListVm>>(() => new List<RecruiterListVm>
        {
            new RecruiterListVm{ RecruiterId=1, FullName="Nguyen Van A", CompanyName="Alpha Co.", PositionTitle="HR Lead", WorkEmail="a@alpha.vn", Phone="0909123123", Active=true, CreatedAt=DateTime.Today.AddDays(-20)},
            new RecruiterListVm{ RecruiterId=2, FullName="Tran Thi B", CompanyName="Beta Ltd", PositionTitle="HR", WorkEmail="b@beta.vn", Phone="0909345345", Active=true, CreatedAt=DateTime.Today.AddDays(-15)},
        });

        private static readonly Lazy<List<CandidateListVm>> _candidates = new Lazy<List<CandidateListVm>>(() => new List<CandidateListVm>
        {
            new CandidateListVm{ CandidateId=1, FullName="Pham Quang C", Email="c@ex.com", Phone="0909555666", BirthDate=new DateTime(1996,5,12), Gender="Nam", Active=true, CreatedAt=DateTime.Today.AddDays(-12)},
            new CandidateListVm{ CandidateId=2, FullName="Le Thi D", Email="d@ex.com", Phone="0909666777", BirthDate=new DateTime(1998,10,1), Gender="Nữ", Active=true, CreatedAt=DateTime.Today.AddDays(-8)},
        });

        private static readonly Lazy<List<WorkExperienceVm>> _workExperiences = new Lazy<List<WorkExperienceVm>>(() => new List<WorkExperienceVm>
        {
            new WorkExperienceVm{ ExperienceId=1, CandidateName="Pham Quang C", CompanyName="Alpha Co.", JobTitle="Developer", StartDate=new DateTime(2020,1,1), EndDate=new DateTime(2022,1,1)},
            new WorkExperienceVm{ ExperienceId=2, CandidateName="Le Thi D", CompanyName="Beta Ltd", JobTitle="Tester", StartDate=new DateTime(2021,6,1), EndDate=null},
        });

        private static readonly Lazy<List<CertificateListVm>> _certificates = new Lazy<List<CertificateListVm>>(() => new List<CertificateListVm>
        {
            new CertificateListVm{ CertificateId=1, CertificateName="IELTS 7.0", Issuer="IDP", Industry="Language", Major="English"},
            new CertificateListVm{ CertificateId=2, CertificateName="AWS Solutions Architect", Issuer="Amazon", Industry="Cloud", Major="Architecture"},
        });

        private static readonly Lazy<List<CandidateCertificateVm>> _candidateCertificates = new Lazy<List<CandidateCertificateVm>>(() => new List<CandidateCertificateVm>
        {
            new CandidateCertificateVm{ CandidateCertificateId=1, CandidateName="Pham Quang C", CertificateName="IELTS 7.0", IssuedDate=new DateTime(2022,5,1), ExpiredDate=new DateTime(2024,5,1), ScoreText="7.0"},
            new CandidateCertificateVm{ CandidateCertificateId=2, CandidateName="Le Thi D", CertificateName="AWS Solutions Architect", IssuedDate=new DateTime(2023,3,1), ExpiredDate=null, ScoreText="Pass"},
        });

        private static readonly Lazy<List<JobPostListVm>> _jobPosts = new Lazy<List<JobPostListVm>>(() => new List<JobPostListVm>
        {
            new JobPostListVm{ JobId=1, JobCode="JOB-001", Title=".NET Developer", CompanyName="Alpha Co.", RecruiterName="Nguyen Van A", SalaryMin=15000000, SalaryMax=30000000, SalaryUnit="VND", Employment="Full-time", Deadline=DateTime.Today.AddDays(14), Status="Visible", PostedAt=DateTime.Today.AddDays(-3)},
            new JobPostListVm{ JobId=2, JobCode="JOB-002", Title="QA Engineer", CompanyName="Beta Ltd", RecruiterName="Tran Thi B", SalaryMin=10000000, SalaryMax=20000000, SalaryUnit="VND", Employment="Full-time", Deadline=DateTime.Today.AddDays(7), Status="Draft", PostedAt=DateTime.Today.AddDays(-1)},
            new JobPostListVm{ JobId=3, JobCode="JOB-003", Title="Designer", CompanyName="Alpha Co.", RecruiterName="Nguyen Van A", SalaryMin=null, SalaryMax=null, SalaryUnit="VND", Employment="Part-time", Deadline=null, Status="Hidden", PostedAt=DateTime.Today.AddDays(-20)},
        });

        private static readonly Lazy<List<JobPostDetailVm>> _jobPostDetails = new Lazy<List<JobPostDetailVm>>(() => new List<JobPostDetailVm>
        {
            new JobPostDetailVm{ DetailId=1, JobId=1, Industry="IT", Major="Software", YearsExperience=2, EducationLevel="Bachelor", Gender="Không yêu cầu", AgeFrom=22, AgeTo=35},
            new JobPostDetailVm{ DetailId=2, JobId=1, Industry="IT", Major="Backend", YearsExperience=1, EducationLevel="Bachelor", Gender="Nam", AgeFrom=null, AgeTo=null},
            new JobPostDetailVm{ DetailId=3, JobId=2, Industry="IT", Major="QA", YearsExperience=1, EducationLevel="College", Gender="Nữ", AgeFrom=20, AgeTo=30},
        });

        private static readonly Lazy<List<ApplicationListVm>> _applications = new Lazy<List<ApplicationListVm>>(() => new List<ApplicationListVm>
        {
            new ApplicationListVm{ ApplicationId=1, CandidateName="Pham Quang C", JobTitle=".NET Developer", AppliedAt=DateTime.Today.AddDays(-2), AppStatus="Under review"},
            new ApplicationListVm{ ApplicationId=2, CandidateName="Le Thi D", JobTitle="QA Engineer", AppliedAt=DateTime.Today.AddDays(-1), AppStatus="Interview"},
            new ApplicationListVm{ ApplicationId=3, CandidateName="Pham Quang C", JobTitle="Designer", AppliedAt=DateTime.Today.AddDays(-5), AppStatus="Rejected"},
        });

        private static readonly Lazy<List<TransactionListVm>> _transactions = new Lazy<List<TransactionListVm>>(() => new List<TransactionListVm>
        {
            new TransactionListVm{ TransactionId=1, TransactionNo="TRX0001", AccountEmail="billing@alpha.vn", Amount=5000000, Method="Bank", Status="Completed", TransactedAt=DateTime.Today.AddDays(-2)},
            new TransactionListVm{ TransactionId=2, TransactionNo="TRX0002", AccountEmail="finance@beta.vn", Amount=3000000, Method="Card", Status="Processing", TransactedAt=DateTime.Today.AddDays(-1)},
            new TransactionListVm{ TransactionId=3, TransactionNo="TRX0003", AccountEmail="billing@alpha.vn", Amount=1500000, Method="Card", Status="Failed", TransactedAt=DateTime.Today.AddDays(-10)},
        });

        private static readonly Lazy<List<BankCardListVm>> _bankCards = new Lazy<List<BankCardListVm>>(() => new List<BankCardListVm>
        {
            new BankCardListVm{ CardId=1, CompanyName="Alpha Co.", MaskedNumber=AdminUiHelpers.Mask("9704123412341234"), BankName="Vietcombank", Active=true },
            new BankCardListVm{ CardId=2, CompanyName="Beta Ltd", MaskedNumber=AdminUiHelpers.Mask("9704222211118888"), BankName="Techcombank", Active=false },
        });

        private static readonly Lazy<List<PendingPaymentVm>> _pendingPayments = new Lazy<List<PendingPaymentVm>>(() => new List<PendingPaymentVm>
        {
            new PendingPaymentVm{ PendingId=1, CompanyName="Alpha Co.", AmountDue=2000000, DueDate=DateTime.Today.AddDays(5), Status="Waiting"},
            new PendingPaymentVm{ PendingId=2, CompanyName="Beta Ltd", AmountDue=1000000, DueDate=null, Status="Overdue"},
        });

        private static readonly Lazy<List<PaymentHistoryVm>> _paymentHistory = new Lazy<List<PaymentHistoryVm>>(() => new List<PaymentHistoryVm>
        {
            new PaymentHistoryVm{ PaymentId=1, CompanyName="Alpha Co.", Amount=2000000, PaymentMethod="Bank", PaymentDate=DateTime.Today.AddDays(-20), Status="Completed"},
            new PaymentHistoryVm{ PaymentId=2, CompanyName="Beta Ltd", Amount=1000000, PaymentMethod="Card", PaymentDate=DateTime.Today.AddDays(-12), Status="Completed"},
        });

        private static readonly Lazy<List<PhotoVm>> _photos = new Lazy<List<PhotoVm>>(() => new List<PhotoVm>
        {
            new PhotoVm{ PhotoId=1, FileName="avatar1.jpg", FilePath="~/Content/images/person_1.jpg", SizeKB=120, MimeType="image/jpeg", UploadedAt=DateTime.Today.AddDays(-10)},
            new PhotoVm{ PhotoId=2, FileName="avatar2.jpg", FilePath="~/Content/images/person_2.jpg", SizeKB=130, MimeType="image/jpeg", UploadedAt=DateTime.Today.AddDays(-8)},
            new PhotoVm{ PhotoId=3, FileName="logo1.svg", FilePath="~/Content/images/logo_mailchimp.svg", SizeKB=null, MimeType="image/svg+xml", UploadedAt=DateTime.Today.AddDays(-6)},
        });

        // Public accessors
        public static List<AccountListVm> Accounts => _accounts.Value;
        public static List<CompanyListVm> Companies => _companies.Value;
        public static List<RecruiterListVm> Recruiters => _recruiters.Value;
        public static List<CandidateListVm> Candidates => _candidates.Value;
        public static List<WorkExperienceVm> WorkExperiences => _workExperiences.Value;
        public static List<CertificateListVm> Certificates => _certificates.Value;
        public static List<CandidateCertificateVm> CandidateCertificates => _candidateCertificates.Value;
        public static List<JobPostListVm> JobPosts => _jobPosts.Value;
        public static List<JobPostDetailVm> JobPostDetails => _jobPostDetails.Value;
        public static List<ApplicationListVm> Applications => _applications.Value;
        public static List<TransactionListVm> Transactions => _transactions.Value;
        public static List<BankCardListVm> BankCards => _bankCards.Value;
        public static List<PendingPaymentVm> PendingPayments => _pendingPayments.Value;
        public static List<PaymentHistoryVm> PaymentHistory => _paymentHistory.Value;
        public static List<PhotoVm> Photos => _photos.Value;

        // Simple search helpers
        public static StringComparison CI => StringComparison.OrdinalIgnoreCase;
    }
}



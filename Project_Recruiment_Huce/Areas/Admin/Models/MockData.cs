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
                new AccountListVm{ AccountId=1, Username="admin", Email="admin@site.local", Phone="0900000001", Role="Admin", ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-30), PhotoId=null},
                new AccountListVm{ AccountId=2, Username="alpha", Email="alpha@company.vn", Phone="0900000002", Role="Recruiter", ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-10), PhotoId=null},
                new AccountListVm{ AccountId=3, Username="recruit1", Email="recruit1@company.vn", Phone="0900000003", Role="Recruiter", ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-7), PhotoId=null},
                new AccountListVm{ AccountId=4, Username="candyn", Email="candy@example.com", Phone="0900000004", Role="Candidate", ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-5), PhotoId=1},
                new AccountListVm{ AccountId=5, Username="hungnv", Email="hung@example.com", Phone="0900000005", Role="Candidate", ActiveFlag=0, CreatedAt=DateTime.Today.AddDays(-2), PhotoId=null},
            };
        });

        private static readonly Lazy<List<CompanyListVm>> _companies = new Lazy<List<CompanyListVm>>(() => new List<CompanyListVm>
        {
            new CompanyListVm{ CompanyId=1, CompanyName="Alpha Co.", TaxCode="01010101", Industry="Công nghệ thông tin", Address="123 Đường ABC, Quận 1, TP.HCM", Phone="0901234567", CompanyEmail="contact@alpha.vn", Website="https://alpha.vn", Description="Chuyên phát triển phần mềm quản lý doanh nghiệp và giải pháp điện toán đám mây.", CreatedAt=DateTime.Today.AddMonths(-3), ActiveFlag=1},
            new CompanyListVm{ CompanyId=2, CompanyName="Beta Ltd", TaxCode="02020202", Industry="Thương mại điện tử", Address="456 Đường XYZ, Quận 2, TP.HCM", Phone="0912345678", CompanyEmail="info@beta.vn", Website="https://beta.vn", Description="Cung cấp nền tảng thương mại điện tử và giải pháp bán hàng trực tuyến.", CreatedAt=DateTime.Today.AddMonths(-2), ActiveFlag=1},
            new CompanyListVm{ CompanyId=3, CompanyName="Gamma Technology", TaxCode="03030303", Industry="Điện tử - Viễn thông", Address="789 Đường DEF, Quận 3, TP.HCM", Phone="0923456789", CompanyEmail="support@gamma.vn", Website="https://gamma.vn", Description="Nhà cung cấp thiết bị và dịch vụ viễn thông hàng đầu Việt Nam.", CreatedAt=DateTime.Today.AddMonths(-1), ActiveFlag=1},
            new CompanyListVm{ CompanyId=4, CompanyName="Delta Solutions", TaxCode="04040404", Industry="Tư vấn doanh nghiệp", Address="321 Đường GHI, Quận Hoàn Kiếm, Hà Nội", Phone="0934567890", CompanyEmail="info@delta.vn", Website="https://delta.vn", Description="Cung cấp dịch vụ tư vấn chiến lược, tài chính và quản trị doanh nghiệp.", CreatedAt=DateTime.Today.AddDays(-20), ActiveFlag=1},
            new CompanyListVm{ CompanyId=5, CompanyName="Epsilon Corporation", TaxCode="05050505", Industry="Sản xuất và phân phối", Address="654 Đường JKL, Quận 5, TP.HCM", Phone="0945678901", CompanyEmail="contact@epsilon.vn", Website="https://epsilon.vn", Description="Doanh nghiệp chuyên sản xuất thiết bị công nghiệp và phân phối linh kiện cơ khí.", CreatedAt=DateTime.Today.AddDays(-15), ActiveFlag=0},
        });


        private static readonly Lazy<List<RecruiterListVm>> _recruiters = new Lazy<List<RecruiterListVm>>(() => new List<RecruiterListVm>
        {
            new RecruiterListVm{ RecruiterId=1, AccountId=2, CompanyId=1, FullName="Nguyen Van A", CompanyName="Alpha Co.", PositionTitle="HR Lead", CompanyEmail="a@alpha.vn", Phone="0909123123", ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-20)},
            new RecruiterListVm{ RecruiterId=2, AccountId=3, CompanyId=2, FullName="Tran Thi B", CompanyName="Beta Ltd", PositionTitle="HR", CompanyEmail="b@beta.vn", Phone="0909345345", ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-15)},
            new RecruiterListVm{ RecruiterId=3, AccountId=2, CompanyId=3, FullName="Le Van C", CompanyName="Gamma Technology", PositionTitle="Senior HR", CompanyEmail="c@gamma.vn", Phone="0909456789", ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-10)},
            new RecruiterListVm{ RecruiterId=4, AccountId=3, CompanyId=4, FullName="Pham Thi D", CompanyName="Delta Solutions", PositionTitle="HR Manager", CompanyEmail="d@delta.vn", Phone="0909567890", ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-8)},
            new RecruiterListVm{ RecruiterId=5, AccountId=2, CompanyId=1, FullName="Hoang Van E", CompanyName="Alpha Co.", PositionTitle="Recruiter", CompanyEmail="e@alpha.vn", Phone="0909678901", ActiveFlag=0, CreatedAt=DateTime.Today.AddDays(-5)},
        });

        private static readonly Lazy<List<CandidateListVm>> _candidates = new Lazy<List<CandidateListVm>>(() => new List<CandidateListVm>
        {
            new CandidateListVm{ CandidateId=1, AccountId=4, FullName="Pham Quang C", Email="candy@example.com", Phone="0909555666", DateOfBirth=new DateTime(1996,5,12), Gender="Nam", PhotoId=1, ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-12)},
            new CandidateListVm{ CandidateId=2, AccountId=5, FullName="Le Thi D", Email="hung@example.com", Phone="0909666777", DateOfBirth=new DateTime(1998,10,1), Gender="Nữ", PhotoId=2, ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-8)},
            new CandidateListVm{ CandidateId=3, AccountId=4, FullName="Tran Minh F", Email="tranf@example.com", Phone="0909777888", DateOfBirth=new DateTime(1995,3,15), Gender="Nam", PhotoId=null, ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-6)},
            new CandidateListVm{ CandidateId=4, AccountId=5, FullName="Nguyen Thi G", Email="nguyeng@example.com", Phone="0909888999", DateOfBirth=new DateTime(1997,7,20), Gender="Nữ", PhotoId=null, ActiveFlag=1, CreatedAt=DateTime.Today.AddDays(-4)},
            new CandidateListVm{ CandidateId=5, AccountId=4, FullName="Vo Van H", Email="voh@example.com", Phone="0909999000", DateOfBirth=new DateTime(1999,11,25), Gender="Nam", PhotoId=null, ActiveFlag=0, CreatedAt=DateTime.Today.AddDays(-2)},
        });

        private static readonly Lazy<List<WorkExperienceVm>> _workExperiences = new Lazy<List<WorkExperienceVm>>(() => new List<WorkExperienceVm>
        {
            new WorkExperienceVm{ ExperienceId=1, CandidateName="Pham Quang C", CompanyName="Alpha Co.", JobTitle="Developer", StartDate=new DateTime(2020,1,1), EndDate=new DateTime(2022,1,1)},
            new WorkExperienceVm{ ExperienceId=2, CandidateName="Le Thi D", CompanyName="Beta Ltd", JobTitle="Tester", StartDate=new DateTime(2021,6,1), EndDate=null},
            new WorkExperienceVm{ ExperienceId=3, CandidateName="Pham Quang C", CompanyName="Gamma Technology", JobTitle="Senior Developer", StartDate=new DateTime(2022,2,1), EndDate=null},
            new WorkExperienceVm{ ExperienceId=4, CandidateName="Tran Minh F", CompanyName="Delta Solutions", JobTitle="Junior Developer", StartDate=new DateTime(2023,1,1), EndDate=new DateTime(2023,12,31)},
            new WorkExperienceVm{ ExperienceId=5, CandidateName="Nguyen Thi G", CompanyName="Epsilon Corporation", JobTitle="QA Engineer", StartDate=new DateTime(2021,3,1), EndDate=null},
        });

        private static readonly Lazy<List<CertificateListVm>> _certificates = new Lazy<List<CertificateListVm>>(() => new List<CertificateListVm>
        {
            new CertificateListVm{ CertificateId=1, CertificateName="IELTS 7.0", Issuer="IDP", Industry="Language", Major="English"},
            new CertificateListVm{ CertificateId=2, CertificateName="AWS Solutions Architect", Issuer="Amazon", Industry="Cloud", Major="Architecture"},
            new CertificateListVm{ CertificateId=3, CertificateName="TOEIC 850", Issuer="ETS", Industry="Language", Major="English"},
            new CertificateListVm{ CertificateId=4, CertificateName="Microsoft Certified: Azure Developer", Issuer="Microsoft", Industry="Cloud", Major="Azure"},
            new CertificateListVm{ CertificateId=5, CertificateName="Oracle Certified Professional", Issuer="Oracle", Industry="Database", Major="Oracle Database"},
            new CertificateListVm{ CertificateId=6, CertificateName="Google Cloud Professional", Issuer="Google", Industry="Cloud", Major="GCP"},
        });

        private static readonly Lazy<List<CandidateCertificateVm>> _candidateCertificates = new Lazy<List<CandidateCertificateVm>>(() => new List<CandidateCertificateVm>
        {
            new CandidateCertificateVm{ CandidateCertificateId=1, CandidateName="Pham Quang C", CertificateName="IELTS 7.0", IssuedDate=new DateTime(2022,5,1), ExpiredDate=new DateTime(2024,5,1), ScoreText="7.0"},
            new CandidateCertificateVm{ CandidateCertificateId=2, CandidateName="Le Thi D", CertificateName="AWS Solutions Architect", IssuedDate=new DateTime(2023,3,1), ExpiredDate=null, ScoreText="Pass"},
            new CandidateCertificateVm{ CandidateCertificateId=3, CandidateName="Pham Quang C", CertificateName="TOEIC 850", IssuedDate=new DateTime(2021,8,1), ExpiredDate=new DateTime(2023,8,1), ScoreText="850"},
            new CandidateCertificateVm{ CandidateCertificateId=4, CandidateName="Tran Minh F", CertificateName="Microsoft Certified: Azure Developer", IssuedDate=new DateTime(2023,6,1), ExpiredDate=null, ScoreText="Pass"},
            new CandidateCertificateVm{ CandidateCertificateId=5, CandidateName="Nguyen Thi G", CertificateName="Google Cloud Professional", IssuedDate=new DateTime(2022,9,1), ExpiredDate=null, ScoreText="Pass"},
        });

        

        private static readonly Lazy<List<ApplicationListVm>> _applications = new Lazy<List<ApplicationListVm>>(() => new List<ApplicationListVm>
        {
            new ApplicationListVm{ ApplicationId=1, CandidateId=1, JobId=1, CandidateName="Pham Quang C", JobTitle=".NET Developer", AppliedAt=DateTime.Today.AddDays(-2), AppStatus="Under review", CvPath="/uploads/cv/cv1.pdf", CertificatePath=null, Note=null, UpdatedAt=DateTime.Today.AddDays(-2)},
            new ApplicationListVm{ ApplicationId=2, CandidateId=2, JobId=2, CandidateName="Le Thi D", JobTitle="QA Engineer", AppliedAt=DateTime.Today.AddDays(-1), AppStatus="Interview", CvPath="/uploads/cv/cv2.pdf", CertificatePath="/uploads/cert/cert2.pdf", Note="Good candidate", UpdatedAt=DateTime.Today.AddDays(-1)},
            new ApplicationListVm{ ApplicationId=3, CandidateId=1, JobId=3, CandidateName="Pham Quang C", JobTitle="Designer", AppliedAt=DateTime.Today.AddDays(-5), AppStatus="Rejected", CvPath="/uploads/cv/cv1.pdf", CertificatePath=null, Note="Not suitable", UpdatedAt=DateTime.Today.AddDays(-5)},
            new ApplicationListVm{ ApplicationId=4, CandidateId=3, JobId=4, CandidateName="Tran Minh F", JobTitle="Frontend Developer", AppliedAt=DateTime.Today.AddDays(-3), AppStatus="Under review", CvPath="/uploads/cv/cv3.pdf", CertificatePath=null, Note=null, UpdatedAt=DateTime.Today.AddDays(-3)},
            new ApplicationListVm{ ApplicationId=5, CandidateId=4, JobId=5, CandidateName="Nguyen Thi G", JobTitle="Backend Developer", AppliedAt=DateTime.Today.AddDays(-1), AppStatus="Offered", CvPath="/uploads/cv/cv4.pdf", CertificatePath="/uploads/cert/cert4.pdf", Note="Excellent candidate", UpdatedAt=DateTime.Today.AddDays(-1)},
            new ApplicationListVm{ ApplicationId=6, CandidateId=1, JobId=6, CandidateName="Pham Quang C", JobTitle="DevOps Engineer", AppliedAt=DateTime.Today.AddDays(-6), AppStatus="Hired", CvPath="/uploads/cv/cv1.pdf", CertificatePath="/uploads/cert/cert1.pdf", Note="Hired", UpdatedAt=DateTime.Today.AddDays(-6)},
            new ApplicationListVm{ ApplicationId=7, CandidateId=2, JobId=7, CandidateName="Le Thi D", JobTitle="Mobile Developer", AppliedAt=DateTime.Today.AddDays(-2), AppStatus="Interview", CvPath="/uploads/cv/cv2.pdf", CertificatePath=null, Note="Scheduled interview", UpdatedAt=DateTime.Today.AddDays(-2)},
        });

        private static readonly Lazy<List<TransactionListVm>> _transactions = new Lazy<List<TransactionListVm>>(() => new List<TransactionListVm>
        {
            new TransactionListVm{ TransactionId=1, AccountId=2, TransactionNo="TRX0001", AccountEmail="alpha@company.vn", Amount=5000000, PaymentMethod="Bank", Status="Completed", TransactedAt=DateTime.Today.AddDays(-2), Description="Payment for premium package"},
            new TransactionListVm{ TransactionId=2, AccountId=3, TransactionNo="TRX0002", AccountEmail="recruit1@company.vn", Amount=3000000, PaymentMethod="Card", Status="Processing", TransactedAt=DateTime.Today.AddDays(-1), Description="Monthly subscription"},
            new TransactionListVm{ TransactionId=3, AccountId=2, TransactionNo="TRX0003", AccountEmail="alpha@company.vn", Amount=1500000, PaymentMethod="Card", Status="Failed", TransactedAt=DateTime.Today.AddDays(-10), Description="Payment failed - insufficient funds"},
            new TransactionListVm{ TransactionId=4, AccountId=2, TransactionNo="TRX0004", AccountEmail="alpha@company.vn", Amount=2000000, PaymentMethod="Bank", Status="Completed", TransactedAt=DateTime.Today.AddDays(-5), Description="Monthly subscription"},
            new TransactionListVm{ TransactionId=5, AccountId=3, TransactionNo="TRX0005", AccountEmail="recruit1@company.vn", Amount=4000000, PaymentMethod="Card", Status="Completed", TransactedAt=DateTime.Today.AddDays(-3), Description="Premium upgrade"},
            new TransactionListVm{ TransactionId=6, AccountId=2, TransactionNo="TRX0006", AccountEmail="alpha@company.vn", Amount=1000000, PaymentMethod="Bank", Status="Processing", TransactedAt=DateTime.Today.AddDays(-1), Description="Additional service"},
        });

        private static readonly Lazy<List<BankCardListVm>> _bankCards = new Lazy<List<BankCardListVm>>(() => new List<BankCardListVm>
        {
            new BankCardListVm{ CardId=1, CompanyId=1, CompanyName="Alpha Co.", CardNumber="9704123412341234", BankName="Vietcombank", CardHolderName="Alpha Co.", ExpiryDate=new DateTime(2026,12,31), ActiveFlag=1 },
            new BankCardListVm{ CardId=2, CompanyId=2, CompanyName="Beta Ltd", CardNumber="9704222211118888", BankName="Techcombank", CardHolderName="Beta Ltd", ExpiryDate=new DateTime(2025,6,30), ActiveFlag=0 },
            new BankCardListVm{ CardId=3, CompanyId=3, CompanyName="Gamma Technology", CardNumber="9704333322227777", BankName="BIDV", CardHolderName="Gamma Technology", ExpiryDate=new DateTime(2027,3,31), ActiveFlag=1 },
            new BankCardListVm{ CardId=4, CompanyId=4, CompanyName="Delta Solutions", CardNumber="9704444433336666", BankName="Vietcombank", CardHolderName="Delta Solutions", ExpiryDate=new DateTime(2026,9,30), ActiveFlag=1 },
            new BankCardListVm{ CardId=5, CompanyId=1, CompanyName="Alpha Co.", CardNumber="9704555544445555", BankName="ACB", CardHolderName="Alpha Co.", ExpiryDate=new DateTime(2025,12,31), ActiveFlag=0 },
        });

        private static readonly Lazy<List<PendingPaymentVm>> _pendingPayments = new Lazy<List<PendingPaymentVm>>(() => new List<PendingPaymentVm>
        {
            new PendingPaymentVm{ PendingId=1, CompanyId=1, CompanyName="Alpha Co.", AmountDue=2000000, DueDate=DateTime.Today.AddDays(5), Status="Waiting", CreatedAt=DateTime.Today.AddDays(-10)},
            new PendingPaymentVm{ PendingId=2, CompanyId=2, CompanyName="Beta Ltd", AmountDue=1000000, DueDate=null, Status="Overdue", CreatedAt=DateTime.Today.AddDays(-20)},
            new PendingPaymentVm{ PendingId=3, CompanyId=3, CompanyName="Gamma Technology", AmountDue=3000000, DueDate=DateTime.Today.AddDays(10), Status="Waiting", CreatedAt=DateTime.Today.AddDays(-15)},
            new PendingPaymentVm{ PendingId=4, CompanyId=4, CompanyName="Delta Solutions", AmountDue=1500000, DueDate=DateTime.Today.AddDays(-3), Status="Overdue", CreatedAt=DateTime.Today.AddDays(-18)},
            new PendingPaymentVm{ PendingId=5, CompanyId=1, CompanyName="Alpha Co.", AmountDue=2500000, DueDate=DateTime.Today.AddDays(7), Status="Waiting", CreatedAt=DateTime.Today.AddDays(-12)},
        });

        private static readonly Lazy<List<PaymentHistoryVm>> _paymentHistory = new Lazy<List<PaymentHistoryVm>>(() => new List<PaymentHistoryVm>
        {
            new PaymentHistoryVm{ PaymentId=1, CompanyId=1, CompanyName="Alpha Co.", Amount=2000000, PaymentMethod="Bank", PaymentDate=DateTime.Today.AddDays(-20), Status="Completed", Description="Monthly subscription payment"},
            new PaymentHistoryVm{ PaymentId=2, CompanyId=2, CompanyName="Beta Ltd", Amount=1000000, PaymentMethod="Card", PaymentDate=DateTime.Today.AddDays(-12), Status="Completed", Description="Premium package upgrade"},
            new PaymentHistoryVm{ PaymentId=3, CompanyId=3, CompanyName="Gamma Technology", Amount=3000000, PaymentMethod="Bank", PaymentDate=DateTime.Today.AddDays(-25), Status="Completed", Description="Quarterly subscription"},
            new PaymentHistoryVm{ PaymentId=4, CompanyId=4, CompanyName="Delta Solutions", Amount=1500000, PaymentMethod="Card", PaymentDate=DateTime.Today.AddDays(-18), Status="Completed", Description="Monthly subscription payment"},
            new PaymentHistoryVm{ PaymentId=5, CompanyId=1, CompanyName="Alpha Co.", Amount=5000000, PaymentMethod="Bank", PaymentDate=DateTime.Today.AddDays(-30), Status="Completed", Description="Annual subscription"},
            new PaymentHistoryVm{ PaymentId=6, CompanyId=2, CompanyName="Beta Ltd", Amount=2000000, PaymentMethod="Card", PaymentDate=DateTime.Today.AddDays(-35), Status="Completed", Description="Premium upgrade"},
        });

        private static readonly Lazy<List<PhotoVm>> _photos = new Lazy<List<PhotoVm>>(() => new List<PhotoVm>
        {
            new PhotoVm{ PhotoId=1, FileName="avatar1.jpg", FilePath="~/Content/images/person_1.jpg", SizeKB=120, MimeType="image/jpeg", UploadedAt=DateTime.Today.AddDays(-10)},
            new PhotoVm{ PhotoId=2, FileName="avatar2.jpg", FilePath="~/Content/images/person_2.jpg", SizeKB=130, MimeType="image/jpeg", UploadedAt=DateTime.Today.AddDays(-8)},
            new PhotoVm{ PhotoId=3, FileName="logo1.svg", FilePath="~/Content/images/logo_mailchimp.svg", SizeKB=null, MimeType="image/svg+xml", UploadedAt=DateTime.Today.AddDays(-6)},
            new PhotoVm{ PhotoId=4, FileName="avatar3.jpg", FilePath="~/Content/images/person_3.jpg", SizeKB=110, MimeType="image/jpeg", UploadedAt=DateTime.Today.AddDays(-5)},
            new PhotoVm{ PhotoId=5, FileName="logo2.png", FilePath="~/Content/images/logo2.png", SizeKB=45, MimeType="image/png", UploadedAt=DateTime.Today.AddDays(-4)},
            new PhotoVm{ PhotoId=6, FileName="avatar4.jpg", FilePath="~/Content/images/person_4.jpg", SizeKB=125, MimeType="image/jpeg", UploadedAt=DateTime.Today.AddDays(-3)},
            new PhotoVm{ PhotoId=7, FileName="certificate1.pdf", FilePath="~/Content/documents/cert1.pdf", SizeKB=250, MimeType="application/pdf", UploadedAt=DateTime.Today.AddDays(-2)},
        });

        // Public accessors
        public static List<AccountListVm> Accounts => _accounts.Value;
        public static List<CompanyListVm> Companies => _companies.Value;
        public static List<RecruiterListVm> Recruiters => _recruiters.Value;
        public static List<CandidateListVm> Candidates => _candidates.Value;
        public static List<WorkExperienceVm> WorkExperiences => _workExperiences.Value;
        public static List<CertificateListVm> Certificates => _certificates.Value;
        public static List<CandidateCertificateVm> CandidateCertificates => _candidateCertificates.Value;
 
       
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



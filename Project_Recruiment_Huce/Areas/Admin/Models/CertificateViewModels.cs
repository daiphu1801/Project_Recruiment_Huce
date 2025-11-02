namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách chứng chỉ
    /// </summary>
    public class CertificateListVm
    {
        public int CertificateId { get; set; }
        public string CertificateName { get; set; }
        public string Issuer { get; set; }
        public string Industry { get; set; }
        public string Major { get; set; }
    }

    /// <summary>
    /// ViewModel cho chứng chỉ của ứng viên
    /// </summary>
    public class CandidateCertificateVm
    {
        public int CandidateCertificateId { get; set; }
        public string CandidateName { get; set; }
        public string CertificateName { get; set; }
        public System.DateTime? IssuedDate { get; set; }
        public System.DateTime? ExpiredDate { get; set; }
        public string ScoreText { get; set; }
    }
}


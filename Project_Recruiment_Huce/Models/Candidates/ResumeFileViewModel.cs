using System;

namespace Project_Recruiment_Huce.Models.Candidates
{
    /// <summary>
    /// ViewModel cho quản lý CV files
    /// </summary>
    public class ResumeFileViewModel
    {
        public int ResumeFileID { get; set; }
        public int CandidateID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string FileExtension { get; set; }
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// ViewModel cho upload CV
    /// </summary>
    public class ResumeFileUploadViewModel
    {
        public int CandidateID { get; set; }
        public string Title { get; set; }
    }
}


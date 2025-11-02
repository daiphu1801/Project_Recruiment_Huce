using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho ảnh
    /// </summary>
    public class PhotoVm
    {
        public int PhotoId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int? SizeKB { get; set; }
        public string MimeType { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}


using System.Collections.Generic;

namespace Project_Recruiment_Huce.Services
{
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public string Message { get; set; }
        public Dictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}

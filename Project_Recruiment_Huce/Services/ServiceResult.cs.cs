using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_Recruiment_Huce.Services
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
    }
}
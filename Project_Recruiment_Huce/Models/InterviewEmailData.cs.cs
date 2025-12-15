using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_Recruiment_Huce.Models
{
    public class InterviewEmailData
    {
        public int ApplicationID { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public string JobTitle { get; set; }
        public DateTime InterviewDate { get; set; }
        public TimeSpan InterviewTime { get; set; }
        public string Location { get; set; }
        public string InterviewType { get; set; }
        public string Interviewer { get; set; } // Người phỏng vấn
        public string RequiredDocuments { get; set; }
        public string AdditionalNotes { get; set; }
        public int Duration { get; set; }
    }   
}
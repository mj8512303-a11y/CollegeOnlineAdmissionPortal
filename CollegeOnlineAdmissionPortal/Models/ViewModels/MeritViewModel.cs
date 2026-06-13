using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace CollegeOnlineAdmissionPortal.Models.ViewModels
{
    public class MeritViewModel
    {
        public int MeritListID { get; set; }   // ✅ ADD THIS
        public string Course { get; set; }
        public string AcademicYear { get; set; }
        public DateTime? PublishedDate { get; set; }  // <-- Add this

        public string ApplicantName { get; set; }
        public int? TotalMarks { get; set; }
        public double Percentage { get; set; }
        public int Rank { get; set; }
        public string Status { get; set; } // Selected / Waiting
        public double GPA { get; set; }
        public bool IsCurrentApplicant { get; set; }
    }
}

using System.Collections.Generic;

namespace CollegeOnlineAdmissionPortal.Models.ViewModels
{
    public class ApplicantDashboardViewModel
    {
        public string ApplicantName { get; set; }

        public int TotalApplications { get; set; }
        public int TotalPayments { get; set; }

        public string AdmissionStatus { get; set; }

        public List<ApplicationForm> Applications { get; set; }
        public List<Payment> Payments { get; set; }
    }
}

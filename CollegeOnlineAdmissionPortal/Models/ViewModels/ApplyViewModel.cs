using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CollegeOnlineAdmissionPortal.Models.ViewModels
{
    public class ApplyViewModel
    {
        //public ApplicationForm Application { get; set; }
        //public List<Program> Programs { get; set; }
        public ApplicationForm Application { get; set; } = new ApplicationForm();
        public List<Program> Programs { get; set; } = new List<Program>();
    }
}
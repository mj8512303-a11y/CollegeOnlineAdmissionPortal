using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CollegeOnlineAdmissionPortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult HomePage()
        {
            return View();
        }

        public ActionResult Login()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Dashboard()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
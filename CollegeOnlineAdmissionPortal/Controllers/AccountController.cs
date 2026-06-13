using CollegeOnlineAdmissionPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CollegeOnlineAdmissionPortal.Controllers
{
    public class AccountController : Controller
    {
            private CollegeOnlineAdmissionPortalEntities db = new CollegeOnlineAdmissionPortalEntities();

            // GET: /Account/Login
            public ActionResult Login()
            {
                return View();
            }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Login(string Email, string Password, string Role)
        //{
        //    if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Role))
        //    {
        //        TempData["Error"] = "Please fill in all fields.";
        //        return View();
        //    }

        //    // ================= APPLICANT LOGIN =================
        //    if (Role == "Applicant")
        //    {
        //        var applicant = db.Applicants.FirstOrDefault(a => a.Email == Email);

        //        if (applicant == null || applicant.Password != Password)
        //        {
        //            TempData["Error"] = "Invalid applicant login details.";
        //            return View();
        //        }

        //        if (!applicant.IsActive)
        //        {
        //            TempData["Error"] = "Your account has been deactivated. Please contact admin.";
        //            return View();
        //        }

        //        Session["UserID"] = applicant.ApplicantID;
        //        Session["UserName"] = applicant.FullName;
        //        Session["Role"] = "Applicant";

        //        return RedirectToAction("Dashboard", "Applicant");
        //    }

        //    // ================= INCHARGE LOGIN =================
        //    else if (Role == "AdmissionIncharge")
        //    {
        //        var incharge = db.AdmissionIncharges.FirstOrDefault(i => i.Email == Email);

        //        if (incharge == null || incharge.Password != Password)
        //        {
        //            TempData["Error"] = "Invalid incharge login details.";
        //            return View();
        //        }

        //        if (!incharge.IsActive)
        //        {
        //            TempData["Error"] = "Your account has been deactivated by Admin.";
        //            return View();
        //        }

        //        Session["UserID"] = incharge.InchargeID;
        //        Session["UserName"] = incharge.FullName;
        //        Session["Role"] = "AdmissionIncharge";

        //        return RedirectToAction("Dashboard", "Incharge");
        //    }

        //    // ================= ADMIN LOGIN =================
        //    else if (Role == "Admin")
        //    {
        //        var admin = db.Admins.FirstOrDefault(a => a.Email == Email && a.Password == Password);

        //        if (admin == null)
        //        {
        //            TempData["Error"] = "Invalid admin credentials.";
        //            return View();
        //        }

        //        if (!admin.IsActive)
        //        {
        //            TempData["Error"] = "Admin account is inactive.";
        //            return View();
        //        }

        //        Session["UserID"] = admin.AdminID;
        //        Session["UserName"] = admin.FullName;
        //        Session["Role"] = "Admin";

        //        return RedirectToAction("Dashboard", "Admin");
        //    }

        //    TempData["Error"] = "Invalid role selected.";
        //    return View();
        //}

        // ================= AJAX LOGIN =================
        [HttpPost]
        public JsonResult LoginAjax(string email, string password, string role)
        {
            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(role))
            {
                return Json(new { success = false, message = "All fields are required" });
            }

            if (role == "Applicant")
            {
                var user = db.Applicants
                    .FirstOrDefault(x => x.Email == email && x.Password == password);

                if (user != null)
                {
                    Session["UserID"] = user.ApplicantID;
                    Session["Role"] = "Applicant";
                    return Json(new { success = true, redirect = "/Applicant/Dashboard" });
                }
            }

            if (role == "AdmissionIncharge")
            {
                var user = db.AdmissionIncharges
                    .FirstOrDefault(x => x.Email == email && x.Password == password);

                if (user != null)
                {
                    Session["UserID"] = user.InchargeID;
                    Session["Role"] = "AdmissionIncharge";
                    return Json(new { success = true, redirect = "/Incharge/Dashboard" });
                }
            }

            if (role == "Admin")
            {
                var user = db.Admins
                    .FirstOrDefault(x => x.Email == email && x.Password == password);

                if (user != null)
                {
                    Session["UserID"] = user.AdminID;
                    Session["Role"] = "Admin";
                    return Json(new { success = true, redirect = "/Admin/Dashboard" });
                }
            }

            return Json(new { success = false, message = "Invalid credentials" });
        }

        // Logout
        public ActionResult Logout()
            {
                Session.Clear();
                return RedirectToAction("Login", "Account");
            }
        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(ApplicantRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existing = db.Applicants.FirstOrDefault(a => a.Email == model.Email);
                if (existing != null)
                {
                    ViewBag.Error = "Email already registered.";
                    return View(model);
                }
                // Create new applicant
                var applicant = new Applicant
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,  // optional: encrypt later
                    PhoneNumber = model.PhoneNumber,
                    RegistrationDate = DateTime.Now,
                    AdmissionStatus = "Pending"
                };
                db.Applicants.Add(applicant);
                db.SaveChanges();

                TempData["Success"] = "Registration successful! Please login now.";
                return RedirectToAction("Login", "Account");
            }
            return View(model);

        }

    }
}

using CollegeOnlineAdmissionPortal.Models;
using CollegeOnlineAdmissionPortal.Models.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace COAP.Controllers
{
    public class ApplicantController : Controller
    {
        private CollegeOnlineAdmissionPortalEntities db = new CollegeOnlineAdmissionPortalEntities();

        // GET: /Applicant/Dashboard
        public ActionResult Dashboard()
        {
            if (Session["UserID"] == null || Session["Role"] == null || Session["Role"].ToString() != "Applicant")
                return RedirectToAction("Login", "Account");

            int applicantId = Convert.ToInt32(Session["UserID"]);

            var applicant = db.Applicants.FirstOrDefault(a => a.ApplicantID == applicantId);
            if (applicant == null)
                return RedirectToAction("Login", "Account");

            var applications = db.ApplicationForms
                                 .Where(a => a.ApplicantID == applicantId)
                                 .ToList();

            var payments = db.Payments
                             .Where(p => p.ApplicantID == applicantId)
                             .ToList();

            var model = new ApplicantDashboardViewModel
            {
                ApplicantName = applicant.FullName,
                Applications = applications,
                Payments = payments,
                TotalApplications = applications.Count,
                TotalPayments = payments.Count,
                AdmissionStatus = applications.Any()
                                  ? applications.First().Status
                                  : "Not Applied"
            };

            return View(model);
        }
        public ActionResult Apply()
        {
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Applicant")
                return RedirectToAction("Login", "Account");

            var model = new ApplyViewModel
            {
                Application = new ApplicationForm(),
                Programs = db.Programs.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Apply(ApplyViewModel viewModel)
        {
            // Redisplay dropdown in case of error
            viewModel.Programs = db.Programs.ToList();
            var model = viewModel.Application;
            // Basic validation
            if (!ModelState.IsValid)
                return View(viewModel);

            if (model.ProgramID == 0)
            {
                ModelState.AddModelError("Application.ProgramID", "Please select a program");
                return View(viewModel);
            }
            if (string.IsNullOrEmpty(model.AcademicYear))
            {
                ModelState.AddModelError("Application.AcademicYear", "Academic Year is required");
                return View(viewModel);
            }

            if (!model.TotalMarks.HasValue)
            {
                ModelState.AddModelError("Application.TotalMarks", "Total Marks is required");
                return View(viewModel);
            }
            // ✅ Set required fields
            model.ApplicantID = Convert.ToInt32(Session["UserID"]);
            model.SubmissionDate = DateTime.Now;
            model.Status = "Pending";

            // Map ProgramID to ProgramAppliedFor (required string)
            model.ProgramAppliedFor = db.Programs
                .Where(p => p.ProgramID == model.ProgramID)
                .Select(p => p.ProgramName)
                .FirstOrDefault();

            // Calculate Percentage & GPA
            double maxMarks = 1100.0;
            model.Percentage = (model.TotalMarks.Value * 100.0) / maxMarks;
            double per = model.Percentage;

            if (per >= 85) model.GPA = 4.0;
            else if (per >= 75) model.GPA = 3.5;
            else if (per >= 65) model.GPA = 3.0;
            else if (per >= 55) model.GPA = 2.5;
            else model.GPA = 2.0;

            // ✅ Safe save with exception handling
            try
            {
                db.ApplicationForms.Add(model);
                db.SaveChanges();

                TempData["Success"] = "Application submitted successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        ModelState.AddModelError(ve.PropertyName, ve.ErrorMessage);
                    }
                }
                return View(viewModel);
            }
        }

        [HttpPost]

        public ActionResult EditApplication(ApplicationForm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Programs = db.Programs.ToList();
                return View(model);
            }

            var app = db.ApplicationForms.Find(model.FormID);

            app.ProgramAppliedFor = model.ProgramAppliedFor;
            app.AcademicYear = model.AcademicYear;

            db.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        public ActionResult DeleteApplication(int id)
        {
            var app = db.ApplicationForms.Find(id);

            if (app != null)
            {
                db.ApplicationForms.Remove(app);
                db.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }


        // Example: Logout shortcut for applicant
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        
        public ActionResult ViewMeritLists()
        {
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Applicant")
                return RedirectToAction("Login", "Account");

            int applicantId = Convert.ToInt32(Session["UserID"]);

            // Latest application of current applicant
            var myApplication = db.ApplicationForms
                .Where(a => a.ApplicantID == applicantId)
                .OrderByDescending(a => a.SubmissionDate)
                .FirstOrDefault();

            if (myApplication == null)
            {
                ViewBag.Message = "You have not submitted any application yet.";
                return View(new List<MeritViewModel>());
            }

            // Full merit list of the same program
            var meritList = db.ApplicationForms
                .Where(a => a.ProgramID == myApplication.ProgramID)
                .OrderByDescending(a => a.TotalMarks)
                .ToList();

            if (!meritList.Any())
            {
                ViewBag.Message = "Merit list not generated yet.";
                return View(new List<MeritViewModel>());
            }

            var result = new List<MeritViewModel>();
            int rank = 1;
            int currentApplicantId = applicantId;

            foreach (var a in meritList)
            {
                double percentage = (a.TotalMarks ?? 0) * 100.0 / 1100;

                result.Add(new MeritViewModel
                {
                    MeritListID = a.ApplicantID,
                    ApplicantName = a.Applicant.FullName,
                    TotalMarks = a.TotalMarks,
                    Percentage = percentage,
                    Rank = rank,
                    Status = rank <= 3 ? "Selected" : "Waiting",
                    Course = a.Program.ProgramName,
                    AcademicYear = a.AcademicYear,
                    PublishedDate = DateTime.Now,
                    IsCurrentApplicant = a.ApplicantID == currentApplicantId // for row highlight
                });

                rank++;
            }

            return View(result);
        }
        public ActionResult MakePayment()
        {
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Applicant")
                return RedirectToAction("Login", "Account");

            int applicantId = Convert.ToInt32(Session["UserID"]);

            var model = new Payment
            {
                ApplicantID = applicantId,
                PaymentDate = DateTime.Now,
                PaymentStatus = "Pending" // default status
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MakePayment(Payment model)
        {
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Applicant")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            int applicantId = Convert.ToInt32(Session["UserID"]);
            model.ApplicantID = applicantId;
            model.PaymentDate = DateTime.Now;

            // Optional: default status if not set
            if (string.IsNullOrEmpty(model.PaymentStatus))
                model.PaymentStatus = "Pending";

            db.Payments.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Payment submitted successfully!";
            return RedirectToAction("Dashboard");
        }
    }
    
    }

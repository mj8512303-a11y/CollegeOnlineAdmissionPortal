
using CollegeOnlineAdmissionPortal.Models;
using CollegeOnlineAdmissionPortal.Models.ViewModels;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;


namespace COAP.Controllers
{
    public class InchargeController : Controller
    {
        private CollegeOnlineAdmissionPortalEntities db = new CollegeOnlineAdmissionPortalEntities();

        // ===================== AUTH CHECK =====================
        private bool IsIncharge()
        {
            return Session["Role"] != null && Session["Role"].ToString() == "AdmissionIncharge";
        }

        // ===================== DASHBOARD =====================
        public ActionResult Dashboard()
        {
            if (!IsIncharge())
                return RedirectToAction("Login", "Account");

            ViewBag.TotalApplications = db.ApplicationForms.Count();
            ViewBag.Approved = db.ApplicationForms.Count(a => a.Status == "Approved");
            ViewBag.Rejected = db.ApplicationForms.Count(a => a.Status == "Rejected");

            return View();
        }

        // ===================== LOGOUT =====================
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // ================= VIEW APPLICATIONS =================
        [HttpGet]
        public ActionResult ViewApplications()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "AdmissionIncharge")
                return RedirectToAction("Login", "Account");

            var applications = db.ApplicationForms.ToList();
            return View(applications);
        }

        // ================= APPROVE =================
        [HttpPost]
        public JsonResult ApproveApplicationAjax(int id)
        {
            var app = db.ApplicationForms.FirstOrDefault(x => x.FormID == id);
            if (app == null)
                return Json(new { success = false });

            app.Status = "Approved";
            db.SaveChanges();

            return Json(new { success = true, status = "Approved" });
        }
        

        // ================= REJECT =================
        [HttpPost]
        public JsonResult RejectApplicationAjax(int id)
        {
            var app = db.ApplicationForms.FirstOrDefault(x => x.FormID == id);
            if (app == null)
                return Json(new { success = false });

            app.Status = "Rejected";
            db.SaveChanges();

            return Json(new { success = true, status = "Rejected" });
        }


       
        public ActionResult ViewMeritLists()
        {
            if (!IsIncharge())
                return RedirectToAction("Login", "Account");

            double totalMarksPossible = 1100;

            var meritLists = db.MeritLists
                               .Include("ApplicationForms.Applicant")
                               .OrderByDescending(m => m.PublishedDate)
                               .ToList();

            var finalList = new List<MeritViewModel>();

            foreach (var meritList in meritLists)
            {
                var apps = meritList.ApplicationForms
                                     .OrderBy(a => a.MeritRank)
                                     .ToList();

                foreach (var a in apps)
                {
                    double percentage = totalMarksPossible > 0
                        ? ((a.TotalMarks ?? 0) / totalMarksPossible) * 100
                        : 0;

                    double gpa = CalculateGPA(percentage);

                    finalList.Add(new MeritViewModel
                    {
                        MeritListID = meritList.MeritListID,  // ✅ FIXED

                        Course = meritList.Course,
                        AcademicYear = meritList.AcademicYear,
                        PublishedDate = meritList.PublishedDate,

                        ApplicantName = a.Applicant.FullName,
                        TotalMarks = a.TotalMarks ?? 0,
                        Percentage = percentage,
                        GPA = gpa,

                        Rank = a.MeritRank ?? 0,
                        Status = a.Status
                    });
                }
            }

            return View(finalList);
        }
        // ================= CREATE MERIT LIST (GET) =================
        [HttpGet]
        public ActionResult CreateMeritList()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "AdmissionIncharge")
                return RedirectToAction("Login", "Account");

            // 🔥 THIS LINE WAS MISSING
            ViewBag.Programs = db.Programs.ToList();

            return View();
        }
        [HttpPost]
        public JsonResult CreateMeritListAjax(int programId, string academicYear)
        {
            if (!IsIncharge())
                return Json(new { success = false, message = "Unauthorized" });
            int seatLimit = 5; // 🔥 You can fetch from DB later
            var approvedForms = db.ApplicationForms
                .Where(f => f.ProgramID == programId && f.Status == "Approved")
                .ToList();
            if (!approvedForms.Any())
                return Json(new { success = false, message = "No approved applications found" });
            double totalMarksPossible = 1100;
            var rankedStudents = approvedForms
                .Select(a =>
                {
                    double percentage = CalculatePercentage(a.TotalMarks ?? 0, totalMarksPossible);
                    double gpa = CalculateGPA(percentage);
                    return new
                    {
                        Form = a,
                        Percentage = percentage,
                        GPA = gpa
                    };
                })
                .OrderByDescending(x => x.GPA)
                .ThenByDescending(x => x.Form.TotalMarks)
                .ToList();

            var meritList = new MeritList
            {
                ProgramID = programId,
                AcademicYear = academicYear,
                PublishedDate = DateTime.Now
            };

            db.MeritLists.Add(meritList);
            db.SaveChanges();

            int rank = 1;

            foreach (var student in rankedStudents)
            {
                student.Form.MeritRank = rank;
                student.Form.MeritListID = meritList.MeritListID;

                student.Form.Status = rank <= seatLimit ? "Selected" : "Waiting";

                rank++;
            }

            db.SaveChanges();

            string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}";
            string pdfUrl = $"{baseUrl}/Incharge/DownloadMeritList/{meritList.MeritListID}";

            return Json(new
            {
                success = true,
                message = "Merit List Created Successfully & PDF Generated ✅",
                pdfUrl = pdfUrl
            });
        }

        // ===================== PRINT MERIT LIST =====================
        public ActionResult PrintMeritList(int id)
        {
            if (!IsIncharge())
                return RedirectToAction("Login", "Account");

            var meritList = db.MeritLists
                              .Include("ApplicationForms.Applicant")
                              .FirstOrDefault(m => m.MeritListID == id);

            if (meritList == null)
                return HttpNotFound();

            double totalMarksPossible = 1100;

            var viewModel = meritList.ApplicationForms
                .OrderBy(a => a.MeritRank)
                .Select(a =>
                {
                    double percentage = CalculatePercentage(a.TotalMarks ?? 0, totalMarksPossible);
                    double gpa = CalculateGPA(percentage);

                    return new MeritViewModel
                    {
                        Course = meritList.Course,
                        AcademicYear = meritList.AcademicYear,
                        PublishedDate = meritList.PublishedDate,
                        ApplicantName = a.Applicant.FullName,
                        TotalMarks = a.TotalMarks ?? 0,
                        Percentage = percentage,
                        GPA = gpa,
                        Rank = a.MeritRank ?? 0,
                        Status = a.Status
                    };
                }).ToList();

            return View(viewModel);
        }
        private double CalculatePercentage(int marks, double totalMarks)
        {
            if (totalMarks == 0) return 0;
            return (marks / totalMarks) * 100;
        }

        private double CalculateGPA(double percentage)
        {
            if (percentage >= 90) return 4.0;
            if (percentage >= 80) return 3.5;
            if (percentage >= 70) return 3.0;
            if (percentage >= 60) return 2.5;
            if (percentage >= 50) return 2.0;
            return 1.0;
        }

        // ===================== DOWNLOAD PDF =====================
        public ActionResult DownloadMeritList(int id)
        {
            if (!IsIncharge())
                return RedirectToAction("Login", "Account");

            var meritList = db.MeritLists.FirstOrDefault(m => m.MeritListID == id);
            if (meritList == null)
            {
                TempData["Error"] = "Merit list not found.";
                return RedirectToAction("ViewMeritLists");
            }

            string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}";
            string viewUrl = $"{baseUrl}/Incharge/PrintMeritList/{id}";

            HtmlToPdf converter = new HtmlToPdf();
            var doc = converter.ConvertUrl(viewUrl);
            byte[] pdf = doc.Save();
            doc.Close();

            return File(pdf, "application/pdf", $"{meritList.Course}_MeritList.pdf");
        }

        // ===================== DELETE MERIT LIST =====================
        public ActionResult DeleteMeritList(int id)
        {
            if (!IsIncharge())
                return RedirectToAction("Login", "Account");

            var list = db.MeritLists.FirstOrDefault(m => m.MeritListID == id);
            if (list != null)
            {
                db.MeritLists.Remove(list);
                db.SaveChanges();
                TempData["Success"] = "Merit list deleted.";
            }

            return RedirectToAction("ViewMeritLists");
        }
    }
}


using CollegeOnlineAdmissionPortal.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace COAP.Controllers
{
    public class AdminController : Controller
    {
        private CollegeOnlineAdmissionPortalEntities db = new CollegeOnlineAdmissionPortalEntities();

        // GET: Admin/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Admin/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(AdminRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existing = db.Admins.FirstOrDefault(a => a.Email == model.Email);
                if (existing != null)
                {
                    ViewBag.Error = "This email is already registered.";
                    return View(model);
                }

                // Create new admin record
                var admin = new Admin
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    PhoneNumber = model.PhoneNumber,
                    CreatedDate = DateTime.Now
                };

                db.Admins.Add(admin);
                db.SaveChanges();

                TempData["Success"] = "Admin registered successfully! Please login.";
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }
            // ===================== Dashboard =====================
            [HttpGet]
            public ActionResult Dashboard()
            {
                if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                    return RedirectToAction("Login", "Account");

                ViewBag.TotalApplicants = db.Applicants.Count();
                ViewBag.TotalApproved = db.ApplicationForms.Count(a => a.Status == "Approved");
                ViewBag.TotalIncharges = db.AdmissionIncharges.Count();
                ViewBag.TotalDepartments = db.Departments.Count();
                ViewBag.TotalPrograms = db.Programs.Count();
                ViewBag.TotalMeritLists = db.MeritLists.Count();

                return View();
            }

            // ===================== Manage Departments =====================
            [HttpGet]
            public ActionResult ManageDepartments()
            {
                if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                    return RedirectToAction("Login", "Account");

                var departments = db.Departments.ToList();
                return View(departments);
            }


        //[HttpPost]
        //public JsonResult AddDepartment(string DepartmentName, string DepartmentCode)
        //{
        //    if (string.IsNullOrWhiteSpace(DepartmentName) ||
        //        string.IsNullOrWhiteSpace(DepartmentCode))
        //    {
        //        return Json(new { success = false, message = "All fields are required" });
        //    }

        //    if (db.Departments.Any(d => d.DepartmentName == DepartmentName))
        //    {
        //        return Json(new { success = false, message = "Department already exists" });
        //    }

        //    var dept = new Department
        //    {
        //        DepartmentName = DepartmentName.Trim(),
        //        DepartmentCode = DepartmentCode.Trim()
        //    };

        //    db.Departments.Add(dept);
        //    db.SaveChanges();

        //    return Json(new { success = true, message = "Department added successfully" });
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDepartment(string DepartmentName, string DepartmentCode)
        {
            if (string.IsNullOrWhiteSpace(DepartmentName) ||
                string.IsNullOrWhiteSpace(DepartmentCode))
            {
                TempData["Error"] = "All fields are required";
                return RedirectToAction("ManageDepartments");
            }

            var dept = new Department
            {
                DepartmentName = DepartmentName.Trim(),
                DepartmentCode = DepartmentCode.Trim()
            };

            db.Departments.Add(dept);
            db.SaveChanges();

            TempData["Success"] = "Department added successfully!";
            return RedirectToAction("ManageDepartments");
        }
        public ActionResult DeleteDepartment(int id)
            {
                var dept = db.Departments.Find(id);
                if (dept != null)
                {
                    db.Departments.Remove(dept);
                    db.SaveChanges();
                    TempData["Success"] = "Department deleted successfully!";
                }
                return RedirectToAction("ManageDepartments");
            }
        public ActionResult EditDepartment(int id)
        {
            var dept = db.Departments.Find(id);
            if (dept == null)
                return HttpNotFound();

            return View(dept);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDepartment(Department model)
        {
            if (ModelState.IsValid)
            {
                var dept = db.Departments.Find(model.DepartmentID);

                dept.DepartmentName = model.DepartmentName;
                dept.DepartmentCode = model.DepartmentCode;

                db.SaveChanges();

                TempData["SuccessMessage"] = "Department updated successfully!";
                return RedirectToAction("ManageDepartments");
            }

            return View(model);
        }

        // ===================== Manage Programs =====================
        [HttpGet]
        //public ActionResult ManagePrograms()
        //{
        //    if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
        //        return RedirectToAction("Login", "Account");

        //    ViewBag.Departments = db.Departments.ToList();
        //    var programs = db.Programs.ToList();
        //    return View(programs);
        //}
        public ActionResult ManagePrograms()
        {
            ViewBag.Departments = db.Departments.ToList();
            return View(db.Programs.Include("Department").ToList());
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AddProgram(string ProgramName, int DepartmentID)
        //{
        //    if (string.IsNullOrEmpty(ProgramName))
        //    {
        //        TempData["Error"] = "Program name cannot be empty.";
        //        return RedirectToAction("ManagePrograms");
        //    }

        //    var program = new Program
        //    {
        //        ProgramName = ProgramName,
        //        DepartmentID = DepartmentID
        //    };
        //    db.Programs.Add(program);
        //    db.SaveChanges();

        //    TempData["Success"] = "Program added successfully!";
        //    return RedirectToAction("ManagePrograms");
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProgram(string ProgramName, string ProgramLevel,
                               string Duration, int DepartmentID)
        {
            if (string.IsNullOrWhiteSpace(ProgramName) ||
                string.IsNullOrWhiteSpace(ProgramLevel) ||
                string.IsNullOrWhiteSpace(Duration) ||
                DepartmentID <= 0)
            {
                TempData["Error"] = "All fields are required!";
                return RedirectToAction("ManagePrograms");
            }

            var program = new Program
            {
                ProgramName = ProgramName.Trim(),
                ProgramLevel = ProgramLevel.Trim(),
                Duration = Duration.Trim(),
                DepartmentID = DepartmentID,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            db.Programs.Add(program);
            db.SaveChanges();

            TempData["Success"] = "Program added successfully!";
            return RedirectToAction("ManagePrograms");
        }
        public ActionResult EditProgram(int? id)
        {
            if (id == null)
                return RedirectToAction("ManagePrograms");

            var program = db.Programs.Find(id);

            if (program == null)
                return HttpNotFound();

            ViewBag.Departments = db.Departments.ToList();
            return View(program);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProgram(Program model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = db.Departments.ToList();
                return View(model);
            }

            var program = db.Programs.Find(model.ProgramID);

            if (program == null)
                return HttpNotFound();

            program.ProgramName = model.ProgramName;
            program.ProgramLevel = model.ProgramLevel;
            program.Duration = model.Duration;
            program.DepartmentID = model.DepartmentID;
            program.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["Success"] = "Program updated successfully!";
            return RedirectToAction("ManagePrograms");
        }
        public ActionResult DeleteProgram(int id)
            {
                var program = db.Programs.Find(id);
                if (program != null)
                {
                    db.Programs.Remove(program);
                    db.SaveChanges();
                    TempData["Success"] = "Program deleted successfully!";
                }
                return RedirectToAction("ManagePrograms");
            }
        // ===================== Manage Users (Applicants + Incharges) =====================
        [HttpGet]
        public ActionResult ManageUsers()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                return RedirectToAction("Login", "Account");

            var applicants = db.Applicants.ToList();
            var incharges = db.AdmissionIncharges.ToList();

            ViewBag.Incharges = incharges;
            return View(applicants);
        }

        // -------------------- Activate / Deactivate Applicant --------------------
        public ActionResult ToggleApplicantStatus(int id)
        {
            var applicant = db.Applicants.Find(id);
            if (applicant != null)
            {
                applicant.IsActive = !applicant.IsActive; // toggle
                db.SaveChanges();
                TempData["Success"] = $"Applicant {(applicant.IsActive ? "activated" : "deactivated")} successfully.";
            }
            return RedirectToAction("ManageUsers");
        }

        // -------------------- Delete Applicant --------------------
        public ActionResult DeleteApplicant(int id)
        {
            var applicant = db.Applicants.Find(id);
            if (applicant != null)
            {
                db.Applicants.Remove(applicant);
                db.SaveChanges();
                TempData["Success"] = "Applicant deleted successfully!";
            }
            return RedirectToAction("ManageUsers");
        }

        // -------------------- Activate / Deactivate Incharge --------------------
        public ActionResult ToggleInchargeStatus(int id)
        {
            var incharge = db.AdmissionIncharges.Find(id);
            if (incharge != null)
            {
                incharge.IsActive = !incharge.IsActive;
                db.SaveChanges();
                TempData["Success"] = $"Incharge {(incharge.IsActive ? "activated" : "deactivated")} successfully.";
            }
            return RedirectToAction("ManageUsers");
        }

        // -------------------- Delete Incharge --------------------
        public ActionResult DeleteIncharge(int id)
        {
            var incharge = db.AdmissionIncharges.Find(id);
            if (incharge != null)
            {
                db.AdmissionIncharges.Remove(incharge);
                db.SaveChanges();
                TempData["Success"] = "Incharge deleted successfully!";
            }
            return RedirectToAction("ManageUsers");
        }

        // Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    
    }

    }


  
    

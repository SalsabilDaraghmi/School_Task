using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using School_Task.Models;
using School_Task.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace School_Task.Controllers
{
    public class SchoolController : Controller
    { SchoolContext schoolcontext = new SchoolContext();
        ApplicationDbContext Appcontext=new ApplicationDbContext();
        // GET: School
        [Authorize]
        public ActionResult Index()
        {
            {
                if (User.Identity.IsAuthenticated)
                {
                    var user = User.Identity;
                    ViewBag.Name = user.Name;


                    if ((GetUserRole()=="Admin"))
                    {
                        return RedirectToAction("AdminIndex", "School", new { area = "" });
                    }
                    else if(GetUserRole()=="Student")
                    {
                        return RedirectToAction("StudentIndex", "School", new { area = "" });
                    }
                    else
                        return RedirectToAction("TeacherIndex", "School", new { area = "" });
                }
                else
                {
                    ViewBag.Name = "Not Logged IN";
                }
                return View();

            }
        }

/*==================================== Admin pages ==============================================================*/

        [Authorize(Roles ="Admin")]
        public ActionResult AdminIndex()
        {
            return View();
        }

/*=================================== Teacher pages =======================================================*/

        [Authorize(Roles = "Teacher")]
        public ActionResult TeacherIndex()
        {

            List<Courses> courses = new List<Courses>();
            var user = User.Identity;
            var id = user.GetUserId();
            var coursesIds = (from usercourse in schoolcontext.UserCourses
                              where usercourse.UserId == id
                              select usercourse.CourseID).ToList();

            foreach (var courseid in coursesIds)
            {
                courses.Add(schoolcontext.Courses.Find(courseid));
            }
            return View(courses);
        }


        [Authorize(Roles = "Teacher")]
        public ActionResult TeacherCourseDetails(int id)
        {
            var Students = new List<Users>();
            var StudentsIds = getUsersInCourse(id);
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Appcontext));
               
            {

                foreach (string studentId in StudentsIds)
                {
                    var teacher = Appcontext.Users.Find(studentId);
                    var s = UserManager.GetRoles(studentId);
                    if (s[0].ToString() == "Student")
                    {
                        var student = Appcontext.Users.Find(studentId);
                        Students.Add(new Users()
                            {
                                Username = student.UserName,
                                Firstname= student.UserName.Split(' ')[0],
                                Lastname= student.UserName.Split(' ')[1],
                                age = student.age,
                                Password=student.PasswordHash,
                                Email=student.Email,
                                Id = student.Id,
                                Role="Student"


                        }
                            ) ;
                    }
                }
            }
            var course = schoolcontext.Courses.Find(id);
            ViewBag.course=course;
            return View(Students);

        }

        [Authorize(Roles = "Teacher")]
        public ActionResult AddNewStudent(int id) {
            var users = UsersWithRoles().ToList().FindAll(user => user.Role == "Student");
            ViewBag.userse = users;
            var course = (new List<Courses>());
            course.Add(schoolcontext.Courses.Find(id));
            ViewBag.courses = course;
            return View();
        }
       
        [Authorize(Roles = "Teacher")]
        [HttpPost, ActionName("AddNewStudent")]
        public ActionResult AddStudentToCourse(UserCourse usercourse)
        {

            if (ModelState.IsValid)
            {
                schoolcontext.UserCourses.Add(usercourse);
                schoolcontext.SaveChanges();
                return RedirectToAction("TeacherCourseDetails","School",new {id = usercourse.CourseID});
            }

            return RedirectToAction("AddStudentToCourse");


        }
       
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteStudent(String id )
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var usercourses = (from usercourse in schoolcontext.UserCourses
                               where usercourse.UserId == id
                               select usercourse).ToList();

            if (usercourses == null)
            {
                return HttpNotFound();
            }
            ViewBag.course = schoolcontext.Courses.Find(usercourses[0].CourseID);

            ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            ApplicationUser user = UserManager.FindById(id);
            if (user == null) {return HttpNotFound(); }
            Users user1 = new Users()
            {
                Id = user.Id,
                age = user.age,
                Email = user.Email,
                Role = "Student",
                Firstname = user.UserName.Split(' ')[0],
                Lastname = user.UserName.Split(' ')[1],
                Username = user.UserName

            };
            return View(user1);



        }

        [Authorize(Roles = "Teacher")]
        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]

        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteConfirmed(String id)
        {
            UserCourse userCourse = (from usercourse in schoolcontext.UserCourses
                                     where usercourse.UserId == id
                                     select usercourse).ToList()[0];

            if (userCourse == null) RedirectToAction("Delete", new { id = userCourse.UserId });
            schoolcontext.UserCourses.Remove(userCourse);
            schoolcontext.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Teacher")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                schoolcontext.Dispose();
            }
            base.Dispose(disposing);
        }

        /*================================== Student pages ==========================================================*/

        [Authorize(Roles = "Student")]
        public ActionResult StudentIndex()
        {
            List<Courses> courses= new List<Courses>();
            var user = User.Identity;
            var id= user.GetUserId();
            var coursesIds = (from usercourse in schoolcontext.UserCourses
                        where usercourse.UserId == id
                        select usercourse.CourseID).ToList();

            foreach(var courseid in coursesIds)
            {
                courses.Add(schoolcontext.Courses.Find(courseid));
            }
            return View(courses);
        }
       
        [Authorize(Roles = "Student")]
        public ActionResult StudentCourseDetails(int id)

        {
            var teachersNames = new List<String>();
            var teachersids = getUsersInCourse(id);
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Appcontext));

            if (teachersids == null)
                ViewBag.teacher = "NotExist";
            else
            {

                foreach (string teacherid in teachersids)
                {
                    var teacher = Appcontext.Users.Find(teacherid);
                    var s = UserManager.GetRoles(teacherid);
                    if (s[0].ToString()=="Teacher")
                    teachersNames.Add(Appcontext.Users.Find(teacherid).UserName); 
                }
            } 
            ViewBag.teacher = teachersNames;
            return View(schoolcontext.Courses.Find(id));
        }

/*================================== Helping functions =============================================*/
        private List<String> getUsersInCourse(int id)
        {
            var users = (from uesrcourse in schoolcontext.UserCourses
                        where uesrcourse.CourseID == id
                        select uesrcourse.UserId).ToList();
            if (users.Count > 0)
                return users;
            else 
                return null;
        }

        public String GetUserRole()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext context = new ApplicationDbContext();
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var s = UserManager.GetRoles(user.GetUserId());
                foreach(var ss in s)
                if (ss.ToString() == "Admin")
                {
                    return "Admin";
                }
                else if (s[0].ToString() == "Student")
                {
                    return "Student";
                }
                else return "Teacher";
            }
            return "No";
        }


        public IEnumerable<Users> UsersWithRoles()
        {
            var usersWithRoles = (from user in Appcontext.Users
                                  select new
                                  {
                                      UserId = user.Id,
                                      Username = user.UserName,
                                      Email = user.Email,
                                      RoleNames = (from userRole in user.Roles
                                                   join role in Appcontext.Roles on userRole.RoleId
                                                   equals role.Id
                                                   select role.Name).ToList()
                                  }).ToList().Select(p => new Users

                                  {
                                      Id = p.UserId,
                                      Firstname = p.Username.Split(' ')[0],
                                      Lastname = p.Username.Split(' ')[1],
                                      Username = p.Username,
                                      Email = p.Email,
                                      Role = string.Join(",", p.RoleNames)
                                  });
            return usersWithRoles;

        }


    }
}
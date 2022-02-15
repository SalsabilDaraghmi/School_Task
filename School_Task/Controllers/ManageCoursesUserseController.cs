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
    [Authorize(Roles = "Admin")]
    public class ManageCoursesUserseController : Controller
       
    {
        private ApplicationDbContext appcontext = new ApplicationDbContext();
        private SchoolContext schoolcontext = new SchoolContext();


        // GET: ManageCoursesUserse
        public ActionResult Index()
        {

          
            var coursesIDs = (from usercourse in schoolcontext.UserCourses
                             select usercourse.CourseID).Distinct().ToList();

            List<Courses> courses = new List<Courses>();
            foreach (var courseid in coursesIDs)
            {   if (courseid == null) continue;
                var course = schoolcontext.Courses.Find(courseid);
                course.Id= (int)courseid;
                courses.Add(course);
            }
            return View(courses);
        }
/*==========================================================================================**/
        [HttpGet]
        public ActionResult AddTeacherToCourse()
        {
            var courses = schoolcontext.Courses.ToList();
            var users = UsersWithRoles().ToList().FindAll(user => user.Role == "Teacher");
            
            ViewBag.courses=courses;
            ViewBag.userse = users;
  
            return View();

            /*  var userscourses = new UsersCourse();*/
            /*userscourses.courses = courses;
            userscourses.userse= users.ToList();*/
        }
        [HttpPost,ActionName("AddTeacherToCourse")]
        public ActionResult AddTeacherToCourse(UserCourse usercourse)
        {
           
            if (ModelState.IsValid)
            {
                schoolcontext.UserCourses.Add(usercourse);
                schoolcontext.SaveChanges();
                return RedirectToAction("Index");
            }

            return RedirectToAction("AddTeacherToCourse");


        }



        [HttpGet]
        public ActionResult AddStudentToCourse()
        {
            var courses = schoolcontext.Courses.ToList();
            var users = UsersWithRoles().ToList().FindAll(user => user.Role == "Student");

            ViewBag.courses = courses;
            ViewBag.userse = users;

            return View();

 
        }
        [HttpPost, ActionName("AddStudentToCourse")]
        public ActionResult AddStudentToCourse(UserCourse usercourse)
        {

            if (ModelState.IsValid)
            {
                schoolcontext.UserCourses.Add(usercourse);
                schoolcontext.SaveChanges();
                return RedirectToAction("Index");
            }

            return RedirectToAction("AddStudentToCourse");


        }



        /*====================================================================================================*/
        [HttpGet]
        public ActionResult ShowUserse(String id, String role) 
        {
            var courseid = Convert.ToInt32(id);
            var course = schoolcontext.Courses.Find(courseid);

            var userseIDs = from usercourse in schoolcontext.UserCourses
                            where usercourse.CourseID == courseid
                            select usercourse.UserId;
            var useresnames = new List<Users>();

            foreach (var userid in userseIDs)
            {
                ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                ApplicationUser user = UserManager.FindById(userid);
                if (GetUserRole(user) == role)
                {
                    useresnames.Add(new Users()
                    {
                        Id = user.Id,
                        Firstname = user.UserName.Split(' ')[0],
                        Lastname = user.UserName.Split(' ')[1],
                        Username = user.UserName,
                        Email = user.Email,
                        age = user.age,
                        Password = user.PasswordHash,
                        Role = role

                    });

                }
            }
            ViewBag.courseName = course.CourseName;



            return View(useresnames);
        }


        /*=======================================================================*/
        
        public ActionResult Delete(String id)
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
            if (user == null) { return HttpNotFound(); }
            Users user1 = new Users()
            {
                Id = user.Id,
                age = user.age,
                Email = user.Email,
                Role = GetUserRole(user),
                Firstname = user.UserName.Split(' ')[0],
                Lastname = user.UserName.Split(' ')[1],
                Username=user.UserName

            };
            return View(user1);



        }


        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(String id)
        {
            UserCourse userCourse = (from usercourse in schoolcontext.UserCourses
                                     where usercourse.UserId == id
                                     select usercourse).ToList()[0];
       
           if(userCourse == null) RedirectToAction("Delete", new {id= userCourse.UserId});
            schoolcontext.UserCourses.Remove(userCourse);
            schoolcontext.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                schoolcontext.Dispose();
            }
            base.Dispose(disposing);
        }



        /*====================================================================================================*/



        public IEnumerable<Users> UsersWithRoles()
        {
            var usersWithRoles = (from user in appcontext.Users
                                  select new
                                  {
                                      UserId = user.Id,
                                      Username = user.UserName,
                                      Email = user.Email,
                                      RoleNames = (from userRole in user.Roles
                                                   join role in appcontext.Roles on userRole.RoleId
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
                                  }) ;
            return usersWithRoles;

        }


        public String GetUserRole(ApplicationUser user)
        {
            if (user == null) return "None";
            ApplicationDbContext context = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var s = UserManager.GetRoles(user.Id);
            foreach (var ss in s)
                return ss.ToString();

            return "None";

        }

    }
}






/* 
 *      to chick the modelState errors
 * var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { x.Key, x.Value.Errors })
            .ToArray();
            string s = "";
            foreach (var e in errors)
            {
                s += e.ToString();
                s += "\n";
            }
            s += "id = " + usercourse.UserId;
            Response.Write(s);*/
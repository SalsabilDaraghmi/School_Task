using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using School_Task.Models;
using School_Task.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;

namespace School_Task.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();
        // GET: Admin
        public ActionResult TeacherIndex()

        {
            return View(UsersWithRoles());
        }

        public ActionResult StudentIndex()
        {
            return View(UsersWithRoles());
        }
        public ActionResult CoursesIndex()
        {
           
            return RedirectToAction("Index", "ManageCourses");
        }

  

        public ActionResult CoursesUserse()
        {
            return RedirectToAction("Index", "ManageCoursesUserse");
            /*List<UsersCourse> userscourse = new List<UsersCourse>();

            foreach( var courseid in coursesIDs)
            {   *//*get course with this id to get course name*//*
                var course =context.Courses.Find(courseid);
                *//* get all useres in this cource *//*
                var userseIDs = from usercourse in context.UserCourses
                                where usercourse.CourseID == courseid
                                select usercourse.UserId;
                var useresnames = new List<Users>();

                foreach (var userid in userseIDs) {
                    ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    ApplicationUser user = UserManager.FindById(userid);
                    useresnames.Add(new Users()
                    {
                        Firstname = user.UserName.Split(' ')[0],
                        Lastname = user.UserName.Split(' ')[0],
                        Role = GetUserRole(user)
                    }); ;
                }

                userscourse.Add(new UsersCourse()
                {
                    courseName = course.CourseName,
                    useresNames = useresnames
                }) ;
            }*/
            /*return View(courses);*/
        }



        public String GetUserRole(ApplicationUser user)
        {   if (user == null) return "None";
                ApplicationDbContext context = new ApplicationDbContext();
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var s = UserManager.GetRoles(user.Id);
            foreach (var ss in s)
                return ss.ToString();
                    
            return "None";
            
        }

        /*=======================================================Manag  Useres ===============================================================*/

        /*========================================= Create new User ==========================================*/

        // GET: Users/Create
        public ActionResult Create(String role)
        {   Users user =new Users();
            user.Role = role;
            return View(user);
        }

        [HttpPost,ActionName("Create")]
        public ActionResult AddUser(Users model)
        {   
            if (ModelState.IsValid)
            {

                ApplicationUser user = new ApplicationUser
                {
                    UserName=model.Firstname+" "+model.Lastname,
                    age=model.age,
                    Email = model.Email,
                    PasswordHash = model.Password
                };

                ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

                IdentityResult result = UserManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, model.Role);
                    return RedirectToAction(model.Role+"Index", "Admin");
                }

                foreach (string error in result.Errors)
                    ModelState.AddModelError("", error);
            }

            return View(model);
        }

        /*========================================= Edit User ============================================*/

        public ActionResult Edit(String id,String role)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            ApplicationUser user = UserManager.FindById(id);
            if (user == null) { return HttpNotFound(); }
            Users user1 = new Users()
            {
                Id = id,
                age = user.age,
                Email = user.Email,
                Role =role ,
                Firstname = user.UserName.Split(' ')[0],
                Lastname = user.UserName.Split(' ')[1],
                Password=user.PasswordHash

            };
            return View(user1);

        }


        [HttpPost,ActionName("Edit")]
        public ActionResult Edit(Users model)
        {
            /*var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { x.Key, x.Value.Errors })
            .ToArray();
            string s = "";
            foreach (var e in errors) {
                s+= e.ToString();
                s += "\n";
            }
             Response.Write(s);*/

            if (ModelState.IsValid)
            {
                /*return RedirectToAction("Delete", "Admin", new { id = model.Id });*/

                ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

                ApplicationUser UserToEdit = UserManager.FindById(model.Id);

                if (UserToEdit.UserName !=( model.Firstname+" "+model.Lastname))
                    UserToEdit.UserName = (model.Firstname + " " + model.Lastname);


                if (UserToEdit.Email != model.Email)
                    UserToEdit.Email = model.Email;

                if (UserToEdit.age != model.age)
                    UserToEdit.age = model.age;
                


                IdentityResult result = UserManager.Update(UserToEdit);
                if (result.Succeeded)
                {
                    return RedirectToAction(model.Role+"Index", "Admin");
                }

                foreach (string error in result.Errors)
                    ModelState.AddModelError("", error);
            }

            return View(model);
        }


        /*========================================= Delete User ==========================================*/
        public ActionResult Delete(String id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            ApplicationUser user = UserManager.FindById(id);
            if (user == null) { return HttpNotFound(); }
            Users user1=new Users()
            {
                Id = user.Id,
                age = user.age,
                Email= user.Email,
                Role= "",
                Firstname =user.UserName.Split(' ')[0],
                Lastname= user.UserName.Split(' ')[1]

            };
            return View(user1);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteUser(string Id)
        {
            ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            ApplicationUser UserToDelete = UserManager.FindById(Id);

            if (UserToDelete != null)
            {
                IdentityResult result = UserManager.Delete(UserToDelete);
                if (result.Succeeded)
                {
                    return RedirectToAction("TeacherIndex", "Admin");
                }

                foreach (string error in result.Errors)
                    ModelState.AddModelError("", error);

                return View(Id);
            }

            return HttpNotFound();
        }


        public IEnumerable<Users> UsersWithRoles()
        {
            var usersWithRoles = (from user in context.Users
                                  select new
                                  {
                                      UserId = user.Id,
                                      Username = user.UserName,
                                      Email = user.Email,
                                      age= user.age,
                                      RoleNames = (from userRole in user.Roles
                                                   join role in context.Roles on userRole.RoleId
                                                   equals role.Id
                                                   select role.Name).ToList()
                                  }).ToList().Select(p => new Users

                                  { 
                                      Id = p.UserId,
                                      Firstname = p.Username.Split(' ')[0],
                                      Lastname = p.Username.Split(' ')[1],
                                      Email = p.Email,
                                      age= p.age,
                                      Role = string.Join(",", p.RoleNames)
                                  });
            return usersWithRoles;

        }




    }
}
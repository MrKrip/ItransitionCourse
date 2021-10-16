using Abp.Web.Mvc.Alerts;
using ItransitionCourse.Data;
using ItransitionCourse.Helpers;
using ItransitionCourse.Models;
using ItransitionCourse.Models.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ItransitionCourse.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHelper _helper;
        IEnumerable<string> Themes = new List<string>() { "Java", "C#", "Math", "Geometry", "Теория чисел" };
        private readonly int pageSize = 10;

        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> SignInManager, ApplicationDbContext applicationDb,
            UserManager<IdentityUser> userManager, IHelper helper)
        {
            _logger = logger;
            _signInManager = SignInManager;
            db = applicationDb;
            _userManager = userManager;
            _helper = helper;
        }

        public IActionResult Index(int? id)
        {
            if (id == null)
                id = 1;

            int TotalPages = (int)Math.Ceiling(db.Tasks.Count() / (double)pageSize);

            if(id<1 || id>TotalPages)
                return Redirect("~/");

            ViewBag.HasPreviousPage = id > 1;
            ViewBag.HasNextPage = TotalPages > id;
            ViewBag.CurrentPage = id;

           var users = _userManager.Users;

            var Tasks = (from T in db.Tasks
                        join U in users on T.UserId equals U.Id
                        select new TaskViewModel()
                        {
                            TaskId = T.TaskId,
                            UserName = U.UserName,
                            UserId=T.UserId,
                            TaskText = T.TaskText.Substring(0, 50) + "....",
                            Theme = T.Theme,
                            Title = T.Title,
                            Image = T.Image1
                        }).ToList();
            Tasks.Reverse();
            return View(Tasks.Skip((int)((id - 1) * pageSize)).Take(pageSize));
        }


        public IActionResult Newtask(string id)
        {
            if(_signInManager.IsSignedIn(User))
            {
                if (!string.IsNullOrEmpty(id) && User.IsInRole("Admin"))
                    ViewBag.UserId = id;
                ViewBag.Themes =new SelectList(Themes);
                return View();
            }
            else
            {
                return Redirect("~/");
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> Newtask(TaskEntity task)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user;
                if (!User.IsInRole("Admin"))
                {
                    user = await _userManager.GetUserAsync(HttpContext.User);
                }
                else
                {
                    user = await _userManager.FindByIdAsync(task.UserId);
                }
                
                TaskEntity newTask = new TaskEntity() { Title = task.Title,
                    TaskText = task.TaskText,
                    Answer1 = task.Answer1, Answer2 = task.Answer2,
                    Answer3 = task.Answer3,
                    CreationDate = DateTime.Now,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    User = user,
                    Theme = task.Theme
                };
                db.Tasks.Add(newTask);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
            
        }

        public async Task<IActionResult> Profile(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return Redirect("~/");
            }

           var user=await _userManager.FindByIdAsync(id);
           if(user==null)
           {
                return Redirect("~/");
           }

            var profile = new ProfileViewModel() { UserName = user.UserName,UserId=id, CanEdit=await _helper.PermissionToEdit(HttpContext,_userManager,id) ,
                    Tasks =db.Tasks.Where(T=>T.UserId==id).Select(T=>new ProfileViewModel.ProfileTask() { TaskId=T.TaskId, Title=T.Title })};
             return View(profile);

        }

        public IActionResult ReadTask(int? id)
        {
            if (id == null || db.Tasks.Where(T => T.TaskId == id).Count() == 0)
            {
                return Redirect("~/");
            }
            else
            {
                var users = _userManager.Users;
                var TaskView = from T in db.Tasks
                             join U in users on T.UserId equals U.Id
                             where T.TaskId==id
                             select new TaskViewModel()
                             {
                                 TaskId = T.TaskId,
                                 UserName = U.UserName,
                                 UserId = T.UserId,
                                 TaskText = T.TaskText,
                                 Theme = T.Theme,
                                 Title = T.Title,
                                 Image = T.Image1
                             };
                return View(TaskView.First());
            }
        }

        [HttpPost]
        public IActionResult ReadTask(int id, string Answer)
        {
            var task = db.Tasks.Where(T => T.TaskId == id).First();
            bool AnswerBool = (Answer == task.Answer1) || (Answer == task.Answer2) || (Answer == task.Answer3);
            if(AnswerBool)
            {
                ViewBag.Message= "Correct answer";
            }
            else
            {
                string AnswerString = $"First answer : {task.Answer1};    ";
                if (!string.IsNullOrEmpty(task.Answer2))
                    AnswerString += $"Optional answer : {task.Answer2};    ";
                if (!string.IsNullOrEmpty(task.Answer3))
                    AnswerString += $"Optional answer : {task.Answer3};";
                ViewBag.Message=$"Wrong. Right answers:    {AnswerString}";
            }
            var users = _userManager.Users;
            var TaskView = from T in db.Tasks
                           join U in users on T.UserId equals U.Id
                           where T.TaskId == id
                           select new TaskViewModel()
                           {
                               TaskId = T.TaskId,
                               UserName = U.UserName,
                               UserId = T.UserId,
                               TaskText = T.TaskText,
                               Theme = T.Theme,
                               Title = T.Title,
                               Image = T.Image1
                           };
            return View(TaskView.First());
        }

        public async Task<IActionResult> Edit(int? id)
        {            
            if (id == null || db.Tasks.Where(T => T.TaskId == id).Count() == 0 
                ||! await _helper.PermissionToEdit(HttpContext,_userManager,db.Tasks.Single(T=>T.TaskId==id).UserId))
                return Redirect("~/");

            TaskEntity taskEntity = db.Tasks.Single(T=>T.TaskId==id);
            ViewBag.Themes = new SelectList(Themes);
            return View(taskEntity);
        }

        [HttpPost]
        public IActionResult Edit(TaskEntity task)
        {
            db.Tasks.Update(task);
            db.SaveChanges(); 
            return Redirect("~/");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!(id == null || db.Tasks.Where(T => T.TaskId == id).Count() == 0
                || !await _helper.PermissionToEdit(HttpContext, _userManager, db.Tasks.Single(T => T.TaskId == id).UserId)))
            {
                var task = db.Tasks.Single(T=>T.TaskId==id);
                db.Tasks.Remove(task);
                db.SaveChanges();
            }
            return Redirect("~/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

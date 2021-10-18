using Abp.Web.Mvc.Alerts;
using Dropbox.Api;
using Dropbox.Api.Files;
using ItransitionCourse.Data;
using ItransitionCourse.Helpers;
using ItransitionCourse.Models;
using ItransitionCourse.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHelper _helper;
        private readonly IConfiguration _IConfiguration;
        IEnumerable<string> Themes = new List<string>() { "Java", "C#", "Math", "Geometry" };
        private readonly int pageSize = 10;

        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> SignInManager, ApplicationDbContext applicationDb,
            UserManager<IdentityUser> userManager, IHelper helper, RoleManager<IdentityRole> roleManager, IConfiguration IConfiguration)
        {
            _logger = logger;
            _signInManager = SignInManager;
            db = applicationDb;
            _userManager = userManager;
            _helper = helper;
            _roleManager = roleManager;
            _IConfiguration = IConfiguration;
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
            var CurrentUser = _userManager.GetUserAsync(User).Result;
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
                            Image = db.Images.Where(I=>I.UserId==U.Id).First().Name,
                            HasAnAnswer=false
                        }).ToList();
            Tasks.Reverse();
            var OutTasks = Tasks.Skip((int)((id - 1) * pageSize)).Take(pageSize);
            if(_signInManager.IsSignedIn(HttpContext.User))
            {
                foreach(var task in OutTasks)
                {
                    var userAnswer = db.Answers.Where(A => A.TaskId == task.TaskId && A.UserID == CurrentUser.Id).ToList();
                    task.HasAnAnswer = userAnswer.Count > 0;
                    if (userAnswer.Count > 0)
                        task.CorrectAnswer = userAnswer.First().CorrectAnswer;
                }
            }

            return View(OutTasks);
        }


        public async Task<IActionResult> Newtask(string id)
        {
            
            if(_signInManager.IsSignedIn(User))
            {
                if (!string.IsNullOrEmpty(id)&& _signInManager.IsSignedIn(HttpContext.User) && (await _helper.IsInRole(HttpContext.User,_userManager,"Admin")))
                    ViewBag.UserId = id;
                ViewBag.Admin = _signInManager.IsSignedIn(HttpContext.User) && (await _helper.IsInRole(HttpContext.User, _userManager, "Admin"));
                ViewBag.Themes = new SelectList(Themes);
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
                if (!(_signInManager.IsSignedIn(HttpContext.User) && (await _helper.IsInRole(HttpContext.User, _userManager, "Admin"))))
                {
                    user = await _userManager.GetUserAsync(HttpContext.User);
                }
                else
                {
                    user = await _userManager.FindByIdAsync(task.UserId);
                }
                var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                TaskEntity newTask = new TaskEntity() { Title = task.Title,
                    TaskText = task.TaskText,
                    Answer1 = task.Answer1, Answer2 = task.Answer2,
                    Answer3 = task.Answer3,
                    CreationDate = DateTime.Now,
                    UserId = userid,
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

        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file.ContentType.StartsWith("image/"))
            {                
                return Ok();
            }
            else
            {
                return NoContent();
            }
            
        }

        public async Task<IActionResult> Profile(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return Redirect("~/");
            }
            ViewBag.Admin =_signInManager.IsSignedIn(HttpContext.User) && (await _helper.IsInRole(HttpContext.User, _userManager, "Admin"));
           var user=await _userManager.FindByIdAsync(id);
           if(user==null)
           {
                return Redirect("~/");
           }

            var profile = new ProfileViewModel() { UserName = user.UserName,UserId=id, CanEdit=await _helper.PermissionToEdit(HttpContext,_userManager,id) ,
                    Tasks =db.Tasks.Where(T=>T.UserId==id).Select(T=>new ProfileViewModel.ProfileTask() { TaskId=T.TaskId, Title=T.Title })
                    , CorrectTasks=db.Answers.Where(A=>A.UserID==id && A.CorrectAnswer).Count()};
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
                                 CreationDate=T.CreationDate,
                                 Answer1=T.Answer1,
                                 Answer2=T.Answer2,
                                 Answer3=T.Answer3,
                                 Image = db.Images.Where(I => I.UserId == U.Id).First().Name
                             };
                if(_signInManager.IsSignedIn(User))
                {
                    var CurrentUser = _userManager.GetUserAsync(User).Result;
                    var userAnswer = db.Answers.Where(A => A.TaskId == id && A.UserID == CurrentUser.Id).ToList();
                    if(userAnswer.Count>0)
                        ViewBag.UserAnswer = userAnswer.First();
                }                
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
                               Image = db.Images.Where(I => I.UserId == U.Id).First().Name
                           };
            db.Answers.Add(new UserAnswerEntity() { TaskId=task.TaskId,UserID=task.UserId,Answer=Answer,CorrectAnswer=AnswerBool });
            db.SaveChanges();
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
            string userid=string.Empty;
            if (!(id == null || db.Tasks.Where(T => T.TaskId == id).Count() == 0
                || !await _helper.PermissionToEdit(HttpContext, _userManager, db.Tasks.Single(T => T.TaskId == id).UserId)))
            {
                var task = db.Tasks.Single(T=>T.TaskId==id);
                userid = task.UserId;
                db.Tasks.Remove(task);
                db.SaveChanges();
            }
            return RedirectToAction("Profile","Home",new { id=userid });
        }

        public IActionResult Admin()
        {
            if (!(_signInManager.IsSignedIn(HttpContext.User) && (_helper.IsInRole(HttpContext.User, _userManager, "Admin")).Result))
                return Redirect("~/");
            var result =_userManager.Users.Select(U=>new AdminViewModel() { UserId=U.Id,UserName=U.UserName }).ToList();
            foreach(var user in result)
            {
                user.UserRoles = _helper.UserRoles(user.UserId, _userManager).ToList();
            }
            return View(result);
        }

        public IActionResult Search(int? id,string SearchString)
        {
            if (id == null)
                id = 1;

            var SearchTasks = db.Tasks.Where(T => T.TaskText.Contains(SearchString) || T.Title.Contains(SearchString) || T.Theme.Contains(SearchString)||T.User.UserName.Contains(SearchString));

            int TotalPages = (int)Math.Ceiling(SearchTasks.Count() / (double)pageSize);

            if (id < 1 || id > TotalPages)
                return Redirect("~/");

            ViewBag.HasPreviousPage = id > 1;
            ViewBag.HasNextPage = TotalPages > id;
            ViewBag.CurrentPage = id;

            var users = _userManager.Users;
            var CurrentUser = _userManager.GetUserAsync(User).Result;
            var Tasks = (from T in SearchTasks
                         join U in users on T.UserId equals U.Id
                         select new TaskViewModel()
                         {
                             TaskId = T.TaskId,
                             UserName = U.UserName,
                             UserId = T.UserId,
                             TaskText = T.TaskText.Substring(0, 50) + "....",
                             Theme = T.Theme,
                             Title = T.Title,
                             Image = db.Images.Where(I => I.UserId == U.Id).First().Name,
                             HasAnAnswer = false
                         }).ToList();
            Tasks.Reverse();
            var OutTasks = Tasks.Skip((int)((id - 1) * pageSize)).Take(pageSize);
            if (_signInManager.IsSignedIn(HttpContext.User))
            {
                foreach (var task in OutTasks)
                {
                    var userAnswer = db.Answers.Where(A => A.TaskId == task.TaskId && A.UserID == CurrentUser.Id).ToList();
                    task.HasAnAnswer = userAnswer.Count > 0;
                    if (userAnswer.Count > 0)
                        task.CorrectAnswer = userAnswer.First().CorrectAnswer;
                }
            }
            return View(OutTasks);
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

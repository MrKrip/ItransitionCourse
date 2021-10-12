using ItransitionCourse.Data;
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
        IEnumerable<string> Themes = new List<string>() { "Java", "C#", "Math", "Geometry", "Теория чисел" };

        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> SignInManager, ApplicationDbContext applicationDb,UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _signInManager = SignInManager;
            db = applicationDb;
            _userManager = userManager;
        }

        public IActionResult Index(int? id)
        {
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
            return View(Tasks);
        }


        public IActionResult Newtask()
        {
            if(_signInManager.IsSignedIn(User))
            { 
                ViewBag.Themes =new SelectList(Themes);
                return View();
            }
            else
            {
                return Redirect("~/");
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> Newtask(Models.Entity.Task task)
        {
            var user =await _userManager.GetUserAsync(HttpContext.User);
            Models.Entity.Task newTask = new Models.Entity.Task() { Title = task.Title,
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

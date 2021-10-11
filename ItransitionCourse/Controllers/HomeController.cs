using ItransitionCourse.Data;
using ItransitionCourse.Models;
using ItransitionCourse.Models.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> SignInManager, ApplicationDbContext applicationDb,UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _signInManager = SignInManager;
            db = applicationDb;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Newtask()
        {
            if(_signInManager.IsSignedIn(User))
            {
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
            var user =await _userManager.GetUserAsync(HttpContext.User);
            TaskEntity newTask = new TaskEntity() { Title=task.Title,TaskText=task.TaskText,Answer1=task.Answer1, Answer2 = task.Answer2 ,
                Answer3 = task.Answer3,CreationDate=DateTime.Now,UserId= User.FindFirstValue(ClaimTypes.NameIdentifier),
                User=user };
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Data;
using SMS.Models;
using SMS.ViewModels;
using System.Diagnostics;

namespace SMS.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        public readonly SmsDbContext _context;
        public HomeController(SmsDbContext context)
        {
            _context = context;
        }

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Index(LoginViewModel logdto)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Admins.SingleOrDefaultAsync(u =>
                u.Email == logdto.Email && u.Password == logdto.Password);

                if (user != null)
                {
                    return Redirect("~/Student/Index");
                }
                else
                {
                    //ModelState.AddModelError("Email", "Email address is already in use.");
                    ModelState.AddModelError(string.Empty,"Invalid email or password") ;
                    return View(logdto);
                }
            }
            //ModelState.AddModelError("Email", "Email address is already in use.");
            return View(logdto);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
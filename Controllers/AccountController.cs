using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace SMS.Controllers
{
 /*   public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }*/
    public class AccountController : Controller
    {
        // Other actions

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("~/Home/Home"); // Redirect to the login page
        }
    }

}

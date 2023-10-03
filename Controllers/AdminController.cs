using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Data;
using SMS.Models;
using SMS.ViewModels;

namespace SMS.Controllers
{
    public class AdminController : Controller
    {
        private readonly SmsDbContext _context;
        public AdminController(SmsDbContext context)
        {
            _context = context;
        }
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult>Register(AdminViewModel mandto)
        {
            var existingUser = await _context.Admins.FirstOrDefaultAsync(u => u.Email == mandto.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email address is already in use.");
                return View(mandto);
            }
            Admin add = new Admin()
            {
                Name = mandto.Name,
                Email = mandto.Email,
                Phone =mandto.Phone,
                Username =mandto.Username,
                Password = mandto.Password, 
            };
            await _context.AddAsync(add);
            await _context.SaveChangesAsync();
            return Redirect("~/Home?index");
        }
    }
}

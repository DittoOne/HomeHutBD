using System.Linq;
using Microsoft.AspNetCore.Mvc;
using HomeHutBD.Data;
using HomeHutBD.Models;
using HomeHutBD.ViewModels;
using System;

namespace HomeHutBD.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In production, you'd hash and compare passwords
                var user = _context.Users
                    .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

                if (user != null)
                {
                    // TODO: Sign the user in (e.g., using cookies or Identity).
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid login credentials.");
            }
            return View(model);
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email, username, or phone already exist
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already in use.");
                    return View(model);
                }
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username is already in use.");
                    return View(model);
                }
                if (_context.Users.Any(u => u.PhoneNumber == model.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Phone number is already in use.");
                    return View(model);
                }

                // Create new user (plain text password for demo only!)
                var user = new Users
                {
                    Username = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Password = model.Password,
                    ProfileImage = "",  // or set a default
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                // Possibly sign in user automatically here.
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // GET: /Account/VerifyEmail
        public IActionResult VerifyEmail()
        {
            return View();
        }

        // POST: /Account/VerifyEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                // e.g., check if email exists and send a password reset link
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "No user found with that email.");
                    return View(model);
                }
                // TODO: Email user a reset token or direct them to the ChangePassword page
                // For simplicity, let's just go to ChangePassword
                return RedirectToAction("ChangePassword", new { email = model.Email });
            }
            return View(model);
        }

        // GET: /Account/ChangePassword
        public IActionResult ChangePassword(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("VerifyEmail");

            var model = new ChangePasswordViewModel { Email = email };
            return View(model);
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "No user found with that email.");
                    return View(model);
                }

                // Update the password
                user.Password = model.NewPassword; // In production, hash it!
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            return View(model);
        }
    }
}

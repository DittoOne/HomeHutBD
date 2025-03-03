using System.Linq;
using Microsoft.AspNetCore.Mvc;
using HomeHutBD.Data;
using HomeHutBD.Models;
using HomeHutBD.ViewModels;
using System;
using System.Security.Cryptography;
using System.Text;

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
                    // Manual authentication using sessions
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("Username", user.Username);

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid login credentials.");
            }
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout (alternative for links)
        public IActionResult LogoutGet()
        {
            // Clear the session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
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
                // In production, you should hash the password
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

                // Update the password (in production, hash it!)
                user.Password = model.NewPassword;
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            return View(model);
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
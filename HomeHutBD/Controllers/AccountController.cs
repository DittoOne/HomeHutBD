using System.Linq;
using Microsoft.AspNetCore.Mvc;
using HomeHutBD.Data;
using HomeHutBD.Models;
using HomeHutBD.ViewModels;
using System;
using System.Security.Cryptography;
using System.Text;
using HomeHutBD.Helpers;

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

        // GET: /Account/Profile
        [Authorize]
        public IActionResult Profile()
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }


        // POST: /Account/UpdateProfile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Users model)
        {
            // Get the current user ID from session
            int userId = HttpContext.Session.GetInt32("UserId").Value;

            // Find the user in the database
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(model.FirstName))
            {
                ModelState.AddModelError("FirstName", "First name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                ModelState.AddModelError("LastName", "Last name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            else if (model.Email != user.Email)
            {
                // Check if the new email is already in use by another user
                if (_context.Users.Any(u => u.Email == model.Email && u.UserId != userId))
                {
                    ModelState.AddModelError("Email", "This email is already in use by another account.");
                }
            }

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Phone number is required.");
            }
            else if (model.PhoneNumber != user.PhoneNumber)
            {
                // Check if the new phone number is already in use by another user
                if (_context.Users.Any(u => u.PhoneNumber == model.PhoneNumber && u.UserId != userId))
                {
                    ModelState.AddModelError("PhoneNumber", "This phone number is already in use by another account.");
                }
            }

            // If there are validation errors, return to the profile view with errors
            if (!ModelState.IsValid)
            {
                // We need to fetch the full user object again to pass to the view
                return View("Profile", user);
            }

            try
            {
                // Update user properties
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                // Only update profile image if a new URL is provided
                if (!string.IsNullOrWhiteSpace(model.ProfileImage))
                {
                    user.ProfileImage = model.ProfileImage;
                }

                // Save changes to database
                await _context.SaveChangesAsync();

                // Update session data
                HttpContext.Session.SetString("Username", user.Username);

                // Add success message
                TempData["SuccessMessage"] = "Profile updated successfully.";

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                //// Log the exception
                //_logger.LogError(ex, "Error updating profile for user {UserId}", userId);

                // Add error message
                TempData["ErrorMessage"] = "An error occurred while updating your profile. Please try again.";

                return View("Profile", user);
            }
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
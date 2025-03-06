using HomeHutBD.Data;
using HomeHutBD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HomeHutBD.Controllers
{
    public class VerificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VerificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult RequestVerification()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestVerification(VerificationRequests verification)
        {
            if (ModelState.IsValid)
            {
                // Get current user
                int userId = HttpContext.Session.GetInt32("UserId").Value;

                // Set the verification request properties
                verification.UserId = userId;
                verification.RequestDate = DateTime.Now;
                verification.VerificationStatus = "Pending";

                // Add to database
                _context.Add(verification);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your NID verification request has been submitted. Please wait for approval.";
                return RedirectToAction("Index", "Home");
            }
            return View(verification);
        }
    }
}
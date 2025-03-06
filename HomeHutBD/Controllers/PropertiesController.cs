using HomeHutBD.Data;
using HomeHutBD.Helpers;
using HomeHutBD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HomeHutBD.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PropertiesController> _logger;

        public PropertiesController(ApplicationDbContext context, ILogger<PropertiesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Browse(string location = null, string type = null, decimal? maxPrice = null)
        {
            // Start with all properties
            var propertiesQuery = _context.Properties
                .Include(p => p.User)
                .AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(location))
            {
                propertiesQuery = propertiesQuery.Where(p => p.Address.Contains(location));
            }

            if (!string.IsNullOrEmpty(type))
            {
                propertiesQuery = propertiesQuery.Where(p => p.Type == type);
            }

            if (maxPrice.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.Price <= maxPrice.Value);
            }

            // Order by newest first
            var properties = await propertiesQuery.OrderByDescending(p => p.LastUpdate).ToListAsync();

            // Pass data to view
            ViewData["Location"] = location;
            ViewData["Type"] = type;
            ViewData["MaxPrice"] = maxPrice;

            return View(properties);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Properties property)
        {
            // Remove User and VerificationRequest from validation
            ModelState.Remove("User");
            ModelState.Remove("VerificationRequest");

            if (ModelState.IsValid)
            {
                // Get current user
                int userId = HttpContext.Session.GetInt32("UserId").Value;
                var user = await _context.Users.FindAsync(userId);

                // Check if user is verified
                if (!user.IsVerified)
                {
                    // Instead of redirecting to verification page, just show message and return to Home
                    TempData["ErrorMessage"] = "You need to verify your NID before listing properties. Please contact support.";
                    return RedirectToAction("Index", "Home");
                }

                // Set the current user as the owner
                property.UserId = userId;
                property.LastUpdate = DateTime.Now;
                property.VerificationId = null;

                // Explicitly set navigation properties to null
                property.User = null;
                property.VerificationRequest = null;

                // Add to database
                _context.Add(property);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Browse));
            }
            return View(property);
        }

        [Authorize]
        public async Task<IActionResult> MyProperties()
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;

            var properties = await _context.Properties
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.LastUpdate)
                .ToListAsync();

            return View(properties);
        }

        public async Task<IActionResult> Details(int id)
        {
            var property = await _context.Properties
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }
    }
}
using System.Diagnostics;
using HomeHutBD.Models;
using Microsoft.AspNetCore.Mvc;
using HomeHutBD.Helpers;
using HomeHutBD.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeHutBD.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Add this line

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context) // Modify constructor
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Get 3 random properties from the database
            var featuredProperties = _context.Properties
                .Include(p => p.User)
                .OrderBy(p => Guid.NewGuid()) // Random order
                .Take(3)
                .ToList();

            return View(featuredProperties);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(string location, string type, string budget)
        {
            // Parse budget string to get the maximum price
            decimal? maxPrice = null;
            switch (budget)
            {
                case "Under ?20 Lac":
                    maxPrice = 2000000;
                    break;
                case "?20 Lac - ?50 Lac":
                    maxPrice = 5000000;
                    break;
                case "?50 Lac - ?1 Crore":
                    maxPrice = 10000000;
                    break;
                case "Above ?1 Crore":
                    maxPrice = null; // No maximum price
                    break;
            }

            // Redirect to Properties/Browse with search parameters
            return RedirectToAction("Browse", "Properties", new
            {
                location = location,
                type = type,
                maxPrice = maxPrice
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
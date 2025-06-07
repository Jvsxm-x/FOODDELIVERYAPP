using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FoodDeliveryApp.Models;
using Microsoft.AspNetCore.Http;

namespace FoodDeliveryApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Service()
        {
            return View();
        }

        public IActionResult Team()
        {
            return View();
        }

        public IActionResult Testimonial()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string name, string email, string message)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                ViewData["Error"] = "All fields are required.";
                return View();
            }

            // Simulate sending email or saving to database
            _logger.LogInformation("Contact form submitted: Name={Name}, Email={Email}, Message={Message}", name, email, message);
            ViewData["Success"] = "Thank you for your message! We'll get back to you soon.";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult Cookies()
        {
            return View();
        }

        public IActionResult Help()
        {
            return View();
        }

        public IActionResult FAQs()
        {
            return View();
        }

        public IActionResult Restaurants()
        {
            return RedirectToAction("Browse", "Restaurants");
        }

        public IActionResult Categories()
        {
            return RedirectToAction("Browse", "Categories");
        }

        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users");
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.Scripting;

namespace FoodDeliveryApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private static readonly string[] sourceArray = new[] { "Customer", "Admin", "Delivery" };

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,Password,Role")] User user)
        {
            ModelState.Remove("Orders");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                return View(user);
            }

            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(user);
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Role = string.IsNullOrEmpty(user.Role) ? "Customer" : user.Role;
                if (!sourceArray.Contains(user.Role))
                {
                    ModelState.AddModelError("Role", "Invalid role. Must be Customer, Admin, or Delivery.");
                    return View(user);
                }

                user.IsEmailVerified = false; // New users need verification
                _context.Add(user);
                await _context.SaveChangesAsync();

                // Send verification email (placeholder; implement email service)
                // await SendVerificationEmail(user);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating user: {ex.Message}");
                return View(user);
            }
        }

        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Portal.Register
        [HttpPost("Portal.Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("FullName,Email,Password,Role")] User user)
        {
            ModelState.Remove("Orders");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                return View(user);
            }

            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(user);
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Role = string.IsNullOrEmpty(user.Role) ? "Customer" : user.Role;
                if (!sourceArray.Contains(user.Role))
                {
                    ModelState.AddModelError("Role", "Invalid role. Must be Customer, Admin, or Delivery.");
                    return View(user);
                }

                user.IsEmailVerified = false;
                _context.Add(user);
                await _context.SaveChangesAsync();

                // Send verification email (placeholder)
                // await SendVerificationEmail(user);

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error registering user: {ex.Message}");
                return View(user);
            }
        }

        // GET: Users/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            if (!user.IsEmailVerified)
            {
                ModelState.AddModelError("", "Please verify your email before logging in.");
                return View();
            }

            // Set session (or use JWT for production)
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", user.FullName);

            return RedirectToAction("Index", "Home");
        }

        // GET: Users/VerifyEmail
        [HttpGet("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid verification request.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Placeholder: Validate token (e.g., compare with stored token or use expiration)
            // For simplicity, assume token is valid
            user.IsEmailVerified = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // GET: Users/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,Password,Role,IsEmailVerified")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Orders");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                return View(user);
            }

            try
            {
                var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != user.Id))
                {
                    ModelState.AddModelError("Email", "This email is already registered by another user.");
                    return View(user);
                }

                if (string.IsNullOrEmpty(user.Password))
                {
                    user.Password = existingUser.Password;
                }
                else
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                if (!sourceArray.Contains(user.Role))
                {
                    ModelState.AddModelError("Role", "Invalid role. Must be Customer, Admin, or Delivery.");
                    return View(user);
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating user: {ex.Message}");
                return View(user);
            }
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Orders != null && user.Orders.Any())
            {
                ModelState.AddModelError("", "Cannot delete user with existing orders.");
                return View(user);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    

}
}
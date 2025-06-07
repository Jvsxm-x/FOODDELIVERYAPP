using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using FoodDeliveryApp.Filters;
namespace FoodDeliveryApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context; private static readonly string[] ValidStatuses = new[] { "Pending", "Preparing", "Delivered" }; private static readonly string[] ValidPaymentMethods = new[] { "Card", "Cash", "Paypal" };

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }
        [AuthorizeByRole("Orders", "Create")]
        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Delivery)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .ToListAsync();
            return View(orders);
        }

        // GET: Orders/TrackOrder/5
        public async Task<IActionResult> TrackOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Delivery)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (!ValidStatuses.Contains(status))
            {
                ModelState.AddModelError("", "Invalid status. Must be Pending, Preparing, or Delivered.");
                return RedirectToAction(nameof(TrackOrder), new { id });
            }

            try
            {
                order.Status = status;
                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating order status: {ex.Message}");
                return RedirectToAction(nameof(TrackOrder), new { id });
            }
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["Users"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["FoodItems"] = new SelectList(_context.FoodItems, "Id", "Name");
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string UserFullName,
            string UserEmail,
            string DeliveryAddress,
            string SpecialRequest,
            int? FoodItemId,
            int Quantity = 1,
            string PaymentMethod = "Cash")
        {
            ModelState.Remove("User");
            ModelState.Remove("Delivery");
            ModelState.Remove("Payment");
            ModelState.Remove("OrderItems");

            if (!ValidPaymentMethods.Contains(PaymentMethod))
            {
                ModelState.AddModelError("", "Invalid payment method. Must be Card, Cash, or Paypal.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                ViewData["Users"] = new SelectList(_context.Users, "Id", "FullName");
                ViewData["FoodItems"] = new SelectList(_context.FoodItems, "Id", "Name", FoodItemId);
                return View();
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == UserEmail);
                if (user == null)
                {
                    user = new User
                    {
                        FullName = UserFullName,
                        Email = UserEmail,
                        Password = BCrypt.Net.BCrypt.HashPassword("defaultPassword"), // Replace with user input in production
                        Role = "Customer",
                        IsEmailVerified = false
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    UserId = user.Id
                };

                var delivery = new Delivery
                {
                    Address = DeliveryAddress,
                    EstimatedDeliveryTime = DateTime.Now.AddHours(1),
                    Order = order
                };
                order.Delivery = delivery;

                var payment = new Payment
                {
                    PaymentMethod = PaymentMethod,
                    IsPaid = false,
                    Order = order
                };
                order.Payment = payment;

                if (FoodItemId.HasValue && Quantity > 0)
                {
                    var foodItem = await _context.FoodItems.FindAsync(FoodItemId.Value);
                    if (foodItem == null)
                    {
                        ModelState.AddModelError("", "Selected food item not found.");
                        ViewData["Users"] = new SelectList(_context.Users, "Id", "FullName", user.Id);
                        ViewData["FoodItems"] = new SelectList(_context.FoodItems, "Id", "Name", FoodItemId);
                        return View();
                    }

                    var orderItem = new OrderItem
                    {
                        Quantity = Quantity,
                        UnitPrice = foodItem.Price,
                        FoodItemId = foodItem.Id,
                        Order = order
                    };
                    order.OrderItems = new List<OrderItem> { orderItem };
                }
                else if (!string.IsNullOrEmpty(SpecialRequest))
                {
                    var orderItem = new OrderItem
                    {
                        Quantity = 1,
                        UnitPrice = 0,
                        FoodItem = new FoodItem { Name = "Special Request", Description = SpecialRequest },
                        Order = order
                    };
                    order.OrderItems = new List<OrderItem> { orderItem };
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating order: {ex.Message}");
                ViewData["Users"] = new SelectList(_context.Users, "Id", "FullName");
                ViewData["FoodItems"] = new SelectList(_context.FoodItems, "Id", "Name", FoodItemId);
                return View();
            }
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Delivery)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }

}
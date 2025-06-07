using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace FoodDeliveryApp.Controllers
{
    public class DeliveriesController : Controller
    {
        private readonly AppDbContext _context;

        public DeliveriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Deliveries
        public async Task<IActionResult> Index()
        {
            var deliveries = await _context.Deliveries
                .Include(d => d.Order)
                .Include(d => d.DeliveryUser)
                .ToListAsync();
            return View(deliveries);
        }

        // GET: Deliveries/TrackDelivery/5
        public async Task<IActionResult> TrackDelivery(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var delivery = await _context.Deliveries
                .Include(d => d.Order)
                .Include(d => d.DeliveryUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (delivery == null)
            {
                return NotFound();
            }

            return View(delivery);
        }

        // POST: Deliveries/AssignDelivery/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDelivery(int id, int deliveryUserId)
        {
            var delivery = await _context.Deliveries.FindAsync(id);
            if (delivery == null)
            {
                return NotFound();
            }

            var deliveryUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == deliveryUserId && u.Role == "Delivery");
            if (deliveryUser == null)
            {
                ModelState.AddModelError("", "Invalid delivery user.");
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                delivery.DeliveryUserId = deliveryUserId;
                _context.Update(delivery);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error assigning delivery: {ex.Message}");
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Deliveries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var delivery = await _context.Deliveries
                .Include(d => d.Order)
                .Include(d => d.DeliveryUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (delivery == null)
            {
                return NotFound();
            }

            return View(delivery);
        }

        // GET: Deliveries/Create
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            ViewData["DeliveryUserId"] = new SelectList(_context.Users.Where(u => u.Role == "Delivery"), "Id", "FullName");
            return View();
        }

        // POST: Deliveries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Address,EstimatedDeliveryTime,OrderId,DeliveryUserId")] Delivery delivery)
        {
            ModelState.Remove("Order");
            ModelState.Remove("DeliveryUser");
            if (delivery.EstimatedDeliveryTime <= DateTime.Now)
            {
                ModelState.AddModelError("EstimatedDeliveryTime", "Estimated delivery time must be in the future.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", delivery.OrderId);
                ViewData["DeliveryUserId"] = new SelectList(_context.Users.Where(u => u.Role == "Delivery"), "Id", "FullName", delivery.DeliveryUserId);
                return View(delivery);
            }

            try
            {
                _context.Add(delivery);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating delivery: {ex.Message}");
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", delivery.OrderId);
                ViewData["DeliveryUserId"] = new SelectList(_context.Users.Where(u => u.Role == "Delivery"), "Id", "FullName", delivery.DeliveryUserId);
                return View(delivery);
            }
        }

        // GET: Deliveries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var delivery = await _context.Deliveries.FindAsync(id);
            if (delivery == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", delivery.OrderId);
            ViewData["DeliveryUserId"] = new SelectList(_context.Users.Where(u => u.Role == "Delivery"), "Id", "FullName", delivery.DeliveryUserId);
            return View(delivery);
        }

        // POST: Deliveries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Address,EstimatedDeliveryTime,OrderId,DeliveryUserId")] Delivery delivery)
        {
            if (id != delivery.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Order");
            ModelState.Remove("DeliveryUser");
            if (delivery.EstimatedDeliveryTime <= DateTime.Now)
            {
                ModelState.AddModelError("EstimatedDeliveryTime", "Estimated delivery time must be in the future.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", delivery.OrderId);
                ViewData["DeliveryUserId"] = new SelectList(_context.Users.Where(u => u.Role == "Delivery"), "Id", "FullName", delivery.DeliveryUserId);
                return View(delivery);
            }

            try
            {
                _context.Update(delivery);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeliveryExists(delivery.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating delivery: {ex.Message}");
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", delivery.OrderId);
                ViewData["DeliveryUserId"] = new SelectList(_context.Users.Where(u => u.Role == "Delivery"), "Id", "FullName", delivery.DeliveryUserId);
                return View(delivery);
            }
        }

        // GET: Deliveries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var delivery = await _context.Deliveries
                .Include(d => d.Order)
                .Include(d => d.DeliveryUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (delivery == null)
            {
                return NotFound();
            }

            return View(delivery);
        }

        // POST: Deliveries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var delivery = await _context.Deliveries.FindAsync(id);
            if (delivery == null)
            {
                return NotFound();
            }

            _context.Deliveries.Remove(delivery);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeliveryExists(int id)
        {
            return _context.Deliveries.Any(e => e.Id == id);
        }
    }

}
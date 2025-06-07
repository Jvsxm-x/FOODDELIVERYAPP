using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodDeliveryApp.Controllers
{
    public class OrderItemsController : Controller
    {
        private readonly AppDbContext _context;

        public OrderItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrderItems
        public async Task<IActionResult> Index()
        {
            var orderItems = await _context.OrderItems
                .Include(o => o.FoodItem)
                .Include(o => o.Order)
                .ToListAsync();
            return View(orderItems);
        }

        // GET: OrderItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.FoodItem)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // GET: OrderItems/Create
        public IActionResult Create()
        {
            ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "Id", "Name");
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            return View();
        }

        // POST: OrderItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Quantity,UnitPrice,OrderId,FoodItemId")] OrderItem orderItem)
        {
            ModelState.Remove("FoodItem");
            ModelState.Remove("Order");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "Id", "Name", orderItem.FoodItemId);
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
                return View(orderItem);
            }

            try
            {
                _context.Add(orderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating order item: {ex.Message}");
                ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "Id", "Name", orderItem.FoodItemId);
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
                return View(orderItem);
            }
        }

        // POST: OrderItems/AddToOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToOrder(int orderId, int foodItemId, int quantity = 1)
        {
            var order = await _context.Orders.FindAsync(orderId);
            var foodItem = await _context.FoodItems.FindAsync(foodItemId);
            if (order == null || foodItem == null)
            {
                return NotFound();
            }

            if (quantity < 1)
            {
                ModelState.AddModelError("", "Quantity must be at least 1.");
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            try
            {
                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    FoodItemId = foodItemId,
                    Quantity = quantity,
                    UnitPrice = foodItem.Price
                };
                _context.OrderItems.Add(orderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = orderId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error adding item to order: {ex.Message}");
                return RedirectToAction(nameof(Details), new { id = orderId });
            }
        }

        // GET: OrderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound();
            }
            ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "Id", "Name", orderItem.FoodItemId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            return View(orderItem);
        }

        // POST: OrderItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Quantity,UnitPrice,OrderId,FoodItemId")] OrderItem orderItem)
        {
            if (id != orderItem.Id)
            {
                return NotFound();
            }

            ModelState.Remove("FoodItem");
            ModelState.Remove("Order");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "Id", "Name", orderItem.FoodItemId);
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
                return View(orderItem);
            }

            try
            {
                _context.Update(orderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderItemExists(orderItem.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating order item: {ex.Message}");
                ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "Id", "Name", orderItem.FoodItemId);
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
                return View(orderItem);
            }
        }

        // GET: OrderItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.FoodItem)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // POST: OrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound();
            }

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderItemExists(int id)
        {
            return _context.OrderItems.Any(e => e.Id == id);
        }
    }

}
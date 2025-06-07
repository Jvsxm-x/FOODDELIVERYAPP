using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodDeliveryApp.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context; private static readonly string[] ValidPaymentMethods = new[] { "Card", "Cash", "Paypal" };

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments.Include(p => p.Order).ToListAsync();
            return View(payments);
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            return View();
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PaymentMethod,IsPaid,OrderId")] Payment payment)
        {
            ModelState.Remove("Order");
            if (!ModelState.IsValid || !ValidPaymentMethods.Contains(payment.PaymentMethod))
            {
                if (!ValidPaymentMethods.Contains(payment.PaymentMethod))
                {
                    ModelState.AddModelError("PaymentMethod", "Invalid payment method. Must be Card, Cash, or Paypal.");
                }
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", payment.OrderId);
                return View(payment);
            }

            try
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating payment: {ex.Message}");
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", payment.OrderId);
                return View(payment);
            }
        }

        // POST: Payments/ConfirmPayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            try
            {
                payment.IsPaid = true;
                _context.Update(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error confirming payment: {ex.Message}");
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", payment.OrderId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PaymentMethod,IsPaid,OrderId")] Payment payment)
        {
            if (id != payment.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Order");
            if (!ModelState.IsValid || !ValidPaymentMethods.Contains(payment.PaymentMethod))
            {
                if (!ValidPaymentMethods.Contains(payment.PaymentMethod))
                {
                    ModelState.AddModelError("PaymentMethod", "Invalid payment method. Must be Card, Cash, or Paypal.");
                }
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", payment.OrderId);
                return View(payment);
            }

            try
            {
                _context.Update(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentExists(payment.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating payment: {ex.Message}");
                ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", payment.OrderId);
                return View(payment);
            }
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }

}
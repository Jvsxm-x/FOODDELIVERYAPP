using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodDeliveryApp.Controllers
{
    public class FoodItemsController : Controller
    {
        private readonly AppDbContext _context;

        public FoodItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: FoodItems
        public async Task<IActionResult> Index()
        {
            var foodItems = await _context.FoodItems
                .Include(f => f.Restaurant)
                .Include(f => f.Category)
                .ToListAsync();
            return View(foodItems);
        }

        // GET: FoodItems/Menu
        public async Task<IActionResult> Menu()
        {
            var categories = await _context.Categories
                .Include(c => c.FoodItems)
                .ThenInclude(f => f.Restaurant)
                .ToListAsync();
            return View(categories);
        }

        // GET: FoodItems/BrowseByCategory/5
        public async Task<IActionResult> BrowseByCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.FoodItems)
                .ThenInclude(f => f.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: FoodItems/BrowseByRestaurant/5
        public async Task<IActionResult> BrowseByRestaurant(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants
                .Include(r => r.FoodItems)
                .ThenInclude(f => f.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // GET: FoodItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItems
                .Include(f => f.Restaurant)
                .Include(f => f.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        // GET: FoodItems/Create
        public IActionResult Create()
        {
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: FoodItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,ImageUrl,RestaurantId,CategoryId")] FoodItem foodItem)
        {
            ModelState.Remove("Restaurant");
            ModelState.Remove("Category");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", foodItem.RestaurantId);
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", foodItem.CategoryId);
                return View(foodItem);
            }

            try
            {
                _context.Add(foodItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating food item: {ex.Message}");
                ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", foodItem.RestaurantId);
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", foodItem.CategoryId);
                return View(foodItem);
            }
        }

        // GET: FoodItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", foodItem.RestaurantId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", foodItem.CategoryId);
            return View(foodItem);
        }

        // POST: FoodItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,RestaurantId,CategoryId")] FoodItem foodItem)
        {
            if (id != foodItem.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Restaurant");
            ModelState.Remove("Category");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ModelState.AddModelError("", "Validation failed: " + string.Join(", ", errors));
                ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", foodItem.RestaurantId);
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", foodItem.CategoryId);
                return View(foodItem);
            }

            try
            {
                _context.Update(foodItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodItemExists(foodItem.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating food item: {ex.Message}");
                ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", foodItem.RestaurantId);
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", foodItem.CategoryId);
                return View(foodItem);
            }
        }

        // GET: FoodItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItems
                .Include(f => f.Restaurant)
                .Include(f => f.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        // POST: FoodItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            _context.FoodItems.Remove(foodItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FoodItemExists(int id)
        {
            return _context.FoodItems.Any(e => e.Id == id);
        }
    }

}
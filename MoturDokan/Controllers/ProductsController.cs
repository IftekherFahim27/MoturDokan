using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoturDokan.Data;
using MoturDokan.Models;

namespace MoturDokan.Controllers
{
    public class ProductsController : Controller
    {
        private readonly MoturDokanContext _context;

        public ProductsController(MoturDokanContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        public async Task<IActionResult> LowStock(int threshold = 100)
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.Stock < threshold)
                .ToListAsync();

            return View(lowStockProducts);
        }

        public async Task<IActionResult> Unordered()
        {
            var unorderedProducts = await _context.Products
                .Where(p => !_context.Orders.Any(o => o.ProductId == p.ProductId))
                .ToListAsync();

            return View(unorderedProducts);
        }
    }
}

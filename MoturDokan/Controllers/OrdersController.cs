using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoturDokan.Data;
using MoturDokan.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoturDokan.Controllers
{
    public class OrdersController : Controller
    {
        private readonly MoturDokanContext _context;

        public OrdersController(MoturDokanContext context)
        {
            _context = context;
        }

        // API 01: Create a new order
        [HttpPost]
        public async Task<IActionResult> CreateOrder(int productId, string customerName, decimal quantity)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null || product.Stock < quantity)
            {
                return Json(new { message = "Insufficient stock or product not found." });
            }

            var order = new Order
            {
                ProductId = productId,
                CustomerName = customerName,
                Quantity = quantity,
                OrderDate = DateTime.Now
            };

            product.Stock -= quantity;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Json(new { message = "Order created successfully!" });
        }

        // API 02: Update an order's quantity
        [HttpPost]
        public async Task<IActionResult> UpdateOrder(int orderId, decimal newQuantity)
        {
            var order = await _context.Orders.Include(o => o.Product).FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return Json(new { message = "Order not found." });
            }

            var stockDifference = newQuantity - order.Quantity;

            if (order.Product.Stock < stockDifference)
            {
                return Json(new { message = "Insufficient stock to update the order." });
            }

            order.Product.Stock -= stockDifference;
            order.Quantity = newQuantity;

            await _context.SaveChangesAsync();

            return Json(new { message = "Order updated successfully!" });
        }

        // API 03: Delete an order
        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.Orders.Include(o => o.Product).FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return Json(new { message = "Order not found." });
            }

            order.Product.Stock += order.Quantity;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Json(new { message = "Order deleted successfully!" });
        }

        // API 04: Retrieve all orders with product details
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Product)
                .Select(o => new
                {
                    o.OrderId,
                    o.CustomerName,
                    o.Quantity,
                    o.OrderDate,
                    ProductName = o.Product.ProductName,
                    UnitPrice = o.Product.UnitPrice
                })
                .ToListAsync();

            return Json(orders);
        }

        // API 05: Summary of total quantity ordered and total revenue
        [HttpGet]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _context.Products
                .Select(p => new
                {
                    ProductName = p.ProductName,
                    TotalQuantity = _context.Orders.Where(o => o.ProductId == p.ProductId).Sum(o => o.Quantity),
                    TotalRevenue = _context.Orders.Where(o => o.ProductId == p.ProductId)
                                      .Sum(o => o.Quantity * p.UnitPrice)
                })
                .ToListAsync();

            return Json(summary);
        }

        // API 06: Products with stock below a threshold
        [HttpGet]
        public async Task<IActionResult> GetLowStockProducts(int threshold)
        {
            var products = await _context.Products
                .Where(p => p.Stock < threshold)
                .Select(p => new
                {
                    p.ProductName,
                    p.UnitPrice,
                    p.Stock
                })
                .ToListAsync();

            return Json(products);
        }

        // API 07: Top 3 customers by total quantity ordered
        [HttpGet]
        public async Task<IActionResult> GetTopCustomers()
        {
            var topCustomers = await _context.Orders
                .GroupBy(o => o.CustomerName)
                .Select(g => new
                {
                    CustomerName = g.Key,
                    TotalQuantity = g.Sum(o => o.Quantity)
                })
                .OrderByDescending(c => c.TotalQuantity)
                .Take(3)
                .ToListAsync();

            return Json(topCustomers);
        }

        // API 08: Products not ordered at all
        [HttpGet]
        public async Task<IActionResult> GetUnorderedProducts()
        {
            var products = await _context.Products
                .Where(p => !_context.Orders.Any(o => o.ProductId == p.ProductId))
                .Select(p => new
                {
                    p.ProductName,
                    p.UnitPrice,
                    p.Stock
                })
                .ToListAsync();

            return Json(products);
        }

        // API 09: Bulk order creation (transactional)
        [HttpPost]
        public async Task<IActionResult> BulkOrderCreation(List<Order> orders)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var order in orders)
                {
                    var product = await _context.Products.FindAsync(order.ProductId);

                    if (product == null || product.Stock < order.Quantity)
                    {
                        throw new Exception($"Insufficient stock for product {order.ProductId}");
                    }

                    product.Stock -= order.Quantity;
                    _context.Orders.Add(order);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { message = "Bulk order created successfully!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { message = $"Bulk order failed: {ex.Message}" });
            }
        }
    }
}

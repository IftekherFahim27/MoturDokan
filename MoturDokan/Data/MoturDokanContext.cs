using Microsoft.EntityFrameworkCore;
using MoturDokan.Models;

namespace MoturDokan.Data
{
    public class MoturDokanContext : DbContext
    {
        public MoturDokanContext(DbContextOptions<MoturDokanContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
    }
}

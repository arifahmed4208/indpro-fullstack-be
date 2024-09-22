using IndProBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace IndProBackend.Context
{
    public class MyContext:DbContext
    {
        public MyContext(DbContextOptions<MyContext> options): base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}

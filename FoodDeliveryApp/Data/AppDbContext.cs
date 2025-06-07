using Microsoft.EntityFrameworkCore;
using FoodDeliveryApp.Models;

namespace FoodDeliveryApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .IsRequired(false);

            modelBuilder.Entity<Restaurant>()
                .HasMany(r => r.FoodItems)
                .WithOne(f => f.Restaurant)
                .HasForeignKey(f => f.RestaurantId)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .HasMany(c => c.FoodItems)
                .WithOne(f => f.Category)
                .HasForeignKey(f => f.CategoryId)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Delivery)
                .WithOne(d => d.Order)
                .HasForeignKey<Delivery>(d => d.OrderId)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .IsRequired();

            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.DeliveryUser)
                .WithMany()
                .HasForeignKey(d => d.DeliveryUserId)
                .IsRequired(false);

            base.OnModelCreating(modelBuilder);
        }
    }

}
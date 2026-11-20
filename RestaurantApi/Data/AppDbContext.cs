using Microsoft.EntityFrameworkCore;
using RestaurantApi.Models;

namespace RestaurantApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<SubCategory>().HasIndex(x => new { x.CategoryId, x.Name }).IsUnique();
        modelBuilder.Entity<SubCategory>().HasOne(x => x.Category).WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Product>().HasOne(x => x.SubCategory).WithMany(x => x.Products)
            .HasForeignKey(x => x.SubCategoryId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Customer>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Product>().Property(x => x.Price).HasPrecision(10, 2);
        modelBuilder.Entity<Order>().Property(x => x.Total).HasPrecision(10, 2);
        modelBuilder.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(10, 2);
        modelBuilder.Entity<Order>().HasMany(x => x.Items).WithOne(x => x.Order).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrderItem>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Order>().HasOne(x => x.Customer).WithMany(x => x.Orders).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}

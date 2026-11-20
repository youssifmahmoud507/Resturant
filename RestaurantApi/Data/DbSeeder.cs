using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RestaurantApi.Models;

namespace RestaurantApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration configuration)
    {
        var adminEmail = configuration["Admin:Email"] ?? "admin@alharamain2.local";
        var adminPassword = configuration["Admin:Password"] ?? "ChangeMe123!";
        var admin = await db.Customers.FirstOrDefaultAsync(x => x.Email == adminEmail.ToLower());
        if (admin is null)
        {
            admin = new Customer { FullName = "Al Haramain Admin", Email = adminEmail.ToLowerInvariant(), Phone = "01000000000", Role = "Admin" };
            admin.PasswordHash = new PasswordHasher<Customer>().HashPassword(admin, adminPassword);
            db.Customers.Add(admin);
            await db.SaveChangesAsync();
        }

        var categoryData = new[]
        {
            (Name: "Pizza", Description: "Freshly baked pizzas"),
            (Name: "Feteer", Description: "Egyptian feteer"),
            (Name: "Crepe", Description: "Sweet and savory crepes"),
            (Name: "Pasta", Description: "Pasta dishes")
        };

        foreach (var item in categoryData)
        {
            if (!await db.Categories.AnyAsync(c => c.Name == item.Name))
                db.Categories.Add(new Category { Name = item.Name, Description = item.Description });
        }
        await db.SaveChangesAsync();

        var categories = await db.Categories.ToDictionaryAsync(c => c.Name, c => c);
        foreach (var categoryName in new[] { "Pizza", "Feteer", "Crepe" })
        {
            var category = categories[categoryName];
            foreach (var subName in new[] { "Meat", "Cheese" })
            {
                if (!await db.SubCategories.AnyAsync(s => s.CategoryId == category.Id && s.Name == subName))
                    db.SubCategories.Add(new SubCategory { Name = subName, CategoryId = category.Id });
            }
        }
        await db.SaveChangesAsync();

        if (await db.Products.AnyAsync()) return;
        var subCategories = await db.SubCategories.ToDictionaryAsync(s => $"{s.CategoryId}:{s.Name}", s => s.Id);
        int SubId(string category, string sub) => subCategories[$"{categories[category].Id}:{sub}"];
        var pizza = categories["Pizza"];
        var feteer = categories["Feteer"];
        var crepe = categories["Crepe"];
        var pasta = categories["Pasta"];

        db.Products.AddRange(
            new Product { Name = "Margherita Pizza", Description = "Tomato sauce, mozzarella and basil", Price = 150, CategoryId = pizza.Id, SubCategoryId = SubId("Pizza", "Cheese"), IsFeatured = true },
            new Product { Name = "Chicken Ranch Pizza", Description = "Chicken, ranch sauce and mozzarella", Price = 190, CategoryId = pizza.Id, SubCategoryId = SubId("Pizza", "Meat") },
            new Product { Name = "Mix Cheese Feteer", Description = "Egyptian feteer with mixed cheese", Price = 135, CategoryId = feteer.Id, SubCategoryId = SubId("Feteer", "Cheese"), IsFeatured = true },
            new Product { Name = "Sausage Feteer", Description = "Feteer with sausage and peppers", Price = 165, CategoryId = feteer.Id, SubCategoryId = SubId("Feteer", "Meat") },
            new Product { Name = "Chicken Crepe", Description = "Chicken, cheese and special sauce", Price = 125, CategoryId = crepe.Id, SubCategoryId = SubId("Crepe", "Meat"), IsFeatured = true },
            new Product { Name = "Mix Cheese Crepe", Description = "Crepe with mixed cheese", Price = 110, CategoryId = crepe.Id, SubCategoryId = SubId("Crepe", "Cheese") },
            new Product { Name = "Alfredo Pasta", Description = "Creamy Alfredo sauce and parmesan", Price = 145, CategoryId = pasta.Id, IsFeatured = true },
            new Product { Name = "Penne Arrabbiata", Description = "Penne with spicy tomato sauce", Price = 120, CategoryId = pasta.Id }
        );
        await db.SaveChangesAsync();
    }
}

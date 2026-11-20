using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;

namespace RestaurantApi.Controllers;

public record SaveCategoryRequest(string Name, string? Description, string? ImageUrl);

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(SaveCategoryRequest request)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Category name is required." });
        if (await db.Categories.AnyAsync(c => c.Name == name)) return Conflict(new { message = "Category already exists." });
        var category = new Category { Name = name, Description = request.Description, ImageUrl = request.ImageUrl };
        db.Categories.Add(category); await db.SaveChangesAsync();
        return Created("/api/categories", new { category.Id, category.Name });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveCategoryRequest request)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return NotFound();
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Category name is required." });
        if (await db.Categories.AnyAsync(c => c.Id != id && c.Name == name)) return Conflict(new { message = "Category already exists." });
        category.Name = name; category.Description = request.Description; category.ImageUrl = request.ImageUrl;
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return NotFound();
        if (await db.Products.AnyAsync(p => p.CategoryId == id)) return Conflict(new { message = "Move or delete this category's products first." });
        db.Categories.Remove(category); await db.SaveChangesAsync(); return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;

namespace RestaurantApi.Controllers;

public record SaveSubCategoryRequest(string Name, int CategoryId);

[ApiController]
[Route("api/admin/subcategories")]
[Authorize(Roles = "Admin")]
public class AdminSubCategoriesController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(SaveSubCategoryRequest request)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Subcategory name is required." });
        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId)) return BadRequest(new { message = "Category not found." });
        if (await db.SubCategories.AnyAsync(s => s.CategoryId == request.CategoryId && s.Name == name))
            return Conflict(new { message = "Subcategory already exists in this category." });

        var subCategory = new SubCategory { Name = name, CategoryId = request.CategoryId };
        db.SubCategories.Add(subCategory);
        await db.SaveChangesAsync();
        return Created("/api/categories", new { subCategory.Id, subCategory.Name, subCategory.CategoryId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveSubCategoryRequest request)
    {
        var subCategory = await db.SubCategories.FindAsync(id);
        if (subCategory is null) return NotFound();
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Subcategory name is required." });
        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId)) return BadRequest(new { message = "Category not found." });
        if (await db.SubCategories.AnyAsync(s => s.Id != id && s.CategoryId == request.CategoryId && s.Name == name))
            return Conflict(new { message = "Subcategory already exists in this category." });

        subCategory.Name = name;
        subCategory.CategoryId = request.CategoryId;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var subCategory = await db.SubCategories.FindAsync(id);
        if (subCategory is null) return NotFound();
        if (await db.Products.AnyAsync(p => p.SubCategoryId == id))
            return Conflict(new { message = "Remove this subcategory from its products first." });
        db.SubCategories.Remove(subCategory);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

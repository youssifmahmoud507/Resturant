using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.DTOs;
using RestaurantApi.Models;

namespace RestaurantApi.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var query = db.Products.AsNoTracking().Include(p => p.Category).Include(p => p.SubCategory).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        return Ok(await query.OrderByDescending(p => p.Id).Select(p => new
        {
            p.Id, p.Name, p.Description, p.Price, p.ImageUrl, p.IsAvailable, p.IsFeatured,
            p.CategoryId, CategoryName = p.Category!.Name, p.SubCategoryId,
            SubCategoryName = p.SubCategory == null ? null : p.SubCategory.Name
        }).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(SaveProductRequest request)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId)) return BadRequest(new { message = "Category not found." });
        if (request.SubCategoryId.HasValue && !await db.SubCategories.AnyAsync(s => s.Id == request.SubCategoryId.Value && s.CategoryId == request.CategoryId))
            return BadRequest(new { message = "Subcategory does not belong to the selected category." });
        var product = new Product { Name = request.Name.Trim(), Description = request.Description.Trim(), Price = request.Price,
            ImageUrl = request.ImageUrl, CategoryId = request.CategoryId, SubCategoryId = request.SubCategoryId, IsAvailable = request.IsAvailable, IsFeatured = request.IsFeatured };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return Created($"/api/products/{product.Id}", new { product.Id, product.Name });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveProductRequest request)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId)) return BadRequest(new { message = "Category not found." });
        if (request.SubCategoryId.HasValue && !await db.SubCategories.AnyAsync(s => s.Id == request.SubCategoryId.Value && s.CategoryId == request.CategoryId))
            return BadRequest(new { message = "Subcategory does not belong to the selected category." });
        product.Name = request.Name.Trim(); product.Description = request.Description.Trim(); product.Price = request.Price;
        product.ImageUrl = request.ImageUrl; product.CategoryId = request.CategoryId; product.SubCategoryId = request.SubCategoryId;
        product.IsAvailable = request.IsAvailable; product.IsFeatured = request.IsFeatured;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        // Keep products referenced by historical orders; hide them from the menu instead of deleting.
        if (await db.OrderItems.AnyAsync(i => i.ProductId == id)) product.IsAvailable = false;
        else db.Products.Remove(product);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

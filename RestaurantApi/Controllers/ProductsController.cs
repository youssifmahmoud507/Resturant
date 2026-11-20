using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;

namespace RestaurantApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? categoryId, [FromQuery] int? subCategoryId,
        [FromQuery] string? search, [FromQuery] bool? featured)
    {
        var query = db.Products.AsNoTracking().Include(p => p.Category).Include(p => p.SubCategory).Where(p => p.IsAvailable);
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (subCategoryId.HasValue) query = query.Where(p => p.SubCategoryId == subCategoryId.Value);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Name.Contains(search));
        if (featured == true) query = query.Where(p => p.IsFeatured);
        return Ok(await query.OrderBy(p => p.Name).Select(p => new
        {
            p.Id, p.Name, p.Description, p.Price, p.ImageUrl, p.IsAvailable, p.IsFeatured,
            p.CategoryId, CategoryName = p.Category!.Name, p.SubCategoryId,
            SubCategoryName = p.SubCategory == null ? null : p.SubCategory.Name
        }).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.SubCategory)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsAvailable);
        return p is null ? NotFound() : Ok(new
        {
            p.Id, p.Name, p.Description, p.Price, p.ImageUrl, p.IsAvailable, p.IsFeatured,
            p.CategoryId, CategoryName = p.Category!.Name, p.SubCategoryId,
            SubCategoryName = p.SubCategory == null ? null : p.SubCategory.Name
        });
    }
}

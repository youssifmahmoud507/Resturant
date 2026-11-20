using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;

namespace RestaurantApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await db.Categories.AsNoTracking()
        .OrderBy(c => c.Id)
        .Select(c => new
        {
            c.Id, c.Name, c.Description, c.ImageUrl,
            SubCategories = c.SubCategories.OrderBy(s => s.Id).Select(s => new { s.Id, s.Name })
        }).ToListAsync());
}

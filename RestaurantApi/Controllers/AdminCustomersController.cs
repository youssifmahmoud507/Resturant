using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;

namespace RestaurantApi.Controllers;

[ApiController]
[Route("api/admin/customers")]
[Authorize(Roles = "Admin")]
public class AdminCustomersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        var query = db.Customers.AsNoTracking().Where(c => c.Role == "Customer");
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(c => c.FullName.Contains(search) || c.Phone.Contains(search) || c.Email.Contains(search));
        return Ok(await query.OrderByDescending(c => c.CreatedAt).Select(c => new { c.Id, c.FullName, c.Phone, c.Email, c.IsActive, c.CreatedAt, OrdersCount = c.Orders.Count() }).ToListAsync());
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, CustomerStatusRequest request)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id && c.Role == "Customer");
        if (customer is null) return NotFound();
        customer.IsActive = request.IsActive;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await db.Customers.Include(c => c.Orders).FirstOrDefaultAsync(c => c.Id == id && c.Role == "Customer");
        if (customer is null) return NotFound();
        // Preserve order history: customers with orders are deactivated, not physically deleted.
        if (customer.Orders.Count > 0) customer.IsActive = false;
        else db.Customers.Remove(customer);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
public record CustomerStatusRequest(bool IsActive);

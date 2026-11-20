using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;

namespace RestaurantApi.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var today = DateTime.UtcNow.Date;
        return Ok(new {
            products = await db.Products.CountAsync(),
            availableProducts = await db.Products.CountAsync(p => p.IsAvailable),
            customers = await db.Customers.CountAsync(c => c.Role == "Customer"),
            activeCustomers = await db.Customers.CountAsync(c => c.Role == "Customer" && c.IsActive),
            orders = await db.Orders.CountAsync(),
            pendingOrders = await db.Orders.CountAsync(o => o.Status == "Pending"),
            todayOrders = await db.Orders.CountAsync(o => o.CreatedAt >= today),
            todayRevenue = await db.Orders.Where(o => o.CreatedAt >= today && o.Status != "Cancelled").SumAsync(o => (decimal?)o.Total) ?? 0m
        });
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.DTOs;
using RestaurantApi.Models;

namespace RestaurantApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request)
    {
        var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ids = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => ids.Contains(p.Id) && p.IsAvailable).ToDictionaryAsync(p => p.Id);
        if (products.Count != ids.Count) return BadRequest(new { message = "One or more products are unavailable." });
        var order = new Order { CustomerId = customerId, DeliveryAddress = request.DeliveryAddress.Trim(), Notes = request.Notes,
            PaymentMethod = request.PaymentMethod, Status = "Pending" };
        foreach (var item in request.Items)
        {
            var product = products[item.ProductId];
            order.Items.Add(new OrderItem { ProductId = product.Id, ProductName = product.Name, UnitPrice = product.Price, Quantity = item.Quantity });
        }
        order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return Created("/api/orders/my", new { order.Id, order.Total, order.Status, order.CreatedAt });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMine()
    {
        var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var orders = await db.Orders.AsNoTracking().Where(o => o.CustomerId == customerId).Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt).Select(o => new { o.Id, o.CreatedAt, o.Status, o.Total, o.DeliveryAddress, Items = o.Items.Select(i => new { i.ProductId, i.ProductName, i.UnitPrice, i.Quantity }) }).ToListAsync();
        return Ok(orders);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() => Ok(await db.Orders.AsNoTracking().Include(o => o.Customer).Include(o => o.Items)
        .OrderByDescending(o => o.CreatedAt).Select(o => new { o.Id, o.CreatedAt, o.Status, o.Total, CustomerName = o.Customer!.FullName, o.DeliveryAddress, Items = o.Items.Select(i => new { i.ProductName, i.UnitPrice, i.Quantity }) }).ToListAsync());

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var allowed = new[] { "Pending", "Confirmed", "Preparing", "OutForDelivery", "Delivered", "Cancelled" };
        if (!allowed.Contains(request.Status)) return BadRequest(new { message = "Invalid order status." });
        var order = await db.Orders.FindAsync(id);
        if (order is null) return NotFound();
        order.Status = request.Status;
        await db.SaveChangesAsync();
        return NoContent();
    }
}

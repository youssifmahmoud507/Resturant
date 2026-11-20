using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.DTOs;
using RestaurantApi.Models;
using RestaurantApi.Services;

namespace RestaurantApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, JwtTokenService tokens) : ControllerBase
{
    private readonly PasswordHasher<Customer> _hasher = new();

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Customers.AnyAsync(x => x.Email == email)) return Conflict(new { message = "Email is already registered." });
        var customer = new Customer { FullName = request.FullName.Trim(), Email = email, Phone = request.Phone.Trim() };
        customer.PasswordHash = _hasher.HashPassword(customer, request.Password);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return Ok(new AuthResponse(customer.Id, customer.FullName, customer.Email, customer.Role, tokens.CreateToken(customer)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var customer = await db.Customers.SingleOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant());
        if (customer is null || !customer.IsActive || _hasher.VerifyHashedPassword(customer, customer.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });
        return Ok(new AuthResponse(customer.Id, customer.FullName, customer.Email, customer.Role, tokens.CreateToken(customer)));
    }
}

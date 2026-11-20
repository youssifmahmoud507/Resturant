using System.ComponentModel.DataAnnotations;

namespace RestaurantApi.DTOs;

public record RegisterRequest([Required, MaxLength(100)] string FullName, [Required, EmailAddress] string Email,
    [Required, Phone] string Phone, [Required, MinLength(8)] string Password);
public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public record AuthResponse(int Id, string FullName, string Email, string Role, string Token);

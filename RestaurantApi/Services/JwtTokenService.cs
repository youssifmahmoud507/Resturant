using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RestaurantApi.Models;

namespace RestaurantApi.Services;

public class JwtTokenService(IConfiguration configuration)
{
    public string CreateToken(Customer customer)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new Claim(ClaimTypes.Name, customer.FullName),
            new Claim(ClaimTypes.Email, customer.Email),
            new Claim(ClaimTypes.Role, customer.Role)
        };
        var expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpiresInMinutes", 120));
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims,
            expires: expires, signingCredentials: credentials));
    }
}

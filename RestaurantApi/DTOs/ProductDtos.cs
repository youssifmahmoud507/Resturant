using System.ComponentModel.DataAnnotations;

namespace RestaurantApi.DTOs;

public record SaveProductRequest([Required, MaxLength(120)] string Name, [Required, MaxLength(1000)] string Description,
    [Range(0.01, 100000)] decimal Price, string? ImageUrl, [Range(1, int.MaxValue)] int CategoryId,
    int? SubCategoryId = null, bool IsAvailable = true, bool IsFeatured = false);

namespace RestaurantApi.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

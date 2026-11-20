namespace RestaurantApi.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending";
    public string PaymentMethod { get; set; } = "Cash";
    public decimal Total { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

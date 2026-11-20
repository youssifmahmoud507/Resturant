using System.ComponentModel.DataAnnotations;

namespace RestaurantApi.DTOs;

public record CreateOrderRequest([Required, MinLength(1)] List<CreateOrderItemRequest> Items,
    [Required, MinLength(5)] string DeliveryAddress, string? Notes, string PaymentMethod = "Cash");
public record CreateOrderItemRequest([Range(1, int.MaxValue)] int ProductId, [Range(1, 50)] int Quantity);
public record UpdateOrderStatusRequest([Required] string Status);

using OrderService.Models.Enums;

namespace OrderService.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public OrderType OrderType { get; set; }
    public OrderStatus Status { get; set; }
    public string? ShippingAddress { get; set; }
    public DateTime? RentalStartDate { get; set; }
    public DateTime? RentalEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserDto? User { get; set; }
    public PaymentDto? Payment { get; set; }
}

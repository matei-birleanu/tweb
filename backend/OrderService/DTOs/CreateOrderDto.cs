using System.ComponentModel.DataAnnotations;
using OrderService.Models.Enums;

namespace OrderService.DTOs;

public class CreateOrderDto
{
    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Product ID is required")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Total price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0")]
    public decimal TotalPrice { get; set; }

    [Required(ErrorMessage = "Order type is required")]
    public OrderType OrderType { get; set; }

    [MaxLength(500)]
    public string? ShippingAddress { get; set; }

    public DateTime? RentalStartDate { get; set; }

    public DateTime? RentalEndDate { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OrderService.Models.Enums;

namespace OrderService.Models;

[Table("orders")]
public class Order
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [Column("product_id")]
    public int ProductId { get; set; }

    [Required]
    [Column("quantity")]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Column("total_price")]
    public decimal TotalPrice { get; set; }

    [Required]
    [Column("order_type")]
    public OrderType OrderType { get; set; }

    [Required]
    [Column("status")]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [MaxLength(500)]
    [Column("shipping_address")]
    public string? ShippingAddress { get; set; }

    [Column("rental_start_date")]
    public DateTime? RentalStartDate { get; set; }

    [Column("rental_end_date")]
    public DateTime? RentalEndDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    public virtual Payment? Payment { get; set; }
}

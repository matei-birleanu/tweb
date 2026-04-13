using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class UpdateProductDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal? Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    public int? Stock { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(500)]
    [Url(ErrorMessage = "Invalid URL format")]
    public string? ImageUrl { get; set; }

    public bool? IsAvailable { get; set; }
}

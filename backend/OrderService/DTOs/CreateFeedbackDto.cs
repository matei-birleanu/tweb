using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs;

public class CreateFeedbackDto
{
    [Required(ErrorMessage = "Category is required")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Comment is required")]
    [MaxLength(2000)]
    public string Comment { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    public bool AgreedToTerms { get; set; }
}

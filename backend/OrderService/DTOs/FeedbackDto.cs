using OrderService.Models.Enums;

namespace OrderService.DTOs;

public class FeedbackDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public FeedbackCategory Category { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public bool IsResolved { get; set; }
    public string? AdminResponse { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserDto? User { get; set; }
}

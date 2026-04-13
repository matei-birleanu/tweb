using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OrderService.Models.Enums;

namespace OrderService.Models;

[Table("feedbacks")]
public class Feedback
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [Column("category")]
    public FeedbackCategory Category { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("subject")]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("rating")]
    [Range(1, 5)]
    public int? Rating { get; set; }

    [Column("is_resolved")]
    public bool IsResolved { get; set; } = false;

    [MaxLength(2000)]
    [Column("admin_response")]
    public string? AdminResponse { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models;

public class Notification
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [Required, StringLength(180)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Message { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Link { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

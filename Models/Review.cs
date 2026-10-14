using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models;

public class Review
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int? OrderId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required, StringLength(1500)]
    public string Comment { get; set; } = string.Empty;

    [StringLength(400)]
    public string? ImageUrl { get; set; }

    public bool IsApproved { get; set; } = true;
    public bool IsHidden { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

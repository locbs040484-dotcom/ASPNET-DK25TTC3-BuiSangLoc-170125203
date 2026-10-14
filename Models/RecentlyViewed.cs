using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models;

public class RecentlyViewed
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}

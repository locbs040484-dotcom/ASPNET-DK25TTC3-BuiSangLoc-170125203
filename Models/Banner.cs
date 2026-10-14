using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models;

public class Banner
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Subtitle { get; set; }

    [Required, StringLength(400)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Link { get; set; }

    [StringLength(80)]
    public string? ButtonText { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

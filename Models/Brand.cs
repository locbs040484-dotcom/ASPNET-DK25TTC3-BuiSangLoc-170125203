using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models;

public class Brand
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Logo { get; set; }

    [StringLength(800)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

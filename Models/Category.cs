using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(400)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? ImageUrl { get; set; }

    [StringLength(80)]
    public string? Icon { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

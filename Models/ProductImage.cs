using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required, StringLength(400)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }
}

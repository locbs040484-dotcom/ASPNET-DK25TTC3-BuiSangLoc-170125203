using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopStore.Models;

public class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required, StringLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [StringLength(400)]
    public string? ProductImage { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal Price { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal SubTotal { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LaptopStore.Models.Enums;

namespace LaptopStore.Models;

public class Voucher
{
    public int Id { get; set; }

    [Required, StringLength(40)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Description { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal DiscountValue { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? MaximumDiscount { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal MinimumOrderAmount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int Quantity { get; set; }
    public int UsedQuantity { get; set; }

    public bool IsActive { get; set; } = true;
}

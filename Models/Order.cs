using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LaptopStore.Models.Enums;

namespace LaptopStore.Models;

public class Order
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string OrderCode { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [Required, StringLength(120)]
    public string ReceiverName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(400)]
    public string Address { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,0)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal ShippingFee { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal TotalAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public OrderStatus OrderStatus { get; set; }

    [StringLength(40)]
    public string? VoucherCode { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [StringLength(500)]
    public string? StaffNote { get; set; }

    public bool StockDeducted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    public ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
}

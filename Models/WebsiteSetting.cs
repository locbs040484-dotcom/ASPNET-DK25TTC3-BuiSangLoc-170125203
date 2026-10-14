using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopStore.Models;

public class WebsiteSetting
{
    public int Id { get; set; }

    [StringLength(120)]
    public string StoreName { get; set; } = "LaptopZone";

    [StringLength(30)]
    public string Hotline { get; set; } = "1900 6868";

    [StringLength(150)]
    public string Email { get; set; } = "support@laptopzone.vn";

    [StringLength(250)]
    public string Address { get; set; } = "12 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh";

    [StringLength(80)]
    public string BankName { get; set; } = "Vietcombank";

    [StringLength(40)]
    public string BankAccountNumber { get; set; } = "0071000888888";

    [StringLength(120)]
    public string BankAccountName { get; set; } = "CONG TY LAPTOPZONE";

    [Column(TypeName = "decimal(18,0)")]
    public decimal DefaultShippingFee { get; set; } = 50000;

    [Column(TypeName = "decimal(18,0)")]
    public decimal FreeShippingThreshold { get; set; } = 20000000;

    [StringLength(400)]
    public string? LogoUrl { get; set; }

    [StringLength(400)]
    public string? QrImageUrl { get; set; }
}

public class NewsletterSubscriber
{
    public int Id { get; set; }

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class VoucherUsage
{
    public int Id { get; set; }
    public int VoucherId { get; set; }
    public Voucher Voucher { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
}

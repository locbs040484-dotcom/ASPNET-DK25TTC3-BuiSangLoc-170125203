using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopStore.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(40)]
    public string ProductCode { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(220)]
    public string Slug { get; set; } = string.Empty;

    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? CpuId { get; set; }
    public Cpu? Cpu { get; set; }

    public int? GpuId { get; set; }
    public Gpu? Gpu { get; set; }

    public int? RamOptionId { get; set; }
    public RamOption? RamOption { get; set; }

    public int? StorageOptionId { get; set; }
    public StorageOption? StorageOption { get; set; }

    public int? ScreenOptionId { get; set; }
    public ScreenOption? ScreenOption { get; set; }

    public int? ColorOptionId { get; set; }
    public ColorOption? ColorOption { get; set; }

    [StringLength(400)]
    public string ShortDescription { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,0)")]
    [Range(0, double.MaxValue)]
    public decimal OriginalPrice { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    [Range(0, double.MaxValue)]
    public decimal SalePrice { get; set; }

    [Range(0, 100)]
    public int DiscountPercent { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public int SoldQuantity { get; set; }

    [Range(0, 60)]
    public int WarrantyMonths { get; set; } = 24;

    [StringLength(40)]
    public string Status { get; set; } = "Active";

    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsActive { get; set; } = true;

    [StringLength(200)]
    public string? MetaTitle { get; set; }

    [StringLength(400)]
    public string? MetaDescription { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(120)]
    public string? CpuName { get; set; }

    [StringLength(40)]
    public string? RamType { get; set; }

    [StringLength(40)]
    public string? RamBus { get; set; }

    [StringLength(40)]
    public string? MaxRam { get; set; }

    [StringLength(80)]
    public string? StorageLabel { get; set; }

    [StringLength(40)]
    public string? StorageType { get; set; }

    [StringLength(80)]
    public string? ScreenLabel { get; set; }

    [StringLength(60)]
    public string? ScreenResolution { get; set; }

    public int? RefreshRate { get; set; }

    [StringLength(40)]
    public string? PanelType { get; set; }

    [StringLength(80)]
    public string? ColorGamut { get; set; }

    [StringLength(80)]
    public string? OperatingSystem { get; set; }

    [StringLength(80)]
    public string? Battery { get; set; }

    [StringLength(40)]
    public string? Weight { get; set; }

    [StringLength(80)]
    public string? Material { get; set; }

    [StringLength(120)]
    public string? Keyboard { get; set; }

    [StringLength(80)]
    public string? Webcam { get; set; }

    [StringLength(80)]
    public string? WiFi { get; set; }

    [StringLength(80)]
    public string? Bluetooth { get; set; }

    [StringLength(300)]
    public string? Ports { get; set; }

    [StringLength(60)]
    public string? Color { get; set; }

    [StringLength(80)]
    public string? Dimensions { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

    [NotMapped]
    public decimal DisplayPrice => SalePrice > 0 && SalePrice < OriginalPrice ? SalePrice : OriginalPrice;

    [NotMapped]
    public bool HasDiscount => SalePrice > 0 && SalePrice < OriginalPrice;

    [NotMapped]
    public int ComputedDiscount => HasDiscount && OriginalPrice > 0
        ? (int)Math.Round((1 - SalePrice / OriginalPrice) * 100)
        : 0;

    [NotMapped]
    public string? PrimaryImage => Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;
}

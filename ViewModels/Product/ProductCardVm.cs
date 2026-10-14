namespace LaptopStore.ViewModels.Product;

public class ProductCardVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = "/images/products/placeholder.jpg";
    public decimal OriginalPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int DiscountPercent { get; set; }
    public bool HasDiscount { get; set; }
    public string? CpuName { get; set; }
    public string? RamLabel { get; set; }
    public string? StorageLabel { get; set; }
    public string? GpuName { get; set; }
    public string? ScreenLabel { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsNew { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsBestSeller { get; set; }
    public int StockQuantity { get; set; }
}

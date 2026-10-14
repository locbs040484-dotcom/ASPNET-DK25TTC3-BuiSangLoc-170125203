using System.ComponentModel.DataAnnotations;
using LaptopStore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LaptopStore.ViewModels.Admin;

public class ProductFormVm
{
    public int Id { get; set; }

    [Required, StringLength(40)]
    public string ProductCode { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Slug { get; set; }

    [Required]
    public int BrandId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public int? CpuId { get; set; }
    public int? GpuId { get; set; }
    public int? RamOptionId { get; set; }
    public int? StorageOptionId { get; set; }
    public int? ScreenOptionId { get; set; }
    public int? ColorOptionId { get; set; }

    [Required, StringLength(400)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal OriginalPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal SalePrice { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Range(0, 60)]
    public int WarrantyMonths { get; set; } = 24;

    public string Status { get; set; } = "Active";
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsActive { get; set; } = true;

    public string? CpuName { get; set; }
    public string? RamType { get; set; }
    public string? RamBus { get; set; }
    public string? MaxRam { get; set; }
    public string? StorageLabel { get; set; }
    public string? StorageType { get; set; }
    public string? ScreenLabel { get; set; }
    public string? ScreenResolution { get; set; }
    public int? RefreshRate { get; set; }
    public string? PanelType { get; set; }
    public string? ColorGamut { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Battery { get; set; }
    public string? Weight { get; set; }
    public string? Material { get; set; }
    public string? Keyboard { get; set; }
    public string? Webcam { get; set; }
    public string? WiFi { get; set; }
    public string? Bluetooth { get; set; }
    public string? Ports { get; set; }
    public string? Color { get; set; }
    public string? Dimensions { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    public int? PrimaryImageId { get; set; }
    public List<ProductImage> ExistingImages { get; set; } = [];

    public IEnumerable<SelectListItem> Brands { get; set; } = [];
    public IEnumerable<SelectListItem> Categories { get; set; } = [];
    public IEnumerable<SelectListItem> Cpus { get; set; } = [];
    public IEnumerable<SelectListItem> Gpus { get; set; } = [];
    public IEnumerable<SelectListItem> Rams { get; set; } = [];
    public IEnumerable<SelectListItem> Storages { get; set; } = [];
    public IEnumerable<SelectListItem> Screens { get; set; } = [];
    public IEnumerable<SelectListItem> Colors { get; set; } = [];
}

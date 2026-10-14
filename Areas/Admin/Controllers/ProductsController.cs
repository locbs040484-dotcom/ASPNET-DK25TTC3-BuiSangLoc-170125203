using LaptopStore.Data;
using LaptopStore.Helpers;
using LaptopStore.Models;
using LaptopStore.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ApplicationDbContext db, IWebHostEnvironment env, ILogger<ProductsController> logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? q, string? brand, string? status, int page = 1)
    {
        var query = _db.Products.Include(p => p.Brand).Include(p => p.Images).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q) || p.ProductCode.Contains(q));
        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(p => p.Brand.Slug == brand);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.UpdatedAt).Skip((page - 1) * 12).Take(12).ToListAsync();
        ViewBag.Page = page;
        ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(total / 12.0));
        ViewBag.Q = q;
        ViewBag.Brands = await _db.Brands.ToListAsync();
        return View(items);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create() => View(await BuildForm(new ProductFormVm()));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormVm vm, List<IFormFile> images)
    {
        if (!ModelState.IsValid)
            return View(await BuildForm(vm));

        var product = MapToEntity(new Product(), vm);
        product.Slug = UniqueSlug(SlugHelper.Generate(vm.Name));
        product.DiscountPercent = product.ComputedDiscount;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        await SaveImages(product, images, null);
        _logger.LogInformation("Create Product {Code} {Name}", product.ProductCode, product.Name);
        TempData["Toast"] = "Đã tạo sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await _db.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        return View(await BuildForm(MapToVm(p)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormVm vm, List<IFormFile> images)
    {
        var p = await _db.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        if (!ModelState.IsValid)
            return View(await BuildForm(vm));

        MapToEntity(p, vm);
        p.Slug = string.IsNullOrWhiteSpace(vm.Slug) ? UniqueSlug(SlugHelper.Generate(vm.Name), p.Id) : vm.Slug;
        p.DiscountPercent = p.ComputedDiscount;
        p.UpdatedAt = DateTime.UtcNow;
        await SaveImages(p, images, vm.PrimaryImageId);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Update Product {Code}", p.ProductCode);
        TempData["Toast"] = "Đã cập nhật sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await _db.Products.Include(x => x.Brand).Include(x => x.Category).Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);
        return p == null ? NotFound() : View(p);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        p.IsActive = false;
        p.Status = "Hidden";
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Delete Product {Code}", p.ProductCode);
        TempData["Toast"] = "Đã ẩn sản phẩm (xóa mềm).";
        return RedirectToAction(nameof(Index));
    }

    private async Task SaveImages(Product product, List<IFormFile> files, int? primaryId)
    {
        foreach (var file in files)
        {
            var url = await FileUploadHelper.SaveAsync(file, _env.WebRootPath, "uploads/products");
            if (url == null) continue;
            product.Images.Add(new ProductImage
            {
                ImageUrl = url,
                DisplayOrder = product.Images.Count + 1,
                IsPrimary = false
            });
        }

        if (primaryId.HasValue)
        {
            foreach (var img in product.Images)
                img.IsPrimary = img.Id == primaryId;
        }
        else if (product.Images.Any() && product.Images.All(i => !i.IsPrimary))
        {
            product.Images.First().IsPrimary = true;
        }
        await _db.SaveChangesAsync();
    }

    private string UniqueSlug(string slug, int? ignoreId = null)
    {
        var baseSlug = slug;
        var i = 2;
        while (_db.Products.Any(p => p.Slug == slug && (!ignoreId.HasValue || p.Id != ignoreId)))
            slug = $"{baseSlug}-{i++}";
        return slug;
    }

    private async Task<ProductFormVm> BuildForm(ProductFormVm vm)
    {
        vm.Brands = await _db.Brands.Select(b => new SelectListItem(b.Name, b.Id.ToString())).ToListAsync();
        vm.Categories = await _db.Categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToListAsync();
        vm.Cpus = await _db.Cpus.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToListAsync();
        vm.Gpus = await _db.Gpus.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToListAsync();
        vm.Rams = await _db.RamOptions.Select(c => new SelectListItem($"{c.SizeGB}GB {c.Type}", c.Id.ToString())).ToListAsync();
        vm.Storages = await _db.StorageOptions.Select(c => new SelectListItem($"{c.SizeGB}GB {c.Type}", c.Id.ToString())).ToListAsync();
        vm.Screens = await _db.ScreenOptions.Select(c => new SelectListItem($"{c.SizeInch}\" {c.RefreshRate}Hz", c.Id.ToString())).ToListAsync();
        vm.Colors = await _db.ColorOptions.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToListAsync();
        return vm;
    }

    private static ProductFormVm MapToVm(Product p) => new()
    {
        Id = p.Id,
        ProductCode = p.ProductCode,
        Name = p.Name,
        Slug = p.Slug,
        BrandId = p.BrandId,
        CategoryId = p.CategoryId,
        CpuId = p.CpuId,
        GpuId = p.GpuId,
        RamOptionId = p.RamOptionId,
        StorageOptionId = p.StorageOptionId,
        ScreenOptionId = p.ScreenOptionId,
        ColorOptionId = p.ColorOptionId,
        ShortDescription = p.ShortDescription,
        Description = p.Description,
        OriginalPrice = p.OriginalPrice,
        SalePrice = p.SalePrice,
        StockQuantity = p.StockQuantity,
        WarrantyMonths = p.WarrantyMonths,
        Status = p.Status,
        IsFeatured = p.IsFeatured,
        IsNew = p.IsNew,
        IsBestSeller = p.IsBestSeller,
        IsActive = p.IsActive,
        CpuName = p.CpuName,
        RamType = p.RamType,
        RamBus = p.RamBus,
        MaxRam = p.MaxRam,
        StorageLabel = p.StorageLabel,
        StorageType = p.StorageType,
        ScreenLabel = p.ScreenLabel,
        ScreenResolution = p.ScreenResolution,
        RefreshRate = p.RefreshRate,
        PanelType = p.PanelType,
        ColorGamut = p.ColorGamut,
        OperatingSystem = p.OperatingSystem,
        Battery = p.Battery,
        Weight = p.Weight,
        Material = p.Material,
        Keyboard = p.Keyboard,
        Webcam = p.Webcam,
        WiFi = p.WiFi,
        Bluetooth = p.Bluetooth,
        Ports = p.Ports,
        Color = p.Color,
        Dimensions = p.Dimensions,
        MetaTitle = p.MetaTitle,
        MetaDescription = p.MetaDescription,
        ExistingImages = p.Images.ToList(),
        PrimaryImageId = p.Images.FirstOrDefault(i => i.IsPrimary)?.Id
    };

    private static Product MapToEntity(Product p, ProductFormVm vm)
    {
        p.ProductCode = vm.ProductCode;
        p.Name = vm.Name;
        p.BrandId = vm.BrandId;
        p.CategoryId = vm.CategoryId;
        p.CpuId = vm.CpuId;
        p.GpuId = vm.GpuId;
        p.RamOptionId = vm.RamOptionId;
        p.StorageOptionId = vm.StorageOptionId;
        p.ScreenOptionId = vm.ScreenOptionId;
        p.ColorOptionId = vm.ColorOptionId;
        p.ShortDescription = vm.ShortDescription;
        p.Description = vm.Description;
        p.OriginalPrice = vm.OriginalPrice;
        p.SalePrice = vm.SalePrice;
        p.StockQuantity = vm.StockQuantity;
        p.WarrantyMonths = vm.WarrantyMonths;
        p.Status = vm.Status;
        p.IsFeatured = vm.IsFeatured;
        p.IsNew = vm.IsNew;
        p.IsBestSeller = vm.IsBestSeller;
        p.IsActive = vm.IsActive;
        p.CpuName = vm.CpuName;
        p.RamType = vm.RamType;
        p.RamBus = vm.RamBus;
        p.MaxRam = vm.MaxRam;
        p.StorageLabel = vm.StorageLabel;
        p.StorageType = vm.StorageType;
        p.ScreenLabel = vm.ScreenLabel;
        p.ScreenResolution = vm.ScreenResolution;
        p.RefreshRate = vm.RefreshRate;
        p.PanelType = vm.PanelType;
        p.ColorGamut = vm.ColorGamut;
        p.OperatingSystem = vm.OperatingSystem;
        p.Battery = vm.Battery;
        p.Weight = vm.Weight;
        p.Material = vm.Material;
        p.Keyboard = vm.Keyboard;
        p.Webcam = vm.Webcam;
        p.WiFi = vm.WiFi;
        p.Bluetooth = vm.Bluetooth;
        p.Ports = vm.Ports;
        p.Color = vm.Color;
        p.Dimensions = vm.Dimensions;
        p.MetaTitle = vm.MetaTitle ?? vm.Name;
        p.MetaDescription = vm.MetaDescription ?? vm.ShortDescription;
        return p;
    }
}

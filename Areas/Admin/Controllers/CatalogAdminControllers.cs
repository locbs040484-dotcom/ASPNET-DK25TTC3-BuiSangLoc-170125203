using LaptopStore.Data;
using LaptopStore.Helpers;
using LaptopStore.Models;
using LaptopStore.Models.Enums;
using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _db;
    public CategoriesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() => View(await _db.Categories.OrderBy(c => c.DisplayOrder).ToListAsync());

    public IActionResult Create() => View(new Category());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        model.Slug = SlugHelper.Generate(model.Name);
        _db.Categories.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id) => View(await _db.Categories.FindAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category model)
    {
        model.Slug = string.IsNullOrWhiteSpace(model.Slug) ? SlugHelper.Generate(model.Name) : model.Slug;
        _db.Categories.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BrandsController : Controller
{
    private readonly ApplicationDbContext _db;
    public BrandsController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index() => View(await _db.Brands.OrderBy(b => b.Name).ToListAsync());
    public IActionResult Create() => View(new Brand());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Brand model)
    {
        model.Slug = SlugHelper.Generate(model.Name);
        _db.Brands.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Edit(int id) => View(await _db.Brands.FindAsync(id));
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Brand model)
    {
        _db.Brands.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _db;
    public InventoryController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? filter)
    {
        var query = _db.Products.Include(p => p.Brand).Include(p => p.Images).Where(p => p.IsActive);
        query = filter switch
        {
            "out" => query.Where(p => p.StockQuantity <= 0),
            "low" => query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= 5),
            _ => query
        };
        ViewBag.Filter = filter;
        return View(await query.OrderBy(p => p.StockQuantity).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(int id, int stock)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        p.StockQuantity = Math.Max(0, stock);
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Toast"] = "Đã cập nhật tồn kho.";
        return RedirectToAction(nameof(Index));
    }
}

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class VouchersController : Controller
{
    private readonly ApplicationDbContext _db;
    public VouchersController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index() => View(await _db.Vouchers.OrderByDescending(v => v.Id).ToListAsync());
    public IActionResult Create() => View(new Voucher { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3), Quantity = 100, IsActive = true });
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Voucher model)
    {
        model.Code = model.Code.Trim().ToUpperInvariant();
        _db.Vouchers.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Edit(int id) => View(await _db.Vouchers.FindAsync(id));
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Voucher model)
    {
        model.Code = model.Code.Trim().ToUpperInvariant();
        _db.Vouchers.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IReviewService _reviews;
    public ReviewsController(ApplicationDbContext db, IReviewService reviews)
    {
        _db = db;
        _reviews = reviews;
    }

    public async Task<IActionResult> Index()
        => View(await _db.Reviews.Include(r => r.Product).Include(r => r.User).OrderByDescending(r => r.CreatedAt).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Hide(int id, bool hide)
    {
        var result = await _reviews.ModerateAsync(id, hide);
        TempData["Toast"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BannersController : Controller
{
    private readonly ApplicationDbContext _db;
    public BannersController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index() => View(await _db.Banners.OrderBy(b => b.DisplayOrder).ToListAsync());
    public IActionResult Create() => View(new Banner());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Banner model)
    {
        _db.Banners.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Edit(int id) => View(await _db.Banners.FindAsync(id));
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Banner model)
    {
        _db.Banners.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BlogAdminController : Controller
{
    private readonly ApplicationDbContext _db;
    public BlogAdminController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.BlogPosts.Include(p => p.BlogCategory).OrderByDescending(p => p.PublishedAt).ToListAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _db.BlogCategories.ToListAsync();
        return View(new BlogPost { PublishedAt = DateTime.UtcNow, IsPublished = true, Author = "LaptopZone" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogPost model)
    {
        model.Slug = SlugHelper.Generate(model.Title);
        _db.BlogPosts.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.Categories = await _db.BlogCategories.ToListAsync();
        return View(await _db.BlogPosts.FindAsync(id));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BlogPost model)
    {
        model.Slug = string.IsNullOrWhiteSpace(model.Slug) ? SlugHelper.Generate(model.Title) : model.Slug;
        _db.BlogPosts.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class ContactsController : Controller
{
    private readonly ApplicationDbContext _db;
    public ContactsController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index() => View(await _db.ContactMessages.OrderByDescending(c => c.CreatedAt).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, ContactStatus status, string? adminNote)
    {
        var item = await _db.ContactMessages.FindAsync(id);
        if (item == null) return NotFound();
        item.Status = status;
        item.AdminNote = adminNote;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _db;
    public SettingsController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index()
        => View(await _db.WebsiteSettings.FirstOrDefaultAsync() ?? new WebsiteSetting());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(WebsiteSetting model)
    {
        var current = await _db.WebsiteSettings.FirstOrDefaultAsync();
        if (current == null) _db.WebsiteSettings.Add(model);
        else
        {
            current.StoreName = model.StoreName;
            current.Hotline = model.Hotline;
            current.Email = model.Email;
            current.Address = model.Address;
            current.BankName = model.BankName;
            current.BankAccountNumber = model.BankAccountNumber;
            current.BankAccountName = model.BankAccountName;
            current.DefaultShippingFee = model.DefaultShippingFee;
            current.FreeShippingThreshold = model.FreeShippingThreshold;
        }
        await _db.SaveChangesAsync();
        TempData["Toast"] = "Đã lưu cấu hình.";
        return RedirectToAction(nameof(Index));
    }
}

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SpecsController : Controller
{
    private readonly ApplicationDbContext _db;
    public SpecsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.Cpus = await _db.Cpus.ToListAsync();
        ViewBag.Gpus = await _db.Gpus.ToListAsync();
        ViewBag.Rams = await _db.RamOptions.ToListAsync();
        ViewBag.Storages = await _db.StorageOptions.ToListAsync();
        ViewBag.Screens = await _db.ScreenOptions.ToListAsync();
        ViewBag.Colors = await _db.ColorOptions.ToListAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCpu(string name, string brand, string filterKey)
    {
        _db.Cpus.Add(new Cpu { Name = name, Brand = brand, FilterKey = filterKey });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGpu(string name, string type, string filterKey)
    {
        _db.Gpus.Add(new Gpu { Name = name, Type = type, FilterKey = filterKey });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

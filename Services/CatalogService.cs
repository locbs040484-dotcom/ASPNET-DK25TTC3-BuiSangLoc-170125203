using LaptopStore.Data;
using LaptopStore.Models;
using LaptopStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Services;

public class CatalogService : ICatalogService
{
    private readonly ApplicationDbContext _db;

    public CatalogService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<List<Category>> GetCategoriesAsync()
        => _db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();

    public Task<List<Brand>> GetBrandsAsync()
        => _db.Brands.AsNoTracking().Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();

    public async Task<WebsiteSetting> GetSettingsAsync()
        => await _db.WebsiteSettings.AsNoTracking().FirstOrDefaultAsync() ?? new WebsiteSetting();

    public Task<List<Banner>> GetBannersAsync()
        => _db.Banners.AsNoTracking().Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToListAsync();
}

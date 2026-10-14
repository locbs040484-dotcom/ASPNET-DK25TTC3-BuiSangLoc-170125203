using LaptopStore.Data;
using LaptopStore.Models;
using LaptopStore.Services.Interfaces;
using LaptopStore.ViewModels.Product;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Services;

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _db;
    private readonly IProductService _products;

    public WishlistService(ApplicationDbContext db, IProductService products)
    {
        _db = db;
        _products = products;
    }

    public async Task<(bool Ok, string Message, bool Added)> ToggleAsync(string userId, int productId)
    {
        var exists = await _db.Products.AnyAsync(p => p.Id == productId && p.IsActive);
        if (!exists)
            return (false, "Sản phẩm không tồn tại.", false);

        var item = await _db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        if (item != null)
        {
            _db.WishlistItems.Remove(item);
            await _db.SaveChangesAsync();
            return (true, "Đã xóa khỏi danh sách yêu thích.", false);
        }

        _db.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId });
        await _db.SaveChangesAsync();
        return (true, "Đã thêm vào danh sách yêu thích.", true);
    }

    public async Task<List<ProductCardVm>> GetAsync(string userId)
    {
        var products = await _db.WishlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Product).ThenInclude(p => p.Brand)
            .Include(w => w.Product).ThenInclude(p => p.Images)
            .Include(w => w.Product).ThenInclude(p => p.Cpu)
            .Include(w => w.Product).ThenInclude(p => p.Gpu)
            .Include(w => w.Product).ThenInclude(p => p.RamOption)
            .Include(w => w.Product).ThenInclude(p => p.StorageOption)
            .Include(w => w.Product).ThenInclude(p => p.ScreenOption)
            .Include(w => w.Product).ThenInclude(p => p.Reviews)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => w.Product)
            .ToListAsync();
        return products.Select(_products.MapCard).ToList();
    }

    public Task<int> CountAsync(string userId)
        => _db.WishlistItems.CountAsync(w => w.UserId == userId);

    public async Task<bool> RemoveAsync(string userId, int productId)
    {
        var item = await _db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        if (item == null) return false;
        _db.WishlistItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

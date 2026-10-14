using LaptopStore.Data;
using LaptopStore.Models;
using LaptopStore.Services.Interfaces;
using LaptopStore.ViewModels.Cart;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _db;
    private readonly IVoucherService _vouchers;
    private readonly ICatalogService _catalog;

    public CartService(ApplicationDbContext db, IVoucherService vouchers, ICatalogService catalog)
    {
        _db = db;
        _vouchers = vouchers;
        _catalog = catalog;
    }

    public async Task<CartVm> GetCartAsync(string userId, string? voucherCode = null)
    {
        var items = await _db.CartItems
            .Include(c => c.Product).ThenInclude(p => p.Images)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

        var vm = new CartVm
        {
            Items = items.Select(c => new CartItemVm
            {
                Id = c.Id,
                ProductId = c.ProductId,
                Name = c.Product.Name,
                Slug = c.Product.Slug,
                ImageUrl = c.Product.PrimaryImage ?? "/images/products/placeholder.jpg",
                Price = c.Product.DisplayPrice,
                Quantity = Math.Min(c.Quantity, Math.Max(c.Product.StockQuantity, 1)),
                StockQuantity = c.Product.StockQuantity
            }).ToList()
        };

        vm.SubTotal = vm.Items.Sum(i => i.SubTotal);
        var setting = await _catalog.GetSettingsAsync();
        vm.ShippingFee = vm.SubTotal >= setting.FreeShippingThreshold ? 0 : setting.DefaultShippingFee;

        if (!string.IsNullOrWhiteSpace(voucherCode))
        {
            var applied = await _vouchers.ApplyAsync(voucherCode, vm.SubTotal, vm.ShippingFee);
            vm.VoucherCode = voucherCode.Trim().ToUpperInvariant();
            vm.VoucherMessage = applied.Message;
            vm.VoucherValid = applied.Ok;
            if (applied.Ok)
            {
                vm.Discount = applied.Discount;
                vm.ShippingFee = applied.ShippingFee;
            }
        }

        vm.Total = Math.Max(0, vm.SubTotal - vm.Discount + vm.ShippingFee);
        return vm;
    }

    public async Task<(bool Ok, string Message, int Count)> AddAsync(string userId, int productId, int quantity)
    {
        quantity = Math.Max(1, quantity);
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
        if (product == null)
            return (false, "Sản phẩm không tồn tại.", 0);
        if (product.StockQuantity <= 0)
            return (false, "Sản phẩm đã hết hàng.", 0);

        var item = await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        var nextQty = (item?.Quantity ?? 0) + quantity;
        if (nextQty > product.StockQuantity)
            return (false, $"Chỉ còn {product.StockQuantity} sản phẩm trong kho.", 0);

        if (item == null)
        {
            _db.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = productId,
                Quantity = quantity
            });
        }
        else
        {
            item.Quantity = nextQty;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        var count = await CountAsync(userId);
        return (true, "Đã thêm vào giỏ hàng.", count);
    }

    public async Task<(bool Ok, string Message)> UpdateAsync(string userId, int itemId, int quantity)
    {
        var item = await _db.CartItems.Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId);
        if (item == null)
            return (false, "Không tìm thấy sản phẩm trong giỏ.");

        if (quantity <= 0)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            return (true, "Đã xóa sản phẩm khỏi giỏ.");
        }

        if (quantity > item.Product.StockQuantity)
            return (false, $"Chỉ còn {item.Product.StockQuantity} sản phẩm trong kho.");

        item.Quantity = quantity;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Đã cập nhật giỏ hàng.");
    }

    public async Task<(bool Ok, string Message)> RemoveAsync(string userId, int itemId)
    {
        var item = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId);
        if (item == null)
            return (false, "Không tìm thấy sản phẩm trong giỏ.");
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return (true, "Đã xóa sản phẩm khỏi giỏ.");
    }

    public async Task ClearAsync(string userId)
    {
        var items = await _db.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    public async Task<int> CountAsync(string userId)
        => await _db.CartItems.Where(c => c.UserId == userId).SumAsync(c => (int?)c.Quantity) ?? 0;
}

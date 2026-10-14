using LaptopStore.Data;
using LaptopStore.Models;
using LaptopStore.Services.Interfaces;
using LaptopStore.ViewModels.Product;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;

    public ProductService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ProductListVm> SearchAsync(ProductFilterVm filter)
    {
        filter.Page = Math.Max(1, filter.Page);
        filter.PageSize = filter.PageSize <= 0 ? 12 : filter.PageSize;

        var query = BuildQuery(filter);

        query = filter.Sort switch
        {
            "bestseller" => query.OrderByDescending(p => p.SoldQuantity),
            "price-asc" => query.OrderBy(p => p.SalePrice > 0 ? p.SalePrice : p.OriginalPrice),
            "price-desc" => query.OrderByDescending(p => p.SalePrice > 0 ? p.SalePrice : p.OriginalPrice),
            "discount" => query.OrderByDescending(p => p.DiscountPercent),
            "rating" => query.OrderByDescending(p => p.Reviews.Where(r => r.IsApproved && !r.IsHidden).Average(r => (double?)r.Rating) ?? 0),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new ProductListVm
        {
            Filter = filter,
            Products = items.Select(MapCard).ToList(),
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)filter.PageSize)
        };
    }

    public async Task<ProductDetailVm?> GetBySlugAsync(string slug, string? userId)
    {
        var product = await _db.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Cpu)
            .Include(p => p.Gpu)
            .Include(p => p.RamOption)
            .Include(p => p.StorageOption)
            .Include(p => p.ScreenOption)
            .Include(p => p.ColorOption)
            .Include(p => p.Images)
            .Include(p => p.Reviews.Where(r => r.IsApproved && !r.IsHidden))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

        if (product == null)
            return null;

        var reviews = product.Reviews.OrderByDescending(r => r.CreatedAt).ToList();
        var breakdown = Enumerable.Range(1, 5).ToDictionary(i => i, i => reviews.Count(r => r.Rating == i));

        var canReview = false;
        var hasReviewed = false;
        var inWishlist = false;

        if (!string.IsNullOrEmpty(userId))
        {
            hasReviewed = await _db.Reviews.AnyAsync(r => r.ProductId == product.Id && r.UserId == userId);
            canReview = !hasReviewed && await _db.OrderDetails
                .AnyAsync(d => d.ProductId == product.Id
                    && d.Order.UserId == userId
                    && d.Order.OrderStatus == Models.Enums.OrderStatus.Completed);
            inWishlist = await _db.WishlistItems.AnyAsync(w => w.UserId == userId && w.ProductId == product.Id);
            await TrackViewAsync(product.Id, userId);
        }

        return new ProductDetailVm
        {
            Product = product,
            Images = product.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).ToList(),
            AverageRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 1),
            ReviewCount = reviews.Count,
            RatingBreakdown = breakdown,
            Reviews = reviews,
            Related = await GetRelatedAsync(product),
            CanReview = canReview,
            HasReviewed = hasReviewed,
            InWishlist = inWishlist
        };
    }

    public Task<List<ProductCardVm>> GetFeaturedAsync(int take = 8)
        => QueryCards(q => q.Where(p => p.IsFeatured).OrderByDescending(p => p.SoldQuantity), take);

    public Task<List<ProductCardVm>> GetOnSaleAsync(int take = 8)
        => QueryCards(q => q.Where(p => p.SalePrice > 0 && p.SalePrice < p.OriginalPrice).OrderByDescending(p => p.DiscountPercent), take);

    public Task<List<ProductCardVm>> GetByCategorySlugAsync(string slug, int take = 8)
        => QueryCards(q => q.Where(p => p.Category.Slug == slug).OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.SoldQuantity), take);

    public Task<List<ProductCardVm>> GetBestSellersAsync(int take = 8)
        => QueryCards(q => q.OrderByDescending(p => p.SoldQuantity), take);

    public Task<List<ProductCardVm>> GetNewAsync(int take = 8)
        => QueryCards(q => q.Where(p => p.IsNew).OrderByDescending(p => p.CreatedAt), take);

    public async Task<List<ProductCardVm>> SuggestAsync(string q, int take = 8)
    {
        if (string.IsNullOrWhiteSpace(q))
            return [];

        q = q.Trim();
        var items = await BaseQuery()
            .Where(p => p.Name.Contains(q)
                || p.Brand.Name.Contains(q)
                || (p.CpuName != null && p.CpuName.Contains(q))
                || (p.Gpu != null && p.Gpu.Name.Contains(q)))
            .OrderByDescending(p => p.SoldQuantity)
            .Take(take)
            .ToListAsync();
        return items.Select(MapCard).ToList();
    }

    public async Task<List<ProductCardVm>> GetRelatedAsync(Product product, int take = 8)
    {
        var min = product.DisplayPrice * 0.7m;
        var max = product.DisplayPrice * 1.3m;
        var items = await BaseQuery()
            .Where(p => p.Id != product.Id && (
                p.BrandId == product.BrandId
                || p.CategoryId == product.CategoryId
                || ((p.SalePrice > 0 ? p.SalePrice : p.OriginalPrice) >= min
                    && (p.SalePrice > 0 ? p.SalePrice : p.OriginalPrice) <= max)))
            .OrderByDescending(p => p.BrandId == product.BrandId)
            .ThenByDescending(p => p.CategoryId == product.CategoryId)
            .ThenByDescending(p => p.SoldQuantity)
            .Take(take)
            .ToListAsync();
        return items.Select(MapCard).ToList();
    }

    public async Task TrackViewAsync(int productId, string userId)
    {
        var existing = await _db.RecentlyViewed.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
        if (existing != null)
        {
            existing.ViewedAt = DateTime.UtcNow;
        }
        else
        {
            _db.RecentlyViewed.Add(new RecentlyViewed { UserId = userId, ProductId = productId });
            var extras = await _db.RecentlyViewed
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.ViewedAt)
                .Skip(20)
                .ToListAsync();
            if (extras.Count > 0)
                _db.RecentlyViewed.RemoveRange(extras);
        }
        await _db.SaveChangesAsync();
    }

    public async Task<List<ProductCardVm>> GetRecentlyViewedAsync(string userId, int take = 8)
    {
        var ids = await _db.RecentlyViewed
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ViewedAt)
            .Select(x => x.ProductId)
            .Take(take)
            .ToListAsync();
        if (ids.Count == 0) return [];

        var products = await BaseQuery().Where(p => ids.Contains(p.Id)).ToListAsync();
        return ids.Select(id => products.FirstOrDefault(p => p.Id == id)).Where(p => p != null).Select(p => MapCard(p!)).ToList();
    }

    public ProductCardVm MapCard(Product p)
    {
        var reviews = p.Reviews?.Where(r => r.IsApproved && !r.IsHidden).ToList() ?? [];
        var sale = p.SalePrice > 0 && p.SalePrice < p.OriginalPrice ? p.SalePrice : p.OriginalPrice;
        var discount = p.OriginalPrice > 0 && sale < p.OriginalPrice
            ? (int)Math.Round((1 - sale / p.OriginalPrice) * 100)
            : 0;

        return new ProductCardVm
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            BrandName = p.Brand?.Name ?? "",
            ImageUrl = p.Images?.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl
                       ?? "/images/products/placeholder.jpg",
            OriginalPrice = p.OriginalPrice,
            SalePrice = sale,
            DiscountPercent = discount,
            HasDiscount = discount > 0,
            CpuName = p.CpuName ?? p.Cpu?.Name,
            RamLabel = p.RamOption != null ? $"{p.RamOption.SizeGB}GB" : null,
            StorageLabel = p.StorageLabel ?? (p.StorageOption != null ? $"{p.StorageOption.SizeGB}GB" : null),
            GpuName = p.Gpu?.Name,
            ScreenLabel = p.ScreenLabel ?? (p.ScreenOption != null ? $"{p.ScreenOption.SizeInch}\"" : null),
            AverageRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 1),
            ReviewCount = reviews.Count,
            IsNew = p.IsNew,
            IsFeatured = p.IsFeatured,
            IsBestSeller = p.IsBestSeller,
            StockQuantity = p.StockQuantity
        };
    }

    private IQueryable<Product> BaseQuery()
        => _db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.Status == "Active")
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Cpu)
            .Include(p => p.Gpu)
            .Include(p => p.RamOption)
            .Include(p => p.StorageOption)
            .Include(p => p.ScreenOption)
            .Include(p => p.Images)
            .Include(p => p.Reviews);

    private IQueryable<Product> BuildQuery(ProductFilterVm filter)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var q = filter.Q.Trim();
            query = query.Where(p => p.Name.Contains(q)
                || p.Brand.Name.Contains(q)
                || (p.CpuName != null && p.CpuName.Contains(q))
                || (p.Gpu != null && p.Gpu.Name.Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(p => p.Category.Slug == filter.Category);

        if (filter.Brands.Count > 0)
            query = query.Where(p => filter.Brands.Contains(p.Brand.Slug));

        if (filter.Cpus.Count > 0)
            query = query.Where(p => p.Cpu != null && filter.Cpus.Contains(p.Cpu.FilterKey!));

        if (filter.Rams.Count > 0)
        {
            var ramSizes = filter.Rams.Select(r => int.TryParse(r, out var n) ? n : 0).Where(n => n > 0).ToList();
            query = query.Where(p => p.RamOption != null && ramSizes.Contains(p.RamOption.SizeGB));
        }

        if (filter.Storages.Count > 0)
        {
            var sizes = filter.Storages.Select(s => int.TryParse(s, out var n) ? n : 0).Where(n => n > 0).ToList();
            query = query.Where(p => p.StorageOption != null && sizes.Contains(p.StorageOption.SizeGB));
        }

        if (filter.Gpus.Count > 0)
            query = query.Where(p => p.Gpu != null && filter.Gpus.Contains(p.Gpu.FilterKey!));

        if (filter.Screens.Count > 0)
        {
            var screens = filter.Screens.Select(s => decimal.TryParse(s, out var n) ? n : 0).Where(n => n > 0).ToList();
            query = query.Where(p => p.ScreenOption != null && screens.Contains(p.ScreenOption.SizeInch));
        }

        if (filter.RefreshRates.Count > 0)
        {
            var rates = filter.RefreshRates.Select(s => int.TryParse(s, out var n) ? n : 0).Where(n => n > 0).ToList();
            query = query.Where(p => p.RefreshRate != null && rates.Contains(p.RefreshRate.Value));
        }

        if (filter.PriceRanges.Count > 0)
        {
            query = query.Where(p =>
                (filter.PriceRanges.Contains("under-10") && (p.SalePrice > 0 && p.SalePrice < p.OriginalPrice ? p.SalePrice : p.OriginalPrice) < 10000000)
                || (filter.PriceRanges.Contains("10-15") && (p.SalePrice > 0 && p.SalePrice < p.OriginalPrice ? p.SalePrice : p.OriginalPrice) >= 10000000
                    && (p.SalePrice > 0 && p.SalePrice < p.OriginalPrice ? p.SalePrice : p.OriginalPrice) < 15000000)
                || (filter.PriceRanges.Contains("15-20") && (p.SalePrice > 0 && p.SalePrice < p.OriginalPrice ? p.SalePrice : p.OriginalPrice) >= 15000000
                    && (p.SalePrice > 0 && p.SalePrice < p.OriginalPrice ? p.SalePrice : p.OriginalPrice) < 20000000)
                || (filter.PriceRanges.Contains("20-30") && (p.SalePrice > 0 && p.SalePrice < p.OriginalPrice ? p.SalePrice : p.OriginalPrice) >= 20000000
                    && (p.SalePrice > 0 && p.SalePrice < p.OriginalPrice ? p.SalePrice : p.OriginalPrice) < 30000000)
                || (filter.PriceRanges.Contains("30-40") && (p.SalePrice > 0 && p.SalePrice < p.OriginalPrice ? p.SalePrice : p.OriginalPrice) >= 30000000
                    && (p.SalePrice > 0 && p.SalePrice < p.OriginalPrice ? p.SalePrice : p.OriginalPrice) < 40000000)
                || (filter.PriceRanges.Contains("over-40") && (p.SalePrice > 0 && p.SalePrice < p.OriginalPrice ? p.SalePrice : p.OriginalPrice) >= 40000000));
        }

        return query;
    }

    private async Task<List<ProductCardVm>> QueryCards(Func<IQueryable<Product>, IQueryable<Product>> apply, int take)
    {
        var items = await apply(BaseQuery()).Take(take).ToListAsync();
        return items.Select(MapCard).ToList();
    }
}

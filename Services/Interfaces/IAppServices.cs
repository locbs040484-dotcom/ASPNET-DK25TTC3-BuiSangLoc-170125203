using LaptopStore.Models;
using LaptopStore.Models.Enums;
using LaptopStore.ViewModels.Admin;
using LaptopStore.ViewModels.Cart;
using LaptopStore.ViewModels.Product;

namespace LaptopStore.Services.Interfaces;

public interface IProductService
{
    Task<ProductListVm> SearchAsync(ProductFilterVm filter);
    Task<ProductDetailVm?> GetBySlugAsync(string slug, string? userId);
    Task<List<ProductCardVm>> GetFeaturedAsync(int take = 8);
    Task<List<ProductCardVm>> GetOnSaleAsync(int take = 8);
    Task<List<ProductCardVm>> GetByCategorySlugAsync(string slug, int take = 8);
    Task<List<ProductCardVm>> GetBestSellersAsync(int take = 8);
    Task<List<ProductCardVm>> GetNewAsync(int take = 8);
    Task<List<ProductCardVm>> SuggestAsync(string q, int take = 8);
    Task<List<ProductCardVm>> GetRelatedAsync(Product product, int take = 8);
    Task TrackViewAsync(int productId, string userId);
    Task<List<ProductCardVm>> GetRecentlyViewedAsync(string userId, int take = 8);
    ProductCardVm MapCard(Product p);
}

public interface ICartService
{
    Task<CartVm> GetCartAsync(string userId, string? voucherCode = null);
    Task<(bool Ok, string Message, int Count)> AddAsync(string userId, int productId, int quantity);
    Task<(bool Ok, string Message)> UpdateAsync(string userId, int itemId, int quantity);
    Task<(bool Ok, string Message)> RemoveAsync(string userId, int itemId);
    Task ClearAsync(string userId);
    Task<int> CountAsync(string userId);
}

public interface IOrderService
{
    Task<(bool Ok, string Message, Order? Order)> CheckoutAsync(string userId, CheckoutInput input);
    Task<List<Order>> GetUserOrdersAsync(string userId, OrderStatus? status);
    Task<Order?> GetUserOrderAsync(string userId, string orderCode);
    Task<(bool Ok, string Message)> CancelAsync(string userId, string orderCode, bool isStaff);
    Task<(bool Ok, string Message)> UpdateStatusAsync(int orderId, OrderStatus status, string? note, string updatedBy, bool markPaid = false);
    Task<Order?> GetByIdAsync(int id);
}

public class CheckoutInput
{
    public string ReceiverName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Note { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? VoucherCode { get; set; }
}

public interface IVoucherService
{
    Task<(bool Ok, string Message, decimal Discount, decimal ShippingFee)> ApplyAsync(string code, decimal subTotal, decimal defaultShipping);
}

public interface IWishlistService
{
    Task<(bool Ok, string Message, bool Added)> ToggleAsync(string userId, int productId);
    Task<List<ProductCardVm>> GetAsync(string userId);
    Task<int> CountAsync(string userId);
    Task<bool> RemoveAsync(string userId, int productId);
}

public interface IReviewService
{
    Task<(bool Ok, string Message)> CreateAsync(string userId, int productId, int rating, string comment, string? imageUrl);
    Task<(bool Ok, string Message)> UpdateAsync(string userId, int reviewId, int rating, string comment);
    Task<(bool Ok, string Message)> DeleteAsync(string userId, int reviewId);
    Task<(bool Ok, string Message)> ModerateAsync(int reviewId, bool hide);
}

public interface INotificationService
{
    Task NotifyAsync(string userId, string title, string message, string? link = null);
    Task<List<Notification>> GetLatestAsync(string userId, int take = 8);
    Task<int> UnreadCountAsync(string userId);
    Task MarkReadAsync(string userId, int id);
    Task MarkAllReadAsync(string userId);
}

public interface IDashboardService
{
    Task<DashboardVm> GetAsync();
}

public interface ICatalogService
{
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Brand>> GetBrandsAsync();
    Task<WebsiteSetting> GetSettingsAsync();
    Task<List<Banner>> GetBannersAsync();
}

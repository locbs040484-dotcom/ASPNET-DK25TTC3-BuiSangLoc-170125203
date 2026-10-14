using LaptopStore.Extensions;
using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.ViewComponents;

public class HeaderStatsViewComponent : ViewComponent
{
    private readonly ICartService _carts;
    private readonly IWishlistService _wishlist;
    private readonly INotificationService _notifications;
    private readonly ICatalogService _catalog;

    public HeaderStatsViewComponent(
        ICartService carts,
        IWishlistService wishlist,
        INotificationService notifications,
        ICatalogService catalog)
    {
        _carts = carts;
        _wishlist = wishlist;
        _notifications = notifications;
        _catalog = catalog;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = HttpContext.User.GetUserId();
        ViewBag.CartCount = userId == null ? 0 : await _carts.CountAsync(userId);
        ViewBag.WishCount = userId == null ? 0 : await _wishlist.CountAsync(userId);
        ViewBag.Unread = userId == null ? 0 : await _notifications.UnreadCountAsync(userId);
        ViewBag.Notes = userId == null ? new List<Models.Notification>() : await _notifications.GetLatestAsync(userId);
        ViewBag.Setting = await _catalog.GetSettingsAsync();
        ViewBag.Categories = await _catalog.GetCategoriesAsync();
        return View();
    }
}

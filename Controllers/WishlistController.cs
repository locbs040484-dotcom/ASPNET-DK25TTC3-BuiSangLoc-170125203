using LaptopStore.Extensions;
using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.Controllers;

[Authorize]
[Route("wishlist")]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlist;

    public WishlistController(IWishlistService wishlist)
    {
        _wishlist = wishlist;
    }

    [HttpPost("toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int productId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Json(new { ok = false, login = true, message = "Vui lòng đăng nhập." });
        var result = await _wishlist.ToggleAsync(userId, productId);
        var count = await _wishlist.CountAsync(userId);
        return Json(new { ok = result.Ok, message = result.Message, added = result.Added, count });
    }

    [HttpPost("remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await _wishlist.RemoveAsync(User.GetUserId()!, productId);
        return Json(new { ok = true, message = "Đã xóa khỏi yêu thích." });
    }
}

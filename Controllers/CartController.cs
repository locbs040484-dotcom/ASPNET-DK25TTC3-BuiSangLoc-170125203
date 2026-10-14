using LaptopStore.Extensions;
using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.Controllers;

[Authorize]
[Route("cart")]
public class CartController : Controller
{
    private readonly ICartService _carts;

    public CartController(ICartService carts)
    {
        _carts = carts;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? voucher)
    {
        var vm = await _carts.GetCartAsync(User.GetUserId()!, voucher);
        if (!string.IsNullOrWhiteSpace(voucher))
            HttpContext.Session.SetString("voucher", voucher);
        return View(vm);
    }

    [HttpPost("add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Json(new { ok = false, login = true, message = "Vui lòng đăng nhập để mua hàng." });

        var result = await _carts.AddAsync(userId, productId, quantity);
        return Json(new { ok = result.Ok, message = result.Message, count = result.Count });
    }

    [HttpPost("update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int itemId, int quantity)
    {
        var result = await _carts.UpdateAsync(User.GetUserId()!, itemId, quantity);
        var cart = await _carts.GetCartAsync(User.GetUserId()!, HttpContext.Session.GetString("voucher"));
        return Json(new
        {
            ok = result.Ok,
            message = result.Message,
            subTotal = Helpers.CurrencyHelper.ToVnd(cart.SubTotal),
            discount = Helpers.CurrencyHelper.ToVnd(cart.Discount),
            shipping = Helpers.CurrencyHelper.ToVnd(cart.ShippingFee),
            total = Helpers.CurrencyHelper.ToVnd(cart.Total),
            count = cart.Items.Sum(i => i.Quantity)
        });
    }

    [HttpPost("remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int itemId)
    {
        var result = await _carts.RemoveAsync(User.GetUserId()!, itemId);
        return Json(new { ok = result.Ok, message = result.Message });
    }

    [HttpPost("clear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        await _carts.ClearAsync(User.GetUserId()!);
        return Json(new { ok = true, message = "Đã xóa toàn bộ giỏ hàng." });
    }

    [HttpPost("voucher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyVoucher(string code)
    {
        HttpContext.Session.SetString("voucher", code ?? "");
        var cart = await _carts.GetCartAsync(User.GetUserId()!, code);
        return Json(new
        {
            ok = cart.VoucherValid,
            message = cart.VoucherMessage,
            discount = Helpers.CurrencyHelper.ToVnd(cart.Discount),
            shipping = Helpers.CurrencyHelper.ToVnd(cart.ShippingFee),
            total = Helpers.CurrencyHelper.ToVnd(cart.Total)
        });
    }
}

using LaptopStore.Data;
using LaptopStore.Extensions;
using LaptopStore.Models.Enums;
using LaptopStore.Services.Interfaces;
using LaptopStore.ViewModels.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Controllers;

[Authorize]
[Route("checkout")]
public class CheckoutController : Controller
{
    private readonly ICartService _carts;
    private readonly IOrderService _orders;
    private readonly ICatalogService _catalog;
    private readonly ApplicationDbContext _db;

    public CheckoutController(ICartService carts, IOrderService orders, ICatalogService catalog, ApplicationDbContext db)
    {
        _carts = carts;
        _orders = orders;
        _catalog = catalog;
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId()!;
        var cart = await _carts.GetCartAsync(userId, HttpContext.Session.GetString("voucher"));
        if (cart.Items.Count == 0)
            return RedirectToAction("Index", "Cart");

        var user = await _db.Users.FirstAsync(u => u.Id == userId);
        var addresses = await _db.UserAddresses.Where(a => a.UserId == userId).ToListAsync();
        var def = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();

        var vm = new CheckoutVm
        {
            Cart = cart,
            Setting = await _catalog.GetSettingsAsync(),
            Addresses = addresses,
            ReceiverName = def?.ReceiverName ?? user.FullName,
            Phone = def?.Phone ?? user.PhoneNumber ?? "",
            Email = user.Email ?? "",
            Province = def?.Province ?? "TP. Hồ Chí Minh",
            District = def?.District ?? "Quận 1",
            Ward = def?.Ward ?? "Phường Bến Nghé",
            AddressLine = def?.AddressLine ?? ""
        };
        return View(vm);
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutVm vm)
    {
        var userId = User.GetUserId()!;
        vm.Cart = await _carts.GetCartAsync(userId, HttpContext.Session.GetString("voucher"));
        vm.Setting = await _catalog.GetSettingsAsync();
        vm.Addresses = await _db.UserAddresses.Where(a => a.UserId == userId).ToListAsync();

        if (!ModelState.IsValid)
            return View(vm);

        var address = $"{vm.AddressLine}, {vm.Ward}, {vm.District}, {vm.Province}";
        var result = await _orders.CheckoutAsync(userId, new CheckoutInput
        {
            ReceiverName = vm.ReceiverName,
            Phone = vm.Phone,
            Email = vm.Email,
            Address = address,
            Note = vm.Note,
            PaymentMethod = vm.PaymentMethod,
            VoucherCode = vm.Cart.VoucherCode
        });

        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(vm);
        }

        HttpContext.Session.Remove("voucher");
        return RedirectToAction("Success", new { code = result.Order!.OrderCode });
    }

    [HttpGet("success/{code}")]
    public async Task<IActionResult> Success(string code)
    {
        var order = await _orders.GetUserOrderAsync(User.GetUserId()!, code);
        if (order == null) return NotFound();
        return View(order);
    }
}

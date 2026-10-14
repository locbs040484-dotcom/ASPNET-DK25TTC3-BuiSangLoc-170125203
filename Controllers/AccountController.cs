using LaptopStore.Data;
using LaptopStore.Extensions;
using LaptopStore.Helpers;
using LaptopStore.Models;
using LaptopStore.Models.Enums;
using LaptopStore.Services.Interfaces;
using LaptopStore.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ApplicationDbContext _db;
    private readonly IOrderService _orders;
    private readonly IWishlistService _wishlist;
    private readonly INotificationService _notifications;
    private readonly IWebHostEnvironment _env;

    public AccountController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        ApplicationDbContext db,
        IOrderService orders,
        IWishlistService wishlist,
        INotificationService notifications,
        IWebHostEnvironment env)
    {
        _users = users;
        _signIn = signIn;
        _db = db;
        _orders = orders;
        _wishlist = wishlist;
        _notifications = notifications;
        _env = env;
    }

    [HttpGet("register")]
    public IActionResult Register(string? returnUrl = null) => View(new RegisterVm { ReturnUrl = returnUrl });

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            FullName = vm.FullName,
            PhoneNumber = vm.Phone,
            EmailConfirmed = true,
            Avatar = "/images/avatars/default.svg"
        };
        var result = await _users.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(vm);
        }
        await _users.AddToRoleAsync(user, "Customer");
        await _signIn.SignInAsync(user, false);
        return Redirect(string.IsNullOrWhiteSpace(vm.ReturnUrl) ? "/" : vm.ReturnUrl);
    }

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null) => View(new LoginVm { ReturnUrl = returnUrl });

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = await _users.FindByEmailAsync(vm.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.");
            return View(vm);
        }

        var result = await _signIn.PasswordSignInAsync(user, vm.Password, vm.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(vm);
        }

        if (await _users.IsInRoleAsync(user, "Admin") || await _users.IsInRoleAsync(user, "Staff"))
            return Redirect("/admin");
        return Redirect(string.IsNullOrWhiteSpace(vm.ReturnUrl) ? "/" : vm.ReturnUrl);
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet("access-denied")]
    public IActionResult AccessDenied() => View();

    [Authorize]
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));
        return View(new ProfileVm
        {
            FullName = user.FullName,
            Email = user.Email ?? "",
            Phone = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Avatar = user.Avatar
        });
    }

    [Authorize]
    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileVm vm, IFormFile? avatar)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));
        if (!ModelState.IsValid)
        {
            vm.Avatar = user.Avatar;
            vm.Email = user.Email ?? "";
            return View(vm);
        }

        user.FullName = vm.FullName;
        user.PhoneNumber = vm.Phone;
        user.DateOfBirth = vm.DateOfBirth;
        user.Gender = vm.Gender;
        if (avatar != null)
            user.Avatar = await FileUploadHelper.SaveAsync(avatar, _env.WebRootPath, "uploads/avatars") ?? user.Avatar;

        await _users.UpdateAsync(user);
        TempData["Toast"] = "Đã cập nhật hồ sơ.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpGet("password")]
    public IActionResult Password() => View(new ChangePasswordVm());

    [Authorize]
    [HttpPost("password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Password(ChangePasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));
        var result = await _users.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(vm);
        }
        await _signIn.RefreshSignInAsync(user);
        TempData["Toast"] = "Đã đổi mật khẩu.";
        return RedirectToAction(nameof(Password));
    }

    [Authorize]
    [HttpGet("addresses")]
    public async Task<IActionResult> Addresses()
    {
        var list = await _db.UserAddresses.Where(a => a.UserId == User.GetUserId()).ToListAsync();
        return View(list);
    }

    [Authorize]
    [HttpGet("addresses/create")]
    public IActionResult CreateAddress() => View("AddressForm", new AddressVm());

    [Authorize]
    [HttpPost("addresses/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAddress(AddressVm vm)
    {
        if (!ModelState.IsValid) return View("AddressForm", vm);
        var userId = User.GetUserId()!;
        if (vm.IsDefault)
        {
            var others = await _db.UserAddresses.Where(a => a.UserId == userId).ToListAsync();
            others.ForEach(a => a.IsDefault = false);
        }
        _db.UserAddresses.Add(new UserAddress
        {
            UserId = userId,
            ReceiverName = vm.ReceiverName,
            Phone = vm.Phone,
            Province = vm.Province,
            District = vm.District,
            Ward = vm.Ward,
            AddressLine = vm.AddressLine,
            IsDefault = vm.IsDefault
        });
        await _db.SaveChangesAsync();
        TempData["Toast"] = "Đã thêm địa chỉ.";
        return RedirectToAction(nameof(Addresses));
    }

    [Authorize]
    [HttpGet("addresses/{id:int}/edit")]
    public async Task<IActionResult> EditAddress(int id)
    {
        var item = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == User.GetUserId());
        if (item == null) return NotFound();
        return View("AddressForm", new AddressVm
        {
            Id = item.Id,
            ReceiverName = item.ReceiverName,
            Phone = item.Phone,
            Province = item.Province,
            District = item.District,
            Ward = item.Ward,
            AddressLine = item.AddressLine,
            IsDefault = item.IsDefault
        });
    }

    [Authorize]
    [HttpPost("addresses/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(int id, AddressVm vm)
    {
        var item = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == User.GetUserId());
        if (item == null) return NotFound();
        if (!ModelState.IsValid) return View("AddressForm", vm);
        if (vm.IsDefault)
        {
            var others = await _db.UserAddresses.Where(a => a.UserId == item.UserId && a.Id != id).ToListAsync();
            others.ForEach(a => a.IsDefault = false);
        }
        item.ReceiverName = vm.ReceiverName;
        item.Phone = vm.Phone;
        item.Province = vm.Province;
        item.District = vm.District;
        item.Ward = vm.Ward;
        item.AddressLine = vm.AddressLine;
        item.IsDefault = vm.IsDefault;
        await _db.SaveChangesAsync();
        TempData["Toast"] = "Đã cập nhật địa chỉ.";
        return RedirectToAction(nameof(Addresses));
    }

    [Authorize]
    [HttpPost("addresses/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var item = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == User.GetUserId());
        if (item != null)
        {
            _db.UserAddresses.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Addresses));
    }

    [Authorize]
    [HttpGet("orders")]
    public async Task<IActionResult> Orders(OrderStatus? status)
    {
        ViewBag.Status = status;
        return View(await _orders.GetUserOrdersAsync(User.GetUserId()!, status));
    }

    [Authorize]
    [HttpGet("orders/{code}")]
    public async Task<IActionResult> OrderDetail(string code)
    {
        var order = await _orders.GetUserOrderAsync(User.GetUserId()!, code);
        if (order == null) return NotFound();
        return View(order);
    }

    [Authorize]
    [HttpPost("orders/{code}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(string code)
    {
        var result = await _orders.CancelAsync(User.GetUserId()!, code, false);
        TempData["Toast"] = result.Message;
        return RedirectToAction(nameof(OrderDetail), new { code });
    }

    [Authorize]
    [HttpGet("wishlist")]
    public async Task<IActionResult> Wishlist()
        => View(await _wishlist.GetAsync(User.GetUserId()!));

    [Authorize]
    [HttpGet("notifications")]
    public async Task<IActionResult> Notifications()
        => View(await _notifications.GetLatestAsync(User.GetUserId()!, 50));

    [Authorize]
    [HttpPost("notifications/read/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReadNotification(int id)
    {
        await _notifications.MarkReadAsync(User.GetUserId()!, id);
        return Ok();
    }

    [Authorize]
    [HttpPost("notifications/read-all")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReadAll()
    {
        await _notifications.MarkAllReadAsync(User.GetUserId()!);
        return RedirectToAction(nameof(Notifications));
    }

    [Authorize]
    [HttpGet("recent")]
    public async Task<IActionResult> Recent([FromServices] IProductService products)
        => View(await products.GetRecentlyViewedAsync(User.GetUserId()!));
}

using LaptopStore.Data;
using LaptopStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class CustomersController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _db;

    public CustomersController(UserManager<ApplicationUser> users, ApplicationDbContext db)
    {
        _users = users;
        _db = db;
    }

    public async Task<IActionResult> Index(string? q)
    {
        var customers = await _users.GetUsersInRoleAsync("Customer");
        if (!string.IsNullOrWhiteSpace(q))
            customers = customers.Where(u =>
                (u.FullName ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                || (u.Email ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        var ids = customers.Select(c => c.Id).ToList();
        var stats = await _db.Orders.Where(o => ids.Contains(o.UserId))
            .GroupBy(o => o.UserId)
            .Select(g => new { g.Key, Count = g.Count(), Spent = g.Sum(x => x.TotalAmount) })
            .ToListAsync();
        ViewBag.Stats = stats.ToDictionary(x => x.Key, x => (x.Count, x.Spent));
        return View(customers.OrderByDescending(c => c.CreatedAt).ToList());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound();
        if (await _users.IsInRoleAsync(user, "Admin"))
        {
            TempData["Toast"] = "Không thể khóa tài khoản Admin.";
            return RedirectToAction(nameof(Index));
        }
        user.IsActive = !user.IsActive;
        await _users.UpdateAsync(user);
        TempData["Toast"] = user.IsActive ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
        return RedirectToAction(nameof(Index));
    }
}

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class StaffController : Controller
{
    private readonly UserManager<ApplicationUser> _users;

    public StaffController(UserManager<ApplicationUser> users) => _users = users;

    public async Task<IActionResult> Index()
        => View((await _users.GetUsersInRoleAsync("Staff")).ToList());

    public IActionResult Create() => View(new StaffForm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffForm form)
    {
        if (!ModelState.IsValid) return View(form);
        var user = new ApplicationUser
        {
            UserName = form.Email,
            Email = form.Email,
            FullName = form.FullName,
            PhoneNumber = form.Phone,
            EmailConfirmed = true,
            IsActive = true,
            Avatar = "/images/avatars/default.svg"
        };
        var result = await _users.CreateAsync(user, form.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View(form);
        }
        await _users.AddToRoleAsync(user, form.Role);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound();
        var roles = await _users.GetRolesAsync(user);
        return View(new StaffForm { Id = user.Id, Email = user.Email ?? "", FullName = user.FullName, Phone = user.PhoneNumber, Role = roles.FirstOrDefault() ?? "Staff", IsActive = user.IsActive });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StaffForm form)
    {
        var user = await _users.FindByIdAsync(form.Id!);
        if (user == null) return NotFound();
        user.FullName = form.FullName;
        user.PhoneNumber = form.Phone;
        user.IsActive = form.IsActive;
        await _users.UpdateAsync(user);
        var current = await _users.GetRolesAsync(user);
        if (!current.Contains(form.Role))
        {
            await _users.RemoveFromRolesAsync(user, current);
            await _users.AddToRoleAsync(user, form.Role);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound();
        if (await _users.IsInRoleAsync(user, "Admin"))
        {
            TempData["Toast"] = "Không thể khóa Admin.";
            return RedirectToAction(nameof(Index));
        }
        user.IsActive = !user.IsActive;
        await _users.UpdateAsync(user);
        return RedirectToAction(nameof(Index));
    }
}

public class StaffForm
{
    public string? Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Password { get; set; } = "Staff@123";
    public string Role { get; set; } = "Staff";
    public bool IsActive { get; set; } = true;
}

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roles;
    public RolesController(RoleManager<IdentityRole> roles) => _roles = roles;
    public IActionResult Index() => View(_roles.Roles.ToList());
}

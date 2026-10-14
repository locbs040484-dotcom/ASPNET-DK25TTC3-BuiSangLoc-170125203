using LaptopStore.Extensions;
using LaptopStore.Models.Enums;
using LaptopStore.Services.Interfaces;
using LaptopStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IOrderService _orders;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ApplicationDbContext db, IOrderService orders, ILogger<OrdersController> logger)
    {
        _db = db;
        _orders = orders;
        _logger = logger;
    }

    public async Task<IActionResult> Index(OrderStatus? status, string? q, int page = 1)
    {
        var query = _db.Orders.Include(o => o.User).AsQueryable();
        if (status.HasValue)
            query = query.Where(o => o.OrderStatus == status);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(o => o.OrderCode.Contains(q) || o.ReceiverName.Contains(q) || o.Phone.Contains(q));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(o => o.CreatedAt).Skip((page - 1) * 12).Take(12).ToListAsync();
        ViewBag.Page = page;
        ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(total / 12.0));
        ViewBag.Status = status;
        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orders.GetByIdAsync(id);
        return order == null ? NotFound() : View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, string? note, bool markPaid = false)
    {
        var result = await _orders.UpdateStatusAsync(id, status, note, User.GetUserId() ?? "staff", markPaid);
        TempData["Toast"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Note(int id, string staffNote)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        order.StaffNote = staffNote;
        await _db.SaveChangesAsync();
        TempData["Toast"] = "Đã lưu ghi chú.";
        return RedirectToAction(nameof(Details), new { id });
    }
}

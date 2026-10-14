using LaptopStore.Data;
using LaptopStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _db;

    public ContactController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index() => View(new ContactMessage());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactMessage model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.ContactMessages.Add(model);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "Đã gửi liên hệ. Chúng tôi sẽ phản hồi sớm.";
        return RedirectToAction(nameof(Index));
    }
}

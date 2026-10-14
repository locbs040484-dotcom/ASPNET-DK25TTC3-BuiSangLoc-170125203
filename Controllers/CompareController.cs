using LaptopStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Controllers;

[Route("compare")]
public class CompareController : Controller
{
    private const string Key = "compare_ids";
    private readonly ApplicationDbContext _db;

    public CompareController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var ids = GetIds();
        var products = await _db.Products
            .Include(p => p.Brand).Include(p => p.Cpu).Include(p => p.Gpu)
            .Include(p => p.RamOption).Include(p => p.StorageOption).Include(p => p.Images)
            .Where(p => ids.Contains(p.Id)).ToListAsync();
        products = ids.Select(id => products.FirstOrDefault(p => p.Id == id)).Where(p => p != null).ToList()!;
        return View(products);
    }

    [HttpPost("toggle")]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(int productId)
    {
        var ids = GetIds();
        if (ids.Contains(productId))
        {
            ids.Remove(productId);
        }
        else
        {
            if (ids.Count >= 4)
                return Json(new { ok = false, message = "Chỉ so sánh tối đa 4 laptop." });
            ids.Add(productId);
        }
        Save(ids);
        return Json(new { ok = true, count = ids.Count, added = ids.Contains(productId), message = ids.Contains(productId) ? "Đã thêm vào so sánh." : "Đã bỏ khỏi so sánh." });
    }

    [HttpPost("clear")]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        HttpContext.Session.Remove(Key);
        return RedirectToAction(nameof(Index));
    }

    private List<int> GetIds()
    {
        var raw = HttpContext.Session.GetString(Key);
        if (string.IsNullOrWhiteSpace(raw)) return [];
        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
    }

    private void Save(List<int> ids)
        => HttpContext.Session.SetString(Key, string.Join(',', ids));
}

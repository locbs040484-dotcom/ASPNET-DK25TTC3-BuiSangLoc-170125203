using LaptopStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Controllers;

[Route("blog")]
public class BlogController : Controller
{
    private readonly ApplicationDbContext _db;

    public BlogController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? category, int page = 1)
    {
        var query = _db.BlogPosts.AsNoTracking().Include(p => p.BlogCategory).Where(p => p.IsPublished);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.BlogCategory.Slug == category);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.PublishedAt).Skip((page - 1) * 6).Take(6).ToListAsync();
        ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(total / 6.0));
        ViewBag.Page = page;
        ViewBag.Categories = await _db.BlogCategories.AsNoTracking().ToListAsync();
        ViewBag.Category = category;
        return View(items);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var post = await _db.BlogPosts.AsNoTracking().Include(p => p.BlogCategory)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);
        if (post == null) return NotFound();
        ViewBag.Related = await _db.BlogPosts.AsNoTracking()
            .Where(p => p.Id != post.Id && p.IsPublished)
            .OrderByDescending(p => p.PublishedAt).Take(3).ToListAsync();
        return View(post);
    }
}

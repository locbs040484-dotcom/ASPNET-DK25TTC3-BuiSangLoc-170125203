using LaptopStore.Extensions;
using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _products;
    private readonly ICatalogService _catalog;

    public HomeController(IProductService products, ICatalogService catalog)
    {
        _products = products;
        _catalog = catalog;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Banners = await _catalog.GetBannersAsync();
        ViewBag.Categories = await _catalog.GetCategoriesAsync();
        ViewBag.Brands = await _catalog.GetBrandsAsync();
        ViewBag.Featured = await _products.GetFeaturedAsync();
        ViewBag.OnSale = await _products.GetOnSaleAsync();
        ViewBag.Gaming = await _products.GetByCategorySlugAsync("laptop-gaming");
        ViewBag.Office = await _products.GetByCategorySlugAsync("laptop-van-phong");
        ViewBag.BestSellers = await _products.GetBestSellersAsync();
        ViewBag.NewArrivals = await _products.GetNewAsync();

        var userId = User.GetUserId();
        if (!string.IsNullOrEmpty(userId))
            ViewBag.Recent = await _products.GetRecentlyViewedAsync(userId);

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Newsletter(string email, [FromServices] Data.ApplicationDbContext db)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Json(new { ok = false, message = "Email không hợp lệ." });

        if (!db.NewsletterSubscribers.Any(x => x.Email == email))
        {
            db.NewsletterSubscribers.Add(new Models.NewsletterSubscriber { Email = email.Trim() });
            await db.SaveChangesAsync();
        }
        return Json(new { ok = true, message = "Đăng ký nhận tin thành công." });
    }
}

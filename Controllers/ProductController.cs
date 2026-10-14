using LaptopStore.Extensions;
using LaptopStore.Services.Interfaces;
using LaptopStore.ViewModels.Product;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.Controllers;

[Route("products")]
public class ProductController : Controller
{
    private readonly IProductService _products;

    public ProductController(IProductService products)
    {
        _products = products;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] ProductFilterVm filter)
    {
        var vm = await _products.SearchAsync(filter);
        vm.Title = string.IsNullOrWhiteSpace(filter.Category) ? "Laptop" : filter.Category;
        return View(vm);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var vm = await _products.GetBySlugAsync(slug, User.GetUserId());
        if (vm == null)
            return NotFound();
        return View(vm);
    }

    [HttpGet("~/search/suggest")]
    public async Task<IActionResult> Suggest(string q)
    {
        var items = await _products.SuggestAsync(q ?? string.Empty);
        return Json(items.Select(p => new
        {
            p.Name,
            p.Slug,
            p.ImageUrl,
            price = Helpers.CurrencyHelper.ToVnd(p.SalePrice)
        }));
    }
}

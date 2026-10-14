using LaptopStore.Extensions;
using LaptopStore.Helpers;
using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.Controllers;

[Authorize]
[Route("reviews")]
public class ReviewController : Controller
{
    private readonly IReviewService _reviews;
    private readonly IWebHostEnvironment _env;

    public ReviewController(IReviewService reviews, IWebHostEnvironment env)
    {
        _reviews = reviews;
        _env = env;
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int productId, int rating, string comment, IFormFile? image, string slug)
    {
        string? url = null;
        if (image != null)
            url = await FileUploadHelper.SaveAsync(image, _env.WebRootPath, "uploads/reviews");
        var result = await _reviews.CreateAsync(User.GetUserId()!, productId, rating, comment, url);
        TempData["Toast"] = result.Message;
        return Redirect($"/products/{slug}");
    }

    [HttpPost("{id:int}/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int rating, string comment, string slug)
    {
        var result = await _reviews.UpdateAsync(User.GetUserId()!, id, rating, comment);
        TempData["Toast"] = result.Message;
        return Redirect($"/products/{slug}");
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string slug)
    {
        var result = await _reviews.DeleteAsync(User.GetUserId()!, id);
        TempData["Toast"] = result.Message;
        return Redirect($"/products/{slug}");
    }
}

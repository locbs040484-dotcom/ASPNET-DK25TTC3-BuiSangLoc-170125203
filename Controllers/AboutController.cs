using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.Controllers;

public class AboutController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Warranty() => View();
    public IActionResult ReturnPolicy() => View();
    public IActionResult Shipping() => View();
    public IActionResult Payment() => View();
    public IActionResult Faq() => View();
}

using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.Controllers;

public class ErrorController : Controller
{
    [Route("error/{code:int}")]
    public IActionResult Index(int code)
    {
        Response.StatusCode = code is 404 or 403 or 500 ? code : 500;
        ViewBag.Code = Response.StatusCode;
        ViewBag.Title = Response.StatusCode switch
        {
            404 => "Không tìm thấy trang",
            403 => "Không có quyền truy cập",
            _ => "Đã xảy ra lỗi"
        };
        ViewBag.Message = Response.StatusCode switch
        {
            404 => "Trang bạn tìm không tồn tại hoặc đã được chuyển đi.",
            403 => "Bạn không có quyền xem nội dung này.",
            _ => "Hệ thống gặp sự cố. Vui lòng thử lại sau."
        };
        return View();
    }
}

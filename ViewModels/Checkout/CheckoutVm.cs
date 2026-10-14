using System.ComponentModel.DataAnnotations;
using LaptopStore.Models;
using LaptopStore.Models.Enums;
using LaptopStore.ViewModels.Cart;

namespace LaptopStore.ViewModels.Checkout;

public class CheckoutVm
{
    public List<UserAddress> Addresses { get; set; } = [];
    public int? SelectedAddressId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(120)]
    public string ReceiverName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^(0|\+84)[0-9]{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tỉnh/thành phố")]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập quận/huyện")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập phường/xã")]
    public string Ward { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    public string AddressLine { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Note { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

    public CartVm Cart { get; set; } = new();
    public WebsiteSetting Setting { get; set; } = new();
}

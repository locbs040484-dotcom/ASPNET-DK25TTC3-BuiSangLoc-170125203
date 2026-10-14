using System.ComponentModel.DataAnnotations;
using LaptopStore.Models.Enums;

namespace LaptopStore.ViewModels.Account;

public class RegisterVm
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class LoginVm
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public class ProfileVm
{
    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    public Gender? Gender { get; set; }

    public string? Avatar { get; set; }
}

public class ChangePasswordVm
{
    [Required, DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class AddressVm
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string ReceiverName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Province { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string District { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Ward { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string AddressLine { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}

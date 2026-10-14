using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models;

public class UserAddress
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

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

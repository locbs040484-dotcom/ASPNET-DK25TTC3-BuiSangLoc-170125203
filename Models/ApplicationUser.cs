using Microsoft.AspNetCore.Identity;
using LaptopStore.Models.Enums;

namespace LaptopStore.Models;

public class ApplicationUser : IdentityUser
{
    [System.ComponentModel.DataAnnotations.StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(300)]
    public string? Avatar { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender? Gender { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

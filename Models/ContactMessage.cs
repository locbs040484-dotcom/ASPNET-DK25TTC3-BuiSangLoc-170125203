using System.ComponentModel.DataAnnotations;
using LaptopStore.Models.Enums;

namespace LaptopStore.Models;

public class ContactMessage
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(180)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    public ContactStatus Status { get; set; } = ContactStatus.New;

    [StringLength(800)]
    public string? AdminNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

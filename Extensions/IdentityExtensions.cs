using System.Security.Claims;

namespace LaptopStore.Extensions;

public static class IdentityExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier);

    public static bool IsStaffOrAdmin(this ClaimsPrincipal user)
        => user.IsInRole("Admin") || user.IsInRole("Staff");
}

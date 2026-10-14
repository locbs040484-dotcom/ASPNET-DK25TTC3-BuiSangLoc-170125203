using System.Globalization;

namespace LaptopStore.Helpers;

public static class CurrencyHelper
{
    private static readonly CultureInfo Vi = new("vi-VN");

    public static string ToVnd(decimal amount)
        => string.Format(Vi, "{0:N0} ₫", amount);
}

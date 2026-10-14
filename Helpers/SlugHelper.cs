using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LaptopStore.Helpers;

public static class SlugHelper
{
    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = slug.Replace("đ", "d").Replace("Đ", "d");
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
        slug = Regex.Replace(slug, "-{2,}", "-");
        return slug;
    }
}

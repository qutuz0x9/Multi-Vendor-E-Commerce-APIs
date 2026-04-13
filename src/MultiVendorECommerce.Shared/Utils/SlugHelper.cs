using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MultiVendorECommerce.Shared.Utils;

public static partial class SlugHelper
{
    public static string GenerateSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string normalized = value.Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder(normalized.Length);
        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        string slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        slug = NonAlphanumericRegex().Replace(slug, "-");
        slug = MultipleHyphensRegex().Replace(slug, "-");
        slug = slug.Trim('-');

        return slug;
    }

    public static string GenerateUniqueSlug(string value, Func<string, bool> slugExists)
    {
        string baseSlug = GenerateSlug(value);
        string slug = baseSlug;
        int counter = 1;

        while (slugExists(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex MultipleHyphensRegex();
}

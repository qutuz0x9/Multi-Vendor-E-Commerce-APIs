using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MultiVendorECommerce.Shared.Utils;

public static partial class SlugHelper
{
    public static string GenerateProductSlug(string name, JsonDocument? features)
    {
        var parts = new List<string> { name };

        if (features is not null)
            ExtractJsonValues(features.RootElement, parts);

        return GenerateSlug(string.Join(" ", parts));
    }

    public static string GenerateUniqueProductSlug(string name, JsonDocument? features, Func<string, bool> slugExists)
    {
        string baseSlug = GenerateProductSlug(name, features);
        string slug = baseSlug;
        int counter = 1;

        while (slugExists(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private static void ExtractJsonValues(JsonElement element, List<string> values)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    ExtractJsonValues(property.Value, values);
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    ExtractJsonValues(item, values);
                break;
            case JsonValueKind.String:
                var str = element.GetString();
                if (!string.IsNullOrWhiteSpace(str))
                    values.Add(str);
                break;
            case JsonValueKind.Number:
                values.Add(element.GetRawText());
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                values.Add(element.GetBoolean().ToString());
                break;
        }
    }

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

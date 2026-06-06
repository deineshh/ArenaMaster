using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ArenaMaster.Api.Helpers;

public static class SlugHelper
{
    public static string Generate(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? Guid.NewGuid().ToString("N")[..8] : slug;
    }

    public static async Task<string> EnsureUniqueAsync(Func<string, Task<bool>> exists, string baseSlug)
    {
        var slug = baseSlug;
        var i = 1;
        while (await exists(slug))
        {
            slug = $"{baseSlug}-{i++}";
        }
        return slug;
    }
}

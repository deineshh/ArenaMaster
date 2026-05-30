namespace ArenaMaster.Api.Helpers;

public static class EntityImageHelper
{
    public static string EnsureImage(Guid entityId, string entityFolder, string query, UnsplashClient? unsplash = null)
    {
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", entityFolder);
        Directory.CreateDirectory(uploadsDir);
        var jpgPath = Path.Combine(uploadsDir, $"{entityId}.jpg");
        var svgPath = Path.Combine(uploadsDir, $"{entityId}.svg");

        if (File.Exists(jpgPath))
        {
            var url = $"/uploads/{entityFolder}/{entityId}.jpg";
            UnsplashFallback.AddToPool(url);
            return url;
        }

        if (File.Exists(svgPath))
        {
            File.Delete(svgPath);
        }

        var coverUrl = unsplash?.DownloadAndSaveAsync(entityFolder, entityId, query)
            .GetAwaiter().GetResult();

        if (coverUrl != null)
        {
            UnsplashFallback.AddToPool(coverUrl);
            return coverUrl;
        }

        coverUrl = UnsplashFallback.CopyFromPool(entityId, entityFolder);
        UnsplashFallback.AddToPool(coverUrl);
        return coverUrl;
    }
}

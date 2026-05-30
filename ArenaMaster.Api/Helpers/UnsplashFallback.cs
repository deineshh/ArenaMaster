namespace ArenaMaster.Api.Helpers;

public static class UnsplashFallback
{
    public static List<string> GlobalPool { get; } = [];

    public static string CopyFromPool(Guid entityId, string entityFolder)
    {
        if (GlobalPool.Count == 0)
            return WritePlaceholder(entityId, entityFolder);

        var source = GlobalPool[Random.Shared.Next(GlobalPool.Count)];
        var sourcePath = GetRealPath(source);

        var destFileName = $"{entityId}.jpg";
        var destDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", entityFolder);
        Directory.CreateDirectory(destDir);
        var destPath = Path.Combine(destDir, destFileName);

        if (File.Exists(sourcePath))
        {
            try
            {
                File.Copy(sourcePath, destPath, overwrite: true);
                return $"/uploads/{entityFolder}/{destFileName}";
            }
            catch { }
        }

        foreach (var alt in GlobalPool.Where(f => f != source))
        {
            var altPath = GetRealPath(alt);
            if (File.Exists(altPath))
            {
                try
                {
                    File.Copy(altPath, destPath, overwrite: true);
                    return $"/uploads/{entityFolder}/{destFileName}";
                }
                catch { }
            }
        }

        return WritePlaceholder(entityId, entityFolder);
    }

    public static string? DownloadFromUrl(string imageUrl, string entityFolder, Guid entityId)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var response = client.GetAsync(imageUrl).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode) return null;

            var destFileName = $"{entityId}.jpg";
            var destDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", entityFolder);
            Directory.CreateDirectory(destDir);
            var destPath = Path.Combine(destDir, destFileName);

            using var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            using var fileStream = File.Create(destPath);
            stream.CopyTo(fileStream);

            return $"/uploads/{entityFolder}/{destFileName}";
        }
        catch
        {
            return null;
        }
    }

    public static void AddToPool(string url)
    {
        if (url != null && url.EndsWith(".jpg") && !GlobalPool.Contains(url))
            GlobalPool.Add(url);
    }

    private static string GetRealPath(string url)
    {
        var relative = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(Directory.GetCurrentDirectory(), relative);
    }

    private static string WritePlaceholder(Guid entityId, string entityFolder)
    {
        var svg = entityFolder switch
        {
            "avatars" => $"""
                <svg xmlns="http://www.w3.org/2000/svg" width="200" height="200" viewBox="0 0 200 200">
                  <circle cx="100" cy="100" r="100" fill="#6B5BFF"/>
                  <text x="100" y="120" text-anchor="middle" font-family="Arial,sans-serif" font-size="80" font-weight="bold" fill="white">?</text>
                </svg>
                """,
            "teams" => $"""
                <svg xmlns="http://www.w3.org/2000/svg" width="200" height="200" viewBox="0 0 200 200">
                  <rect width="200" height="200" rx="20" fill="#6B5BFF"/>
                  <text x="100" y="115" text-anchor="middle" font-family="Arial,sans-serif" font-size="64" font-weight="bold" fill="white">?</text>
                </svg>
                """,
            _ => $"""
                <svg xmlns="http://www.w3.org/2000/svg" width="800" height="300" viewBox="0 0 800 300">
                  <rect width="800" height="300" fill="#2A2B32"/>
                  <text x="400" y="160" text-anchor="middle" font-family="Arial,sans-serif" font-size="36" fill="#666">Image</text>
                </svg>
                """
        };

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", entityFolder);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{entityId}.svg");
        File.WriteAllText(path, svg.Trim());
        return $"/uploads/{entityFolder}/{entityId}.svg";
    }
}

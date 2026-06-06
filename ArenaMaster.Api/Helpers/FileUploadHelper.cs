namespace ArenaMaster.Api.Helpers;

public static class FileUploadHelper
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    private const long MaxSize = 5 * 1024 * 1024;

    public static async Task<string?> SaveUploadedFileAsync(
        IFormFile file,
        IWebHostEnvironment env,
        string entityFolder,
        Guid entityId)
    {
        if (file.Length == 0 || file.Length > MaxSize)
            return null;

        if (!AllowedTypes.Contains(file.ContentType))
            return null;

        var ext = file.ContentType switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };

        var uploadsDir = Path.Combine(env.ContentRootPath, "uploads", entityFolder);
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{entityId}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);

        return $"/uploads/{entityFolder}/{fileName}";
    }
}

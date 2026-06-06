using System.Net.Http.Headers;
using System.Text.Json;

namespace ArenaMaster.Api.Helpers;

public class UnsplashClient(IConfiguration config, IHttpClientFactory httpFactory, IWebHostEnvironment env)
{
    private readonly string? _accessKey = config["UNSPLASH_ACCESS_KEY"];

    public async Task<string?> DownloadAndSaveAsync(string entityFolder, Guid entityId, string query)
    {
        if (string.IsNullOrWhiteSpace(_accessKey))
            return null;

        var client = httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", _accessKey);

        var url = $"https://api.unsplash.com/photos/random?query={Uri.EscapeDataString(query)}&orientation=landscape";
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var downloadUrl = doc.RootElement.GetProperty("urls").GetProperty("regular").GetString();
        if (string.IsNullOrEmpty(downloadUrl))
            return null;

        var imageResponse = await client.GetAsync(downloadUrl);
        if (!imageResponse.IsSuccessStatusCode)
            return null;

        var uploadsDir = Path.Combine(env.ContentRootPath, "uploads", entityFolder);
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{entityId}.jpg";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var fileStream = File.Create(filePath);
        await (await imageResponse.Content.ReadAsStreamAsync()).CopyToAsync(fileStream);

        return $"/uploads/{entityFolder}/{fileName}";
    }
}

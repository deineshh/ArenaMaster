using System.Text.RegularExpressions;

namespace ArenaMaster.Api.Helpers;

public static partial class PlaceholderImageGenerator
{
    public static void WriteTeamLogo(string teamName, Guid teamId)
    {
        var initials = GetInitials(teamName);
        var hue = Math.Abs(teamName.GetHashCode()) % 360;
        var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="200" height="200" viewBox="0 0 200 200">
              <rect width="200" height="200" rx="20" fill="hsl({hue}, 55%, 40%)"/>
              <text x="100" y="115" text-anchor="middle" font-family="Arial,sans-serif" font-size="64" font-weight="bold" fill="white">{initials}</text>
            </svg>
            """;

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "teams");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, $"{teamId}.svg"), svg.Trim());
    }

    public static string WriteUserAvatar(string username, Guid userId)
    {
        var letter = username.Length > 0 ? username[..1].ToUpper() : "?";
        var hue = Math.Abs(username.GetHashCode()) % 360;
        var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="200" height="200" viewBox="0 0 200 200">
              <circle cx="100" cy="100" r="100" fill="hsl({hue}, 50%, 35%)"/>
              <text x="100" y="120" text-anchor="middle" font-family="Arial,sans-serif" font-size="80" font-weight="bold" fill="white">{letter}</text>
            </svg>
            """;

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "avatars");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{userId}.svg");
        File.WriteAllText(path, svg.Trim());
        return $"/uploads/avatars/{userId}.svg";
    }

    public static string WriteTournamentCover(string title, Guid tournamentId)
    {
        var hue = Math.Abs(title.GetHashCode()) % 360;
        var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="800" height="300" viewBox="0 0 800 300">
              <defs>
                <linearGradient id="g" x1="0%" y1="0%" x2="100%" y2="100%">
                  <stop offset="0%" stop-color="hsl({hue}, 55%, 25%)"/>
                  <stop offset="100%" stop-color="hsl({(hue + 40) % 360}, 55%, 15%)"/>
                </linearGradient>
              </defs>
              <rect width="800" height="300" fill="url(#g)"/>
              <text x="400" y="160" text-anchor="middle" font-family="Arial,sans-serif" font-size="36" font-weight="bold" fill="white">{title}</text>
            </svg>
            """;

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "tournaments");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{tournamentId}.svg");
        File.WriteAllText(path, svg.Trim());
        return $"/uploads/tournaments/{tournamentId}.svg";
    }

    public static void WriteDisciplinePlaceholder()
    {
        var svg = """<svg xmlns="http://www.w3.org/2000/svg" width="800" height="200" viewBox="0 0 800 200"><rect width="800" height="200" fill="#2A2B32"/><text x="400" y="115" text-anchor="middle" font-family="Arial,sans-serif" font-size="24" fill="#666">Discipline Cover</text></svg>""";

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "disciplines");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "placeholder.svg"), svg.Trim());
    }

    private static string GetInitials(string name)
    {
        var parts = MyRegex().Split(name.Trim());
        if (parts.Length == 0) return "??";
        if (parts.Length == 1) return parts[0][..Math.Min(2, parts[0].Length)].ToUpper();
        return string.Concat(parts[0][..1], parts[^1][..1]).ToUpper();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MyRegex();
}

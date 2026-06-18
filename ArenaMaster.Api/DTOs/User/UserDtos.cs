namespace ArenaMaster.Api.DTOs.User;

public record UserProfileDto(
    Guid Id,
    string Username,
    string Email,
    string Role,
    string? AvatarUrl,
    string? Bio,
    string? DiscordUrl,
    List<string> SocialLinks,
    int TournamentsCount,
    int WinsCount,
    int MatchesPlayed);

public record UpdateProfileRequest(string? Bio, string? DiscordUrl, List<string>? SocialLinks);

public record TournamentHistoryItemDto(
    Guid TournamentId,
    string Title,
    string Slug,
    string Status,
    string? Result);

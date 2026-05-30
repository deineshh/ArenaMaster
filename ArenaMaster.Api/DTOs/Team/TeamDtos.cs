namespace ArenaMaster.Api.DTOs.Team;

public record TeamListItemDto(Guid Id, string Name, string Slug, string? LogoUrl, string CaptainUsername, int MemberCount);
public record TeamDetailDto(
    Guid Id, string Name, string Slug, string? LogoUrl, DateTime CreatedAt,
    List<TeamMemberDto> Members, int TournamentsCount, int WinsCount);
public record TeamMemberDto(Guid UserId, string Username, string? AvatarUrl, string Role);
public record CreateTeamRequest(string Name);
public record UpdateTeamRequest(string Name);
public record InvitePlayerRequest(string? Username, string? Email);

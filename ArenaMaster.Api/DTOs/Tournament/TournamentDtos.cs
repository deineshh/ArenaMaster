namespace ArenaMaster.Api.DTOs.Tournament;

public record TournamentListItemDto(
    Guid Id, string Title, string Slug, string DisciplineName, string Format,
    string ParticipantType, string Status, DateTime StartsAt, int ParticipantsCount, int MaxParticipants, string? CoverUrl);

public record TournamentDetailDto(
    Guid Id, string Title, string Slug, string DisciplineName, string Format,
    string ParticipantType, int? TeamSize, int MaxParticipants, DateTime RegistrationEndsAt,
    DateTime StartsAt, string Status, string? PrizeDescription, string? Description,
    string? CoverUrl, string? StreamUrl, bool AutoAccept, string OrganizerUsername,
    List<PrizePlaceDto> Prizes);

public record PrizePlaceDto(int Place, string Description);
public record CreateTournamentRequest(
    string Title, Guid DisciplineId, string Format, string ParticipantType,
    int? TeamSize, int MaxParticipants, DateTime RegistrationEndsAt, DateTime StartsAt,
    string? PrizeDescription, string? Description, string? StreamUrl, bool AutoAccept);
public record UpdateTournamentRequest(
    string? Title, Guid? DisciplineId, string? Format, int? MaxParticipants,
    DateTime? RegistrationEndsAt, DateTime? StartsAt, string? PrizeDescription,
    string? Description, string? StreamUrl, bool? AutoAccept);
public record UpdateStatusRequest(string Status);
public record ParticipantDto(
    Guid Id, string? Username, string? TeamName, string? AvatarUrl, string Status, int? Seed);
public record RegisterParticipantRequest(Guid? TeamId);

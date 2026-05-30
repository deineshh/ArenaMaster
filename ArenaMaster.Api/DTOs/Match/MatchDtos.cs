namespace ArenaMaster.Api.DTOs.Match;

public record MatchDetailDto(
    Guid Id, Guid TournamentId, int Round, int MatchNumber, string BracketSide,
    Guid? Participant1Id, string? Participant1Name, Guid? Participant2Id, string? Participant2Name,
    int? Score1, int? Score2, Guid? WinnerId, string Status,
    DateTime? ScheduledAt, DateTime? PlayedAt);

public record MatchResultRequest(int Score1, int Score2, Guid WinnerId);
public record MatchScheduleRequest(DateTime ScheduledAt);

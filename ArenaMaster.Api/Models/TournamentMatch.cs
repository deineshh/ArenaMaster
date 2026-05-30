namespace ArenaMaster.Api.Models;

public class TournamentMatch
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public int Round { get; set; }
    public int MatchNumber { get; set; }
    public string BracketSide { get; set; } = "winners";
    public Guid? Participant1Id { get; set; }
    public Guid? Participant2Id { get; set; }
    public Guid? WinnerId { get; set; }
    public int? Score1 { get; set; }
    public int? Score2 { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PlayedAt { get; set; }
    public Guid? NextMatchId { get; set; }
    public int? NextMatchSlot { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public TournamentParticipant? Participant1 { get; set; }
    public TournamentParticipant? Participant2 { get; set; }
    public TournamentParticipant? Winner { get; set; }
    public TournamentMatch? NextMatch { get; set; }
}

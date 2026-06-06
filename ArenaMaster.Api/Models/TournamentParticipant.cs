namespace ArenaMaster.Api.Models;

public class TournamentParticipant
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? TeamId { get; set; }
    public string Status { get; set; } = "pending";
    public int? Seed { get; set; }
    public DateTime RegisteredAt { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public User? User { get; set; }
    public Team? Team { get; set; }
}

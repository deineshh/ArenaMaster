namespace ArenaMaster.Api.Models;

public class Team
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public Guid CaptainId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Captain { get; set; } = null!;
    public ICollection<TeamMember> Members { get; set; } = [];
    public ICollection<TeamInvitation> Invitations { get; set; } = [];
    public ICollection<TournamentParticipant> TournamentParticipations { get; set; } = [];
}

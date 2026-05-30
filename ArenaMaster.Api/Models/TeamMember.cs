namespace ArenaMaster.Api.Models;

public class TeamMember
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "member";
    public DateTime JoinedAt { get; set; }

    public Team Team { get; set; } = null!;
    public User User { get; set; } = null!;
}

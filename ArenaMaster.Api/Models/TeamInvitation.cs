namespace ArenaMaster.Api.Models;

public class TeamInvitation
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public Guid InviteeId { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }

    public Team Team { get; set; } = null!;
    public User Invitee { get; set; } = null!;
}

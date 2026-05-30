namespace ArenaMaster.Api.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string Role { get; set; } = "player";
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? DiscordUrl { get; set; }
    public List<string> SocialLinks { get; set; } = [];
    public bool EmailConfirmed { get; set; }
    public string? EmailConfirmToken { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<OAuthAccount> OAuthAccounts { get; set; } = [];
    public ICollection<Team> CaptainedTeams { get; set; } = [];
    public ICollection<TeamMember> TeamMemberships { get; set; } = [];
    public ICollection<Tournament> OrganizedTournaments { get; set; } = [];
    public ICollection<TournamentParticipant> TournamentParticipations { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}

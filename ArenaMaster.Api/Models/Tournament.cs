namespace ArenaMaster.Api.Models;

public class Tournament
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid DisciplineId { get; set; }
    public Guid OrganizerId { get; set; }
    public string Format { get; set; } = "single_elimination";
    public string ParticipantType { get; set; } = "solo";
    public int? TeamSize { get; set; }
    public int MaxParticipants { get; set; }
    public DateTime RegistrationEndsAt { get; set; }
    public DateTime StartsAt { get; set; }
    public string Status { get; set; } = "draft";
    public string? PrizeDescription { get; set; }
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public string? StreamUrl { get; set; }
    public bool AutoAccept { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Discipline Discipline { get; set; } = null!;
    public User Organizer { get; set; } = null!;
    public ICollection<TournamentParticipant> Participants { get; set; } = [];
    public ICollection<TournamentMatch> Matches { get; set; } = [];
}

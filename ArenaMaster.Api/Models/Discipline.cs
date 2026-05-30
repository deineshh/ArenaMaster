namespace ArenaMaster.Api.Models;

public class Discipline
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }

    public ICollection<Tournament> Tournaments { get; set; } = [];
}

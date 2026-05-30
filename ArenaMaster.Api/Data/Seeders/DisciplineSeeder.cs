using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;

namespace ArenaMaster.Api.Data.Seeders;

public static class DisciplineSeeder
{
    public static List<Discipline> Seed(AppDbContext db)
    {
        var names = new[]
        {
            "Counter-Strike 2",
            "Dota 2",
            "Valorant",
            "League of Legends",
            "FIFA / EA FC"
        };

        PlaceholderImageGenerator.WriteDisciplinePlaceholder();

        var list = names.Select(n => new Discipline
        {
            Id = Guid.NewGuid(),
            Name = n,
            Slug = SlugHelper.Generate(n),
            CoverUrl = "/uploads/disciplines/placeholder.svg"
        }).ToList();

        db.Disciplines.AddRange(list);
        return list;
    }
}

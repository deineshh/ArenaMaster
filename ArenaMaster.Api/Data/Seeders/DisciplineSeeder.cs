using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;

namespace ArenaMaster.Api.Data.Seeders;

public static class DisciplineSeeder
{
    private static readonly Dictionary<string, string> Queries = new()
    {
        ["Counter-Strike 2"]    = "counter strike 2 gameplay",
        ["Dota 2"]              = "dota 2 arena battle",
        ["Valorant"]            = "valorant agents gameplay",
        ["League of Legends"]   = "league of legends rift",
        ["FIFA / EA FC"]        = "fifa football stadium",
        ["Rainbow Six Siege"]   = "rainbow six siege operator",
        ["Apex Legends"]        = "apex legends battle royale",
        ["Overwatch 2"]         = "overwatch 2 heroes",
        ["StarCraft II"]        = "starcraft 2 protoss",
        ["Fortnite"]            = "fortnite battle royale",
    };

    public static List<Discipline> Seed(AppDbContext db, UnsplashClient? unsplash = null)
    {
        var list = new List<Discipline>();

        foreach (var (name, query) in Queries)
        {
            var id = DeterministicGuid.Create($"discipline-{name}");
            var coverUrl = EntityImageHelper.EnsureImage(id, "disciplines", query, unsplash);

            var discipline = new Discipline
            {
                Id = id,
                Name = name,
                Slug = SlugHelper.Generate(name),
                CoverUrl = coverUrl
            };
            db.Disciplines.Add(discipline);
            list.Add(discipline);
        }

        return list;
    }
}

using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;

namespace ArenaMaster.Api.Data.Seeders;

public static class TeamSeeder
{
    private static readonly string[] LogoQueries =
    [
        "esports team logo gaming",
        "professional esports logo",
        "gaming clan emblem",
        "competitive gaming badge",
        "esports team crest",
    ];

    public static List<Team> Seed(AppDbContext db, Dictionary<string, User> users, UnsplashClient? unsplash = null)
    {
        var allPlayers = users.Values.Where(u => u.Role == "player").ToList();
        var rng = new System.Random(42);

        var teams = new (string name, string captain, int memberCount)[]
        {
            ("Залізні Вовки",       "serhiy_blade",    4),
            ("Нічні Хижаки",        "yaroslav_cs",     4),
            ("Буревій",             "bohdan_dota",     4),
            ("Степові Рейдери",     "mariia_riot",     5),
            ("Кібер Козаки",        "oksana_sniper",   4),
            ("Арена Шторм",         "ivan_ace",        3),
            ("Кришталеві Дракони",  "sofia_flash",     4),
            ("Тіньові Вовкулаки",   "pavlo_storm",     3),
            ("Полум'яні Соколи",    "anna_stealth",    4),
            ("Громові Ведмеді",     "mykola_boom",     3),
            ("Місячні Примари",     "diana_hex",       4),
            ("Сталеві Леви",        "artem_wave",      3),
            ("Бурштинові Шершні",   "liliya_shield",   4),
            ("Козацька Січ",        "volodymyr_lex",   5),
            ("Електричні Вугрі",    "nina_volt",       3),
            ("Вогняні Лис",         "roman_phoenix",   4),
            ("Срібні Яструби",      "tetiana_luna",    3),
            ("Північні Вітри",      "denys_knight",    4),
            ("Карпатські Ведмеді",  "alina_beam",      3),
            ("Небесна Варта",       "oleh_titan",      4),
        };

        var createdTeams = new List<Team>();
        var usedPlayers = new HashSet<Guid>();

        foreach (var (name, captainName, memberCount) in teams)
        {
            var captain = users[captainName];
            var teamId = DeterministicGuid.Create($"team-{name}");
            var query = LogoQueries[Random.Shared.Next(LogoQueries.Length)];

            var logoUrl = EntityImageHelper.EnsureImage(teamId, "teams", query, unsplash);

            var team = new Team
            {
                Id = teamId,
                Name = name,
                Slug = SlugHelper.Generate(name) + $"-{teamId:N}",
                CaptainId = captain.Id,
                LogoUrl = logoUrl,
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(30, 180))
            };
            db.Teams.Add(team);
            createdTeams.Add(team);

            db.TeamMembers.Add(new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                UserId = captain.Id,
                Role = "captain",
                JoinedAt = team.CreatedAt
            });
            usedPlayers.Add(captain.Id);

            var available = allPlayers
                .Where(p => p.Id != captain.Id && !usedPlayers.Contains(p.Id))
                .OrderBy(_ => rng.Next())
                .Take(memberCount)
                .ToList();

            foreach (var member in available)
            {
                db.TeamMembers.Add(new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = team.Id,
                    UserId = member.Id,
                    Role = "member",
                    JoinedAt = team.CreatedAt.AddDays(rng.Next(1, 20))
                });
                usedPlayers.Add(member.Id);
            }
        }

        return createdTeams;
    }
}

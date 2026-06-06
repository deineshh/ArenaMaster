using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Data.Seeders;

public static class TournamentSeeder
{
    private static readonly Dictionary<string, string> GameQueries = new()
    {
        ["Counter-Strike 2"]    = "counter strike 2 tournament arena",
        ["Dota 2"]              = "dota 2 battle arena",
        ["Valorant"]            = "valorant competitive match",
        ["League of Legends"]   = "league of legends championship",
        ["FIFA / EA FC"]        = "fifa football stadium crowd",
        ["Rainbow Six Siege"]   = "rainbow six siege tactical",
        ["Apex Legends"]        = "apex legends squad",
        ["Overwatch 2"]         = "overwatch 2 team fight",
        ["StarCraft II"]        = "starcraft 2 protoss battle",
        ["Fortnite"]            = "fortnite creative mode",
    };

    public static void Seed(AppDbContext db, Dictionary<string, User> users, List<Team> teams, UnsplashClient? unsplash = null)
    {
        var dota    = db.Disciplines.First(d => d.Name == "Dota 2");
        var cs2     = db.Disciplines.First(d => d.Name == "Counter-Strike 2");
        var val     = db.Disciplines.First(d => d.Name == "Valorant");
        var fifa    = db.Disciplines.First(d => d.Name == "FIFA / EA FC");
        var lol     = db.Disciplines.First(d => d.Name == "League of Legends");
        var r6      = db.Disciplines.First(d => d.Name == "Rainbow Six Siege");
        var apex    = db.Disciplines.First(d => d.Name == "Apex Legends");
        var ow2     = db.Disciplines.First(d => d.Name == "Overwatch 2");
        var sc2     = db.Disciplines.First(d => d.Name == "StarCraft II");
        var fn      = db.Disciplines.First(d => d.Name == "Fortnite");

        var organizers = users.Values.Where(u => u.Role is "organizer" or "admin").ToList();
        var rng = new System.Random(1337);

        var tournaments = new (string title, Discipline disc, string format, string type, string status, int max, string prize, string desc)[]
        {
            ("Весняний Кубок CS2 2026",         cs2,    "single_elimination", "team", "finished",     8,  "1 місце — 1000 грн, 2 місце — 500 грн, 3 місце — 250 грн", "Командний турнір з Counter-Strike 2. Система Single Elimination. Вік гравців — від 14 років."),
            ("Dota 2 Літня Ліга",               dota,   "double_elimination", "team", "ongoing",      8,  "1 місце — 1500 грн + мерч, 2 місце — 700 грн", "Літня першість з Dota 2. Подвійне вибування. Дозволені всі герої."),
            ("Valorant Чемпіонат України",      val,    "single_elimination", "team", "registration", 16, "1 місце — 2000 грн, 2 місце — 1000 грн, 3 місце — 500 грн", "Національний чемпіонат з Valorant. Лише українські команди."),
            ("FIFA Відкритий Кубок",            fifa,   "single_elimination", "solo", "registration", 16, "1 місце — 500 грн, 2 місце — 300 грн", "Особистий турнір з FIFA 26. Реєстрація індивідуальна."),
            ("League Open Series #3",           lol,    "single_elimination", "team", "draft",        16, "1 місце — 1200 грн", "Третій сезон League Open Series. Збір команд за тиждень до старту."),
            ("Весняна Битва 1v1 CS2",           cs2,    "single_elimination", "solo", "finished",     8,  "1 місце — 300 грн, 2 місце — 150 грн", "Особистий турнір 1v1 на Dust2. Переможець отримує грошовий приз."),
            ("Rainbow Six: Штурм",              r6,     "single_elimination", "team", "registration", 8,  "1 місце — 800 грн", "Турнір з Rainbow Six Siege. Дозволені всі оперативники."),
            ("Apex Legends: Королівська Битва", apex,   "single_elimination", "team", "registration", 12, "1 місце — 1500 грн, 2 місце — 750 грн", "Турнір з Apex Legends. Команди по 3 гравці."),
            ("Overwatch 2 Кубок Героїв",        ow2,    "single_elimination", "team", "draft",        8,  "1 місце — 1000 грн", "Кубок Героїв Overwatch 2. Формат 5v5."),
            ("StarCraft II: Тактична Арена",    sc2,    "single_elimination", "solo", "finished",     8,  "1 місце — 400 грн, 2 місце — 200 грн", "Індивідуальний турнір зі StarCraft II."),
            ("Fortnite Build Battle",           fn,     "single_elimination", "solo", "finished",     8,  "1 місце — 600 грн", "Турнір з Fortnite. Лише режим Build Battle."),
            ("Dota 2 Нічна Ліга",               dota,   "single_elimination", "team", "ongoing",      8,  "1 місце — 1000 грн", "Нічна ліга Dota 2. Матчі проводяться ввечері."),
            ("CS2 Ветеран Кап",                 cs2,    "double_elimination", "team", "ongoing",      8,  "1 місце — 2000 грн", "Турнір для ветеранів CS2. Досвід від 1000 годин."),
            ("Valorant: Стрімкий Командний",    val,    "single_elimination", "team", "registration", 8,  "1 місце — 700 грн", "Швидкий командний турнір з Valorant на один вечір."),
            ("FIFA Кубок Легенд",               fifa,   "single_elimination", "solo", "draft",        8,  "1 місце — 350 грн", "Турнір легенд FIFA. Тільки класичні команди."),
            ("League of Legends: Битва за Приз", lol,    "single_elimination", "team", "registration", 8,  "1 місце — 900 грн", "Битва за призовий фонд. Реєстрація команд 5x5."),
            ("Overwatch 2 Нічні Вовки",          ow2,    "single_elimination", "team", "registration", 8,  "1 місце — 600 грн", "Нічний турнір з Overwatch 2."),
            ("Apex Legends: Швидка Реакція",    apex,   "single_elimination", "team", "finished",     8,  "1 місце — 1000 грн", "Турнір на швидкість реакції. Команди 3x3."),
            ("Rainbow Six: Тактичний Удар",     r6,     "single_elimination", "team", "ongoing",      8,  "1 місце — 800 грн", "Тактичний турнір з Rainbow Six Siege."),
            ("Fortnite: Двобій",                fn,     "single_elimination", "solo", "registration", 8,  "1 місце — 500 грн", "Індивідуальний турнір Fortnite Zero Build."),
        };

        var idx = 0;
        foreach (var (title, disc, format, type, status, max, prize, desc) in tournaments)
        {
            var tournamentId = DeterministicGuid.Create($"tournament-{idx}");
            var gameQuery = GameQueries.GetValueOrDefault(disc.Name, "esports tournament");

            var coverUrl = EntityImageHelper.EnsureImage(tournamentId, "tournaments", gameQuery, unsplash);

            var t = new Tournament
            {
                Id = tournamentId,
                Title = title,
                Slug = SlugHelper.Generate(title) + $"-{idx}",
                DisciplineId = disc.Id,
                OrganizerId = organizers[idx % organizers.Count].Id,
                Format = format,
                ParticipantType = type,
                TeamSize = type == "team" ? 5 : null,
                MaxParticipants = max,
                RegistrationEndsAt = status == "finished"
                    ? DateTime.UtcNow.AddDays(-15)
                    : status == "ongoing"
                        ? DateTime.UtcNow.AddDays(-3)
                        : DateTime.UtcNow.AddDays(7),
                StartsAt = status == "finished"
                    ? DateTime.UtcNow.AddDays(-10)
                    : status == "ongoing"
                        ? DateTime.UtcNow.AddDays(-1)
                        : DateTime.UtcNow.AddDays(14),
                Status = status,
                PrizeDescription = prize,
                Description = desc,
                CoverUrl = coverUrl,
                AutoAccept = status != "draft",
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(20, 60)),
                UpdatedAt = DateTime.UtcNow
            };
            db.Tournaments.Add(t);

            if (status is "finished" or "ongoing" or "registration")
            {
                if (type == "team")
                {
                    var selectedTeams = teams.OrderBy(_ => rng.Next()).Take(Math.Min(teams.Count, max)).ToList();
                    foreach (var team in selectedTeams)
                    {
                        db.TournamentParticipants.Add(new TournamentParticipant
                        {
                            Id = Guid.NewGuid(),
                            TournamentId = t.Id,
                            TeamId = team.Id,
                            Status = "accepted",
                            Seed = rng.Next(1, max + 1),
                            RegisteredAt = t.CreatedAt.AddDays(rng.Next(1, 10))
                        });
                    }
                }
                else
                {
                    var players = users.Values.Where(u => u.Role == "player")
                        .OrderBy(_ => rng.Next())
                        .Take(Math.Min(users.Values.Count(u => u.Role == "player"), max))
                        .ToList();
                    foreach (var player in players)
                    {
                        db.TournamentParticipants.Add(new TournamentParticipant
                        {
                            Id = Guid.NewGuid(),
                            TournamentId = t.Id,
                            UserId = player.Id,
                            Status = "accepted",
                            Seed = rng.Next(1, max + 1),
                            RegisteredAt = t.CreatedAt.AddDays(rng.Next(1, 10))
                        });
                    }
                }
            }
            idx++;
        }
    }
}

using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Data.Seeders;

public static class TournamentSeeder
{
    public static void Seed(AppDbContext db, List<Discipline> disciplines, Dictionary<string, User> users)
    {
        var cs2 = disciplines.First(d => d.Name == "Counter-Strike 2");
        var dota = disciplines.First(d => d.Name == "Dota 2");
        var valorant = disciplines.First(d => d.Name == "Valorant");
        var fifa = disciplines.First(d => d.Name == "FIFA / EA FC");
        var lol = disciplines.First(d => d.Name == "League of Legends");

        var organizer = users["dmytro_arena"];
        var teams = db.Teams.Include(t => t.Members).ToList();

        var tournaments = new (string title, Discipline disc, string format, string type, string status, int max)[]
        {
            ("Весняний Кубок CS2 2026", cs2, "single_elimination", "team", "finished", 8),
            ("Dota 2 Літня Ліга", dota, "double_elimination", "team", "ongoing", 8),
            ("Valorant Чемпіонат України", valorant, "single_elimination", "team", "registration", 16),
            ("FIFA Відкритий Кубок", fifa, "single_elimination", "solo", "registration", 8),
            ("League Open Series #3", lol, "single_elimination", "team", "draft", 16),
            ("Весняна Битва 1v1 CS2", cs2, "single_elimination", "solo", "finished", 8),
        };

        var i = 0;
        foreach (var (title, disc, format, type, status, max) in tournaments)
        {
            var tournamentId = Guid.NewGuid();
            var t = new Tournament
            {
                Id = tournamentId,
                Title = title,
                Slug = SlugHelper.Generate(title) + $"-{i++}",
                DisciplineId = disc.Id,
                OrganizerId = organizer.Id,
                Format = format,
                ParticipantType = type,
                TeamSize = type == "team" ? 5 : null,
                MaxParticipants = max,
                RegistrationEndsAt = DateTime.UtcNow.AddDays(7),
                StartsAt = DateTime.UtcNow.AddDays(14),
                Status = status,
                PrizeDescription = "1 місце — 500 грн, 2 місце — 200 грн, 3 місце — 100 грн",
                Description = "Аматорський турнір ArenaMaster. Реєстрація обов'язкова.",
                CoverUrl = PlaceholderImageGenerator.WriteTournamentCover(title, tournamentId),
                AutoAccept = status != "draft",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow
            };
            db.Tournaments.Add(t);

            if (status is "finished" or "ongoing" or "registration")
            {
                var participantTeams = teams.Take(Math.Min(teams.Count, max)).ToList();
                var seed = 1;
                foreach (var team in participantTeams)
                {
                    if (type == "team")
                    {
                        db.TournamentParticipants.Add(new TournamentParticipant
                        {
                            Id = Guid.NewGuid(),
                            TournamentId = t.Id,
                            TeamId = team.Id,
                            Status = "accepted",
                            Seed = seed++,
                            RegisteredAt = DateTime.UtcNow.AddDays(-10)
                        });
                    }
                }

                if (type == "solo" && status == "finished")
                {
                    var soloUsers = users.Values.Where(u => u.Role == "player").Take(max).ToList();
                    seed = 1;
                    foreach (var u in soloUsers)
                    {
                        db.TournamentParticipants.Add(new TournamentParticipant
                        {
                            Id = Guid.NewGuid(),
                            TournamentId = t.Id,
                            UserId = u.Id,
                            Status = "accepted",
                            Seed = seed++,
                            RegisteredAt = DateTime.UtcNow.AddDays(-10)
                        });
                    }
                }
            }
        }
    }
}

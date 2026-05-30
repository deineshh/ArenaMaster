using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;

namespace ArenaMaster.Api.Data.Seeders;

public static class TeamSeeder
{
    public static void Seed(AppDbContext db, Dictionary<string, User> users)
    {
        var teams = new (string name, string captain, string[] members)[]
        {
            ("Залізні Вовки", "serhiy_blade", ["oksana_sniper", "andriy_frag", "kateryna_aim"]),
            ("Нічні Хижаки", "yaroslav_cs", ["oleksandr_rush", "nadia_clutch", "taras_headshot"]),
            ("Буревій", "bohdan_dota", ["iryna_val", "yulia_support", "mariia_riot"]),
            ("Степові Рейдери", "mariia_riot", ["serhiy_blade", "oksana_sniper", "andriy_frag", "kateryna_aim"]),
            ("Кібер Козаки", "oksana_sniper", ["yaroslav_cs", "bohdan_dota", "iryna_val"]),
        };

        foreach (var (name, captainName, memberNames) in teams)
        {
            var captain = users[captainName];
            var teamId = Guid.NewGuid();
            var team = new Team
            {
                Id = teamId,
                Name = name,
                Slug = SlugHelper.Generate(name),
                CaptainId = captain.Id,
                LogoUrl = $"/uploads/teams/{teamId}.svg",
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            };
            PlaceholderImageGenerator.WriteTeamLogo(name, teamId);
            db.Teams.Add(team);

            db.TeamMembers.Add(new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                UserId = captain.Id,
                Role = "captain",
                JoinedAt = team.CreatedAt
            });

            foreach (var memberName in memberNames.Distinct())
            {
                if (!users.TryGetValue(memberName, out var member) || member.Id == captain.Id) continue;
                db.TeamMembers.Add(new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = team.Id,
                    UserId = member.Id,
                    Role = "member",
                    JoinedAt = team.CreatedAt.AddDays(Random.Shared.Next(1, 30))
                });
            }
        }
    }
}

using ArenaMaster.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Data.Seeders;

public static class NotificationSeeder
{
    public static void Seed(AppDbContext db)
    {
        var rng = new System.Random(777);
        var users = db.Users.ToList();
        var teams = db.Teams.ToList();
        var tournaments = db.Tournaments.ToList();

        var templates = new List<(string type, string title, string body, Func<string>? entityType, Func<Guid?>? entityId)>
        {
            ("team_invitation", "Запрошення до команди", "Вас запрошено приєднатися до команди \"{0}\". Перейдіть до розділу запрошень.", null, null),
            ("tournament_reminder", "Нагадування про турнір", "Турнір \"{0}\" розпочнеться через 24 години. Підготуйтеся!", null, null),
            ("match_result", "Результат матчу", "Ваш матч у турнірі \"{0}\" завершено. Перегляньте результати.", null, null),
            ("tournament_status", "Статус турніру змінено", "Турнір \"{0}\" змінив статус на \"{1}\".", null, null),
            ("participant_confirmed", "Участь підтверджено", "Вашу заявку на турнір \"{0}\" підтверджено.", null, null),
        };

        var pastDays = new[] { 1, 2, 3, 5, 7, 10, 14, 21 };
        var notificationCount = 0;

        foreach (var user in users)
        {
            var count = rng.Next(3, 8);
            for (var n = 0; n < count && notificationCount < 300; n++)
            {
                var template = templates[rng.Next(templates.Count)];
                var tournament = tournaments[rng.Next(tournaments.Count)];
                var team = teams[rng.Next(teams.Count)];
                var daysAgo = pastDays[rng.Next(pastDays.Length)];

                var body = template.body switch
                {
                    var b when b.Contains("{1}") => string.Format(b, tournament.Title, "активний"),
                    var b when b.Contains("{0}") => string.Format(b, tournament.Title),
                    _ => template.body
                };

                db.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Type = template.type,
                    Title = template.title,
                    Body = body,
                    IsRead = rng.Next(0, 3) > 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-daysAgo).AddHours(-rng.Next(1, 12)),
                    EntityType = template.type switch
                    {
                        "team_invitation" => "team",
                        "match_result" => "match",
                        _ => "tournament"
                    },
                    EntityId = template.type switch
                    {
                        "team_invitation" => team.Id,
                        _ => tournament.Id
                    }
                });
                notificationCount++;
            }
        }
    }
}

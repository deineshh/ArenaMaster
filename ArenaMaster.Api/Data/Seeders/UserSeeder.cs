using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;

namespace ArenaMaster.Api.Data.Seeders;

public static class UserSeeder
{
    public static Dictionary<string, User> Seed(AppDbContext db)
    {
        var data = new (string username, string email, string role, string password)[]
        {
            ("serhiy_blade", "serhiy.blade@gmail.com", "player", "Test1234!"),
            ("oksana_sniper", "oksana.sniper@ukr.net", "player", "Test1234!"),
            ("dmytro_arena", "dmytro.arena@gmail.com", "organizer", "Organizer1!"),
            ("admin_master", "admin@arenamaster.ua", "admin", "Test1234!"),
            ("yaroslav_cs", "yaroslav.cs@gmail.com", "player", "Test1234!"),
            ("mariia_riot", "mariia.riot@gmail.com", "player", "Test1234!"),
            ("bohdan_dota", "bohdan.dota@ukr.net", "player", "Test1234!"),
            ("iryna_val", "iryna.val@gmail.com", "player", "Test1234!"),
            ("viktor_pro", "viktor.pro@ukr.net", "organizer", "Organizer1!"),
            ("andriy_frag", "andriy.frag@gmail.com", "player", "Test1234!"),
            ("kateryna_aim", "kateryna.aim@ukr.net", "player", "Test1234!"),
            ("oleksandr_rush", "oleksandr.rush@gmail.com", "player", "Test1234!"),
            ("nadia_clutch", "nadia.clutch@ukr.net", "player", "Test1234!"),
            ("taras_headshot", "taras.headshot@gmail.com", "player", "Test1234!"),
            ("yulia_support", "yulia.support@ukr.net", "player", "Test1234!"),
        };

        var dict = new Dictionary<string, User>();
        foreach (var (username, email, role, password) in data)
        {
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 12),
                Role = role,
                EmailConfirmed = true,
                Bio = $"Гравець ArenaMaster — {username}",
                AvatarUrl = $"/uploads/avatars/{userId}.svg",
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(30, 365)),
                UpdatedAt = DateTime.UtcNow
            };
            PlaceholderImageGenerator.WriteUserAvatar(username, userId);
            db.Users.Add(user);
            dict[username] = user;
        }

        return dict;
    }
}

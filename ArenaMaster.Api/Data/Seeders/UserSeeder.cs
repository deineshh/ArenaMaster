using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;

namespace ArenaMaster.Api.Data.Seeders;

public static class UserSeeder
{
    public static Dictionary<string, User> Seed(AppDbContext db, UnsplashClient? unsplash = null)
    {
        var data = new (string username, string email, string role, string password)[]
        {
            ("dmytro_arena",    "dmytro.arena@gmail.com",    "organizer", "Organizer1!"),
            ("viktor_pro",      "viktor.pro@ukr.net",        "organizer", "Organizer1!"),
            ("olena_organizer", "olena.org@gmail.com",       "organizer", "Organizer1!"),
            ("max_coach",       "max.coach@ukr.net",         "organizer", "Organizer1!"),
            ("admin_master",    "admin@arenamaster.ua",       "admin",     "Test1234!"),

            ("serhiy_blade",    "serhiy.blade@gmail.com",    "player", "Test1234!"),
            ("oksana_sniper",   "oksana.sniper@ukr.net",     "player", "Test1234!"),
            ("yaroslav_cs",     "yaroslav.cs@gmail.com",     "player", "Test1234!"),
            ("mariia_riot",     "mariia.riot@gmail.com",     "player", "Test1234!"),
            ("bohdan_dota",     "bohdan.dota@ukr.net",       "player", "Test1234!"),
            ("iryna_val",       "iryna.val@gmail.com",       "player", "Test1234!"),
            ("andriy_frag",     "andriy.frag@gmail.com",     "player", "Test1234!"),
            ("kateryna_aim",    "kateryna.aim@ukr.net",      "player", "Test1234!"),
            ("oleksandr_rush",  "oleksandr.rush@gmail.com",  "player", "Test1234!"),
            ("nadia_clutch",    "nadia.clutch@ukr.net",      "player", "Test1234!"),
            ("taras_headshot",  "taras.headshot@gmail.com",  "player", "Test1234!"),
            ("yulia_support",   "yulia.support@ukr.net",     "player", "Test1234!"),

            ("ivan_ace",        "ivan.ace@gmail.com",        "player", "Test1234!"),
            ("sofia_flash",     "sofia.flash@ukr.net",       "player", "Test1234!"),
            ("pavlo_storm",     "pavlo.storm@gmail.com",     "player", "Test1234!"),
            ("anna_stealth",    "anna.stealth@ukr.net",      "player", "Test1234!"),
            ("mykola_boom",     "mykola.boom@gmail.com",     "player", "Test1234!"),
            ("diana_hex",       "diana.hex@ukr.net",         "player", "Test1234!"),
            ("artem_wave",      "artem.wave@gmail.com",      "player", "Test1234!"),
            ("liliya_shield",   "liliya.shield@ukr.net",     "player", "Test1234!"),
            ("volodymyr_lex",   "volodymyr.lex@gmail.com",   "player", "Test1234!"),
            ("nina_volt",       "nina.volt@ukr.net",         "player", "Test1234!"),
            ("roman_phoenix",   "roman.phoenix@gmail.com",   "player", "Test1234!"),
            ("tetiana_luna",    "tetiana.luna@ukr.net",      "player", "Test1234!"),
            ("denys_knight",    "denys.knight@gmail.com",    "player", "Test1234!"),
            ("alina_beam",      "alina.beam@ukr.net",        "player", "Test1234!"),
            ("oleh_titan",      "oleh.titan@gmail.com",      "player", "Test1234!"),
            ("marina_echo",     "marina.echo@ukr.net",       "player", "Test1234!"),
            ("vitaliy_vex",     "vitaliy.vex@gmail.com",     "player", "Test1234!"),
            ("svitlana_pix",    "svitlana.pix@ukr.net",      "player", "Test1234!"),
            ("kyrylo_nova",     "kyrylo.nova@gmail.com",     "player", "Test1234!"),
            ("hanna_bloom",     "hanna.bloom@ukr.net",       "player", "Test1234!"),

            ("oleksiy_viper",   "oleksiy.viper@gmail.com",   "player", "Test1234!"),
            ("yana_cipher",     "yana.cipher@ukr.net",       "player", "Test1234!"),
            ("dmytro_quake",    "dmytro.quake@gmail.com",    "player", "Test1234!"),
            ("valentyna_flux",  "valentyna.flux@ukr.net",    "player", "Test1234!"),
            ("ostap_blitz",     "ostap.blitz@gmail.com",     "player", "Test1234!"),
            ("angelina_mist",   "angelina.mist@ukr.net",     "player", "Test1234!"),
            ("yevhen_cyber",    "yevhen.cyber@gmail.com",    "player", "Test1234!"),
            ("zoryana_drift",   "zoryana.drift@ukr.net",     "player", "Test1234!"),
            ("petro_nitro",     "petro.nitro@gmail.com",     "player", "Test1234!"),
            ("larysa_orbit",    "larysa.orbit@ukr.net",      "player", "Test1234!"),
            ("vasyl_shade",     "vasyl.shade@gmail.com",     "player", "Test1234!"),
            ("iryna_pulse",     "iryna.pulse@ukr.net",       "player", "Test1234!"),
        };

        var dict = new Dictionary<string, User>();

        foreach (var (username, email, role, password) in data)
        {
            var userId = DeterministicGuid.Create($"user-{username}");
            var query = role == "player" ? "ukrainian esports gamer portrait" : "ukrainian esports organizer portrait";

            var avatarUrl = EntityImageHelper.EnsureImage(userId, "avatars", query, unsplash);

            var user = new User
            {
                Id = userId,
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 12),
                Role = role,
                EmailConfirmed = true,
                Bio = $"Гравець платформи ArenaMaster — {username}. Любитель кіберспорту та активний учасник турнірів.",
                DiscordUrl = $"{username}#{Random.Shared.Next(1000, 9999)}",
                SocialLinks = [ $"https://twitch.tv/{username}", $"https://steamcommunity.com/id/{username}" ],
                AvatarUrl = avatarUrl,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(30, 365)),
                UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            dict[username] = user;
        }

        return dict;
    }
}

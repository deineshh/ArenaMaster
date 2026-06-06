using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Data.Seeders;

public static class DataSeeder
{
    private static readonly string[] FallbackUrls =
    [
        "https://images.unsplash.com/photo-1592372112489-0d6bf5e90906?w=600",
        "https://images.unsplash.com/photo-1561136319-da6393efaa7c?w=600",
        "https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600",
        "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=600",
        "https://images.unsplash.com/photo-1538481199705-c710c4e965fc?w=600",
        "https://images.unsplash.com/photo-1493711662062-fa541adb3fc8?w=600",
        "https://images.unsplash.com/photo-1605899435973-ca2d1a8861cf?w=600",
        "https://images.unsplash.com/photo-1560253023-3ec5d502959f?w=600",
    ];

    private static void CleanupOrphanedFiles(AppDbContext db)
    {
        var dirs = new[] { "disciplines", "avatars", "teams", "tournaments" };
        var validIds = new HashSet<string>();

        foreach (var d in db.Disciplines.Select(e => e.Id.ToString()).ToList()) validIds.Add(d);
        foreach (var u in db.Users.Select(e => e.Id.ToString()).ToList()) validIds.Add(u);
        foreach (var t in db.Teams.Select(e => e.Id.ToString()).ToList()) validIds.Add(t);
        foreach (var t in db.Tournaments.Select(e => e.Id.ToString()).ToList()) validIds.Add(t);

        foreach (var folder in dirs)
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", folder);
            if (!Directory.Exists(dir)) continue;
            foreach (var file in Directory.GetFiles(dir))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (!validIds.Contains(name))
                {
                    try { File.Delete(file); Console.WriteLine($"[Seeder] Видалено зайвий файл: {file}"); }
                    catch { }
                }
                else if (file.EndsWith(".svg"))
                {
                    try { File.Delete(file); Console.WriteLine($"[Seeder] Видалено застарілий SVG: {file}"); }
                    catch { }
                }
            }
        }
    }

    private static async Task FixupSvgUrls(AppDbContext db, UnsplashClient? unsplash)
    {
        var folders = new Dictionary<Type, (string folder, Func<AppDbContext, IQueryable<object>> query, Func<object, string> getUrl, Action<object, string> setUrl)>
        {
            [typeof(Discipline)] = ("disciplines",
                ctx => ctx.Disciplines.Cast<object>(),
                e => ((Discipline)e).CoverUrl ?? "",
                (e, url) => ((Discipline)e).CoverUrl = url),
            [typeof(User)] = ("avatars",
                ctx => ctx.Users.Cast<object>(),
                e => ((User)e).AvatarUrl ?? "",
                (e, url) => ((User)e).AvatarUrl = url),
            [typeof(Team)] = ("teams",
                ctx => ctx.Teams.Cast<object>(),
                e => ((Team)e).LogoUrl ?? "",
                (e, url) => ((Team)e).LogoUrl = url),
            [typeof(Tournament)] = ("tournaments",
                ctx => ctx.Tournaments.Cast<object>(),
                e => ((Tournament)e).CoverUrl ?? "",
                (e, url) => ((Tournament)e).CoverUrl = url),
        };

        var rng = new System.Random();
        var changed = false;

        foreach (var (folder, query, getUrl, setUrl) in folders.Values)
        {
            var entities = query(db).ToList();
            foreach (var entity in entities)
            {
                var url = getUrl(entity);
                if (!url.EndsWith(".svg")) continue;

                var idProp = entity.GetType().GetProperty("Id")!;
                var entityId = (Guid)idProp.GetValue(entity)!;
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", folder);
                Directory.CreateDirectory(uploadsDir);
                var jpgPath = Path.Combine(uploadsDir, $"{entityId}.jpg");
                var svgPath = Path.Combine(uploadsDir, $"{entityId}.svg");

                if (File.Exists(svgPath))
                {
                    try { File.Delete(svgPath); } catch { }
                }

                if (File.Exists(jpgPath))
                {
                    setUrl(entity, $"/uploads/{folder}/{entityId}.jpg");
                    changed = true;
                    Console.WriteLine($"[Seeder] Виправлено URL для {folder}/{entityId}: SVG→JPG (файл існував)");
                    continue;
                }

                var newUrl = unsplash?.DownloadAndSaveAsync(folder, entityId, "esports tournament")
                    .GetAwaiter().GetResult();

                if (newUrl == null)
                {
                    var fallbackUrl = FallbackUrls[rng.Next(FallbackUrls.Length)];
                    newUrl = UnsplashFallback.DownloadFromUrl(fallbackUrl, folder, entityId);
                }

                if (newUrl != null)
                {
                    setUrl(entity, newUrl);
                    UnsplashFallback.AddToPool(newUrl);
                    changed = true;
                    Console.WriteLine($"[Seeder] Виправлено URL для {folder}/{entityId}: SVG→JPG (завантажено)");
                }
                else
                {
                    Console.WriteLine($"[Seeder] НЕ ВДАЛОСЯ виправити URL для {folder}/{entityId}");
                }
            }
        }

        if (changed)
            await db.SaveChangesAsync();
    }

    private static void Preload(UnsplashClient? unsplash)
    {
        var rng = new System.Random();

        foreach (var folder in new[] { "avatars", "teams", "tournaments" })
        {
            for (var i = 0; i < 3; i++)
            {
                var id = DeterministicGuid.Create($"preload-{folder}-{i}");
                var query = folder switch
                {
                    "avatars" => "ukrainian esports gamer portrait",
                    "teams" => "esports team logo gaming",
                    "tournaments" => "esports tournament arena",
                    _ => "esports"
                };
                var url = unsplash?.DownloadAndSaveAsync(folder, id, query).GetAwaiter().GetResult();
                if (url != null)
                {
                    UnsplashFallback.AddToPool(url);
                    continue;
                }

                var fallbackUrl = FallbackUrls[rng.Next(FallbackUrls.Length)];
                url = UnsplashFallback.DownloadFromUrl(fallbackUrl, folder, id);
                if (url != null)
                    UnsplashFallback.AddToPool(url);
            }
        }
    }

    public static async Task SeedAsync(AppDbContext db, UnsplashClient? unsplash = null)
    {
        Preload(unsplash);

        if (!await db.Disciplines.AnyAsync())
        {
            var disciplines = DisciplineSeeder.Seed(db, unsplash);
            await db.SaveChangesAsync();
            Console.WriteLine($"[Seeder] Додано {disciplines.Count} дисциплін");
        }

        if (!await db.Users.AnyAsync())
        {
            var users = UserSeeder.Seed(db, unsplash);
            await db.SaveChangesAsync();
            Console.WriteLine($"[Seeder] Додано {users.Count} користувачів");

            var teams = TeamSeeder.Seed(db, users, unsplash);
            await db.SaveChangesAsync();
            Console.WriteLine($"[Seeder] Додано {teams.Count} команд");

            TournamentSeeder.Seed(db, users, teams, unsplash);
            await db.SaveChangesAsync();
            Console.WriteLine($"[Seeder] Додано {await db.Tournaments.CountAsync()} турнірів");

            MatchSeeder.Seed(db);
            await db.SaveChangesAsync();
            Console.WriteLine($"[Seeder] Додано {await db.Matches.CountAsync()} матчів");

            NotificationSeeder.Seed(db);
            await db.SaveChangesAsync();
            Console.WriteLine($"[Seeder] Додано {await db.Notifications.CountAsync()} сповіщень");

            CleanupOrphanedFiles(db);
        }

        await FixupSvgUrls(db, unsplash);
        Console.WriteLine("[Seeder] Ініціалізація даних завершена");
    }
}

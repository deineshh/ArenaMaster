using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Data.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Disciplines.AnyAsync())
        {
            DisciplineSeeder.Seed(db);
            await db.SaveChangesAsync();
        }

        if (!await db.Users.AnyAsync())
        {
            var disciplines = await db.Disciplines.ToListAsync();
            var users = UserSeeder.Seed(db);
            await db.SaveChangesAsync();

            TeamSeeder.Seed(db, users);
            await db.SaveChangesAsync();

            TournamentSeeder.Seed(db, disciplines, users);
            await db.SaveChangesAsync();

            MatchSeeder.Seed(db);
            await db.SaveChangesAsync();
        }
    }
}

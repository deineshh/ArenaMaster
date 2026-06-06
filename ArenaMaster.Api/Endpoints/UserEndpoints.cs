using ArenaMaster.Api.Data;
using ArenaMaster.Api.DTOs.User;
using ArenaMaster.Api.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users");

        group.MapGet("/me", GetMe).RequireAuthorization()
            .WithSummary("Отримати мій профіль")
            .WithDescription("Повертає профіль поточного автентифікованого користувача зі статистикою.")
            .Produces<UserProfileDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Users");

        group.MapPut("/me", UpdateMe).RequireAuthorization()
            .WithSummary("Оновити мій профіль")
            .WithDescription("Оновлює bio, Discord URL та соціальні посилання. Поля зі значенням null не змінюються.")
            .Accepts<UpdateProfileRequest>("application/json")
            .Produces<UserProfileDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Users");

        group.MapPost("/me/avatar", UploadAvatar).RequireAuthorization().DisableAntiforgery()
            .WithSummary("Завантажити аватар")
            .WithDescription("Завантажує файл зображення як аватар користувача. Приймає multipart/form-data. Якщо файл некоректний, автоматично генерує аватар через Unsplash.")
            .Produces<AvatarUploadResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("Users");

        group.MapGet("/{username}", GetPublicProfile)
            .WithSummary("Публічний профіль користувача")
            .WithDescription("Повертає публічну інформацію про користувача та список його команд.")
            .Produces<PublicProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Users");

        group.MapGet("/{username}/tournaments", GetTournamentHistory)
            .WithSummary("Історія турнірів користувача")
            .WithDescription("Повертає історію участі користувача в турнірах.")
            .Produces<List<TournamentHistoryItemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Users");
    }

    private static async Task<IResult> GetMe(System.Security.Claims.ClaimsPrincipal principal, AppDbContext db)
    {
        var userId = principal.GetUserId();
        if (userId is null) return Results.Unauthorized();

        var user = await db.Users.FindAsync(userId.Value);
        if (user is null) return Results.NotFound();

        var stats = await GetStats(db, user.Id);
        return Results.Ok(new UserProfileDto(
            user.Id, user.Username, user.AvatarUrl, user.Bio, user.DiscordUrl,
            user.SocialLinks, stats.tournaments, stats.wins, stats.matches));
    }

    private static async Task<IResult> UpdateMe(
        UpdateProfileRequest req, System.Security.Claims.ClaimsPrincipal principal, AppDbContext db)
    {
        var userId = principal.GetUserId();
        if (userId is null) return Results.Unauthorized();

        var user = await db.Users.FindAsync(userId.Value);
        if (user is null) return Results.NotFound();

        if (req.Bio is not null) user.Bio = req.Bio;
        if (req.DiscordUrl is not null) user.DiscordUrl = req.DiscordUrl;
        if (req.SocialLinks is not null) user.SocialLinks = req.SocialLinks.Take(3).ToList();
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var stats = await GetStats(db, user.Id);
        return Results.Ok(new UserProfileDto(
            user.Id, user.Username, user.AvatarUrl, user.Bio, user.DiscordUrl,
            user.SocialLinks, stats.tournaments, stats.wins, stats.matches));
    }

    private static async Task<IResult> UploadAvatar(
        IFormFile file, System.Security.Claims.ClaimsPrincipal principal, AppDbContext db,
        IWebHostEnvironment env, UnsplashClient unsplash)
    {
        var userId = principal.GetUserId();
        if (userId is null) return Results.Unauthorized();

        var user = await db.Users.FindAsync(userId.Value);
        if (user is null) return Results.NotFound();

        var path = await FileUploadHelper.SaveUploadedFileAsync(file, env, "avatars", user.Id);
        if (path is null)
        {
            path = await unsplash.DownloadAndSaveAsync("avatars", user.Id, "gaming portrait avatar");
            if (path is null) return Results.BadRequest(new { message = "Невірний файл" });
        }

        user.AvatarUrl = path;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(new AvatarUploadResponse(path));
    }

    private static async Task<IResult> GetPublicProfile(string username, AppDbContext db)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null) return Results.NotFound();

        var stats = await GetStats(db, user.Id);
        var teams = await db.TeamMembers
            .Where(m => m.UserId == user.Id)
            .Include(m => m.Team)
            .Select(m => new { m.Team.Id, m.Team.Name, m.Team.Slug, m.Team.LogoUrl, m.Role })
            .ToListAsync();

        return Results.Ok(new PublicProfileResponse(
            new UserProfileDto(
                user.Id, user.Username, user.AvatarUrl, user.Bio, user.DiscordUrl,
                user.SocialLinks, stats.tournaments, stats.wins, stats.matches),
            teams));
    }

    private static async Task<IResult> GetTournamentHistory(string username, AppDbContext db)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null) return Results.NotFound();

        var items = await db.TournamentParticipants
            .Where(p => p.UserId == user.Id && p.Status == "accepted")
            .Include(p => p.Tournament)
            .OrderByDescending(p => p.RegisteredAt)
            .Select(p => new TournamentHistoryItemDto(
                p.TournamentId,
                p.Tournament.Title,
                p.Tournament.Slug,
                p.Tournament.Status,
                p.Tournament.Status == "finished" ? "участь" : null))
            .ToListAsync();

        return Results.Ok(items);
    }

    private static async Task<(int tournaments, int wins, int matches)> GetStats(AppDbContext db, Guid userId)
    {
        var participations = await db.TournamentParticipants
            .Where(p => p.UserId == userId && p.Status == "accepted")
            .Select(p => p.Id)
            .ToListAsync();

        var wins = await db.Matches.CountAsync(m =>
            m.WinnerId != null && participations.Contains(m.WinnerId.Value));

        var matches = await db.Matches.CountAsync(m =>
            (m.Participant1Id != null && participations.Contains(m.Participant1Id.Value)) ||
            (m.Participant2Id != null && participations.Contains(m.Participant2Id.Value)));

        return (participations.Count, wins, matches);
    }
}

public record AvatarUploadResponse(string AvatarUrl);
public record PublicProfileResponse(UserProfileDto Profile, object Teams);

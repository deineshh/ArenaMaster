using System.Security.Claims;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin").RequireAuthorization();

        group.MapGet("/users", ListUsers)
            .WithSummary("Список користувачів (адмін-панель)")
            .WithDescription("Повертає пагінований список усіх користувачів системи. Потрібна роль admin.")
            .Produces<AdminUserListResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapPatch("/users/{id:guid}/block", ToggleBlock)
            .WithSummary("Заблокувати / розблокувати користувача")
            .WithDescription("Перемикає статус блокування користувача. Заблоковані користувачі не можуть увійти. Потрібна роль admin.")
            .Produces<BlockStatusResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        group.MapPatch("/users/{id:guid}/role", ChangeRole)
            .WithSummary("Змінити роль користувача")
            .WithDescription("Змінює роль користувача. Доступні ролі: `player`, `organizer`, `admin`. Потрібна роль admin.")
            .Accepts<ChangeRoleRequest>("application/json")
            .Produces<RoleChangeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        group.MapDelete("/users/{id:guid}", DeleteUser)
            .WithSummary("Видалити користувача")
            .WithDescription("Повністю видаляє користувача з системи. Потрібна роль admin.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        group.MapGet("/tournaments", ListAllTournaments)
            .WithSummary("Список турнірів (адмін-панель)")
            .WithDescription("Повертає всі турніри, включаючи чернетки. Потрібна роль admin.")
            .Produces<List<AdminTournamentItem>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapDelete("/tournaments/{id:guid}", DeleteTournament)
            .WithSummary("Видалити турнір")
            .WithDescription("Видаляє турнір разом із усіма учасниками та матчами. Потрібна роль admin.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        group.MapDelete("/teams/{id:guid}", DeleteTeam)
            .WithSummary("Видалити команду")
            .WithDescription("Видаляє команду разом із її складом та запрошеннями. Потрібна роль admin.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");
    }

    private static async Task<IResult> ListUsers(ClaimsPrincipal principal, AppDbContext db, int page = 1, int pageSize = 20)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var total = await db.Users.CountAsync();
        var items = await db.Users
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserItem(u.Id, u.Username, u.Email, u.Role, u.IsBlocked, u.EmailConfirmed, u.CreatedAt))
            .ToListAsync();

        return Results.Ok(new AdminUserListResponse(items, total, page, pageSize));
    }

    private static async Task<IResult> ToggleBlock(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();
        var user = await db.Users.FindAsync(id);
        if (user is null) return Results.NotFound();
        user.IsBlocked = !user.IsBlocked;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(new BlockStatusResponse(user.IsBlocked));
    }

    private static async Task<IResult> ChangeRole(
        Guid id, [FromBody] ChangeRoleRequest req, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();
        if (req.Role is not ("player" or "organizer" or "admin"))
            return Results.BadRequest(new { message = "Невірна роль" });

        var user = await db.Users.FindAsync(id);
        if (user is null) return Results.NotFound();
        user.Role = req.Role;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(new RoleChangeResponse(user.Role));
    }

    private static async Task<IResult> DeleteUser(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();
        var user = await db.Users.FindAsync(id);
        if (user is null) return Results.NotFound();
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> ListAllTournaments(ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var items = await db.Tournaments
            .Include(t => t.Discipline)
            .Include(t => t.Organizer)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new AdminTournamentItem(
                t.Id, t.Title, t.Slug, t.Discipline.Name,
                t.Organizer.Username, t.Status, t.StartsAt,
                t.Participants.Count(p => p.Status == "accepted")))
            .ToListAsync();

        return Results.Ok(items);
    }

    private static async Task<IResult> DeleteTournament(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();
        var t = await db.Tournaments
            .Include(x => x.Participants)
            .Include(x => x.Matches)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return Results.NotFound();

        db.Matches.RemoveRange(t.Matches);
        db.TournamentParticipants.RemoveRange(t.Participants);
        db.Tournaments.Remove(t);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteTeam(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();
        var team = await db.Teams
            .Include(t => t.Members)
            .Include(t => t.Invitations)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (team is null) return Results.NotFound();

        db.TeamMembers.RemoveRange(team.Members);
        db.TeamInvitations.RemoveRange(team.Invitations);
        db.Teams.Remove(team);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}

public record ChangeRoleRequest(string Role);
public record AdminUserItem(Guid Id, string Username, string Email, string Role, bool IsBlocked, bool EmailConfirmed, DateTime CreatedAt);
public record AdminUserListResponse(List<AdminUserItem> Items, int Total, int Page, int PageSize);
public record BlockStatusResponse(bool IsBlocked);
public record RoleChangeResponse(string Role);
public record AdminTournamentItem(Guid Id, string Title, string Slug, string Discipline, string Organizer, string Status, DateTime StartsAt, int Participants);

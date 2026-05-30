using System.Security.Claims;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin").RequireAuthorization();

        group.MapGet("/users", ListUsers);
        group.MapPatch("/users/{id:guid}/block", ToggleBlock);
        group.MapPatch("/users/{id:guid}/role", ChangeRole);
        group.MapDelete("/users/{id:guid}", DeleteUser);
        group.MapGet("/tournaments", ListAllTournaments);
        group.MapDelete("/tournaments/{id:guid}", DeleteTournament);
        group.MapDelete("/teams/{id:guid}", DeleteTeam);
    }

    private static IResult RequireAdmin(ClaimsPrincipal principal)
    {
        return principal.IsInRole("admin") ? Results.Ok() : Results.Forbid();
    }

    private static async Task<IResult> ListUsers(ClaimsPrincipal principal, AppDbContext db, int page = 1, int pageSize = 20)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var total = await db.Users.CountAsync();
        var items = await db.Users
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new { u.Id, u.Username, u.Email, u.Role, u.IsBlocked, u.EmailConfirmed, u.CreatedAt })
            .ToListAsync();

        return Results.Ok(new { items, total, page, pageSize });
    }

    private static async Task<IResult> ToggleBlock(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();
        var user = await db.Users.FindAsync(id);
        if (user is null) return Results.NotFound();
        user.IsBlocked = !user.IsBlocked;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(new { user.IsBlocked });
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
        return Results.Ok(new { user.Role });
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
            .Select(t => new
            {
                t.Id, t.Title, t.Slug, Discipline = t.Discipline.Name,
                Organizer = t.Organizer.Username, t.Status, t.StartsAt,
                Participants = t.Participants.Count(p => p.Status == "accepted")
            })
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

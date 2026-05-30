using System.Security.Claims;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notifications").RequireAuthorization();

        group.MapGet("/", List);
        group.MapPost("/read-all", MarkAllRead);
        group.MapPatch("/{id:guid}/read", MarkRead);
    }

    private static async Task<IResult> List(ClaimsPrincipal principal, AppDbContext db, int page = 1, int pageSize = 20)
    {
        var userId = principal.GetUserId()!;
        var items = await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id, n.Type, n.Title, n.Body, n.EntityType, n.EntityId, n.IsRead, n.CreatedAt
            })
            .ToListAsync();

        var unread = await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        return Results.Ok(new { items, unread });
    }

    private static async Task<IResult> MarkAllRead(ClaimsPrincipal principal, AppDbContext db)
    {
        var userId = principal.GetUserId()!;
        await db.Notifications.Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        return Results.Ok();
    }

    private static async Task<IResult> MarkRead(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        var userId = principal.GetUserId()!;
        var notification = await db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (notification is null) return Results.NotFound();

        notification.IsRead = true;
        await db.SaveChangesAsync();
        return Results.Ok();
    }
}

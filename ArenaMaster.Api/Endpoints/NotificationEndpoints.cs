using System.Security.Claims;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notifications").RequireAuthorization();

        group.MapGet("/", List)
            .WithSummary("Список сповіщень")
            .WithDescription("Повертає пагінований список сповіщень поточного користувача разом із кількістю непрочитаних.")
            .Produces<NotificationListResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("Notifications");

        group.MapPost("/read-all", MarkAllRead)
            .WithSummary("Позначити всі сповіщення прочитаними")
            .WithDescription("Позначає всі непрочитані сповіщення користувача як прочитані.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("Notifications");

        group.MapPatch("/{id:guid}/read", MarkRead)
            .WithSummary("Позначити сповіщення прочитаним")
            .WithDescription("Позначає окреме сповіщення як прочитане.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Notifications");
    }

    private static async Task<IResult> List(ClaimsPrincipal principal, AppDbContext db, int page = 1, int pageSize = 20)
    {
        var userId = principal.GetUserId()!;
        var items = await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationItem(
                n.Id, n.Type, n.Title, n.Body, n.EntityType, n.EntityId, n.IsRead, n.CreatedAt))
            .ToListAsync();

        var unread = await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        return Results.Ok(new NotificationListResponse(items, unread));
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

public record NotificationItem(Guid Id, string Type, string Title, string Body, string? EntityType, Guid? EntityId, bool IsRead, DateTime CreatedAt);
public record NotificationListResponse(List<NotificationItem> Items, int Unread);

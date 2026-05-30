using ArenaMaster.Api.Data;
using ArenaMaster.Api.Models;

namespace ArenaMaster.Api.Helpers;

public static class NotificationHelper
{
    public static async Task CreateAsync(
        AppDbContext db,
        Guid userId,
        string type,
        string title,
        string body,
        string? entityType = null,
        Guid? entityId = null)
    {
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            EntityType = entityType,
            EntityId = entityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}

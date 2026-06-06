using System.Security.Claims;
using ArenaMaster.Api.Models;

namespace ArenaMaster.Api.Helpers;

public static class AuthExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var guid) ? guid : null;
    }

    public static bool IsInRole(this ClaimsPrincipal user, string role) =>
        user.IsInRole(role) || user.FindFirstValue(ClaimTypes.Role) == role;

    public static IResult ForbidRole() => Results.Forbid();

    public static IResult RequireRole(ClaimsPrincipal user, params string[] roles)
    {
        if (roles.Any(user.IsInRole))
            return Results.Ok();
        return Results.Forbid();
    }
}

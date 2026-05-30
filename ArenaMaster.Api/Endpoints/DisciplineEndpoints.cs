using System.Security.Claims;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Endpoints;

public static class DisciplineEndpoints
{
    public static void MapDisciplineEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/disciplines");

        group.MapGet("/", List)
            .WithSummary("Список дисциплін")
            .WithDescription("Повертає всі доступні кіберспортивні дисципліни (ігри), відсортовані за назвою.")
            .Produces<List<DisciplineItem>>(StatusCodes.Status200OK)
            .WithTags("Disciplines");

        group.MapPost("/", Create).RequireAuthorization()
            .WithSummary("Створити дисципліну")
            .WithDescription("Додає нову дисципліну. Потрібна роль admin. Автоматично генерує обкладинку.")
            .Accepts<CreateDisciplineRequest>("application/json")
            .Produces<DisciplineItem>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Disciplines");

        group.MapPut("/{id:guid}", Update).RequireAuthorization()
            .WithSummary("Оновити дисципліну")
            .WithDescription("Оновлює назву дисципліни. Потрібна роль admin.")
            .Accepts<CreateDisciplineRequest>("application/json")
            .Produces<DisciplineItem>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Disciplines");

        group.MapDelete("/{id:guid}", Delete).RequireAuthorization()
            .WithSummary("Видалити дисципліну")
            .WithDescription("Видаляє дисципліну. Потрібна роль admin.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Disciplines");
    }

    private static async Task<IResult> List(AppDbContext db) =>
        Results.Ok(await db.Disciplines.OrderBy(d => d.Name)
            .Select(d => new DisciplineItem(d.Id, d.Name, d.Slug, d.CoverUrl))
            .ToListAsync());

    private static async Task<IResult> Create(
        [FromBody] CreateDisciplineRequest req, ClaimsPrincipal principal, AppDbContext db, UnsplashClient unsplash)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var slug = SlugHelper.Generate(req.Name);
        var d = new Discipline { Id = Guid.NewGuid(), Name = req.Name, Slug = slug };
        d.CoverUrl = await unsplash.DownloadAndSaveAsync("disciplines", d.Id, req.Name.ToLower());
        db.Disciplines.Add(d);
        await db.SaveChangesAsync();
        return Results.Created($"/api/disciplines/{d.Id}", new DisciplineItem(d.Id, d.Name, d.Slug, d.CoverUrl));
    }

    private static async Task<IResult> Update(
        Guid id, [FromBody] CreateDisciplineRequest req, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();
        var d = await db.Disciplines.FindAsync(id);
        if (d is null) return Results.NotFound();
        d.Name = req.Name;
        d.Slug = SlugHelper.Generate(req.Name);
        await db.SaveChangesAsync();
        return Results.Ok(new DisciplineItem(d.Id, d.Name, d.Slug, d.CoverUrl));
    }

    private static async Task<IResult> Delete(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();
        var d = await db.Disciplines.FindAsync(id);
        if (d is null) return Results.NotFound();
        db.Disciplines.Remove(d);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}

public record CreateDisciplineRequest(string Name);
public record DisciplineItem(Guid Id, string Name, string Slug, string? CoverUrl);

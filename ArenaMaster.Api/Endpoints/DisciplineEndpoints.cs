using System.Security.Claims;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Endpoints;

public static class DisciplineEndpoints
{
    public static void MapDisciplineEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/disciplines");

        group.MapGet("/", List);
        group.MapPost("/", Create).RequireAuthorization();
        group.MapPut("/{id:guid}", Update).RequireAuthorization();
        group.MapDelete("/{id:guid}", Delete).RequireAuthorization();
    }

    private static async Task<IResult> List(AppDbContext db) =>
        Results.Ok(await db.Disciplines.OrderBy(d => d.Name).Select(d => new { d.Id, d.Name, d.Slug, d.CoverUrl }).ToListAsync());

    private static async Task<IResult> Create(
        [FromBody] CreateDisciplineRequest req, ClaimsPrincipal principal, AppDbContext db, UnsplashClient unsplash)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var slug = SlugHelper.Generate(req.Name);
        var d = new Discipline { Id = Guid.NewGuid(), Name = req.Name, Slug = slug };
        d.CoverUrl = await unsplash.DownloadAndSaveAsync("disciplines", d.Id, req.Name.ToLower());
        db.Disciplines.Add(d);
        await db.SaveChangesAsync();
        return Results.Created($"/api/disciplines/{d.Id}", d);
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
        return Results.Ok(d);
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

using System.Security.Claims;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.DTOs.Team;
using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
using ArenaMaster.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Endpoints;

public static class TeamEndpoints
{
    public static void MapTeamEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/teams");

        group.MapGet("/", ListTeams)
            .WithSummary("Список команд")
            .WithDescription("Повертає пагінований список усіх команд, відсортованих за датою створення.")
            .Produces<PaginatedResult<TeamListItemDto>>(StatusCodes.Status200OK)
            .WithTags("Teams");

        group.MapPost("/", CreateTeam).RequireAuthorization()
            .WithSummary("Створити команду")
            .WithDescription("Створює нову команду. Користувач автоматично стає капітаном. Максимум одна команда на користувача. Автоматично генерує логотип.")
            .Accepts<CreateTeamRequest>("application/json")
            .Produces<CreateResult>(StatusCodes.Status201Created)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("Teams");

        group.MapGet("/{slug}", GetTeam)
            .WithSummary("Деталі команди")
            .WithDescription("Повертає повну інформацію про команду: назву, склад, статистику турнірів та перемог.")
            .Produces<TeamDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Teams");

        group.MapPut("/{id:guid}", UpdateTeam).RequireAuthorization()
            .WithSummary("Оновити назву команди")
            .WithDescription("Оновлює назву команди. Доступно лише капітану.")
            .Accepts<UpdateTeamRequest>("application/json")
            .Produces<TeamUpdateResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Teams");

        group.MapPost("/{id:guid}/logo", UploadLogo).RequireAuthorization().DisableAntiforgery()
            .WithSummary("Завантажити логотип команди")
            .WithDescription("Завантажує логотип команди. Доступно лише капітану. Приймає multipart/form-data.")
            .Produces<LogoUploadResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Teams");

        group.MapDelete("/{id:guid}", DisbandTeam).RequireAuthorization()
            .WithSummary("Розформувати команду")
            .WithDescription("Видаляє команду та всі її склади. Доступно лише капітану.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Teams");

        group.MapPost("/{id:guid}/invitations", InvitePlayer).RequireAuthorization()
            .WithSummary("Запросити гравця")
            .WithDescription("Надсилає запрошення гравцю за нікнеймом або email. Доступно лише капітану. Гравцю надсилається сповіщення.")
            .Accepts<InvitePlayerRequest>("application/json")
            .Produces<InviteResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Teams");

        group.MapGet("/invitations/my", MyInvitations).RequireAuthorization()
            .WithSummary("Мої запрошення до команд")
            .WithDescription("Повертає список активних (очікуючих) запрошень для поточного користувача.")
            .Produces<List<TeamInvitationDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("Teams");

        group.MapPost("/invitations/{invId:guid}/accept", AcceptInvitation).RequireAuthorization()
            .WithSummary("Прийняти запрошення")
            .WithDescription("Приймає запрошення до команди. Користувача додають як учасника.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Teams");

        group.MapPost("/invitations/{invId:guid}/decline", DeclineInvitation).RequireAuthorization()
            .WithSummary("Відхилити запрошення")
            .WithDescription("Відхиляє запрошення до команди.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Teams");

        group.MapDelete("/{id:guid}/members/{userId:guid}", RemoveMember).RequireAuthorization()
            .WithSummary("Виключити гравця з команди")
            .WithDescription("Виключає гравця з команди. Доступно лише капітану. Капітана виключити неможливо. Гравцю надсилається сповіщення.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Teams");
    }

    private static async Task<IResult> ListTeams(AppDbContext db, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var query = db.Teams.Include(t => t.Captain).Include(t => t.Members);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TeamListItemDto(
                t.Id, t.Name, t.Slug, t.LogoUrl, t.Captain.Username, t.Members.Count))
            .ToListAsync();

        return Results.Ok(new PaginatedResult<TeamListItemDto>(items, total, page, pageSize));
    }

    private static async Task<IResult> CreateTeam(
        CreateTeamRequest req, ClaimsPrincipal principal, AppDbContext db,
        UnsplashClient unsplash, IValidator<CreateTeamRequest> validator)
    {
        var validation = await validator.ValidateAsync(req);
        if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

        var userId = principal.GetUserId()!;
        if (await db.Teams.AnyAsync(t => t.CaptainId == userId))
            return Results.BadRequest(new { message = "Ви вже капітан команди" });

        var slug = await SlugHelper.EnsureUniqueAsync(
            async s => await db.Teams.AnyAsync(t => t.Slug == s),
            SlugHelper.Generate(req.Name));

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Slug = slug,
            CaptainId = userId.Value,
            CreatedAt = DateTime.UtcNow
        };

        team.LogoUrl = await unsplash.DownloadAndSaveAsync("teams", team.Id, "esports team logo abstract");
        if (team.LogoUrl is null)
        {
            PlaceholderImageGenerator.WriteTeamLogo(team.Name, team.Id);
            team.LogoUrl = $"/uploads/teams/{team.Id}.svg";
        }

        db.Teams.Add(team);
        db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = team.Id,
            UserId = userId.Value,
            Role = "captain",
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return Results.Created($"/api/teams/{team.Slug}", new CreateResult(team.Id, team.Slug));
    }

    private static async Task<IResult> GetTeam(string slug, AppDbContext db)
    {
        var team = await db.Teams
            .Include(t => t.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Slug == slug);
        if (team is null) return Results.NotFound();

        var tournamentCount = await db.TournamentParticipants.CountAsync(p =>
            p.TeamId == team.Id && p.Status == "accepted");

        var wins = await db.Matches.CountAsync(m => m.WinnerId != null &&
            db.TournamentParticipants.Any(p => p.TeamId == team.Id && p.Id == m.WinnerId));

        return Results.Ok(new TeamDetailDto(
            team.Id, team.Name, team.Slug, team.LogoUrl, team.CreatedAt,
            team.Members.Select(m => new TeamMemberDto(m.UserId, m.User.Username, m.User.AvatarUrl, m.Role)).ToList(),
            tournamentCount, wins));
    }

    private static async Task<IResult> UpdateTeam(
        Guid id, UpdateTeamRequest req, ClaimsPrincipal principal, AppDbContext db)
    {
        var team = await db.Teams.FindAsync(id);
        if (team is null) return Results.NotFound();
        if (team.CaptainId != principal.GetUserId()) return Results.Forbid();

        team.Name = req.Name;
        await db.SaveChangesAsync();
        return Results.Ok(new TeamUpdateResponse(team.Id, team.Name));
    }

    private static async Task<IResult> UploadLogo(
        Guid id, IFormFile file, ClaimsPrincipal principal, AppDbContext db,
        IWebHostEnvironment env, UnsplashClient unsplash)
    {
        var team = await db.Teams.FindAsync(id);
        if (team is null) return Results.NotFound();
        if (team.CaptainId != principal.GetUserId()) return Results.Forbid();

        var path = await FileUploadHelper.SaveUploadedFileAsync(file, env, "teams", team.Id);
        path ??= await unsplash.DownloadAndSaveAsync("teams", team.Id, "esports team logo abstract");
        if (path is null) return Results.BadRequest(new { message = "Невірний файл" });

        team.LogoUrl = path;
        await db.SaveChangesAsync();
        return Results.Ok(new LogoUploadResponse(path));
    }

    private static async Task<IResult> DisbandTeam(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        var team = await db.Teams.Include(t => t.Members).FirstOrDefaultAsync(t => t.Id == id);
        if (team is null) return Results.NotFound();
        if (team.CaptainId != principal.GetUserId()) return Results.Forbid();

        db.TeamMembers.RemoveRange(team.Members);
        db.Teams.Remove(team);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> InvitePlayer(
        Guid id, InvitePlayerRequest req, ClaimsPrincipal principal, AppDbContext db)
    {
        var team = await db.Teams.FindAsync(id);
        if (team is null) return Results.NotFound();
        if (team.CaptainId != principal.GetUserId()) return Results.Forbid();

        User? invitee = null;
        if (!string.IsNullOrEmpty(req.Username))
            invitee = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        else if (!string.IsNullOrEmpty(req.Email))
            invitee = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

        if (invitee is null) return Results.NotFound(new { message = "Гравця не знайдено" });
        if (await db.TeamMembers.AnyAsync(m => m.TeamId == id && m.UserId == invitee.Id))
            return Results.BadRequest(new { message = "Гравець вже у команді" });

        var invitation = new TeamInvitation
        {
            Id = Guid.NewGuid(),
            TeamId = id,
            InviteeId = invitee.Id,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };
        db.TeamInvitations.Add(invitation);
        await db.SaveChangesAsync();

        await NotificationHelper.CreateAsync(db, invitee.Id, "team_invitation",
            "Запрошення до команди", $"Вас запросили до команди {team.Name}", "team", team.Id);

        return Results.Ok(new InviteResponse(invitation.Id));
    }

    private static async Task<IResult> MyInvitations(ClaimsPrincipal principal, AppDbContext db)
    {
        var userId = principal.GetUserId()!;
        var items = await db.TeamInvitations
            .Where(i => i.InviteeId == userId && i.Status == "pending")
            .Include(i => i.Team)
            .Select(i => new TeamInvitationDto(i.Id, i.TeamId, i.Team.Name, i.CreatedAt))
            .ToListAsync();
        return Results.Ok(items);
    }

    private static async Task<IResult> AcceptInvitation(Guid invId, ClaimsPrincipal principal, AppDbContext db)
    {
        var userId = principal.GetUserId()!;
        var inv = await db.TeamInvitations.Include(i => i.Team).FirstOrDefaultAsync(i => i.Id == invId);
        if (inv is null || inv.InviteeId != userId) return Results.NotFound();

        inv.Status = "accepted";
        db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = inv.TeamId,
            UserId = userId.Value,
            Role = "member",
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> DeclineInvitation(Guid invId, ClaimsPrincipal principal, AppDbContext db)
    {
        var userId = principal.GetUserId()!;
        var inv = await db.TeamInvitations.FirstOrDefaultAsync(i => i.Id == invId);
        if (inv is null || inv.InviteeId != userId) return Results.NotFound();

        inv.Status = "declined";
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> RemoveMember(Guid id, Guid userId, ClaimsPrincipal principal, AppDbContext db)
    {
        var team = await db.Teams.FindAsync(id);
        if (team is null) return Results.NotFound();
        if (team.CaptainId != principal.GetUserId()) return Results.Forbid();
        if (userId == team.CaptainId) return Results.BadRequest(new { message = "Не можна виключити капітана" });

        var member = await db.TeamMembers.FirstOrDefaultAsync(m => m.TeamId == id && m.UserId == userId);
        if (member is null) return Results.NotFound();

        db.TeamMembers.Remove(member);
        await db.SaveChangesAsync();

        await NotificationHelper.CreateAsync(db, userId, "team_removed",
            "Виключено з команди", $"Вас виключили з команди {team.Name}", "team", team.Id);

        return Results.NoContent();
    }
}

public record PaginatedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public record CreateResult(Guid Id, string Slug);
public record TeamUpdateResponse(Guid Id, string Name);
public record LogoUploadResponse(string LogoUrl);
public record InviteResponse(Guid InvitationId);
public record TeamInvitationDto(Guid Id, Guid TeamId, string TeamName, DateTime CreatedAt);

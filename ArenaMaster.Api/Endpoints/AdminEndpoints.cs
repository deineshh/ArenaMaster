using System.Security.Claims;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
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

        // --- Disciplines ---
        group.MapGet("/disciplines", ListDisciplines)
            .WithSummary("Список дисциплін")
            .WithDescription("Повертає список усіх дисциплін із кількістю турнірів. Потрібна роль admin.")
            .Produces<List<AdminDisciplineItem>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapPost("/disciplines", CreateDiscipline)
            .WithSummary("Створити дисципліну")
            .WithDescription("Створює нову дисципліну. Потрібна роль admin.")
            .Accepts<AdminCreateDisciplineRequest>("application/json")
            .Produces<AdminDisciplineItem>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapPut("/disciplines/{id:guid}", UpdateDiscipline)
            .WithSummary("Оновити дисципліну")
            .WithDescription("Оновлює назву дисципліни. Потрібна роль admin.")
            .Accepts<AdminUpdateDisciplineRequest>("application/json")
            .Produces<AdminDisciplineItem>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        group.MapDelete("/disciplines/{id:guid}", DeleteDiscipline)
            .WithSummary("Видалити дисципліну")
            .WithDescription("Видаляє дисципліну. Якщо існують турніри, що її використовують, повертає помилку. Потрібна роль admin.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        // --- Teams ---
        group.MapGet("/teams", ListTeams)
            .WithSummary("Список команд")
            .WithDescription("Повертає список усіх команд із капітаном та кількістю учасників. Потрібна роль admin.")
            .Produces<List<AdminTeamItem>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapPost("/teams", CreateTeam)
            .WithSummary("Створити команду")
            .WithDescription("Створює нову команду. Потрібна роль admin.")
            .Accepts<AdminCreateTeamRequest>("application/json")
            .Produces<AdminTeamItem>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapPut("/teams/{id:guid}", UpdateTeam)
            .WithSummary("Оновити команду")
            .WithDescription("Оновлює назву команди. Потрібна роль admin.")
            .Accepts<AdminUpdateTeamRequest>("application/json")
            .Produces<AdminTeamItem>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        // --- Matches ---
        group.MapGet("/matches", ListMatches)
            .WithSummary("Список матчів")
            .WithDescription("Повертає список усіх матчів із турніром та учасниками. Потрібна роль admin.")
            .Produces<List<AdminMatchItem>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapGet("/matches/{id:guid}", GetMatch)
            .WithSummary("Деталі матчу")
            .WithDescription("Повертає детальну інформацію про матч. Потрібна роль admin.")
            .Produces<AdminMatchDetail>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        group.MapPost("/matches", CreateMatch)
            .WithSummary("Створити матч")
            .WithDescription("Створює новий матч. Потрібна роль admin.")
            .Accepts<AdminCreateMatchRequest>("application/json")
            .Produces<AdminMatchDetail>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapPut("/matches/{id:guid}", UpdateMatch)
            .WithSummary("Оновити матч")
            .WithDescription("Оновлює дані матчу (рахунок, переможця, статус, дати). Потрібна роль admin.")
            .Accepts<AdminUpdateMatchRequest>("application/json")
            .Produces<AdminMatchDetail>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        group.MapDelete("/matches/{id:guid}", DeleteMatch)
            .WithSummary("Видалити матч")
            .WithDescription("Видаляє матч. Потрібна роль admin.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        // --- Participants ---
        group.MapGet("/participants", ListParticipants)
            .WithSummary("Список учасників турнірів")
            .WithDescription("Повертає список усіх учасників турнірів. Потрібна роль admin.")
            .Produces<List<AdminParticipantItem>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapGet("/participants/{id:guid}", GetParticipant)
            .WithSummary("Деталі учасника турніру")
            .WithDescription("Повертає детальну інформацію про учасника турніру. Потрібна роль admin.")
            .Produces<AdminParticipantDetail>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        group.MapPost("/participants", CreateParticipant)
            .WithSummary("Додати учасника турніру")
            .WithDescription("Додає нового учасника до турніру. Потрібна роль admin.")
            .Accepts<AdminCreateParticipantRequest>("application/json")
            .Produces<AdminParticipantDetail>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapPatch("/participants/{id:guid}/status", UpdateParticipantStatus)
            .WithSummary("Змінити статус учасника")
            .WithDescription("Змінює статус учасника турніру. Потрібна роль admin.")
            .Accepts<AdminUpdateParticipantStatusRequest>("application/json")
            .Produces<AdminParticipantDetail>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        group.MapDelete("/participants/{id:guid}", DeleteParticipant)
            .WithSummary("Видалити учасника турніру")
            .WithDescription("Видаляє учасника турніру. Якщо учасник має матчі, посилання на нього буде очищено. Потрібна роль admin.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");

        // --- Team Members ---
        group.MapGet("/members", ListMembers)
            .WithSummary("Список учасників команд")
            .WithDescription("Повертає список усіх учасників команд. Потрібна роль admin.")
            .Produces<List<AdminMemberItem>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapPost("/members", AddMember)
            .WithSummary("Додати учасника до команди")
            .WithDescription("Додає користувача до команди. Потрібна роль admin.")
            .Accepts<AdminAddMemberRequest>("application/json")
            .Produces<AdminMemberItem>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Admin");

        group.MapDelete("/members/{id:guid}", RemoveMember)
            .WithSummary("Видалити учасника з команди")
            .WithDescription("Видаляє учасника з команди. Потрібна роль admin.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Admin");
    }

    // ── Users ──────────────────────────────────────────────────────────

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

    // ── Tournaments ────────────────────────────────────────────────────

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

    // ── Disciplines ────────────────────────────────────────────────────

    private static async Task<IResult> ListDisciplines(ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var items = await db.Disciplines
            .OrderBy(d => d.Name)
            .Select(d => new AdminDisciplineItem(
                d.Id, d.Name, d.Slug, d.CoverUrl,
                d.Tournaments.Count))
            .ToListAsync();

        return Results.Ok(items);
    }

    private static async Task<IResult> CreateDiscipline(
        ClaimsPrincipal principal, AppDbContext db, [FromBody] AdminCreateDisciplineRequest req)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var slug = SlugHelper.Generate(req.Name);
        var discipline = new Discipline
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Slug = slug
        };
        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync();

        return Results.Created(
            $"/api/admin/disciplines/{discipline.Id}",
            new AdminDisciplineItem(discipline.Id, discipline.Name, discipline.Slug, discipline.CoverUrl, 0));
    }

    private static async Task<IResult> UpdateDiscipline(
        Guid id, ClaimsPrincipal principal, AppDbContext db, [FromBody] AdminUpdateDisciplineRequest req)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var discipline = await db.Disciplines.FindAsync(id);
        if (discipline is null) return Results.NotFound();

        discipline.Name = req.Name;
        discipline.Slug = SlugHelper.Generate(req.Name);
        await db.SaveChangesAsync();

        var tournamentCount = await db.Tournaments.CountAsync(t => t.DisciplineId == id);
        return Results.Ok(new AdminDisciplineItem(
            discipline.Id, discipline.Name, discipline.Slug, discipline.CoverUrl, tournamentCount));
    }

    private static async Task<IResult> DeleteDiscipline(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var discipline = await db.Disciplines.FindAsync(id);
        if (discipline is null) return Results.NotFound();

        if (await db.Tournaments.AnyAsync(t => t.DisciplineId == id))
            return Results.BadRequest(new { message = "Неможливо видалити дисципліну, яку використовують турніри" });

        db.Disciplines.Remove(discipline);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    // ── Teams ──────────────────────────────────────────────────────────

    private static async Task<IResult> ListTeams(ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var items = await db.Teams
            .Include(t => t.Captain)
            .OrderBy(t => t.Name)
            .Select(t => new AdminTeamItem(
                t.Id, t.Name, t.Slug, t.Captain.Username,
                t.Members.Count))
            .ToListAsync();

        return Results.Ok(items);
    }

    private static async Task<IResult> CreateTeam(
        ClaimsPrincipal principal, AppDbContext db, [FromBody] AdminCreateTeamRequest req)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var captain = await db.Users.FindAsync(req.CaptainId);
        if (captain is null)
            return Results.BadRequest(new { message = "Капітана не знайдено" });

        var slug = await SlugHelper.EnsureUniqueAsync(
            async s => await db.Teams.AnyAsync(t => t.Slug == s),
            SlugHelper.Generate(req.Name));

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Slug = slug,
            CaptainId = req.CaptainId,
            CreatedAt = DateTime.UtcNow
        };
        db.Teams.Add(team);

        db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = team.Id,
            UserId = req.CaptainId,
            Role = "captain",
            JoinedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return Results.Created(
            $"/api/admin/teams/{team.Id}",
            new AdminTeamItem(team.Id, team.Name, team.Slug, captain.Username, 1));
    }

    private static async Task<IResult> UpdateTeam(
        Guid id, ClaimsPrincipal principal, AppDbContext db, [FromBody] AdminUpdateTeamRequest req)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var team = await db.Teams
            .Include(t => t.Captain)
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (team is null) return Results.NotFound();

        team.Name = req.Name;
        team.Slug = await SlugHelper.EnsureUniqueAsync(
            async s => await db.Teams.AnyAsync(t => t.Slug == s && t.Id != id),
            SlugHelper.Generate(req.Name));
        await db.SaveChangesAsync();

        return Results.Ok(new AdminTeamItem(
            team.Id, team.Name, team.Slug, team.Captain.Username, team.Members.Count));
    }

    // ── Matches ────────────────────────────────────────────────────────

    private static async Task<IResult> ListMatches(ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var matches = await db.Matches
            .Include(m => m.Tournament)
            .Include(m => m.Participant1).ThenInclude(p => p.User)
            .Include(m => m.Participant1).ThenInclude(p => p.Team)
            .Include(m => m.Participant2).ThenInclude(p => p.User)
            .Include(m => m.Participant2).ThenInclude(p => p.Team)
            .OrderBy(m => m.TournamentId).ThenBy(m => m.Round).ThenBy(m => m.MatchNumber)
            .ToListAsync();

        var items = matches.Select(m => new AdminMatchItem(
            m.Id,
            m.Tournament.Title,
            m.Round,
            m.MatchNumber,
            m.Participant1?.User?.Username ?? m.Participant1?.Team?.Name,
            m.Participant2?.User?.Username ?? m.Participant2?.Team?.Name,
            m.Score1,
            m.Score2,
            m.Status
        )).ToList();

        return Results.Ok(items);
    }

    private static async Task<IResult> GetMatch(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var m = await db.Matches
            .Include(x => x.Tournament)
            .Include(x => x.Participant1).ThenInclude(p => p.User)
            .Include(x => x.Participant1).ThenInclude(p => p.Team)
            .Include(x => x.Participant2).ThenInclude(p => p.User)
            .Include(x => x.Participant2).ThenInclude(p => p.Team)
            .Include(x => x.Winner).ThenInclude(p => p.User)
            .Include(x => x.Winner).ThenInclude(p => p.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (m is null) return Results.NotFound();

        return Results.Ok(MapMatchDetail(m));
    }

    private static async Task<IResult> CreateMatch(
        ClaimsPrincipal principal, AppDbContext db, [FromBody] AdminCreateMatchRequest req)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        if (!await db.Tournaments.AnyAsync(t => t.Id == req.TournamentId))
            return Results.BadRequest(new { message = "Турнір не знайдено" });

        if (req.Participant1Id.HasValue &&
            !await db.TournamentParticipants.AnyAsync(p => p.Id == req.Participant1Id.Value))
            return Results.BadRequest(new { message = "Учасника 1 не знайдено" });

        if (req.Participant2Id.HasValue &&
            !await db.TournamentParticipants.AnyAsync(p => p.Id == req.Participant2Id.Value))
            return Results.BadRequest(new { message = "Учасника 2 не знайдено" });

        var match = new TournamentMatch
        {
            Id = Guid.NewGuid(),
            TournamentId = req.TournamentId,
            Round = req.Round,
            MatchNumber = req.MatchNumber,
            BracketSide = req.BracketSide,
            Participant1Id = req.Participant1Id,
            Participant2Id = req.Participant2Id
        };
        db.Matches.Add(match);
        await db.SaveChangesAsync();

        // Reload with includes
        var created = await db.Matches
            .Include(x => x.Tournament)
            .Include(x => x.Participant1).ThenInclude(p => p.User)
            .Include(x => x.Participant1).ThenInclude(p => p.Team)
            .Include(x => x.Participant2).ThenInclude(p => p.User)
            .Include(x => x.Participant2).ThenInclude(p => p.Team)
            .Include(x => x.Winner).ThenInclude(p => p.User)
            .Include(x => x.Winner).ThenInclude(p => p.Team)
            .FirstAsync(x => x.Id == match.Id);

        return Results.Created($"/api/admin/matches/{match.Id}", MapMatchDetail(created));
    }

    private static async Task<IResult> UpdateMatch(
        Guid id, ClaimsPrincipal principal, AppDbContext db, [FromBody] AdminUpdateMatchRequest req)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var m = await db.Matches.FindAsync(id);
        if (m is null) return Results.NotFound();

        if (req.Score1.HasValue) m.Score1 = req.Score1;
        if (req.Score2.HasValue) m.Score2 = req.Score2;
        if (req.WinnerId.HasValue)
        {
            if (!await db.TournamentParticipants.AnyAsync(p => p.Id == req.WinnerId.Value))
                return Results.BadRequest(new { message = "Переможця не знайдено" });
            m.WinnerId = req.WinnerId;
        }
        if (req.Status is not null) m.Status = req.Status;
        if (req.ScheduledAt is not null) m.ScheduledAt = req.ScheduledAt;
        if (req.PlayedAt is not null) m.PlayedAt = req.PlayedAt;

        await db.SaveChangesAsync();

        var updated = await db.Matches
            .Include(x => x.Tournament)
            .Include(x => x.Participant1).ThenInclude(p => p.User)
            .Include(x => x.Participant1).ThenInclude(p => p.Team)
            .Include(x => x.Participant2).ThenInclude(p => p.User)
            .Include(x => x.Participant2).ThenInclude(p => p.Team)
            .Include(x => x.Winner).ThenInclude(p => p.User)
            .Include(x => x.Winner).ThenInclude(p => p.Team)
            .FirstAsync(x => x.Id == id);

        return Results.Ok(MapMatchDetail(updated));
    }

    private static async Task<IResult> DeleteMatch(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var m = await db.Matches.FindAsync(id);
        if (m is null) return Results.NotFound();

        db.Matches.Remove(m);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static AdminMatchDetail MapMatchDetail(TournamentMatch m)
    {
        string? p1Name = null;
        if (m.Participant1?.User is not null) p1Name = m.Participant1.User.Username;
        else if (m.Participant1?.Team is not null) p1Name = m.Participant1.Team.Name;

        string? p2Name = null;
        if (m.Participant2?.User is not null) p2Name = m.Participant2.User.Username;
        else if (m.Participant2?.Team is not null) p2Name = m.Participant2.Team.Name;

        string? winnerName = null;
        if (m.Winner?.User is not null) winnerName = m.Winner.User.Username;
        else if (m.Winner?.Team is not null) winnerName = m.Winner.Team.Name;

        return new AdminMatchDetail(
            m.Id,
            m.Tournament.Title,
            m.TournamentId,
            m.Round,
            m.MatchNumber,
            m.BracketSide,
            m.Participant1Id,
            m.Participant2Id,
            p1Name,
            p2Name,
            m.Score1,
            m.Score2,
            m.WinnerId,
            winnerName,
            m.Status,
            m.ScheduledAt,
            m.PlayedAt,
            m.NextMatchId
        );
    }

    // ── Participants ───────────────────────────────────────────────────

    private static async Task<IResult> ListParticipants(ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var participants = await db.TournamentParticipants
            .Include(p => p.Tournament)
            .Include(p => p.User)
            .Include(p => p.Team)
            .OrderByDescending(p => p.RegisteredAt)
            .ToListAsync();

        var items = participants.Select(p => new AdminParticipantItem(
            p.Id,
            p.Tournament.Title,
            p.User?.Username ?? p.Team?.Name,
            p.User is not null ? "solo" : "team",
            p.Status,
            p.Seed,
            p.RegisteredAt
        )).ToList();

        return Results.Ok(items);
    }

    private static async Task<IResult> GetParticipant(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var p = await db.TournamentParticipants
            .Include(x => x.Tournament)
            .Include(x => x.User)
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p is null) return Results.NotFound();

        return Results.Ok(new AdminParticipantDetail(
            p.Id,
            p.TournamentId,
            p.Tournament.Title,
            p.UserId,
            p.TeamId,
            p.User?.Username ?? p.Team?.Name,
            p.User is not null ? "solo" : "team",
            p.Status,
            p.Seed,
            p.RegisteredAt
        ));
    }

    private static async Task<IResult> CreateParticipant(
        ClaimsPrincipal principal, AppDbContext db, [FromBody] AdminCreateParticipantRequest req)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        if (!await db.Tournaments.AnyAsync(t => t.Id == req.TournamentId))
            return Results.BadRequest(new { message = "Турнір не знайдено" });

        if (req.UserId.HasValue && !await db.Users.AnyAsync(u => u.Id == req.UserId.Value))
            return Results.BadRequest(new { message = "Користувача не знайдено" });

        if (req.TeamId.HasValue && !await db.Teams.AnyAsync(t => t.Id == req.TeamId.Value))
            return Results.BadRequest(new { message = "Команду не знайдено" });

        var participant = new TournamentParticipant
        {
            Id = Guid.NewGuid(),
            TournamentId = req.TournamentId,
            UserId = req.UserId,
            TeamId = req.TeamId,
            Status = req.Status,
            Seed = req.Seed,
            RegisteredAt = DateTime.UtcNow
        };
        db.TournamentParticipants.Add(participant);
        await db.SaveChangesAsync();

        var created = await db.TournamentParticipants
            .Include(x => x.Tournament)
            .Include(x => x.User)
            .Include(x => x.Team)
            .FirstAsync(x => x.Id == participant.Id);

        return Results.Created(
            $"/api/admin/participants/{participant.Id}",
            new AdminParticipantDetail(
                created.Id, created.TournamentId, created.Tournament.Title,
                created.UserId, created.TeamId,
                created.User?.Username ?? created.Team?.Name,
                created.User is not null ? "solo" : "team",
                created.Status, created.Seed, created.RegisteredAt));
    }

    private static async Task<IResult> UpdateParticipantStatus(
        Guid id, ClaimsPrincipal principal, AppDbContext db,
        [FromBody] AdminUpdateParticipantStatusRequest req)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var p = await db.TournamentParticipants
            .Include(x => x.Tournament)
            .Include(x => x.User)
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p is null) return Results.NotFound();

        p.Status = req.Status;
        await db.SaveChangesAsync();

        return Results.Ok(new AdminParticipantDetail(
            p.Id, p.TournamentId, p.Tournament.Title,
            p.UserId, p.TeamId,
            p.User?.Username ?? p.Team?.Name,
            p.User is not null ? "solo" : "team",
            p.Status, p.Seed, p.RegisteredAt));
    }

    private static async Task<IResult> DeleteParticipant(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var p = await db.TournamentParticipants
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return Results.NotFound();

        var relatedMatches = await db.Matches
            .Where(m => m.Participant1Id == id || m.Participant2Id == id || m.WinnerId == id)
            .ToListAsync();

        foreach (var m in relatedMatches)
        {
            if (m.Participant1Id == id) m.Participant1Id = null;
            if (m.Participant2Id == id) m.Participant2Id = null;
            if (m.WinnerId == id) m.WinnerId = null;
        }

        db.TournamentParticipants.Remove(p);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    // ── Team Members ───────────────────────────────────────────────────

    private static async Task<IResult> ListMembers(ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var items = await db.TeamMembers
            .Include(m => m.Team)
            .Include(m => m.User)
            .OrderBy(m => m.Team.Name).ThenBy(m => m.User.Username)
            .Select(m => new AdminMemberItem(
                m.Id, m.Team.Name, m.User.Username, m.Role, m.JoinedAt))
            .ToListAsync();

        return Results.Ok(items);
    }

    private static async Task<IResult> AddMember(
        ClaimsPrincipal principal, AppDbContext db, [FromBody] AdminAddMemberRequest req)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        if (!await db.Teams.AnyAsync(t => t.Id == req.TeamId))
            return Results.BadRequest(new { message = "Команду не знайдено" });

        if (!await db.Users.AnyAsync(u => u.Id == req.UserId))
            return Results.BadRequest(new { message = "Користувача не знайдено" });

        if (await db.TeamMembers.AnyAsync(m => m.TeamId == req.TeamId && m.UserId == req.UserId))
            return Results.BadRequest(new { message = "Користувач вже є учасником команди" });

        var member = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = req.TeamId,
            UserId = req.UserId,
            Role = req.Role,
            JoinedAt = DateTime.UtcNow
        };
        db.TeamMembers.Add(member);
        await db.SaveChangesAsync();

        var created = await db.TeamMembers
            .Include(m => m.Team)
            .Include(m => m.User)
            .FirstAsync(m => m.Id == member.Id);

        return Results.Created(
            $"/api/admin/members/{member.Id}",
            new AdminMemberItem(created.Id, created.Team.Name, created.User.Username, created.Role, created.JoinedAt));
    }

    private static async Task<IResult> RemoveMember(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        if (!principal.IsInRole("admin")) return Results.Forbid();

        var member = await db.TeamMembers.FindAsync(id);
        if (member is null) return Results.NotFound();

        db.TeamMembers.Remove(member);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}

// ── Users ──────────────────────────────────────────────────────────────
public record ChangeRoleRequest(string Role);
public record AdminUserItem(Guid Id, string Username, string Email, string Role, bool IsBlocked, bool EmailConfirmed, DateTime CreatedAt);
public record AdminUserListResponse(List<AdminUserItem> Items, int Total, int Page, int PageSize);
public record BlockStatusResponse(bool IsBlocked);
public record RoleChangeResponse(string Role);
public record AdminTournamentItem(Guid Id, string Title, string Slug, string Discipline, string Organizer, string Status, DateTime StartsAt, int Participants);

// ── Disciplines ────────────────────────────────────────────────────────
public record AdminCreateDisciplineRequest(string Name);
public record AdminUpdateDisciplineRequest(string Name);
public record AdminDisciplineItem(Guid Id, string Name, string Slug, string? CoverUrl, int TournamentCount);

// ── Teams ──────────────────────────────────────────────────────────────
public record AdminCreateTeamRequest(string Name, Guid CaptainId);
public record AdminUpdateTeamRequest(string Name);
public record AdminTeamItem(Guid Id, string Name, string Slug, string CaptainUsername, int MemberCount);

// ── Matches ────────────────────────────────────────────────────────────
public record AdminCreateMatchRequest(Guid TournamentId, int Round, int MatchNumber, string BracketSide, Guid? Participant1Id, Guid? Participant2Id);
public record AdminUpdateMatchRequest(int? Score1, int? Score2, Guid? WinnerId, string? Status, DateTime? ScheduledAt, DateTime? PlayedAt);
public record AdminMatchItem(Guid Id, string TournamentTitle, int Round, int MatchNumber, string? Participant1Name, string? Participant2Name, int? Score1, int? Score2, string Status);
public record AdminMatchDetail(
    Guid Id, string TournamentTitle, Guid TournamentId, int Round, int MatchNumber,
    string BracketSide, Guid? Participant1Id, Guid? Participant2Id,
    string? Participant1Name, string? Participant2Name,
    int? Score1, int? Score2, Guid? WinnerId, string? WinnerName,
    string Status, DateTime? ScheduledAt, DateTime? PlayedAt, Guid? NextMatchId);

// ── Participants ───────────────────────────────────────────────────────
public record AdminCreateParticipantRequest(Guid TournamentId, Guid? UserId, Guid? TeamId, string Status, int? Seed);
public record AdminUpdateParticipantStatusRequest(string Status);
public record AdminParticipantItem(Guid Id, string TournamentTitle, string? Name, string Type, string Status, int? Seed, DateTime RegisteredAt);
public record AdminParticipantDetail(
    Guid Id, Guid TournamentId, string TournamentTitle,
    Guid? UserId, Guid? TeamId, string? Name, string Type,
    string Status, int? Seed, DateTime RegisteredAt);

// ── Team Members ───────────────────────────────────────────────────────
public record AdminAddMemberRequest(Guid TeamId, Guid UserId, string Role);
public record AdminMemberItem(Guid Id, string TeamName, string Username, string Role, DateTime JoinedAt);

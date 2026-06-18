using System.Security.Claims;
using System.Text.RegularExpressions;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.DTOs.Tournament;
using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
using ArenaMaster.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Endpoints;

public static class TournamentEndpoints
{
    public static void MapTournamentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tournaments");

        group.MapGet("/", ListTournaments)
            .WithSummary("Список турнірів")
            .WithDescription("Повертає пагінований список активних турнірів (чернетки не включено). Підтримує фільтрацію за дисципліною, форматом, типом учасників, статусом і пошук за назвою.")
            .Produces<TournamentListResponse>(StatusCodes.Status200OK)
            .WithTags("Tournaments");

        group.MapPost("/", CreateTournament).RequireAuthorization()
            .WithSummary("Створити турнір")
            .WithDescription("Створює новий турнір у статусі 'draft'. Потрібна роль organizer або admin. Автоматично генерує обкладинку через Unsplash.")
            .Accepts<CreateTournamentRequest>("application/json")
            .Produces<CreateResult>(StatusCodes.Status201Created)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Tournaments");

        group.MapGet("/{slug}", GetTournament)
            .WithSummary("Деталі турніру")
            .WithDescription("Повертає повну інформацію про турнір, включно з дисципліною, організатором та призами.")
            .Produces<TournamentDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Tournaments");

        group.MapPut("/{id:guid}", UpdateTournament).RequireAuthorization()
            .WithSummary("Оновити турнір")
            .WithDescription("Оновлює поля турніру. Потрібна роль організатора або admin. Усі поля опціональні — змінюються лише надані значення.")
            .Accepts<UpdateTournamentRequest>("application/json")
            .Produces<TournamentDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Tournaments");

        group.MapPost("/{id:guid}/cover", UploadCover).RequireAuthorization().DisableAntiforgery()
            .WithSummary("Завантажити обкладинку турніру")
            .WithDescription("Завантажує обкладинку. Якщо файл не передано або він некоректний, автоматично підбирає зображення через Unsplash. Потрібна роль організатора або admin.")
            .Produces<CoverUploadResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Tournaments");

        group.MapPatch("/{id:guid}/status", UpdateStatus).RequireAuthorization()
            .WithSummary("Змінити статус турніру")
            .WithDescription("Перемикає статус турніру. При переході з 'registration' на 'ongoing' автоматично генерує сітку матчів і сповіщає всіх учасників.")
            .Accepts<UpdateStatusRequest>("application/json")
            .Produces<StatusUpdateResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Tournaments");

        group.MapPost("/{id:guid}/participants", Register).RequireAuthorization()
            .WithSummary("Зареєструватися на турнір")
            .WithDescription("Реєструє поточного користувача (solo) або команду (team) на турнір. Якщо AutoAccept увімкнено, заявку приймають автоматично.")
            .Accepts<RegisterParticipantRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Tournaments");

        group.MapGet("/{id:guid}/participants", ListParticipants)
            .WithSummary("Список учасників турніру")
            .WithDescription("Повертає список усіх зареєстрованих учасників турніру.")
            .Produces<List<ParticipantDto>>(StatusCodes.Status200OK)
            .WithTags("Tournaments");

        group.MapPatch("/{id:guid}/participants/{pid:guid}/status", UpdateParticipantStatus).RequireAuthorization()
            .WithSummary("Змінити статус заявки учасника")
            .WithDescription("Приймає або відхиляє заявку учасника. Потрібна роль організатора або admin. Учаснику надсилається сповіщення.")
            .Accepts<UpdateStatusRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Tournaments");

        group.MapGet("/{id:guid}/bracket", GetBracket)
            .WithSummary("Сітка турніру (bracket)")
            .WithDescription("Повертає всі матчі турніру: раунди, учасників, результати та зв'язки для наступних матчів.")
            .Produces<List<BracketMatchDto>>(StatusCodes.Status200OK)
            .WithTags("Tournaments");

        group.MapPost("/{id:guid}/bracket/generate", GenerateBracket).RequireAuthorization()
            .WithSummary("Згенерувати сітку матчів")
            .WithDescription("Генерує сітку матчів для турніру на основі прийнятих учасників. Якщо сітка вже існує, повертає помилку. Потрібна роль організатора або admin.")
            .Produces<GenerateBracketResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Tournaments");
    }

    private static async Task<IResult> ListTournaments(
        AppDbContext db,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] Guid? disciplineId = null,
        [FromQuery] string? format = null,
        [FromQuery] string? participantType = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] string sort = "date")
    {
        var query = db.Tournaments
            .Include(t => t.Discipline)
            .Include(t => t.Participants)
            .Where(t => t.Status != "draft");

        if (disciplineId.HasValue) query = query.Where(t => t.DisciplineId == disciplineId);
        if (!string.IsNullOrEmpty(format)) query = query.Where(t => t.Format == format);
        if (!string.IsNullOrEmpty(participantType)) query = query.Where(t => t.ParticipantType == participantType);
        if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);
        if (!string.IsNullOrEmpty(search)) query = query.Where(t => t.Title.Contains(search));

        query = sort == "participants"
            ? query.OrderByDescending(t => t.Participants.Count(p => p.Status == "accepted"))
            : query.OrderBy(t => t.StartsAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => new TournamentListItemDto(
                t.Id, t.Title, t.Slug, t.Discipline.Name, t.Format, t.ParticipantType,
                t.Status, t.StartsAt,
                t.Participants.Count(p => p.Status == "accepted"),
                t.MaxParticipants, t.CoverUrl))
            .ToListAsync();

        return Results.Ok(new TournamentListResponse(items, total, page, pageSize));
    }

    private static async Task<IResult> CreateTournament(
        CreateTournamentRequest req, ClaimsPrincipal principal, AppDbContext db,
        UnsplashClient unsplash, IValidator<CreateTournamentRequest> validator)
    {
        if (!principal.IsInRole("organizer") && !principal.IsInRole("admin"))
            return Results.Forbid();

        var validation = await validator.ValidateAsync(req);
        if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

        var userId = principal.GetUserId()!;
        var slug = await SlugHelper.EnsureUniqueAsync(
            async s => await db.Tournaments.AnyAsync(t => t.Slug == s),
            SlugHelper.Generate(req.Title));

        var tournament = new Tournament
        {
            Id = Guid.NewGuid(),
            Title = req.Title,
            Slug = slug,
            DisciplineId = req.DisciplineId,
            OrganizerId = userId.Value,
            Format = req.Format,
            ParticipantType = req.ParticipantType,
            TeamSize = req.TeamSize,
            MaxParticipants = req.MaxParticipants,
            RegistrationEndsAt = req.RegistrationEndsAt.ToUniversalTime(),
            StartsAt = req.StartsAt.ToUniversalTime(),
            Status = "registration",
            PrizeDescription = req.PrizeDescription,
            Description = req.Description,
            StreamUrl = req.StreamUrl,
            AutoAccept = req.AutoAccept,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        tournament.CoverUrl = await unsplash.DownloadAndSaveAsync("tournaments", tournament.Id, "esports gaming tournament");
        if (tournament.CoverUrl is null)
            tournament.CoverUrl = PlaceholderImageGenerator.WriteTournamentCover(tournament.Title, tournament.Id);
        db.Tournaments.Add(tournament);
        await db.SaveChangesAsync();
        return Results.Created($"/api/tournaments/{tournament.Slug}", new CreateResult(tournament.Id, tournament.Slug));
    }

    private static async Task<IResult> GetTournament(string slug, AppDbContext db)
    {
        var t = await db.Tournaments
            .Include(x => x.Discipline)
            .Include(x => x.Organizer)
            .FirstOrDefaultAsync(x => x.Slug == slug);
        if (t is null) return Results.NotFound();

        return Results.Ok(MapDetail(t));
    }

    private static async Task<IResult> UpdateTournament(
        Guid id, UpdateTournamentRequest req, ClaimsPrincipal principal, AppDbContext db)
    {
        var t = await db.Tournaments.FindAsync(id);
        if (t is null) return Results.NotFound();
        if (!CanManageTournament(principal, t)) return Results.Forbid();

        if (req.Title is not null) t.Title = req.Title;
        if (req.DisciplineId.HasValue) t.DisciplineId = req.DisciplineId.Value;
        if (req.Format is not null) t.Format = req.Format;
        if (req.MaxParticipants.HasValue) t.MaxParticipants = req.MaxParticipants.Value;
        if (req.RegistrationEndsAt.HasValue) t.RegistrationEndsAt = req.RegistrationEndsAt.Value.ToUniversalTime();
        if (req.StartsAt.HasValue) t.StartsAt = req.StartsAt.Value.ToUniversalTime();
        if (req.PrizeDescription is not null) t.PrizeDescription = req.PrizeDescription;
        if (req.Description is not null) t.Description = req.Description;
        if (req.StreamUrl is not null) t.StreamUrl = req.StreamUrl;
        if (req.AutoAccept.HasValue) t.AutoAccept = req.AutoAccept.Value;
        t.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await db.Entry(t).Reference(x => x.Discipline).LoadAsync();
        await db.Entry(t).Reference(x => x.Organizer).LoadAsync();
        return Results.Ok(MapDetail(t));
    }

    private static async Task<IResult> UploadCover(
        Guid id, IFormFile file, ClaimsPrincipal principal, AppDbContext db,
        IWebHostEnvironment env, UnsplashClient unsplash)
    {
        var t = await db.Tournaments.FindAsync(id);
        if (t is null) return Results.NotFound();
        if (!CanManageTournament(principal, t)) return Results.Forbid();

        var path = await FileUploadHelper.SaveUploadedFileAsync(file, env, "tournaments", t.Id);
        path ??= await unsplash.DownloadAndSaveAsync("tournaments", t.Id, "esports gaming tournament");
        if (path is null) return Results.BadRequest(new { message = "Невірний файл" });

        t.CoverUrl = path;
        t.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(new CoverUploadResponse(path));
    }

    private static async Task<IResult> UpdateStatus(
        Guid id, UpdateStatusRequest req, ClaimsPrincipal principal, AppDbContext db)
    {
        var t = await db.Tournaments
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return Results.NotFound();
        if (!CanManageTournament(principal, t) && !principal.IsInRole("admin"))
            return Results.Forbid();

        var oldStatus = t.Status;
        t.Status = req.Status;
        t.UpdatedAt = DateTime.UtcNow;

        if (oldStatus == "registration" && req.Status == "ongoing")
        {
            var accepted = t.Participants.Where(p => p.Status == "accepted").ToList();
            var matches = t.Format == "double_elimination"
                ? BracketGenerator.GenerateDoubleElimination(t.Id, accepted)
                : BracketGenerator.GenerateSingleElimination(t.Id, accepted, accepted.Any(p => p.Seed.HasValue));
            db.Matches.AddRange(matches);

            foreach (var p in accepted)
            {
                var userId = p.UserId ?? await db.TeamMembers
                    .Where(m => m.TeamId == p.TeamId)
                    .Select(m => m.UserId)
                    .FirstOrDefaultAsync();
                if (userId != Guid.Empty)
                    await NotificationHelper.CreateAsync(db, userId, "tournament_started",
                        "Турнір розпочався", $"Турнір «{t.Title}» розпочався", "tournament", t.Id);
            }
        }

        await db.SaveChangesAsync();
        return Results.Ok(new StatusUpdateResponse(t.Status));
    }

    private static async Task<IResult> Register(
        Guid id, RegisterParticipantRequest req, ClaimsPrincipal principal, AppDbContext db)
    {
        var userId = principal.GetUserId()!;
        var t = await db.Tournaments.Include(x => x.Participants).FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return Results.NotFound();
        if (t.Status != "registration") return Results.BadRequest(new { message = "Реєстрація закрита" });
        if (DateTime.UtcNow > t.RegistrationEndsAt) return Results.BadRequest(new { message = "Дедлайн реєстрації минув" });

        var acceptedCount = t.Participants.Count(p => p.Status == "accepted");
        if (acceptedCount >= t.MaxParticipants)
            return Results.BadRequest(new { message = "Досягнуто ліміт учасників" });

        if (t.ParticipantType == "solo")
        {
            if (t.Participants.Any(p => p.UserId == userId && p.Status == "rejected"))
                return Results.BadRequest(new { message = "Повторна реєстрація заборонена" });
            if (t.Participants.Any(p => p.UserId == userId && p.Status != "rejected"))
                return Results.BadRequest(new { message = "Ви вже зареєстровані" });

            var p = new TournamentParticipant
            {
                Id = Guid.NewGuid(),
                TournamentId = id,
                UserId = userId,
                Status = t.AutoAccept ? "accepted" : "pending",
                RegisteredAt = DateTime.UtcNow
            };
            db.TournamentParticipants.Add(p);
        }
        else
        {
            if (req.TeamId is null) return Results.BadRequest(new { message = "Потрібна команда" });
            var team = await db.Teams.FindAsync(req.TeamId);
            if (team is null || team.CaptainId != userId) return Results.Forbid();

            if (t.Participants.Any(p => p.TeamId == req.TeamId && p.Status == "rejected"))
                return Results.BadRequest(new { message = "Повторна реєстрація заборонена" });
            if (t.Participants.Any(p => p.TeamId == req.TeamId && p.Status != "rejected"))
                return Results.BadRequest(new { message = "Команда вже зареєстрована" });

            db.TournamentParticipants.Add(new TournamentParticipant
            {
                Id = Guid.NewGuid(),
                TournamentId = id,
                TeamId = req.TeamId,
                Status = t.AutoAccept ? "accepted" : "pending",
                RegisteredAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> ListParticipants(Guid id, AppDbContext db)
    {
        var items = await db.TournamentParticipants
            .Where(p => p.TournamentId == id)
            .Include(p => p.User)
            .Include(p => p.Team)
            .Select(p => new ParticipantDto(
                p.Id,
                p.User != null ? p.User.Username : null,
                p.Team != null ? p.Team.Name : null,
                p.User != null ? p.User.AvatarUrl : p.Team!.LogoUrl,
                p.Status,
                p.Seed))
            .ToListAsync();
        return Results.Ok(items);
    }

    private static async Task<IResult> UpdateParticipantStatus(
        Guid id, Guid pid, UpdateStatusRequest req, ClaimsPrincipal principal, AppDbContext db)
    {
        var t = await db.Tournaments.FindAsync(id);
        if (t is null) return Results.NotFound();
        if (!CanManageTournament(principal, t)) return Results.Forbid();

        var p = await db.TournamentParticipants.FindAsync(pid);
        if (p is null || p.TournamentId != id) return Results.NotFound();

        p.Status = req.Status;
        await db.SaveChangesAsync();

        var notifyUserId = p.UserId;
        if (notifyUserId is null && p.TeamId.HasValue)
        {
            var team = await db.Teams.FindAsync(p.TeamId);
            notifyUserId = team?.CaptainId;
        }

        if (notifyUserId.HasValue)
        {
            var msg = req.Status == "accepted" ? "Вашу заявку прийнято" : "Вашу заявку відхилено";
            await NotificationHelper.CreateAsync(db, notifyUserId.Value, "tournament_application",
                "Статус заявки", msg, "tournament", id);
        }

        return Results.Ok();
    }

    private static async Task<IResult> GetBracket(Guid id, AppDbContext db)
    {
        var matches = await db.Matches
            .Where(m => m.TournamentId == id)
            .Include(m => m.Participant1).ThenInclude(p => p!.User)
            .Include(m => m.Participant1).ThenInclude(p => p!.Team)
            .Include(m => m.Participant2).ThenInclude(p => p!.User)
            .Include(m => m.Participant2).ThenInclude(p => p!.Team)
            .OrderBy(m => m.Round).ThenBy(m => m.MatchNumber)
            .ToListAsync();

        return Results.Ok(matches.Select(m => new BracketMatchDto(
            m.Id, m.Round, m.MatchNumber, m.BracketSide,
            m.Participant1Id, GetParticipantName(m.Participant1),
            m.Participant2Id, GetParticipantName(m.Participant2),
            m.Score1, m.Score2, m.WinnerId, m.Status, m.ScheduledAt, m.PlayedAt, m.NextMatchId, m.NextMatchSlot)));
    }

    private static async Task<IResult> GenerateBracket(Guid id, ClaimsPrincipal principal, AppDbContext db)
    {
        var t = await db.Tournaments.Include(x => x.Participants).FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return Results.NotFound();
        if (!CanManageTournament(principal, t)) return Results.Forbid();

        var existing = await db.Matches.AnyAsync(m => m.TournamentId == id);
        if (existing) return Results.BadRequest(new { message = "Брекет вже згенеровано" });

        var accepted = t.Participants.Where(p => p.Status == "accepted").ToList();
        var matches = t.Format == "double_elimination"
            ? BracketGenerator.GenerateDoubleElimination(t.Id, accepted)
            : BracketGenerator.GenerateSingleElimination(t.Id, accepted, false);

        db.Matches.AddRange(matches);
        await db.SaveChangesAsync();
        return Results.Ok(new GenerateBracketResponse(matches.Count));
    }

    private static bool CanManageTournament(ClaimsPrincipal principal, Tournament t) =>
        principal.IsInRole("admin") || t.OrganizerId == principal.GetUserId();

    private static TournamentDetailDto MapDetail(Tournament t)
    {
        var prizes = ParsePrizes(t.PrizeDescription);
        return new TournamentDetailDto(
            t.Id, t.Title, t.Slug, t.Discipline.Name, t.Format, t.ParticipantType,
            t.TeamSize, t.MaxParticipants, t.RegistrationEndsAt, t.StartsAt, t.Status,
            t.PrizeDescription, t.Description, t.CoverUrl,
            t.Status == "ongoing" ? t.StreamUrl : null,
            t.AutoAccept, t.Organizer.Username, prizes);
    }

    private static List<PrizePlaceDto> ParsePrizes(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var list = new List<PrizePlaceDto>();
        var matches = Regex.Matches(text, @"(\d)\s*місце\s*[—\-:]\s*([^,;]+)", RegexOptions.IgnoreCase);
        foreach (Match m in matches)
            list.Add(new PrizePlaceDto(int.Parse(m.Groups[1].Value), m.Groups[2].Value.Trim()));
        if (list.Count == 0)
            list.Add(new PrizePlaceDto(1, text));
        return list;
    }

    private static string? GetParticipantName(TournamentParticipant? p)
    {
        if (p is null) return null;
        return p.User?.Username ?? p.Team?.Name;
    }
}

public record TournamentListResponse(IReadOnlyList<TournamentListItemDto> Items, int Total, int Page, int PageSize);
public record CoverUploadResponse(string CoverUrl);
public record StatusUpdateResponse(string Status);
public record GenerateBracketResponse(int Count);
public record BracketMatchDto(
    Guid Id, int Round, int MatchNumber, string? BracketSide,
    Guid? Participant1Id, string? Participant1Name,
    Guid? Participant2Id, string? Participant2Name,
    int? Score1, int? Score2, Guid? WinnerId, string Status,
    DateTime? ScheduledAt, DateTime? PlayedAt,
    Guid? NextMatchId, int? NextMatchSlot);

using System.Security.Claims;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.DTOs.Match;
using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Endpoints;

public static class MatchEndpoints
{
    public static void MapMatchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/matches");

        group.MapGet("/{id:guid}", GetMatch);
        group.MapPatch("/{id:guid}/result", SubmitResult).RequireAuthorization();
        group.MapPatch("/{id:guid}/schedule", SetSchedule).RequireAuthorization();
    }

    private static async Task<IResult> GetMatch(Guid id, AppDbContext db)
    {
        var m = await db.Matches
            .Include(x => x.Participant1).ThenInclude(p => p!.User)
            .Include(x => x.Participant1).ThenInclude(p => p!.Team)
            .Include(x => x.Participant2).ThenInclude(p => p!.User)
            .Include(x => x.Participant2).ThenInclude(p => p!.Team)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return Results.NotFound();

        return Results.Ok(new MatchDetailDto(
            m.Id, m.TournamentId, m.Round, m.MatchNumber, m.BracketSide,
            m.Participant1Id, GetName(m.Participant1),
            m.Participant2Id, GetName(m.Participant2),
            m.Score1, m.Score2, m.WinnerId, m.Status, m.ScheduledAt, m.PlayedAt));
    }

    private static async Task<IResult> SubmitResult(
        Guid id, MatchResultRequest req, ClaimsPrincipal principal, AppDbContext db)
    {
        var m = await db.Matches
            .Include(x => x.Tournament)
            .Include(x => x.Participant1)
            .Include(x => x.Participant2)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return Results.NotFound();

        var t = m.Tournament;
        if (t.Status == "finished")
            return Results.BadRequest(new { message = "Турнір завершено" });

        if (!principal.IsInRole("admin") && t.OrganizerId != principal.GetUserId())
            return Results.Forbid();

        if (req.WinnerId != m.Participant1Id && req.WinnerId != m.Participant2Id)
            return Results.BadRequest(new { message = "Невірний переможець" });

        m.Score1 = req.Score1;
        m.Score2 = req.Score2;
        m.WinnerId = req.WinnerId;
        m.Status = "finished";
        m.PlayedAt = DateTime.UtcNow;

        var allMatches = await db.Matches.Where(x => x.TournamentId == t.Id).ToListAsync();
        var winner = await db.TournamentParticipants.FindAsync(req.WinnerId);
        if (winner is not null)
            BracketGenerator.AdvanceWinner(m, winner, allMatches);

        var participantUserIds = new List<Guid>();
        foreach (var pid in new[] { m.Participant1Id, m.Participant2Id })
        {
            if (!pid.HasValue) continue;
            var p = await db.TournamentParticipants.FindAsync(pid);
            if (p?.UserId is not null) participantUserIds.Add(p.UserId.Value);
            else if (p?.TeamId is not null)
            {
                var captain = await db.Teams.Where(tm => tm.Id == p.TeamId).Select(tm => tm.CaptainId).FirstOrDefaultAsync();
                if (captain != Guid.Empty) participantUserIds.Add(captain);
            }
        }

        foreach (var uid in participantUserIds.Distinct())
        {
            await NotificationHelper.CreateAsync(db, uid, "match_result",
                "Результат матчу", $"Результат матчу внесено: {req.Score1}:{req.Score2}", "match", m.Id);
        }

        var pending = allMatches.Count(x => x.Status != "finished" && x.BracketSide != "grand_final");
        if (pending == 0 && allMatches.All(x => x.Status == "finished" || x.Participant1Id is null))
            t.Status = "finished";

        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> SetSchedule(
        Guid id, MatchScheduleRequest req, ClaimsPrincipal principal, AppDbContext db)
    {
        var m = await db.Matches.Include(x => x.Tournament).FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return Results.NotFound();
        if (m.Tournament.OrganizerId != principal.GetUserId() && !principal.IsInRole("admin"))
            return Results.Forbid();

        m.ScheduledAt = req.ScheduledAt.ToUniversalTime();
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static string? GetName(TournamentParticipant? p) =>
        p?.User?.Username ?? p?.Team?.Name;
}

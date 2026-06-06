using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Data.Seeders;

public static class MatchSeeder
{
    public static void Seed(AppDbContext db)
    {
        var rng = new System.Random(2026);
        var tournaments = db.Tournaments
            .Where(t => t.Status == "finished" || t.Status == "ongoing")
            .Include(t => t.Participants)
            .ToList();

        foreach (var t in tournaments)
        {
            var accepted = t.Participants
                .Where(p => p.Status == "accepted")
                .OrderBy(_ => rng.Next())
                .ToList();

            if (accepted.Count < 2) continue;

            var matches = t.Format == "double_elimination"
                ? BracketGenerator.GenerateDoubleElimination(t.Id, accepted)
                : BracketGenerator.GenerateSingleElimination(t.Id, accepted, true);

            var allMatches = matches.ToList();

            foreach (var m in allMatches.Where(m => m.Round == 1 && m.Participant1Id.HasValue && m.Participant2Id.HasValue))
            {
                if (t.Status == "finished")
                {
                    var p1Wins = rng.Next(0, 2) == 0;
                    m.Score1 = p1Wins ? rng.Next(1, 3) : 0;
                    m.Score2 = p1Wins ? 0 : rng.Next(1, 3);
                    m.WinnerId = p1Wins ? m.Participant1Id : m.Participant2Id;
                    m.Status = "finished";
                    m.PlayedAt = DateTime.UtcNow.AddDays(-rng.Next(3, 10));
                    var winner = accepted.FirstOrDefault(p => p.Id == m.WinnerId);
                    if (winner is not null)
                        BracketGenerator.AdvanceWinner(m, winner, allMatches);
                }
                else if (t.Status == "ongoing")
                {
                    var played = rng.Next(0, 2) == 0;
                    if (played)
                    {
                        var p1Wins = rng.Next(0, 2) == 0;
                        m.Score1 = p1Wins ? 2 : 1;
                        m.Score2 = p1Wins ? 1 : 2;
                        m.WinnerId = p1Wins ? m.Participant1Id : m.Participant2Id;
                        m.Status = "finished";
                        m.PlayedAt = DateTime.UtcNow.AddDays(-rng.Next(1, 3));
                        var winner = accepted.FirstOrDefault(p => p.Id == m.WinnerId);
                        if (winner is not null)
                            BracketGenerator.AdvanceWinner(m, winner, allMatches);
                    }
                    else
                    {
                        m.ScheduledAt = DateTime.UtcNow.AddDays(rng.Next(1, 5));
                    }
                }
            }

            if (t.Status == "finished")
            {
                foreach (var m in allMatches.Where(m => m.Status == "pending" && m.Participant1Id.HasValue && m.Participant2Id.HasValue))
                {
                    m.Score1 = rng.Next(1, 3);
                    m.Score2 = rng.Next(0, m.Score1.Value);
                    m.WinnerId = m.Participant1Id;
                    m.Status = "finished";
                    m.PlayedAt = DateTime.UtcNow.AddDays(-rng.Next(1, 3));
                    var winner = accepted.FirstOrDefault(p => p.Id == m.WinnerId);
                    if (winner is not null)
                        BracketGenerator.AdvanceWinner(m, winner, allMatches);
                }
            }

            db.Matches.AddRange(allMatches);
        }
    }
}

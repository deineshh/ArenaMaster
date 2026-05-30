using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Data.Seeders;

public static class MatchSeeder
{
    public static void Seed(AppDbContext db)
    {
        var tournaments = db.Tournaments
            .Where(t => t.Status == "finished" || t.Status == "ongoing")
            .Include(t => t.Participants)
            .ToList();

        foreach (var t in tournaments)
        {
            var accepted = t.Participants.Where(p => p.Status == "accepted").ToList();
            if (accepted.Count < 2) continue;

            var matches = t.Format == "double_elimination"
                ? BracketGenerator.GenerateDoubleElimination(t.Id, accepted)
                : BracketGenerator.GenerateSingleElimination(t.Id, accepted, true);

            var round1 = matches.Where(m => m.Round == 1).ToList();
            foreach (var m in round1.Where(m => m.Participant1Id.HasValue && m.Participant2Id.HasValue))
            {
                var p1Wins = Random.Shared.Next(0, 2) == 0;
                m.Score1 = p1Wins ? 2 : 0;
                m.Score2 = p1Wins ? 0 : 2;
                m.WinnerId = p1Wins ? m.Participant1Id : m.Participant2Id;
                m.Status = "finished";
                m.PlayedAt = DateTime.UtcNow.AddDays(-5);

                var winner = accepted.First(p => p.Id == m.WinnerId);
                BracketGenerator.AdvanceWinner(m, winner, matches);
            }

            if (t.Status == "finished")
            {
                foreach (var m in matches.Where(m => m.Status == "pending" && m.Participant1Id.HasValue && m.Participant2Id.HasValue))
                {
                    m.Score1 = 2;
                    m.Score2 = 1;
                    m.WinnerId = m.Participant1Id;
                    m.Status = "finished";
                    m.PlayedAt = DateTime.UtcNow.AddDays(-1);
                    var winner = accepted.FirstOrDefault(p => p.Id == m.WinnerId);
                    if (winner is not null)
                        BracketGenerator.AdvanceWinner(m, winner, matches);
                }
            }

            db.Matches.AddRange(matches);
        }
    }
}

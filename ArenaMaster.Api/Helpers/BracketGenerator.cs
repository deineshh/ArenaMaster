using ArenaMaster.Api.Models;

namespace ArenaMaster.Api.Helpers;

public static class BracketGenerator
{
    public static List<TournamentMatch> GenerateSingleElimination(
        Guid tournamentId,
        List<TournamentParticipant> participants,
        bool useSeeding)
    {
        var ordered = useSeeding && participants.Any(p => p.Seed.HasValue)
            ? participants.OrderBy(p => p.Seed).ToList()
            : participants.OrderBy(_ => Random.Shared.Next()).ToList();

        var count = NextPowerOfTwo(ordered.Count);
        while (ordered.Count < count)
            ordered.Add(null!);

        var matches = new List<TournamentMatch>();
        var round1Count = count / 2;
        var round1 = new List<TournamentMatch>();

        for (var i = 0; i < round1Count; i++)
        {
            var m = new TournamentMatch
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                Round = 1,
                MatchNumber = i + 1,
                BracketSide = "winners",
                Participant1Id = ordered[i * 2]?.Id,
                Participant2Id = ordered[i * 2 + 1]?.Id,
                Status = "pending"
            };
            round1.Add(m);
            matches.Add(m);
        }

        var prevRound = round1;
        var roundNum = 2;
        while (prevRound.Count > 1)
        {
            var currentRound = new List<TournamentMatch>();
            for (var i = 0; i < prevRound.Count / 2; i++)
            {
                var m = new TournamentMatch
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    Round = roundNum,
                    MatchNumber = i + 1,
                    BracketSide = "winners",
                    Status = "pending"
                };
                prevRound[i * 2].NextMatchId = m.Id;
                prevRound[i * 2].NextMatchSlot = 1;
                prevRound[i * 2 + 1].NextMatchId = m.Id;
                prevRound[i * 2 + 1].NextMatchSlot = 2;
                currentRound.Add(m);
                matches.Add(m);
            }
            prevRound = currentRound;
            roundNum++;
        }

        return matches;
    }

    public static List<TournamentMatch> GenerateDoubleElimination(
        Guid tournamentId,
        List<TournamentParticipant> participants)
    {
        var ordered = participants.OrderBy(_ => Random.Shared.Next()).ToList();
        var count = NextPowerOfTwo(ordered.Count);
        while (ordered.Count < count)
            ordered.Add(null!);

        var matches = new List<TournamentMatch>();
        var winnersR1 = new List<TournamentMatch>();
        var round1Count = count / 2;

        for (var i = 0; i < round1Count; i++)
        {
            var m = new TournamentMatch
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                Round = 1,
                MatchNumber = i + 1,
                BracketSide = "winners",
                Participant1Id = ordered[i * 2]?.Id,
                Participant2Id = ordered[i * 2 + 1]?.Id,
                Status = "pending"
            };
            winnersR1.Add(m);
            matches.Add(m);
        }

        var prevWinners = winnersR1;
        var wRound = 2;
        while (prevWinners.Count > 1)
        {
            var current = new List<TournamentMatch>();
            for (var i = 0; i < prevWinners.Count / 2; i++)
            {
                var m = new TournamentMatch
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    Round = wRound,
                    MatchNumber = i + 1,
                    BracketSide = "winners",
                    Status = "pending"
                };
                Link(prevWinners[i * 2], m, 1);
                Link(prevWinners[i * 2 + 1], m, 2);
                current.Add(m);
                matches.Add(m);
            }
            prevWinners = current;
            wRound++;
        }

        var losersMatches = new List<TournamentMatch>();
        var losersRound = 1;
        var losersCount = round1Count / 2;
        if (losersCount >= 1)
        {
            for (var i = 0; i < losersCount; i++)
            {
                var m = new TournamentMatch
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    Round = losersRound,
                    MatchNumber = i + 1,
                    BracketSide = "losers",
                    Status = "pending"
                };
                losersMatches.Add(m);
                matches.Add(m);
            }
        }

        var grandFinal = new TournamentMatch
        {
            Id = Guid.NewGuid(),
            TournamentId = tournamentId,
            Round = wRound,
            MatchNumber = 1,
            BracketSide = "grand_final",
            Status = "pending"
        };
        matches.Add(grandFinal);

        if (prevWinners.Count == 1)
            Link(prevWinners[0], grandFinal, 1);

        var resetMatch = new TournamentMatch
        {
            Id = Guid.NewGuid(),
            TournamentId = tournamentId,
            Round = wRound + 1,
            MatchNumber = 1,
            BracketSide = "grand_final",
            Status = "pending"
        };
        matches.Add(resetMatch);
        grandFinal.NextMatchId = resetMatch.Id;
        grandFinal.NextMatchSlot = 1;

        return matches;
    }

    private static void Link(TournamentMatch from, TournamentMatch to, int slot)
    {
        from.NextMatchId = to.Id;
        from.NextMatchSlot = slot;
    }

    private static int NextPowerOfTwo(int n)
    {
        var p = 1;
        while (p < n) p *= 2;
        return Math.Max(p, 2);
    }

    public static void AdvanceWinner(TournamentMatch match, TournamentParticipant winner, List<TournamentMatch> allMatches)
    {
        if (match.NextMatchId is null) return;

        var next = allMatches.FirstOrDefault(m => m.Id == match.NextMatchId);
        if (next is null) return;

        if (match.NextMatchSlot == 1)
            next.Participant1Id = winner.Id;
        else
            next.Participant2Id = winner.Id;
    }
}

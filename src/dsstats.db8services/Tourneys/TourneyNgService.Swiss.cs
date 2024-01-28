using dsstats.db8;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;

namespace dsstats.db8services.Tourneys;

public partial class TourneyNgService
{
    public async Task<bool> CreateNewSwissRound(Guid tourneyGuid)
    {
        var tourney = await context.Tourneys
            .Include(i => i.TourneyTeams)
            .Include(i => i.TourneyMatches)
                .ThenInclude(i => i.TourneyTeams)
            .FirstOrDefaultAsync(f => f.TourneyGuid == tourneyGuid);

        if (tourney is null || tourney.TourneyTeams.Count < 2)
        {
            return false;
        }

        int currentRound = tourney.TourneyMatches.Count == 0 ? 0 :
            tourney.TourneyMatches.Max(m => m.Round);

        if (currentRound > 0 && tourney.TourneyMatches
            .Where(x => x.Round == currentRound)
            .Any(a => a.MatchResult == MatchResult.None))
        {
            return false;
        }
        int newRound = currentRound + 1;

        (var existingPairings, var teamWins) = GetExistingPairings(tourney);

        var matches = GetNextRoundSwissMatches(tourney, newRound, existingPairings, teamWins);

        context.TourneyMatches.AddRange(matches);   

        await context.SaveChangesAsync();
        return true;
    }

    private List<TourneyMatch> GetNextRoundSwissMatches(Tourney tourney,
                                                        int newRound,
                                                        Dictionary<TeamPairing, bool> existingPairings,
                                                        Dictionary<Guid, int> teamWins)
    {
        var teamGuidsOrdered = teamWins.OrderByDescending(o => o.Value).Select(s => s.Key).ToList();
        Dictionary<Guid, int> availableTeams = new(teamWins);
        List<TourneyMatch> matches = [];

        for (int i = 0; i < teamGuidsOrdered.Count; i++)
        {
            var teamAGuid = teamGuidsOrdered[i];
            if (!availableTeams.ContainsKey(teamAGuid))
            {
                continue;
            }

            bool hasMatch = false;
            var teamA = tourney.TourneyTeams.First(f => f.TeamGuid == teamAGuid);

            foreach (var teamWin in availableTeams.OrderByDescending(o => o.Value).ToArray())
            {
                var teamB = tourney.TourneyTeams.First(f => f.TeamGuid == teamWin.Key);
                if (!PairingExists(teamA, teamB, existingPairings))
                {
                    matches.Add(new()
                    {
                        Tourney = tourney,
                        TeamAGuid = teamAGuid,
                        Round = newRound,
                        TourneyTeams = new List<TourneyTeam>() { teamA, teamB },
                        MatchResult = MatchResult.TeamABye,
                    });
                    availableTeams.Remove(teamAGuid);
                    availableTeams.Remove(teamWin.Key);
                    hasMatch = true;
                    break;
                }
            }

            if (!hasMatch)
            {
                availableTeams.Remove(teamAGuid);
                matches.Add(new()
                {
                    Tourney = tourney,
                    TeamAGuid = teamAGuid,
                    Round = newRound,
                    TourneyTeams = new List<TourneyTeam>() { teamA },
                    MatchResult = MatchResult.TeamABye,
                });
            }
        }
        return matches;
    }

    private static bool PairingExists(TourneyTeam teamA, TourneyTeam teamB, Dictionary<TeamPairing, bool> pairings)
    {
        TeamPairing teamPairing1 = new() { Team1Guid = teamA.TeamGuid, Team2Guid = teamB.TeamGuid };
        TeamPairing teamPairing2 = new() { Team2Guid = teamA.TeamGuid, Team1Guid = teamB.TeamGuid };

        return pairings.ContainsKey(teamPairing1) || pairings.ContainsKey(teamPairing2);
    }

    private static (Dictionary<TeamPairing, bool>, Dictionary<Guid, int>) GetExistingPairings(Tourney tourney)
    {
        Dictionary<TeamPairing, bool> existingPairings = [];
        Dictionary<Guid, int> teamWins = [];

        if (tourney.TourneyMatches.Count == 0)
        {
            return (existingPairings, tourney.TourneyTeams.ToDictionary(k => k.TeamGuid, v => 0));
        }

        foreach (var tourneyMatch in tourney.TourneyMatches)
        {
            if (tourneyMatch.TourneyTeams.Count != 2)
            {
                continue;
            }

            var teamA = tourneyMatch.TourneyTeams.First(f => f.TeamGuid == tourneyMatch.TeamAGuid);
            var teamB = tourneyMatch.TourneyTeams.First(f => f.TeamGuid != tourneyMatch.TeamAGuid);

            TeamPairing teamPairing = new()
            {
                Team1Guid = teamA.TeamGuid,
                Team2Guid = teamB.TeamGuid,
            };

            if (!existingPairings.TryGetValue(teamPairing, out _))
            {
                existingPairings[teamPairing] = true;
            }

            var winnerTeam = tourneyMatch.MatchResult == MatchResult.TeamAWin ?
                teamA : teamB;

            if (teamWins.ContainsKey(winnerTeam.TeamGuid))
            {
                teamWins[winnerTeam.TeamGuid]++;
            }
            else
            {
                teamWins[winnerTeam.TeamGuid] = 1;
            }
        }
        return (existingPairings, teamWins);
    }
}

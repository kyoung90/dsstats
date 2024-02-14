using dsstats.shared;
using Microsoft.EntityFrameworkCore;

namespace dsstats.db8services.Tourneys;

public partial class TourneyNgService
{
    public async Task<TourneyStatsResponse> GetStats(TourneyStatsRequest request, CancellationToken token = default)
    {
        var tourney = await context.Tourneys
            .Include(i => i.TourneyMatches)
            .Include(i => i.TourneyTeams)
                .ThenInclude(i => i.TourneyPlayers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(f => f.TourneyGuid == request.TourneyGuid, token);

        if (tourney is null)
        {
            return new();
        }

        var stats = await GetTourneyStats(tourney.TourneyId, token);

        return new()
        {
            Players = tourney.TourneyPlayers.Count,
            Matches = tourney.TourneyMatches.Count,
            Teams = tourney.TourneyTeams.Count,
            CommanderStats = stats
        };
    }

    private async Task<List<TourneyCommanderStat>> GetTourneyStats(int tourneyId, CancellationToken token)
    {
        var query = from r in context.Replays
                    from rp in r.ReplayPlayers
                    where r.TourneyMatch != null
                        && r.TourneyMatch.TourneyId == tourneyId
                    group new { r, rp } by rp.Race into g
                    select new TourneyCommanderStat()
                    {
                        Commander = g.Key,
                        Count = g.Count(),
                        Wins = g.Count(c => c.rp.PlayerResult == PlayerResult.Win),
                    };

        var results = await query.ToListAsync(token);

        foreach (var result in results)
        {
            result.Bans = await context.TourneyMatches
                .Where(x => x.TourneyId == tourneyId
                    && (x.Ban1 == result.Commander || x.Ban2 == result.Commander))
                .CountAsync(token);
        }

        return results;
    }
}

using dsstats.db8;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;

namespace dsstats.db8services.Tourneys;

public partial class TourneyNgService
{
    public async Task<bool> CreateRoundRobinBracket(Guid tourneyGuid)
    {
        var tourney = await context.Tourneys
            .Include(i => i.TourneyTeams)
            .FirstOrDefaultAsync(f => f.TourneyGuid == tourneyGuid);

        if (tourney is null || tourney.TourneyTeams.Count < 2)
        {
            return false;
        }

        int numberOfTeams = tourney.TourneyTeams.Count;
        int rounds = numberOfTeams - 1;

        for (int round = 1; round <= rounds; round++)
        {
            foreach (var (team, index) in tourney.TourneyTeams.Select((s, index) => (s, index)))
            {
                int oppIndex = (index + round) % numberOfTeams;
                var oppTeam = tourney.TourneyTeams.ElementAt(oppIndex);

                TourneyMatch tourneyMatch = new()
                {
                    Round = round,
                    Tourney = tourney,
                    TourneyTeams = new List<TourneyTeam>() { team },
                    TeamAGuid = team.TeamGuid
                };

                if (team == oppTeam || (numberOfTeams % 2 != 0 && oppIndex == 0))
                {
                    tourneyMatch.MatchResult = MatchResult.TeamABye;
                }
                else
                {
                    tourneyMatch.TourneyTeams.Add(oppTeam);
                }

                tourney.TourneyMatches.Add(tourneyMatch);
            }
        }

        await context.SaveChangesAsync();

        return true;
    }
}

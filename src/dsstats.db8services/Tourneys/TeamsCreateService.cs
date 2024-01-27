using AutoMapper;
using dsstats.db8;
using dsstats.shared;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace dsstats.db8services.Tourneys;

public class TeamsCreateService(ReplayContext context, IMapper mapper)
{
    public async Task<Guid> CreateTournament(TourneyCreateDto createDto)
    {
        Tourney tourney = new()
        {
            Name = createDto.Name,
            StartDate = createDto.EventStart,
            GameMode = createDto.GameMode,
        };

        context.Tourneys.Add(tourney);
        await context.SaveChangesAsync();

        return tourney.TourneyGuid;
    }

    public async Task<Guid> AddTourneyTeam(TourneyTeamCreateDto createDto)
    {
        var tourney = await context.Tourneys
            .Include(i => i.TourneyPlayers)
                .ThenInclude(i => i.Player)
            .FirstOrDefaultAsync(f => f.TourneyGuid == createDto.TourneyGuid);

        if (tourney is null)
        {
            return Guid.Empty;
        }

        List<TourneyPlayer> tourneyPlayers = [];

        var playersQuery = context.Players.AsNoTracking();

        var predicate = PredicateBuilder.New<Player>();

        foreach (var player in createDto.Players)
        {
            var tourneyPlayer = tourney.TourneyPlayers.FirstOrDefault(p =>
                   p.Player != null
                && p.Player.ToonId == player.ToonId
                && p.Player.RealmId == player.RealmId
                && p.Player.RegionId == player.RegionId);

            if (tourneyPlayer is not null)
            {
                tourneyPlayers.Add(tourneyPlayer);
            }
            else
            {
                predicate = predicate.Or(o => o.ToonId == player.ToonId
                    && o.RealmId == player.RealmId
                    && o.RegionId == player.RegionId);
            }
        }

        var players = await playersQuery
            .Where(predicate)
            .ToListAsync();

        foreach (var player in players)
        {
            tourneyPlayers.Add(new()
            {
                Tourney = tourney,
                Player = player
            });
        }

        TourneyTeam tourneyTeam = new()
        {
            TourneyId = tourney.TourneyId,
            Name = createDto.Name,
            TourneyPlayers = tourneyPlayers
        };

        tourney.TourneyTeams.Add(tourneyTeam);
        await context.SaveChangesAsync();

        return tourney.TourneyGuid;
    }

    public async Task<Guid> AddTourneyMatch(TourneyMatchCreateDto createDto)
    {
        var tourney = await context.Tourneys
            .Include(i => i.TourneyPlayers)
            .Include(i => i.TourneyMatches)
            .FirstOrDefaultAsync(f => f.TourneyGuid == createDto.TourneyGuid);

        if (tourney is null)
        {
            return Guid.Empty;
        }

        var teamA = tourney.TourneyTeams.FirstOrDefault(f => f.TeamGuid == createDto.TeamAGuid);

        if (teamA is null)
        {
            return Guid.Empty;
        }

        var teamB = tourney.TourneyTeams.FirstOrDefault(f => f.TeamGuid == createDto.TeamBGuid);

        if (teamB is null)
        {
            return Guid.Empty;
        }

        TourneyMatch tourneyMatch = new()
        {
            Tourney = tourney,
            Round = createDto.Round,
            Group = createDto.Group,
            IsLowerBracket = createDto.IsLowerBracket,
            TeamAGuid = teamA.TeamGuid,
            TourneyTeams = new List<TourneyTeam>() { teamA, teamB },
            Ban1 = createDto.Ban1,
            Ban2 = createDto.Ban2,
            Ban3 = createDto.Ban3,
        };

        context.TourneyMatches.Add(tourneyMatch);
        await context.SaveChangesAsync();

        return tourneyMatch.TourneyMatchGuid;
    }

    public async Task<bool> AddTournamentPlayers(TourneyPlayersDto playersDto)
    {
        var tourney = await context.Tourneys
            .Include(i => i.TourneyPlayers)
                .ThenInclude(i => i.Player)
            .FirstOrDefaultAsync(f => f.TourneyGuid == playersDto.TourneyGuid);

        if (tourney is null)
        {
            return false;
        }

        List<TourneyPlayer> tourneyPlayers = [];

        var playersQuery = context.Players.AsQueryable();

        var predicate = PredicateBuilder.New<Player>();

        foreach (var player in playersDto.PlayerIds)
        {
            var tourneyPlayer = tourney.TourneyPlayers.FirstOrDefault(p =>
                   p.Player != null
                && p.Player.ToonId == player.ToonId
                && p.Player.RealmId == player.RealmId
                && p.Player.RegionId == player.RegionId);

            if (tourneyPlayer is not null)
            {
                continue;
            }
            else
            {
                predicate = predicate.Or(o => o.ToonId == player.ToonId
                    && o.RealmId == player.RealmId
                    && o.RegionId == player.RegionId);
            }
        }

        var players = await playersQuery
            .Where(predicate)
            .ToListAsync();

        foreach (var player in players)
        {
            tourneyPlayers.Add(new()
            {
                Tourney = tourney,
                Player = player
            });
        }

        context.TourneyPlayers.AddRange(tourneyPlayers);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CreateRandomTeams(Guid tourneyGuid, RatingType ratingType)
    {
        Tourney? tourney;

        if (ratingType == RatingType.StdTE || ratingType == RatingType.CmdrTE)
        {
            tourney = await context.Tourneys
                .Include(i => i.TourneyPlayers)
                    .ThenInclude(i => i.Player)
                        .ThenInclude(i => i!.PlayerRatings.Where(x => x.RatingType == ratingType))
                .Include(i => i.TourneyTeams)
                .FirstOrDefaultAsync(f => f.TourneyGuid == tourneyGuid);
        }
        else
        {
            tourney = await context.Tourneys
                .Include(i => i.TourneyPlayers)
                    .ThenInclude(i => i.Player)
                        .ThenInclude(i => i!.ComboPlayerRatings.Where(x => x.RatingType == ratingType))
                .Include(i => i.TourneyTeams)
                .FirstOrDefaultAsync(f => f.TourneyGuid == tourneyGuid);
        }

        if (tourney is null)
        {
            return false;
        }

        List<PlayerSortHelper> playerHelpers = tourney.TourneyPlayers.Select(s => new PlayerSortHelper()
        {
            TourneyPlayerGuid = s.TourneyPlayerGuid,
            Rating = s.Player!.PlayerRatings.Count > 0 ? Convert.ToInt32(s.Player.PlayerRatings.ElementAt(0).Rating) :
                s.Player.ComboPlayerRatings.Count > 0 ? Convert.ToInt32(s.Player.ComboPlayerRatings.ElementAt(0).Rating)
                    : 1000,
        }).ToList();

        var teamHelpers = CreateEvenThreePlayerTeams(playerHelpers);

        List<TourneyTeam> teams = [];
        foreach (var teamHelper in teamHelpers)
        {
            var tourneyPlayers = teamHelper.Players
                    .Select(s => tourney.TourneyPlayers.First(f => f.TourneyPlayerGuid == s.TourneyPlayerGuid))
                    .ToList();

            if (tourneyPlayers.Count == 3)
            {
                teams.Add(new()
                {
                    Tourney = tourney,
                    TourneyPlayers = tourneyPlayers,
                    Name = $"{tourneyPlayers[0].Player!.Name}'s Team"
                });
            }
        }

        tourney.TourneyTeams = teams;
        await context.SaveChangesAsync();

        return true;
    }

    private List<TeamHelper> CreateEvenThreePlayerTeams(List<PlayerSortHelper> players)
    {
        List<TeamHelper> teams = [];

        int teamCount = players.Count / 3;

        if (teamCount == 0)
        {
            return [];
        }

        var avgRating = players.Average(a => a.Rating);

        var orderedPlayers = players.OrderByDescending(a => a.Rating).ToList();

        List<PlayerSortHelper> availablePlayers = new(orderedPlayers);

        for (int i = 0; i < teamCount; i++)
        {
            var player = orderedPlayers[i];
            availablePlayers.Remove(player);

            teams.Add(new()
            {
                Players = [player]
            });
        }

        // for (int i = 0; i < teams.Count; i++)
        for (int i = teamCount - 1; i >= 0; i--)
        {
            // add two more player to team.Players so the TeamRating is as close to the avgRating as possible

            var team = teams[i];
            var remainingPlayers = 2;

            while (remainingPlayers > 0 && availablePlayers.Count > 0)
            {
                var closestPlayer = availablePlayers
                    .OrderBy(p => Math.Abs((team.TeamRating * team.Players.Count + p.Rating) / (team.Players.Count + 1) - avgRating))
                    .First();

                team.Players.Add(closestPlayer);
                availablePlayers.Remove(closestPlayer);

                remainingPlayers--;
            }
        }

        return teams;
    }

    public async Task<bool> ReportMatchResult(TourneyMatchResult result)
    {
        var tourneyMatch = await context.TourneyMatches
            .FirstOrDefaultAsync(f => f.TourneyMatchGuid == result.TourneyMatchGuid);

        if (tourneyMatch is null)
        {
            return false;
        }

        tourneyMatch.MatchResult = result.MatchResult;
        tourneyMatch.Ban1 = result.Ban1;
        tourneyMatch.Ban2 = result.Ban2;
        tourneyMatch.Ban3 = result.Ban3;

        var replays = await context.Replays
            .Where(x => result.ReplayHashes.Contains(x.ReplayHash))
            .ToListAsync();

        tourneyMatch.Replays = replays;

        await context.SaveChangesAsync();

        return true;
    }

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

        var teamGuidsOrdered = teamWins.OrderByDescending(o => o.Value).Select(s => s.Key).ToList();

        Dictionary<Guid, int> availableTeams = new(teamWins);

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
                    tourney.TourneyMatches.Add(new()
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
                tourney.TourneyMatches.Add(new()
                {
                    Tourney = tourney,
                    TeamAGuid = teamAGuid,
                    Round = newRound,
                    TourneyTeams = new List<TourneyTeam>() { teamA },
                    MatchResult = MatchResult.TeamABye,
                });
            }
        }

        await context.SaveChangesAsync();
        return true;
    }

    private bool PairingExists(TourneyTeam teamA, TourneyTeam teamB, Dictionary<TeamPairing, bool> pairings)
    {
        TeamPairing teamPairing1 = new() { Team1Guid = teamA.TeamGuid, Team2Guid = teamB.TeamGuid };
        TeamPairing teamPairing2 = new() { Team2Guid = teamA.TeamGuid, Team1Guid = teamB.TeamGuid };

        return pairings.ContainsKey(teamPairing1) || pairings.ContainsKey(teamPairing2);
    }

    private (Dictionary<TeamPairing, bool>, Dictionary<Guid, int>) GetExistingPairings(Tourney tourney)
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

internal record PlayerSortHelper
{
    public Guid TourneyPlayerGuid { get; set; }
    public int Rating { get; set; }
}

internal record TeamHelper
{
    public List<PlayerSortHelper> Players { get; set; } = [];
    public int TeamRating => Players.Count == 0 ? 0 : Convert.ToInt32(Players.Average(a => a.Rating));
}

internal record TeamPairing
{
    public Guid Team1Guid { get; set; }
    public Guid Team2Guid { get; set; }
}
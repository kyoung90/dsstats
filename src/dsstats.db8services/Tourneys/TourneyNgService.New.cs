using dsstats.db8;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace dsstats.db8services.Tourneys;

public partial class TourneyNgService
{
    public async Task CreatePewPewTournament()
    {
        //var tourneyGuid = await CreateTournament(new TourneyCreateDto()
        //{
        //    Name = "PewPew Tourney",
        //    EventStart = new DateTime(2024, 3, 2),
        //    GameMode = GameMode.Standard
        //});

        var tourneyGuid = new Guid("96b8163b-a585-42aa-be6f-82e2611d0429");

        Dictionary<RequestNames, double> players = [];
        var paticipants = File.ReadAllLines("/data/ds/Tourneys/PewPew/participants.txt");

        foreach (var line in paticipants)
        {
            var ents = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (ents.Length == 3)
            {
                var profileEnts = ents[1].Split("%7C", StringSplitOptions.RemoveEmptyEntries);
                var profileId = int.Parse(profileEnts[0]);
                var realmId = int.Parse(profileEnts[1]);
                var regionId = int.Parse(profileEnts[2]);
                var rating = double.Parse(ents[2]);
                var name = ents[0];
                players[new(name, profileId, regionId, realmId)] = rating;
            }
        }

        StringBuilder sb = new();
        var avgRating = Math.Round(players.Values.Average(), 0);


        foreach (var player in players.OrderByDescending(o => o.Value))
        {
            sb.AppendLine($"{player.Key.Name} => {Math.Round(player.Value, 0)}");
        }
        sb.AppendLine($"Average rating: {avgRating}");

        Console.WriteLine(sb.ToString());

        var teams = CreateRandomTeams(players);
        Console.WriteLine(teams);
    }


    public async Task CreateTournamentFromEvent(Guid eventGuid)
    {
        var tourneyEvent = await context.Events.FirstOrDefaultAsync(f => f.EventGuid == eventGuid);

        ArgumentNullException.ThrowIfNull(tourneyEvent);

        var tourneyGuid = await CreateTournament(new()
        {
            Name = tourneyEvent.Name,
            EventStart = tourneyEvent.EventStart,
            GameMode = tourneyEvent?.GameMode ?? GameMode.None,
        });

        if (tourneyGuid == Guid.Empty)
        {
            return;
        }

        var replays = await context.Replays
            .Include(i => i.ReplayPlayers)
                .ThenInclude(i => i.Player)
            .Include(i => i.ReplayEvent)
            .Where(x => x.ReplayEvent != null
                && x.ReplayEvent.EventId == tourneyEvent!.EventId)
            .ToListAsync();

        if (replays is null)
        {
            return;
        }

        var players = replays.SelectMany(s => s.ReplayPlayers).Select(s => s.Player).Distinct().ToList();

        var result = await AddTournamentPlayers(new()
        {
            TourneyGuid = tourneyGuid,
            PlayerIds = players.Select(s => new PlayerId(s.ToonId, s.RealmId, s.RegionId)).ToList()
        });

        List<TourneyTeamCreateDto> teams = new();

        foreach (var replay in replays.OrderBy(o => o.GameTime))
        {
            var players1 = replay.ReplayPlayers
                .Where(x => x.Team == 1)
                .Select(s => new PlayerId(s.Player.ToonId, s.Player.RealmId, s.Player.RegionId))
                .OrderBy(o => o.ToonId)
                    .ThenBy(o => o.RealmId)
                .ToList();

            var players2 = replay.ReplayPlayers
                .Where(x => x.Team == 2)
                .Select(s => new PlayerId(s.Player.ToonId, s.Player.RealmId, s.Player.RegionId))
                .OrderBy(o => o.ToonId)
                    .ThenBy(o => o.RealmId)
                .ToList();

            var team1 = AddTeamPlayers(players1, teams, tourneyGuid);
            var team2 = AddTeamPlayers(players2, teams, tourneyGuid);

            team1.Name = replay.WinnerTeam == 1 ? replay.ReplayEvent?.WinnerTeam ?? string.Empty
                : replay.ReplayEvent?.RunnerTeam ?? string.Empty;
            team2.Name = replay.WinnerTeam == 2 ? replay.ReplayEvent?.WinnerTeam ?? string.Empty
                : replay.ReplayEvent?.RunnerTeam ?? string.Empty;
        }

        foreach (var team in teams)
        {
            await AddTourneyTeam(team);
        }

        var tourneyTeams = await context.TourneyTeams
            .Include(i => i.TourneyPlayers)
                .ThenInclude(i => i.Player)
            .Where(x => x.Tourney!.TourneyGuid == tourneyGuid)
            .ToListAsync();

        List<TourneyMatchCreateDto> matches = [];

        foreach (var replay in replays.OrderBy(o => o.GameTime))
        {
            (var team1, var team2) = GetTourneyTeams(replay, tourneyTeams);
            if (team1 is null || team2 is null)
            {
                Console.WriteLine($"teams not found for {replay.FileName}");
                continue;
            }

            var match = matches.FirstOrDefault(f => (f.TeamAGuid == team1.TeamGuid && f.TeamBGuid == team2.TeamGuid)
                || (f.TeamAGuid == team2.TeamGuid && f.TeamBGuid == team1.TeamGuid));

            if (match is null)
            {
                matches.Add(new()
                {
                    TourneyGuid = tourneyGuid,
                    TeamAGuid = team1.TeamGuid,
                    TeamBGuid = team2.TeamGuid
                });
            }
        }

        foreach (var match in matches)
        {
            await AddTourneyMatch(match);
        }

        var tourneyMatches = await context.TourneyMatches
            .Include(i => i.TourneyTeams)
            .Where(x => x.Tourney!.TourneyGuid == tourneyGuid)
            .ToListAsync();

        Dictionary<Guid, List<KeyValuePair<MatchResult, string>>> matchResults = [];

        foreach (var replay in replays.OrderBy(o => o.GameTime))
        {
            (var team1, var team2) = GetTourneyTeams(replay, tourneyTeams);
            if (team1 is null || team2 is null)
            {
                Console.WriteLine($"teams not found for {replay.FileName}");
                continue;
            }

            var match = tourneyMatches
                .FirstOrDefault(f => f.TourneyTeams
                    .Where(x => x.TeamGuid == team1.TeamGuid || x.TeamGuid == team2.TeamGuid).Count() == 2);

            if (match is null)
            {
                Console.WriteLine($"match not found: {replay.FileName}");
                continue;
            }

            var winnerTeam = replay.WinnerTeam == 1 ? team1 : team2;
            MatchResult matchResult = match.TeamAGuid == winnerTeam.TeamGuid ? MatchResult.TeamAWin : MatchResult.TeamBWin;

            if (!matchResults.ContainsKey(match.TourneyMatchGuid))
            {
                matchResults[match.TourneyMatchGuid] = new();
            }
            matchResults[match.TourneyMatchGuid].Add(new(matchResult, replay.ReplayHash));
        }

        List<TourneyMatchResult> results = [];
        foreach (var ent in matchResults)
        {
            var teamAWins = ent.Value.Where(s => s.Key == MatchResult.TeamAWin).Count();
            var teamBWins = ent.Value.Where(s => s.Key == MatchResult.TeamBWin).Count();
            MatchResult matchResult = teamAWins > teamBWins ? MatchResult.TeamAWin : MatchResult.TeamBWin;

            results.Add(new()
            {
                TourneyMatchGuid = ent.Key,
                MatchResult = matchResult,
                ReplayHashes = ent.Value.Select(s => s.Value).ToList()
            });
        }

        foreach (var matchResult in results)
        {
            await ReportMatchResult(matchResult);
        }
    }


    public async Task CreateGDSLTourney()
    {
        var tourneyGuid = await CreateTournament(new()
        {
            Name = "IV. GDSL OFFICIAL TOURNAMENT",
            EventStart = new DateTime(2023, 10, 31),
            GameMode = shared.GameMode.Standard
        });

        if (tourneyGuid == Guid.Empty)
        {
            return;
        }

        var replays = JsonSerializer.Deserialize<List<ReplayDto>>(File.ReadAllText(@"C:\data\ds\gdslreplays.json"));

        if (replays is null)
        {
            return;
        }

        // await importService.Import(replays);

        var players = replays.SelectMany(s => s.ReplayPlayers).Select(s => s.Player).Distinct().ToList();

        var result = await AddTournamentPlayers(new()
        {
            TourneyGuid = tourneyGuid,
            PlayerIds = players.Select(s => new PlayerId(s.ToonId, s.RealmId, s.RegionId)).ToList()
        });

        List<TourneyTeamCreateDto> teams = new();

        foreach (var replay in replays.OrderBy(o => o.GameTime))
        {
            var players1 = replay.ReplayPlayers
                .Where(x => x.Team == 1)
                .Select(s => new PlayerId(s.Player.ToonId, s.Player.RealmId, s.Player.RegionId))
                .OrderBy(o => o.ToonId)
                    .ThenBy(o => o.RealmId)
                .ToList();

            var players2 = replay.ReplayPlayers
                .Where(x => x.Team == 2)
                .Select(s => new PlayerId(s.Player.ToonId, s.Player.RealmId, s.Player.RegionId))
                .OrderBy(o => o.ToonId)
                    .ThenBy(o => o.RealmId)
                .ToList();

            var team1 = AddTeamPlayers(players1, teams, tourneyGuid);
            var team2 = AddTeamPlayers(players2, teams, tourneyGuid);
        }

        foreach (var team in teams)
        {
            await AddTourneyTeam(team);
        }

        var tourneyTeams = await context.TourneyTeams
            .Include(i => i.TourneyPlayers)
                .ThenInclude(i => i.Player)
            .Where(x => x.Tourney!.TourneyGuid == tourneyGuid)
            .ToListAsync();

        List<TourneyMatchCreateDto> matches = [];

        foreach (var replay in replays.OrderBy(o => o.GameTime))
        {
            (var team1, var team2) = GetTourneyTeams(replay, tourneyTeams);
            if (team1 is null || team2 is null)
            {
                Console.WriteLine($"teams not found for {replay.FileName}");
                continue;
            }

            var match = matches.FirstOrDefault(f => (f.TeamAGuid == team1.TeamGuid && f.TeamBGuid == team2.TeamGuid)
                || (f.TeamAGuid == team2.TeamGuid && f.TeamBGuid == team1.TeamGuid));

            if (match is null)
            {
                matches.Add(new()
                {
                    TourneyGuid = tourneyGuid,
                    TeamAGuid = team1.TeamGuid,
                    TeamBGuid = team2.TeamGuid
                });
            }
        }

        foreach (var match in matches)
        {
            await AddTourneyMatch(match);
        }

        var tourneyMatches = await context.TourneyMatches
            .Include(i => i.TourneyTeams)
            .Where(x => x.Tourney!.TourneyGuid == tourneyGuid)
            .ToListAsync();

        Dictionary<Guid, List<KeyValuePair<MatchResult, string>>> matchResults = [];

        foreach (var replay in replays.OrderBy(o => o.GameTime))
        {
            (var team1, var team2) = GetTourneyTeams(replay, tourneyTeams);
            if (team1 is null || team2 is null)
            {
                Console.WriteLine($"teams not found for {replay.FileName}");
                continue;
            }

            var match = tourneyMatches
                .FirstOrDefault(f => f.TourneyTeams
                    .Where(x => x.TeamGuid == team1.TeamGuid || x.TeamGuid == team2.TeamGuid).Count() == 2);

            if (match is null)
            {
                Console.WriteLine($"match not found: {replay.FileName}");
                continue;
            }

            var winnerTeam = replay.WinnerTeam == 1 ? team1 : team2;
            MatchResult matchResult = match.TeamAGuid == winnerTeam.TeamGuid ? MatchResult.TeamAWin : MatchResult.TeamBWin;

            if (!matchResults.ContainsKey(match.TourneyMatchGuid))
            {
                matchResults[match.TourneyMatchGuid] = new();
            }
            matchResults[match.TourneyMatchGuid].Add(new(matchResult, replay.ReplayHash));
        }

        List<TourneyMatchResult> results = [];
        foreach (var ent in matchResults)
        {
            var teamAWins = ent.Value.Where(s => s.Key == MatchResult.TeamAWin).Count();
            var teamBWins = ent.Value.Where(s => s.Key == MatchResult.TeamBWin).Count();
            MatchResult matchResult = teamAWins > teamBWins ? MatchResult.TeamAWin : MatchResult.TeamBWin;

            results.Add(new()
            {
                TourneyMatchGuid = ent.Key,
                MatchResult = matchResult,
                ReplayHashes = ent.Value.Select(s => s.Value).ToList()
            });
        }

        foreach (var matchResult in results)
        {
            await ReportMatchResult(matchResult);
        }
    }

    private (TourneyTeam? team1, TourneyTeam? team2) GetTourneyTeams(ReplayDto replay, List<TourneyTeam> tourneyTeams)
    {
        TourneyTeam? team1 = null;
        TourneyTeam? team2 = null;

        foreach (var replayPlayer in replay.ReplayPlayers)
        {
            var team = tourneyTeams.FirstOrDefault(f => f.TourneyPlayers.Any(a =>
                a.Player!.ToonId == replayPlayer.Player.ToonId
                && a.Player.RealmId == replayPlayer.Player.RealmId
                && a.Player.RegionId == replayPlayer.Player.RegionId));

            if (team is not null)
            {
                if (replayPlayer.Team == 1)
                {
                    team1 = team;
                }
                else
                {
                    team2 = team;
                }
            }
            if (team1 is not null && team2 is not null)
            {
                break;
            }
        }

        return (team1, team2);
    }

    private (TourneyTeam? team1, TourneyTeam? team2) GetTourneyTeams(Replay replay, List<TourneyTeam> tourneyTeams)
    {
        TourneyTeam? team1 = null;
        TourneyTeam? team2 = null;

        foreach (var replayPlayer in replay.ReplayPlayers)
        {
            var team = tourneyTeams.FirstOrDefault(f => f.TourneyPlayers.Any(a =>
                a.Player!.ToonId == replayPlayer.Player.ToonId
                && a.Player.RealmId == replayPlayer.Player.RealmId
                && a.Player.RegionId == replayPlayer.Player.RegionId));

            if (team is not null)
            {
                if (replayPlayer.Team == 1)
                {
                    team1 = team;
                }
                else
                {
                    team2 = team;
                }
            }
            if (team1 is not null && team2 is not null)
            {
                break;
            }
        }

        return (team1, team2);
    }

    private TourneyTeamCreateDto AddTeamPlayers(List<PlayerId> players, List<TourneyTeamCreateDto> teams, Guid tourneyGuid)
    {
        TourneyTeamCreateDto? team = null;

        foreach (var player in players)
        {
            team = teams.FirstOrDefault(f => f.Players.Contains(player));
        }

        if (team is null)
        {
            team = new TourneyTeamCreateDto()
            {
                TourneyGuid = tourneyGuid,
                Players = players
            };
            teams.Add(team);
            return team;
        }
        else
        {
            foreach (var player in players)
            {
                if (!team.Players.Contains(player))
                {
                    team.Players.Add(player);
                }
            }
            return team;
        }
    }

    public void DeleteTourney(Guid tourneyGuid)
    {
        var tourney = context.Tourneys
            .Include(i => i.TourneyMatches)
                .ThenInclude(i => i.TourneyTeams)
            .Include(i => i.TourneyTeams)
                .ThenInclude(i => i.TourneyPlayers)
            .Include(i => i.TourneyTeams)
                .ThenInclude(i => i.TourneyMatches)
            .Include(i => i.TourneyPlayers)
                .ThenInclude(i => i.TourneyTeam)
            .FirstOrDefault(f => f.TourneyGuid == tourneyGuid);

        if (tourney != null)
        {
            foreach (var match in tourney.TourneyMatches)
            {
                context.Entry(match)
                    .Collection(c => c.Replays)
                    .Load();
                match.Replays.Clear();
            }
            context.SaveChanges();
            context.Tourneys.Remove(tourney);
            context.SaveChanges();
        }

    }
}

internal record MatchResultHelper
{

}
using AutoMapper;
using dsstats.db8;
using dsstats.db8.Aram;
using dsstats.shared;
using dsstats.shared.Aram;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace dsstats.db8services;

public partial class AramService(ReplayContext context, IMapper mapper)
{
    public async Task<Guid> CreateAramEvent(AramEventDto aramEvent)
    {
        var dbEvent = mapper.Map<AramEventDto, AramEvent>(aramEvent);
        context.AramEvents.Add(dbEvent);
        await context.SaveChangesAsync();
        return dbEvent.Guid;
    }

    public async Task<Guid> AddPlayer(Guid eventGuid, AramPlayerDto player)
    {
        var aramEvent = await context.AramEvents
            .FirstOrDefaultAsync(f => f.Guid == eventGuid);

        if (aramEvent is null || aramEvent.EndTime < DateTime.UtcNow)
        {
            return Guid.Empty;
        }

        var aramPlayer = mapper.Map<AramPlayerDto, AramPlayer>(player);
        aramPlayer.AramEventId = aramEvent.AramEventId;
        aramPlayer.Status = PlayerStatus.Ready;
        context.AramPlayers.Add(aramPlayer);
        await context.SaveChangesAsync();
        return aramPlayer.Guid;
    }

    public async Task CreateMatches(Guid eventGuid)
    {
        var aramEvent = await context.AramEvents
            .Include(i => i.AramPlayers)
            .Include(i => i.AramMatches)
                .ThenInclude(i => i.AramSlots)
                    .ThenInclude(i => i.AramPlayer)
            .FirstOrDefaultAsync(f => f.Guid == eventGuid);

        if (aramEvent is null || aramEvent.EndTime < DateTime.UtcNow
            || aramEvent.AramPlayers.Count < 6)
        {
            return;
        }

        var playerInfos = GetPlayerInfos(aramEvent);
        var teams = Get3PlayerTeams(playerInfos);
        var matches = CreateMatches(teams);

        foreach (var match in  matches)
        {
            var aramMatch = CreateAramMatch(aramEvent, match);
            context.AramMatches.Add(aramMatch);
            aramEvent.OpenMatches++;
        }
        await context.SaveChangesAsync();
    }

    private AramMatch CreateAramMatch(AramEvent aramEvent, MatchInfo matchInfo)
    {
        AramMatch aramMatch = new()
        {
            AramEventId = aramEvent.AramEventId,
            Team1Rating = matchInfo.Team1.TeamRating,
            Team2Rating = matchInfo.Team2.TeamRating,
            MatchHistoryScore = matchInfo.MatchHistoryScore
        };

        foreach ((var playerInfo, var index) in matchInfo.Team1.PlayerInfos.Select((s, i) => (s, i)))
        {
            var aramPlayer = aramEvent.AramPlayers.First(f => f.Guid == playerInfo.Guid);
            aramPlayer.Matches++;
            aramPlayer.Status = PlayerStatus.MatchOpen;
            aramMatch.AramSlots.Add(new()
            {
                Pos = index + 1,
                Team = 1,
                Commander = GetRandomCommander(aramEvent.GameMode),
                AramPlayerId = aramPlayer.AramPlayerId
            });
        }

        foreach ((var playerInfo, var index) in matchInfo.Team2.PlayerInfos.Select((s, i) => (s, i)))
        {
            var aramPlayer = aramEvent.AramPlayers.First(f => f.Guid == playerInfo.Guid);
            aramPlayer.Matches++;
            aramPlayer.Status = PlayerStatus.MatchOpen;
            aramMatch.AramSlots.Add(new()
            {
                Pos = index + 3 + 1,
                Team = 2,
                Commander = GetRandomCommander(aramEvent.GameMode),
                AramPlayerId = aramPlayer.AramPlayerId
            });
        }
        return aramMatch;
    }

    private Commander GetRandomCommander(GameMode gameMode)
    {
        if (gameMode == GameMode.Standard)
        {
            return (Commander)Random.Shared.Next(1, 4);
        }
        else
        {
            var cmdrs = Data.GetCommanders(Data.CmdrGet.NoStd);
            cmdrs.Remove(Commander.Zeratul);
            return Random.Shared.GetItems<Commander>(cmdrs.ToArray(), 1)[0];
        }
    }

    private List<MatchInfo> CreateMatches(List<TeamInfo> teamInfos)
    {
        List<MatchInfo> matches = [];

        var availableTeams = new List<TeamInfo>(teamInfos);
        var teams = CollectionsMarshal.AsSpan(teamInfos);
        Random.Shared.Shuffle<TeamInfo>(teams);

        foreach (var team in teams)
        {
            availableTeams.Remove(team);
            if (availableTeams.Count == 0)
            {
                break;
            }
            
            var opponentTeam = Random.Shared.GetItems<TeamInfo>(availableTeams.ToArray(), 1).First();

            MatchInfo matchInfo = new()
            {
                Team1 = team,
                Team2 = opponentTeam,
            };
            SetMatchHistoryScore(matchInfo);
            matches.Add(matchInfo);
        }
        return matches;
    }

    private List<TeamInfo> Get3PlayerTeams(Dictionary<Guid, PlayerInfo> playerInfos)
    {
        int teamCount = playerInfos.Count / 3;

        if (teamCount == 0)
        {
            return [];
        }

        var avgRating = playerInfos.Values.Average(a => a.Rating);

        var availablePlayers = new List<PlayerInfo>(playerInfos.Values);
        var infos = CollectionsMarshal.AsSpan(playerInfos.Values.ToList());
        Random.Shared.Shuffle<PlayerInfo>(infos);

        List<TeamInfo> teams = [];

        for (int i = 0; i < teamCount; i++)
        {
            var info = infos[i];
            teams.Add(new TeamInfo() { PlayerInfos = [info] });
            availablePlayers.Remove(info);
        }

        for (int i = teams.Count - 1; i >= 0; i--)
        {
            var team = teams[i];
            int remainingPlayers = 2;

            while (remainingPlayers > 0 && availablePlayers.Count > 0)
            {
                var closestPlayer = availablePlayers
                    .OrderBy(p => Math.Abs((team.TeamRating * team.PlayerInfos.Count + p.Rating) / (team.PlayerInfos.Count + 1) - avgRating))
                    .Take(4)
                    .OrderBy(o => o.Matches)
                    .First();

                team.PlayerInfos.Add(closestPlayer);
                availablePlayers.Remove(closestPlayer);

                remainingPlayers--;
            }
        }
        return teams;
    }

    private Dictionary<Guid, PlayerInfo> GetPlayerInfos(AramEvent aramEvent)
    {
        Dictionary<Guid, PlayerInfo> infos = [];

        foreach (var player in aramEvent.AramPlayers)
        {
            infos[player.Guid] = new() { Guid = player.Guid, Rating = player.StartRating };
        }

        foreach (var match in aramEvent.AramMatches)
        {
            foreach (var slot in match.AramSlots)
            {
                if (!infos.TryGetValue(slot.AramPlayer!.Guid, out PlayerInfo? info)
                    || info is null)
                {
                    info = infos[slot.AramPlayer!.Guid] = new()
                    {
                        Guid = slot.AramPlayer!.Guid,
                        Rating = slot.AramPlayer.StartRating
                    };
                }
                info.Matches++;
                UpdatePlayedWith(info, slot, match.AramSlots);
                UpdatePlayedAgainst(info, slot, match.AramSlots);
            }
        }

        return infos;
    }

    private void UpdatePlayedWith(PlayerInfo info, AramSlot playerSlot, ICollection<AramSlot> slots)
    {
        foreach (var slot in slots)
        {
            if (slot == playerSlot || slot.Team != playerSlot.Team)
            {
                continue;
            }
            if (!info.PlayedWith.ContainsKey(slot.AramPlayer!.Guid))
            {
                info.PlayedWith[slot.AramPlayer!.Guid] = 1;
            }
            else
            {
                info.PlayedWith[slot.AramPlayer!.Guid]++;
            }
        }
    }

    private void UpdatePlayedAgainst(PlayerInfo info, AramSlot playerSlot, ICollection<AramSlot> slots)
    {
        foreach (var slot in slots)
        {
            if (slot.Team == playerSlot.Team)
            {
                continue;
            }
            if (!info.PlayedAgainst.ContainsKey(slot.AramPlayer!.Guid))
            {
                info.PlayedAgainst[slot.AramPlayer!.Guid] = 1;
            }
            else
            {
                info.PlayedAgainst[slot.AramPlayer!.Guid]++;
            }
        }
    }

    private void SetMatchHistoryScore(MatchInfo matchInfo)
    {
        int maxScore = 200;
        var team1Guids = matchInfo.Team1.PlayerInfos.Select(p => p.Guid).ToList();
        var team2Guids = matchInfo.Team2.PlayerInfos.Select(p => p.Guid).ToList();
        int matchHistoryScore = 0;

        // Calculate redundant score based on PlayedWith and PlayedAgainst information
        foreach (var player1 in team1Guids)
        {
            foreach (var player2 in team1Guids)
            {
                if (player1 != player2 && matchInfo.Team1.PlayerInfos.First(p => p.Guid == player1).PlayedWith.ContainsKey(player2))
                {
                    matchHistoryScore += matchInfo.Team1.PlayerInfos.First(p => p.Guid == player1).PlayedWith[player2];
                }
            }
        }

        foreach (var player1 in team2Guids)
        {
            foreach (var player2 in team2Guids)
            {
                if (player1 != player2 && matchInfo.Team2.PlayerInfos.First(p => p.Guid == player1).PlayedWith.ContainsKey(player2))
                {
                    matchHistoryScore += matchInfo.Team2.PlayerInfos.First(p => p.Guid == player1).PlayedWith[player2];
                }
            }
        }

        foreach (var player1 in team1Guids)
        {
            foreach (var player2 in team2Guids)
            {
                if (matchInfo.Team1.PlayerInfos.First(p => p.Guid == player1).PlayedAgainst.ContainsKey(player2))
                {
                    matchHistoryScore += matchInfo.Team1.PlayerInfos.First(p => p.Guid == player1).PlayedAgainst[player2];
                }
            }
        }

        matchInfo.MatchHistoryScore = MathF.Round(MathF.Min(1, (float)matchHistoryScore / maxScore), 2);
    }
}

internal record PlayerInfo
{
    public Guid Guid { get; set; }
    public int Rating { get; set; }
    public int Matches { get; set; }
    public Dictionary<Guid, int> PlayedWith { get; set; } = [];
    public Dictionary<Guid, int> PlayedAgainst { get; set; } = [];
}

internal record TeamInfo
{
    public List<PlayerInfo> PlayerInfos { get; set; } = [];
    public int TeamRating => PlayerInfos.Count == 0 ? 0 : Convert.ToInt32(PlayerInfos.Average(a => a.Rating));
}

internal record MatchInfo
{
    public TeamInfo Team1 { get; set; } = new();
    public TeamInfo Team2 { get; set; } = new();
    public float MatchHistoryScore {  get; set; }
}
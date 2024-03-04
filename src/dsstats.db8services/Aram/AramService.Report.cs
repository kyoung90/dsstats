using dsstats.db8.Aram;
using dsstats.shared.Aram;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using dsstats.db8;

namespace dsstats.db8services;

public partial class AramService
{
    public async Task ReportReplay(ReplayDto replayDto)
    {
        if (!replayDto.TournamentEdition)
        {
            return;
        }

        if (replayDto.GameTime < DateTime.UtcNow.AddHours(-6))
        {
            return;
        }

        var openMatches = await context.AramMatches
            .Include(i => i.AramEvent)
            .Include(i => i.AramSlots)
                .ThenInclude(i => i.AramPlayer)
            .Where(x => x.MatchResult == MatchResult.None)
            .ToListAsync();

        if (openMatches.Count == 0)
        {
            return;
        }

        var toonIds = replayDto.ReplayPlayers.Select(s => s.Player.ToonId).ToList();
        var replayPlayerIds = replayDto.ReplayPlayers
            .Select(s => new PlayerId(s.Player.ToonId, s.Player.RealmId, s.Player.RegionId))
            .ToList();

        var dbplayerIds = await context.Players
            .Where(x => toonIds.Contains(x.ToonId))
            .Select(s => new { s.PlayerId, Id = new PlayerId(s.ToonId, s.RealmId, s.RegionId) })
            .ToListAsync();

        var playerIds = dbplayerIds
            .Where(x => replayPlayerIds.Contains(x.Id)).Select(s => s.PlayerId)
            .OrderBy(o => o);

        bool isEu = false;
        if (replayPlayerIds.Any(a => a.RegionId == 2))
        {
            isEu = true;
        }

        var replayMatches = openMatches.Where(x => x.AramSlots
                .Select(s => (isEu ? s.AramPlayer!.EuPlayerId : s.AramPlayer!.AmPlayerId) ?? 0).OrderBy(o => o)
                .SequenceEqual(playerIds))
            .ToList();

        AramMatch? match = null;
        if (replayMatches.Count == 0)
        {
            return;
        }
        else if (replayMatches.Count == 1)
        {
            match = replayMatches[0];
        }
        else
        {
            // todo
        }

        if (match is null)
        {
            return;
        }

        var replayId = await context.Replays
            .Where(x => x.ReplayHash == replayDto.ReplayHash)
            .Select(s => s.ReplayId)
            .FirstOrDefaultAsync();

        if (replayId == 0)
        {
            return;
        }

        if (match.Replay1Id == null)
        {
            match.Replay1Id = replayId;
        }
        else if (match.Replay1Id == null)
        {
            match.Replay1Id = replayId;
        }

        if (match.Replay1Id != null && match.Replay2Id != null)
        {
            await SetMatchResult(match);
        }

        await context.SaveChangesAsync();
    }

    private async Task SetMatchResult(AramMatch match)
    {
        var replays = await context.Replays
            .Include(i => i.ReplayPlayers)
                .ThenInclude(i => i.Player)
            .Where(x => x.ReplayId == match.Replay1Id || x.ReplayId == match.Replay2Id)
            .ToListAsync();

        if (replays.Count != 2)
        {
            return;
        }

        List<KeyValuePair<MatchResult, int>> results = [];
        foreach (var replay in replays.OrderBy(o => o.GameTime))
        {
            bool isEu = replay.ReplayPlayers.Any(a => a.Player.RegionId == 2);
            var team1PlayerIds = match.AramSlots
                .Where(x => x.Team == 1)
                .Select(s => isEu ? s.AramPlayer!.EuPlayerId : s.AramPlayer!.AmPlayerId);

            bool replayTeamIsMatchTeam = replay.ReplayPlayers
                .Where(x => x.Team == 1).Any(a => team1PlayerIds.Contains(a.Player.PlayerId));

            MatchResult matchResult;
            if (replay.WinnerTeam == 1)
            {
                matchResult = replayTeamIsMatchTeam ? MatchResult.Team1Win : MatchResult.Team2Win;
            }
            else
            {
                matchResult = replayTeamIsMatchTeam ? MatchResult.Team2Win : MatchResult.Team1Win;
            }
            results.Add(new(matchResult, replay.Duration));
        }

        MatchResult finalResult = MatchResult.None;

        if (results.All(a => a.Key == MatchResult.Team1Win))
        {
            finalResult = MatchResult.Team1Win;
        }
        else if (results.All(a => a.Key == MatchResult.Team2Win))
        {
            finalResult = MatchResult.Team2Win;
        }
        else
        {
            if (results[1].Value < results[0].Value)
            {
                finalResult = results[1].Key;
            }
            else
            {
                finalResult = results[0].Key;
            }
        }
        match.MatchResult = finalResult;
    }

    public async Task ReportMatch(MatchReport report)
    {
        var match = await context.AramMatches
            .Include(i => i.AramEvent)
            .Include(i => i.AramSlots)
                .ThenInclude(i => i.AramPlayer)
            .FirstOrDefaultAsync(f => f.Guid == report.MatchGuid);

        if (match is null)
        {
            return;
        }

        if (report.Result != MatchResult.None)
        {
            match.MatchResult = report.Result;
        }

        await context.SaveChangesAsync();
    }

    private async Task<MatchResult> SetMatchReplays(AramMatch match, MatchReport report)
    {
        Replay? replay1 = null;
        Replay? replay2 = null;

        if (!string.IsNullOrEmpty(report.ReplayHash1))
        {
            replay1 = await context.Replays
                .Include(i => i.ReplayPlayers)
                    .ThenInclude(i => i.Player)
                    .FirstOrDefaultAsync(f => f.ReplayHash == report.ReplayHash1);
        }

        if (!string.IsNullOrEmpty(report.ReplayHash2))
        {
            replay2 = await context.Replays
                .Include(i => i.ReplayPlayers)
                    .ThenInclude(i => i.Player)
                    .FirstOrDefaultAsync(f => f.ReplayHash == report.ReplayHash2);
        }

        return MatchResult.None;
    }

    public async Task AssignReplays()
    {
        var aramEvents = await context.AramEvents
            .Include(i => i.AramMatches)
            .Where(x => x.EndTime > DateTime.UtcNow)
            .AsNoTracking()
            .ToListAsync();

        if (aramEvents.Count == 0)
        {
            return;
        }

        var startTime = aramEvents.OrderBy(o => o.StartTime).Select(s => s.StartTime).First();
        var assignedReplayIds = aramEvents.SelectMany(s => s.AramMatches).Select(s => s.Replay1Id ?? 0).ToHashSet();
        assignedReplayIds.UnionWith(aramEvents.SelectMany(s => s.AramMatches).Select(s => s.Replay2Id ?? 0));
        var gameModes = aramEvents.Select(o => o.GameMode).ToHashSet();

        var replays = await context.Replays
            .Include(i => i.ReplayPlayers)
                .ThenInclude(i => i.Player)
            .Where(x => x.GameTime > startTime
                && x.TournamentEdition
                && gameModes.Contains(x.GameMode)
                && !assignedReplayIds.Contains(x.ReplayId))
            .AsNoTracking()
            .ToListAsync();

        if (replays.Count == 0)
        {
            return;
        }

        foreach (var replay in replays)
        {
            await TryAssignReplay(replay);
        }
    }

    private async Task TryAssignReplay(Replay replay)
    {
        var playerIds = replay.ReplayPlayers
            .Select(s => s.PlayerId)
            .OrderBy(o => o).ToList();

        var aramMatchesSelect = from m in context.AramMatches
                                from s in m.AramSlots
                                join p in context.AramPlayers on s.AramPlayerId equals p.AramPlayerId
                                where
                                  m.AramEvent!.EndTime > DateTime.UtcNow
                                  && m.MatchResult == MatchResult.None
                                  && ((p.EuPlayerId != null && playerIds.Contains(p.EuPlayerId ?? 0))
                                   || (p.AmPlayerId != null && playerIds.Contains(p.AmPlayerId ?? 0)))
                                select m;

        var aramMatches = await aramMatchesSelect.ToListAsync();

        if (aramMatches.Count == 0)
        {
            return;
        }

        foreach (var aramMatch in aramMatches)
        {
            List<int> aramPlayerIds = aramMatch.AramSlots.Select(s =>
                s.AramPlayer!.EuPlayerId != null ? s.AramPlayer.EuPlayerId ?? 0 :
                s.AramPlayer.AmPlayerId != null ? s.AramPlayer.AmPlayerId ?? 0 : 0)
                .OrderBy(o => o).ToList();

            if (playerIds.SequenceEqual(aramPlayerIds))
            {
                if (aramMatch.Replay1Id is null)
                {
                    aramMatch.Replay1Id = replay.ReplayId;
                }
                else if (aramMatch.Replay2Id is null)
                {
                    aramMatch.Replay2Id = replay.ReplayId;
                }

                if (aramMatch.Replay1Id is not null
                    && aramMatch.Replay2Id is not null)
                {
                    await SetMatchResult(aramMatch);
                }
                await context.SaveChangesAsync();
                break;
            }
        }
    }
}

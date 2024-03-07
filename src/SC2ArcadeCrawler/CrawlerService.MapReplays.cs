using dsstats.db8;
using dsstats.db8services;
using dsstats.db8services.Import;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Cryptography;

namespace SC2ArcadeCrawler;

public partial class CrawlerService
{
    public async Task MapReplays()
    {
        await CleanUp();
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();

        int arcadeReps = 0;
        int dsstatsReps = 0;
        int hits = 0;
        int skip = 0;
        int take = 10_000;

        var arcadeReplays = await context.ArcadeReplays
            .Include(i => i.ArcadeReplayPlayers)
                .ThenInclude(i => i.ArcadePlayer)
            .OrderBy(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        while (arcadeReplays.Count > 0)
        {

            var startDate = arcadeReplays.First().CreatedAt.AddDays(-2);
            var endDate = arcadeReplays.Last().CreatedAt.AddDays(2);

            var dsstatsReplays = await context.Replays
                .Include(i => i.ReplayPlayers)
                    .ThenInclude(i => i.Player)
                .OrderBy(o => o.GameTime)
                .Where(x => (x.GameTime > startDate && x.GameTime < endDate)
                    && !x.TournamentEdition
                    && x.Playercount == 6
                    && (x.GameMode == GameMode.Standard || x.GameMode == GameMode.Commanders || x.GameMode == GameMode.CommandersHeroic))
                .ToListAsync();

            if (dsstatsReplays.Count == 0)
            {
                return;
            }

            var arcadeDic = GetArcadeDic(arcadeReplays);
            var dsstatsDic = GetDsstatsDic(dsstatsReplays);

            arcadeReps += arcadeReplays.Count;
            dsstatsReps += dsstatsReplays.Count;


            foreach (var ent in arcadeDic)
            {
                if (dsstatsDic.TryGetValue(ent.Key, out var replays)
                    && replays is not null)
                {
                    hits++;
                    AddDsstatsInfo(ent.Value, replays);
                }
            }
            await context.SaveChangesAsync();

            skip += take;
            arcadeReplays = await context.ArcadeReplays
                .Include(i => i.ArcadeReplayPlayers)
                    .ThenInclude(i => i.ArcadePlayer)
                .OrderBy(o => o.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        logger.LogWarning("arcade: {arcadeOnly}, dsstats: {dsstatsOnly}, hits: {hits}", arcadeReps, dsstatsReps, hits);
    }

    private async Task CleanUp()
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();

        var replays = await context.Replays
            .Include(i => i.ReplayPlayers)
            .Where(x => x.GameTime > new DateTime(2021, 2, 1)
                && x.CommandersTeam1 == string.Empty && x.CommandersTeam2 == string.Empty)
            .ToListAsync();

        context.Replays.RemoveRange(replays);
        await context.SaveChangesAsync();
    }

    private void AddDsstatsInfo(List<ArcadeReplay> arcadeReplays, List<Replay> dsstatsReplays)
    {
        if (dsstatsReplays.Count == 0)
        {
            return;
        }

        var availableReplays = new List<Replay>(dsstatsReplays);

        foreach (var arcadeReplay in arcadeReplays)
        {
            var dsstatsReplay = availableReplays.Count == 1 ? availableReplays[0] :
                dsstatsReplays.OrderByDescending(o => GetHitScore(arcadeReplay, o)).First();
            availableReplays.Remove(dsstatsReplay);
            arcadeReplay.ReplayId = dsstatsReplay.ReplayId;
        }
    }

    private static int GetHitScore(ArcadeReplay arcadeReplay, Replay dsstatsReplay)
    {
        int timeHitScore = CalculateTimeHitScore(arcadeReplay.CreatedAt, dsstatsReplay.GameTime);
        int durationHitScore = CalculateDurationHitScore(arcadeReplay.Duration, dsstatsReplay.Duration);
        int playerHitScore = CalculatePlayerHitScore([.. arcadeReplay.ArcadeReplayPlayers], [.. dsstatsReplay.ReplayPlayers]);

        int combinedHitScore = (int)((timeHitScore * 0.4) + (durationHitScore * 0.3) + (playerHitScore * 0.3));

        return combinedHitScore;
    }

    private static int CalculateTimeHitScore(DateTime arcadeReplayTime, DateTime dsstatsReplayTime)
    {
        TimeSpan timeDifference = arcadeReplayTime - dsstatsReplayTime;
        double timeScore = Math.Abs(timeDifference.TotalMinutes);
        return (int)(100 - timeScore);
    }

    private static int CalculateDurationHitScore(int arcadeDurationInSeconds, int dsstatsDurationInSeconds)
    {
        double durationDifference = Math.Abs(arcadeDurationInSeconds - dsstatsDurationInSeconds);
        return (int)(100 - (durationDifference / 10));
    }

    private static int CalculatePlayerHitScore(List<ArcadeReplayPlayer> arcadePlayers, List<ReplayPlayer> dsstatsPlayers)
    {
        var arcadePlayerIds = arcadePlayers.OrderBy(o => o.SlotNumber)
                                           .Select(s => new PlayerId(s.ArcadePlayer.ProfileId, s.ArcadePlayer.RealmId, s.ArcadePlayer.RegionId))
                                           .ToList();

        var dsstatsPlayerIds = dsstatsPlayers.OrderBy(o => o.GamePos)
                                             .Select(s => new PlayerId(s.Player.ToonId, s.Player.RealmId, s.Player.RegionId))
                                             .ToList();

        int orderMatchScore = CalculateOrderMatchScore(arcadePlayerIds, dsstatsPlayerIds);
        int commonPlayers = arcadePlayerIds.Intersect(dsstatsPlayerIds).Count();
        int totalPlayers = Math.Max(arcadePlayerIds.Count, dsstatsPlayerIds.Count);
        int playerHitScore = (int)(((commonPlayers / (double)totalPlayers) * 0.5 + (orderMatchScore
            / (double)Math.Max(arcadePlayerIds.Count, dsstatsPlayerIds.Count)) * 0.5) * 100);

        return playerHitScore;
    }

    private static int CalculateOrderMatchScore(List<PlayerId> arcadePlayerIds, List<PlayerId> dsstatsPlayerIds)
    {
        int matchedOrderCount = 0;

        // Iterate through the player IDs and count how many match in order
        for (int i = 0; i < Math.Min(arcadePlayerIds.Count, dsstatsPlayerIds.Count); i++)
        {
            if (arcadePlayerIds[i].Equals(dsstatsPlayerIds[i]))
            {
                matchedOrderCount++;
            }
        }

        return matchedOrderCount;
    }


    private async Task<Dictionary<int, Player>> GetPlayerIds(ArcadeReplay arcadeReplay, ImportService importService, ReplayContext context)
    {
        Dictionary<int, int> idMap = [];
        foreach (var arcadeReplayPlayer in arcadeReplay.ArcadeReplayPlayers)
        {
            dsstats.shared.PlayerId playerId = new(arcadeReplayPlayer.ArcadePlayer.ProfileId,
                arcadeReplayPlayer.ArcadePlayer.RealmId,
                arcadeReplayPlayer.ArcadePlayer.RegionId);
            int dsId = await importService.GetPlayerIdAsync(playerId, arcadeReplayPlayer.Name);
            idMap[arcadeReplayPlayer.ArcadePlayerId] = dsId;
        }

        var playerIds = idMap.Values.ToList();
        var players = await context.Players
            .Where(x => playerIds.Contains(x.PlayerId))
            .ToListAsync();

        Dictionary<int, Player> playerMap = [];

        foreach (var ent in idMap)
        {
            playerMap[ent.Key] = players.First(f => f.PlayerId == ent.Value);
        }
        return playerMap;
    }

    private static Dictionary<string, List<ArcadeReplay>> GetArcadeDic(List<ArcadeReplay> arcadeReplays)
    {
        Dictionary<string, List<ArcadeReplay>> arcadeDic = [];

        foreach (var replay in arcadeReplays)
        {
            var key = GetKey(replay);
            if (!arcadeDic.TryGetValue(key, out var replays))
            {
                replays = arcadeDic[key] = [];
            }
            replays.Add(replay);
        }
        return arcadeDic;
    }

    private static Dictionary<string, List<Replay>> GetDsstatsDic(List<Replay> dsstatsReplays)
    {
        Dictionary<string, List<Replay>> dsstatsDic = [];
        foreach (var replay in dsstatsReplays)
        {
            var key = GetKey(replay);
            if (!dsstatsDic.TryGetValue(key, out var replays))
            {
                replays = dsstatsDic[key] = [];
            }
            replays.Add(replay);
        }
        return dsstatsDic;
    }

    private static string GetKey(ArcadeReplay replay)
    {
        return string.Join("-", replay.ArcadeReplayPlayers
            .OrderBy(o => o.ArcadePlayer.ProfileId)
            .Select(s => $"{s.ArcadePlayer.ProfileId}|{s.ArcadePlayer.RegionId}|{s.ArcadePlayer.RealmId}"));
    }

    private static string GetKey(Replay replay)
    {
        return string.Join("-", replay.ReplayPlayers
            .OrderBy(o => o.Player.ToonId)
            .Select(s => $"{s.Player.ToonId}|{s.Player.RegionId}|{s.Player.RealmId}"));
    }
}

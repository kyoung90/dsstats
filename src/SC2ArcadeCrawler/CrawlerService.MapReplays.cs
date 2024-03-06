using dsstats.db8;
using dsstats.db8.Extensions;
using dsstats.db8services.Import;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace SC2ArcadeCrawler;

public partial class CrawlerService
{
    public async Task MapReplays()
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();

        var arcadeReplays = await context.ArcadeReplays
            .Include(i => i.ArcadeReplayPlayers)
                .ThenInclude(i => i.ArcadePlayer)
            .OrderBy(o => o.CreatedAt)
            .Skip(0)
            .Take(1000)
            .ToListAsync();

        if (arcadeReplays.Count == 0)
        {
            return;
        }

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

        int arcadeReps = arcadeReplays.Count;
        int dsstatsReps = dsstatsReplays.Count;
        int hits = 0;
        int created = 0;

        MD5 md5 = MD5.Create();
        var importService = scope.ServiceProvider.GetRequiredService<ImportService>();

        foreach (var ent in arcadeDic)
        {
            if (dsstatsDic.TryGetValue(ent.Key, out var replays)
                && replays is not null)
            {
                hits++;
            }
            else
            {
                foreach (var rep in ent.Value)
                {
                    if (await CreateDsstatsReplay(rep, md5, importService, context))
                    {
                        created++;
                    }
                }
            }
        }
        dsstatsReps = dsstatsDic.Count;


        logger.LogWarning("arcade: {arcadeOnly}, dsstats: {dsstatsOnly}, hits: {hits}", arcadeReps, dsstatsReps, hits);
    }

    private async Task<bool> CreateDsstatsReplay(ArcadeReplay arcadeReplay, MD5 md5hash, ImportService importService, ReplayContext context)
    {
        try
        {
            var arcadeInfo = await context.ArcadeInfos.FirstOrDefaultAsync(f => 
                f.BnetBucketId == arcadeReplay.BnetBucketId
                && f.BnetRecordId == arcadeReplay.BnetRecordId);

            if (arcadeInfo is not null)
            {
                return false;
            }

            var playerMap = await GetPlayerIds(arcadeReplay, importService, context);

            Replay replay = new()
            {
                GameMode = arcadeReplay.GameMode,
                GameTime = arcadeReplay.CreatedAt,
                Duration = arcadeReplay.Duration,
                Playercount = (byte)arcadeReplay.PlayerCount,
                WinnerTeam = arcadeReplay.WinnerTeam,
                Minkillsum = Random.Shared.Next(1, 1000),
                Minarmy = Random.Shared.Next(1, 1000),
                Maxkillsum = Random.Shared.Next(999, 2000),
                Imported = DateTime.UtcNow,
                FileName = string.Empty,
                CommandersTeam1 = string.Empty,
                CommandersTeam2 = string.Empty,
                Middle = string.Empty,
                ReplayPlayers = arcadeReplay.ArcadeReplayPlayers.Select(s => new ReplayPlayer()
                {
                    Name = s.Name,
                    GamePos = s.SlotNumber,
                    Team = s.Team,
                    Duration = arcadeReplay.Duration,
                    PlayerResult = s.PlayerResult,
                    Player = playerMap[s.ArcadePlayerId],
                    TierUpgrades = string.Empty,
                    Refineries = string.Empty
                }).ToList(),
                ArcadeInfo = new()
                {
                    BnetBucketId = arcadeReplay.BnetBucketId,
                    BnetRecordId = arcadeReplay.BnetRecordId,
                }
            };
            replay.GenHash(md5hash);
            context.Replays.Add(replay);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("failed creating dsstats replay: {error}", ex.Message);
        }
        return false;
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
        //return string.Join("-", replay.ArcadeReplayPlayers
        //    .OrderBy(o => o.ArcadePlayer.ProfileId)
        //    .Select(s => $"{s.ArcadePlayer.ProfileId}|{s.ArcadePlayer.RegionId}|{s.ArcadePlayer.RealmId}"));
        return string.Join("-", replay.ArcadeReplayPlayers
            .OrderBy(o => o.ArcadePlayer.ProfileId)
            .Select(s => $"{s.ArcadePlayer.ProfileId}|{s.ArcadePlayer.RegionId}"));
    }

    private static string GetKey(Replay replay)
    {
        //return string.Join("-", replay.ReplayPlayers
        //    .OrderBy(o => o.Player.ToonId)
        //    .Select(s => $"{s.Player.ToonId}|{s.Player.RegionId}|{s.Player.RealmId}"));
        return string.Join("-", replay.ReplayPlayers
            .OrderBy(o => o.Player.ToonId)
            .Select(s => $"{s.Player.ToonId}|{s.Player.RegionId}"));
    }
}

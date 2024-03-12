using dsstats.db8;
using dsstats.ratings;
using dsstats.shared;
using dsstats.shared.Calc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Diagnostics;

namespace SC2ArcadeCrawler;

public partial class CrawlerService
{
    public async Task MapCalcReplays(DateTime fromDate)
    {
        Stopwatch sw = Stopwatch.StartNew();
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();

        int arcadeReps = 0;
        int dsstatsReps = 0;
        int hits = 0;
        int skip = 0;
        int take = 100_000;

        var arcadeCalcDtos = await GetArcadeCalcDtos(skip, take, fromDate, context);

        Dictionary<int, int> arcadeDsstatsMap = [];

        while (arcadeCalcDtos.Count > 0)
        {
            var dsstatsCalcDtos = await GetDsstatsCalcDtos(arcadeCalcDtos, context);

            if (dsstatsCalcDtos.Count == 0)
            {
                continue;
            }

            var arcadeDic = GetCalcDic(arcadeCalcDtos);
            var dsstatsDic = GetCalcDic(dsstatsCalcDtos);

            foreach (var ent in  dsstatsDic)
            {
                if (arcadeDic.TryGetValue(ent.Key, out var replays)
                    && replays is not null)
                {
                    hits++;
                    MapCalcDtos(replays, ent.Value, arcadeDsstatsMap);
                }
            }

            await StoreMapInfo(arcadeDsstatsMap);

            skip += take;
            arcadeCalcDtos = await GetArcadeCalcDtos(skip, take, fromDate, context);
        }
        await StoreMapInfo(arcadeDsstatsMap, true);
        sw.Stop();
        logger.LogWarning("arcade: {arcadeOnly}, dsstats: {dsstatsOnly}, hits: {hits}, elapsed: {time} ms", arcadeReps, dsstatsReps, hits, sw.ElapsedMilliseconds);
    }

    private static void MapCalcDtos(List<CalcDto> arcadeCalcDtos, List<CalcDto> dsstatsCalcDtos, Dictionary<int, int> arcadeDsstatsMap)
    {
        if (dsstatsCalcDtos.Count == 0)
        {
            return;
        }

        var availableCalcDtos = new List<CalcDto>(dsstatsCalcDtos);
        foreach (var arcadeCalcDto in arcadeCalcDtos)
        {
            var dsstatsCalcDto = availableCalcDtos.Count == 1 ? availableCalcDtos[0]
                : dsstatsCalcDtos.OrderByDescending(o => GetCalcHitScore(arcadeCalcDto, o)).First();
            availableCalcDtos.Remove(dsstatsCalcDto);
            arcadeDsstatsMap[arcadeCalcDto.ReplayId] = dsstatsCalcDto.ReplayId;
        }
    }

    private static async Task<List<CalcDto>> GetDsstatsCalcDtos(List<CalcDto> arcadeCalcDtos, ReplayContext context)
    {
        if (arcadeCalcDtos.Count == 0)
        {
            return [];
        }

        var fromDate = arcadeCalcDtos.First().GameTime.AddDays(-2);
        var toDate = arcadeCalcDtos.Last().GameTime.AddDays(2);

        var query = context.Replays
            .Where(x => x.Playercount == 6
             && x.Duration >= 300
             && x.WinnerTeam > 0
             && x.GameTime >= fromDate && x.GameTime <= toDate)
            .Select(s => new RawCalcDto()
            {
                DsstatsReplayId = s.ReplayId,
                GameTime = s.GameTime,
                Duration = s.Duration,
                Maxkillsum = s.Maxkillsum,
                GameMode = (int)s.GameMode,
                TournamentEdition = s.TournamentEdition,
                WinnerTeam = s.WinnerTeam,
                Players = s.ReplayPlayers.Select(t => new RawPlayerCalcDto()
                {
                    ReplayPlayerId = t.ReplayPlayerId,
                    GamePos = t.GamePos,
                    PlayerResult = (int)t.PlayerResult,
                    Race = t.Race,
                    Duration = t.Duration,
                    Kills = t.Kills,
                    Team = t.Team,
                    IsUploader = t.Player.UploaderId != null,
                    PlayerId = new(t.Player.ToonId, t.Player.RealmId, t.Player.RegionId)
                }).ToList()

            });

        var rawDtos = await query
            .AsSplitQuery()
            .ToListAsync();

        return rawDtos.Select(s => s.GetCalcDto()).ToList();
    }

    private async Task<List<CalcDto>> GetArcadeCalcDtos(int skip, int take, DateTime fromDate, ReplayContext context)
    {
        if (skip == 0)
        {
            await CreateMaterializedReplays();
        }

        var query = from r in context.MaterializedArcadeReplays
                    where r.CreatedAt >= fromDate
                    orderby r.MaterializedArcadeReplayId
                    select new CalcDto()
                    {
                        ReplayId = r.ArcadeReplayId,
                        GameTime = r.CreatedAt,
                        Duration = r.Duration,
                        GameMode = (int)r.GameMode,
                        WinnerTeam = r.WinnerTeam,
                        DsstatsReplayId = r.ReplayId,
                        Players = context.ArcadeReplayPlayers
                            .Where(x => x.ArcadeReplayId == r.ArcadeReplayId)
                            .Select(t => new PlayerCalcDto()
                            {
                                ReplayPlayerId = t.ArcadeReplayPlayerId,
                                GamePos = t.SlotNumber,
                                PlayerResult = (int)t.PlayerResult,
                                Team = t.Team,
                                PlayerId = new(t.ArcadePlayer.ProfileId, t.ArcadePlayer.RealmId, t.ArcadePlayer.RegionId)
                            }).ToList()
                    };
        return await query
        .AsSplitQuery()
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    private async Task CreateMaterializedReplays()
    {
        Stopwatch sw = Stopwatch.StartNew();

        using var scope = serviceProvider.CreateScope();
        var importOptions = scope.ServiceProvider.GetRequiredService<IOptions<DbImportOptions>>();
        try
        {
            using var connection = new MySqlConnection(importOptions.Value.ImportConnectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandTimeout = 120;
            command.CommandText = "CALL CreateMaterializedArcadeReplays();";

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("failed creating materialized arcade replays: {error}", ex.Message);
        }
        logger.LogWarning("materialized arcade replays produced in {time} ms", sw.ElapsedMilliseconds);
    }

    private async Task StoreMapInfo(Dictionary<int, int> arcadeDsstatsMap, bool force = false)
    {
        if (!force && arcadeDsstatsMap.Count < 10000)
        {
            return;
        }

        Stopwatch sw = Stopwatch.StartNew();

        using var scope = serviceProvider.CreateScope();
        var importOptions = scope.ServiceProvider.GetRequiredService<IOptions<DbImportOptions>>();
        try
        {
            using var connection = new MySqlConnection(importOptions.Value.ImportConnectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandTimeout = 120;
            command.CommandText = string.Join(";", arcadeDsstatsMap.Select(s => 
                $"UPDATE {nameof(ReplayContext.ArcadeReplays)} SET {nameof(ArcadeReplay.ReplayId)} = {s.Value} WHERE {nameof(ArcadeReplay.ArcadeReplayId)} = {s.Key}"
            ));
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("failed storing arcade dsstats info: {error}", ex.Message);
        }
        logger.LogWarning("arcade dsstats info stored in {time} ms", sw.ElapsedMilliseconds);
        arcadeDsstatsMap.Clear();
    }

    private static int GetCalcHitScore(CalcDto arcadeReplay, CalcDto dsstatsReplay)
    {
        int timeHitScore = CalculateTimeHitScore(arcadeReplay.GameTime, dsstatsReplay.GameTime);
        int durationHitScore = CalculateDurationHitScore(arcadeReplay.Duration, dsstatsReplay.Duration);
        int playerHitScore = CalculateCalcPlayerHitScore([.. arcadeReplay.Players], [.. dsstatsReplay.Players]);

        int combinedHitScore = (int)((timeHitScore * 0.4) + (durationHitScore * 0.3) + (playerHitScore * 0.3));

        return combinedHitScore;
    }

    private static int CalculateCalcPlayerHitScore(List<PlayerCalcDto> arcadePlayers, List<PlayerCalcDto> dsstatsPlayers)
    {
        var arcadePlayerIds = arcadePlayers.OrderBy(o => o.GamePos)
                                           .Select(s => s.PlayerId)
                                           .ToList();

        var dsstatsPlayerIds = dsstatsPlayers.OrderBy(o => o.GamePos)
                                             .Select(s => s.PlayerId)
                                             .ToList();

        int orderMatchScore = CalculateOrderMatchScore(arcadePlayerIds, dsstatsPlayerIds);
        int commonPlayers = arcadePlayerIds.Intersect(dsstatsPlayerIds).Count();
        int totalPlayers = Math.Max(arcadePlayerIds.Count, dsstatsPlayerIds.Count);
        int playerHitScore = (int)(((commonPlayers / (double)totalPlayers) * 0.5 + (orderMatchScore
            / (double)Math.Max(arcadePlayerIds.Count, dsstatsPlayerIds.Count)) * 0.5) * 100);

        return playerHitScore;
    }

    private static Dictionary<string, List<CalcDto>> GetCalcDic(List<CalcDto> dsstatsReplays)
    {
        Dictionary<string, List<CalcDto>> dsstatsDic = [];
        foreach (var replay in dsstatsReplays)
        {
            var key = GetCalcKey(replay);
            if (!dsstatsDic.TryGetValue(key, out var replays))
            {
                replays = dsstatsDic[key] = [];
            }
            replays.Add(replay);
        }
        return dsstatsDic;
    }

    private static string GetCalcKey(CalcDto replay)
    {
        return string.Join("-", replay.Players
            .OrderBy(o => o.PlayerId.ToonId)
            .Select(s => $"{s.PlayerId.ToonId}|{s.PlayerId.RegionId}|{s.PlayerId.RealmId}"));
    }
}

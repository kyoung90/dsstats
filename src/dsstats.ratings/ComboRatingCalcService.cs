using dsstats.db8;
using dsstats.shared;
using dsstats.shared.Calc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Diagnostics;

namespace dsstats.ratings;

public class ComboRatingCalcService(ReplayContext context,
                                     IOptions<DbImportOptions> importOptions,
                                     ILogger<ComboRatingCalcService> logger)
    : RatingCalcService(context, importOptions, logger)
{
    private Dictionary<int, bool> processedDsstatsReplayIds = [];

    protected override async Task<List<CalcDto>> GetCalcDtosAsync(CalcRequest calcRequest)
    {
        if (calcRequest.Skip == 0)
        {
            await CreateMaterializedReplays();
        }

        var query = from r in context.MaterializedArcadeReplays
                    where r.ReplayId == null
                    orderby r.MaterializedArcadeReplayId
                    select new CalcDto()
                    {
                        ReplayId = r.ArcadeReplayId,
                        GameTime = r.CreatedAt,
                        Duration = r.Duration,
                        GameMode = (int)r.GameMode,
                        WinnerTeam = r.WinnerTeam,
                        DsstatsReplayId = r.ReplayId,
                        IsArcade = true,
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
        var arcadeCalcDtos = await query
            .AsSplitQuery()
            .Skip(calcRequest.Skip)
            .Take(calcRequest.Take)
            .ToListAsync();

        var dsstatsCalcDtos = await GetDsstatsCalcDtos(arcadeCalcDtos, calcRequest);

        arcadeCalcDtos.AddRange(dsstatsCalcDtos);

        return arcadeCalcDtos.OrderBy(o => o.GameTime).ToList();
    }

    private async Task<List<CalcDto>> GetDsstatsCalcDtos(List<CalcDto> arcadeCalcDtos, CalcRequest calcRequest)
    {
        if (arcadeCalcDtos.Count == 0)
        {
            return [];
        }

        var fromDate = arcadeCalcDtos.First().GameTime.AddDays(-2);
        if (fromDate < calcRequest.FromDate)
        {
            fromDate = calcRequest.FromDate;
        }
        var toDate = arcadeCalcDtos.Last().GameTime.AddDays(2);

        var query = context.Replays
            .Where(x => x.Playercount == 6
             && x.Duration >= 300
             && x.WinnerTeam > 0
             && x.GameTime >= fromDate
             && x.GameTime <= toDate
             && calcRequest.GameModes.Contains(x.GameMode))
            .OrderBy(o => o.GameTime)
                .ThenBy(o => o.ReplayId)
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

        List<CalcDto> calcDtos = [];

        foreach (var rawDto in rawDtos)
        {
            if (processedDsstatsReplayIds.ContainsKey(rawDto.DsstatsReplayId))
            {
                continue;
            }
            else
            {
                processedDsstatsReplayIds[rawDto.DsstatsReplayId] = true;
                calcDtos.Add(rawDto.GetCalcDto());
            }
        }
        return calcDtos;
    }

    private async Task CreateMaterializedReplays()
    {
        Stopwatch sw = Stopwatch.StartNew();
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
        sw.Stop();
        logger.LogWarning("materialized arcade replays produced in {time} ms", sw.ElapsedMilliseconds);
    }

    protected override async Task<CalcRatingRequest> GetCalcRatingRequestAsync(DateTime fromDate)
    {
        return await Task.FromResult(new CalcRatingRequest()
        {
            RatingCalcType = RatingCalcType.Arcade,
            StarTime = fromDate,
            MmrIdRatings = new()
                    {
                        { 1, new() },
                        { 2, new() },
                        { 3, new() },
                        { 4, new() }
                    },
        });
    }
}

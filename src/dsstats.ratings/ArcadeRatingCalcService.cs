
using dsstats.db8;
using dsstats.shared;
using dsstats.shared.Calc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Diagnostics;

namespace dsstats.ratings;

public class ArcadeRatingCalcService(ReplayContext context,
                                     IOptions<DbImportOptions> importOptions,
                                     ILogger<ArcadeRatingCalcService> logger) 
    : RatingCalcService(context, importOptions, logger)
{
    protected override async Task<List<CalcDto>> GetCalcDtosAsync(CalcRequest calcRequest)
    {
        if (calcRequest.Skip == 0)
        {
            await CreateMaterializedReplays();
        }

        var query = from r in context.MaterializedArcadeReplays
                    // where r.CreatedAt >= calcRequest.FromDate
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
            .Skip(calcRequest.Skip)
            .Take(calcRequest.Take)
            .ToListAsync();
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

    protected override async Task<CalcRatingNgRequest> GetCalcRatingRequestAsync(DateTime fromDate)
    {
        return await Task.FromResult(new CalcRatingNgRequest()
        {
            MmrIdRatings = new()
                    {
                        { (int)RatingNgType.All, new() },
                        { (int)RatingNgType.Cmdr, new() },
                        { (int)RatingNgType.Std, new() },
                        { (int)RatingNgType.Brawl, new() },
                        { (int)RatingNgType.CmdrTE, new() },
                        { (int)RatingNgType.StdTE, new() },
                        { (int)RatingNgType.Std1v1, new() },
                        { (int)RatingNgType.Cmdr1v1, new() },
                    },
        });
    }
}

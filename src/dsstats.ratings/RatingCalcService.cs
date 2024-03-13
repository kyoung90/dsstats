﻿using dsstats.db8;
using dsstats.shared;
using dsstats.shared.Calc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;

namespace dsstats.ratings;

public abstract partial class RatingCalcService(ReplayContext context, IOptions<DbImportOptions> importOptions, ILogger logger)
{
    protected readonly ReplayContext context = context;
    protected readonly IOptions<DbImportOptions> importOptions = importOptions;

    protected abstract Task<List<CalcDto>> GetCalcDtosAsync(CalcRequest calcRequest);
    protected abstract Task<CalcRatingNgRequest> GetCalcRatingRequestAsync(DateTime fromDate);

    public async Task ProduceRatings(CalcRequest request)
    {
        Stopwatch sw = Stopwatch.StartNew();
        var calcDtos = await GetCalcDtosAsync(request);
        var ratingRequest = await GetCalcRatingRequestAsync(request.FromDate);

        List<ReplayNgRatingResult> replayRatings = [];
        while(calcDtos.Count > 0)
        {
            for (int i = 0; i < calcDtos.Count; i++)
            {
                var calcDto = calcDtos[i];
                var ratings = lib.RatingsNg.ProcessReplayNg(calcDto, ratingRequest);

                if (calcDto.IsArcade)
                {
                    continue;
                }

                replayRatings.AddRange(ratings);
            }
            await SaveStepResult(replayRatings, ratingRequest);
            replayRatings.Clear();

            // DebugMmrIdRatings(ratingRequest.MmrIdRatings);

            request.Skip += request.Take;
            calcDtos = await GetCalcDtosAsync(request);
        }
        await SaveResult(ratingRequest);
        sw.Stop();
        logger.LogWarning("Ratings produced in {time} min.", sw.Elapsed.ToString(@"mm\:ss"));
    }

    private void DebugMmrIdRatings(Dictionary<int, Dictionary<PlayerId, CalcRating>> mmrIdRatings)
    {
        StringBuilder sb = new();
        foreach (var ent in mmrIdRatings)
        {
            int count = ent.Value.Count;
            int maxGames = ent.Value.Max(m => m.Value.Games);
            int maxRating = Convert.ToInt32(ent.Value.Max(m => m.Value.Mmr));
            sb.AppendLine($"{ent.Key} ({(RatingNgType)ent.Key}) => count: {count}, maxGames: {maxGames}, maxRating {maxRating}");
        }
        logger.LogWarning("debug: {info}", sb.ToString());
    }

    protected virtual async Task SaveResult(CalcRatingNgRequest request)
    {
        var playerIds = (await context.Players
            .Select(s => new { s.ToonId, s.RealmId, s.RegionId, s.PlayerId }).ToListAsync())
            .ToDictionary(k => new PlayerId(k.ToonId, k.RealmId, k.RegionId), v => v.PlayerId);
        var connectionString = importOptions.Value.ImportConnectionString;

        await SavePlayerRatings(request.MmrIdRatings, playerIds);

        await Csv2Mysql(GetFileName("Players"), nameof(context.PlayerNgRatings), importOptions.Value.ImportConnectionString);
        await Csv2Mysql(GetFileName("Replays"), nameof(context.ReplayNgRatings), importOptions.Value.ImportConnectionString);
        await Csv2Mysql(GetFileName("ReplayPlayers"), nameof(context.ReplayPlayerNgRatings), importOptions.Value.ImportConnectionString);

        // todo
        // SetPlayerPos
        // SetPlayerRatingChanges
    }

    private async Task SavePlayerRatings(Dictionary<int, Dictionary<PlayerId, CalcRating>> playerRatings,
                                         Dictionary<PlayerId, int> playerIds)
    {
        List<PlayerNgRatingCsv> ratings = new();
        int i = 0;
        foreach (var ent in playerRatings)
        {
            foreach (var entCalc in ent.Value.Values)
            {
                if (entCalc is null)
                {
                    continue;
                }

                if (!playerIds.TryGetValue(entCalc.PlayerId, out var playerId))
                {
                    continue;
                }

                i++;

                var rating = GetPlayerRatingCsvLine(entCalc, (RatingNgType)ent.Key, i, playerId);

                ratings.Add(rating);

            }
        }
        await SaveCsvFile(ratings, GetFileName("Players"), FileMode.Create);
    }

    protected virtual async Task SaveStepResult(List<ReplayNgRatingResult> replayRatings,
                                                       CalcRatingNgRequest ratingRequest)
    {
        bool append = ratingRequest.ReplayPlayerRatingAppendId > 0;

        List<ReplayNgRatingCsv> replayCsvs = new();
        List<ReplayPlayerNgRatingCsv> replayPlayerCsvs = new();
        for (int i = 0; i < replayRatings.Count; i++)
        {
            var rating = replayRatings[i];
            ratingRequest.ReplayRatingAppendId++;
            replayCsvs.Add(new()
            {
                ReplayNgRatingId = ratingRequest.ReplayRatingAppendId,
                RatingNgType = (int)rating.RatingNgType,
                LeaverType = (int)rating.LeaverType,
                Exp2Win = MathF.Round(rating.Exp2Win, 2),
                ReplayId = rating.ReplayId,
                AvgRating = Convert.ToInt32(rating.ReplayPlayerNgRatingResults.Average(a => a.Rating))
            });
            foreach (var rp in rating.ReplayPlayerNgRatingResults)
            {
                ratingRequest.ReplayPlayerRatingAppendId++;

                replayPlayerCsvs.Add(new()
                {
                    ReplayPlayerNgRatingId = ratingRequest.ReplayPlayerRatingAppendId,
                    Rating = MathF.Round(rp.Rating, 2),
                    Change = MathF.Round(rp.Change, 2),
                    Games = rp.Games,
                    Consistency = MathF.Round(rp.Consistency, 2),
                    Confidence = MathF.Round(rp.Confidence, 2),
                    ReplayPlayerId = rp.ReplayPlayerId ?? 0,
                });
            }
        }

        FileMode fileMode = append ? FileMode.Append : FileMode.Create;
        await SaveCsvFile(replayCsvs, GetFileName("Replays"), fileMode);
        await SaveCsvFile(replayPlayerCsvs, GetFileName("ReplayPlayers"), fileMode);
    }


}

public record CalcRequest
{
    public DateTime FromDate { get; set; } = new DateTime(2021, 2, 1);
    public RatingNgType RatingType { get; set; }
    public List<GameMode> GameModes { get; set; } = [GameMode.Commanders, GameMode.CommandersHeroic, GameMode.Standard];
    public bool Continue { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 100_000;
}




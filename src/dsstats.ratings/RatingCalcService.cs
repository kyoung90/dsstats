using CsvHelper;
using CsvHelper.Configuration;
using dsstats.db8;
using dsstats.shared;
using dsstats.shared.Calc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Globalization;

namespace dsstats.ratings;

public abstract class RatingCalcService(ReplayContext context, IOptions<DbImportOptions> importOptions, ILogger logger)
{
    private static readonly string mysqlDir = "/data/mysqlfiles";
    protected readonly ReplayContext context = context;
    protected readonly IOptions<DbImportOptions> importOptions = importOptions;

    protected abstract Task<List<CalcDto>> GetCalcDtosAsync(CalcRequest calcRequest);
    protected abstract Task<CalcRatingRequest> GetCalcRatingRequestAsync(DateTime fromDate);

    public async Task ProduceRatings(CalcRequest request)
    {
        Stopwatch sw = Stopwatch.StartNew();
        var calcDtos = await GetCalcDtosAsync(request);
        var ratingRequest = await GetCalcRatingRequestAsync(request.FromDate);

        List<shared.Calc.ReplayRatingDto> replayRatings = [];
        while(calcDtos.Count > 0)
        {
            for (int i = 0; i < calcDtos.Count; i++)
            {
                var rating = lib.Ratings.ProcessReplay(calcDtos[i], ratingRequest);
                if (rating is not null)
                {
                    replayRatings.Add(rating);
                }
            }
            await SaveStepResult(replayRatings, ratingRequest);
            replayRatings.Clear();

            request.Skip += request.Take;
            calcDtos = await GetCalcDtosAsync(request);
        }
        await SaveResult(ratingRequest);
        sw.Stop();
        logger.LogWarning("Raings produced in {time} min.", sw.Elapsed.ToString(@"mm\:ss"));
    }

    protected virtual async Task SaveResult(CalcRatingRequest request)
    {
        var playerIds = (await context.Players
            .Select(s => new { s.ToonId, s.RealmId, s.RegionId, s.PlayerId }).ToListAsync())
            .ToDictionary(k => new PlayerId(k.ToonId, k.RealmId, k.RegionId), v => v.PlayerId);
        var connectionString = importOptions.Value.ImportConnectionString;

        await SavePlayerRatings(request.MmrIdRatings, playerIds);

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

                var rating = GetPlayerRatingCsvLine(entCalc, ent.Key, i, playerId);

                ratings.Add(rating);

            }
        }
        await SaveCsvFile(ratings, GetFileName(RatingCalcType.Dsstats, "Replays"), FileMode.Create);
    }

    private static PlayerNgRatingCsv GetPlayerRatingCsvLine(CalcRating calcRating,
                                            int ratingType,
                                            int line,
                                            int playerId)
    {
        var main = calcRating.CmdrCounts
            .OrderByDescending(o => o.Value)
            .FirstOrDefault();
        var maincount = main.Key == Commander.None ? 0 : main.Value;

        return new()
        {
            PlayerNgRatingId = line,
            RatingNgType = ratingType,
            Rating = calcRating.Mmr,
            Games = calcRating.Games,
            Wins = calcRating.Wins,
            Mvp = calcRating.Mvps,
            MainCount = maincount,
            MainCmdr = (int)main.Key,
            Consistency = calcRating.Consistency,
            Confidence = calcRating.Confidence,
            PlayerId = playerId,
        };
    }

    protected virtual async Task SaveStepResult(List<shared.Calc.ReplayRatingDto> replayRatings,
                                                       CalcRatingRequest ratingRequest)
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
                RatingNgType = (int)rating.RatingType,
                LeaverType = (int)rating.LeaverType,
                Exp2Win = MathF.Round(rating.ExpectationToWin, 2),
                ReplayId = rating.ReplayId,
                AvgRating = Convert.ToInt32(rating.RepPlayerRatings.Average(a => a.Rating))
            });
            foreach (var rp in rating.RepPlayerRatings)
            {
                ratingRequest.ReplayPlayerRatingAppendId++;

                replayPlayerCsvs.Add(new()
                {
                    ReplayPlayerNgRatingId = ratingRequest.ReplayPlayerRatingAppendId,
                    Rating = MathF.Round(rp.Rating, 2),
                    Change = MathF.Round(rp.RatingChange, 2),
                    Games = rp.Games,
                    Consistency = MathF.Round(rp.Consistency, 2),
                    Confidence = MathF.Round(rp.Confidence, 2),
                    ReplayPlayerId = rp.ReplayPlayerId,
                });
            }
        }

        FileMode fileMode = append ? FileMode.Append : FileMode.Create;
        await SaveCsvFile(replayCsvs, GetFileName(ratingRequest.RatingCalcType, "Replays"), fileMode);
        await SaveCsvFile(replayPlayerCsvs, GetFileName(ratingRequest.RatingCalcType, "ReplayPlayers"), fileMode);
    }

    private async Task SaveCsvFile<T>(List<T> records, string fileName, FileMode fileMode)
        where T : class, ICsvRecord, new()
    {
        using var stream = File.Open(fileName, fileMode);
        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            });
        await csv.WriteRecordsAsync(records);
    }

    private static string GetFileName(RatingCalcType calcType, string job)
    {
        var path = Path.Combine(mysqlDir, $"{calcType}_{job}.csv");
        return path.Replace("\\", "/");
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

internal interface ICsvRecord { };

internal record ReplayNgRatingCsv : ICsvRecord
{
    public int ReplayNgRatingId { get; set; }
    public int RatingNgType { get; set; }
    public int LeaverType { get; set; }
    public float Exp2Win { get; set; }
    public int AvgRating { get; set; }
    public bool IsPreRating { get; set; }
    public int ReplayId { get; set; }
}

internal record ReplayPlayerNgRatingCsv : ICsvRecord
{
    public int ReplayPlayerNgRatingId { get; set; }
    public float Rating { get; set; }
    public float Change { get; set; }
    public int Games { get; set; }
    public float Consistency { get; set; }
    public float Confidence { get; set; }
    public int ReplayPlayerId { get; set; }
}

internal record PlayerNgRatingCsv : ICsvRecord
{
    public int PlayerNgRatingId { get; set; }
    public int RatingNgType { get; set; }
    public double Rating { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public double Consistency { get; set; }
    public double Confidence { get; set; }
    public int Pos { get; set; }
    public int PlayerId { get; set; }
    public int Mvp { get; set; }
    public int MainCount { get; set; }
    public int MainCmdr { get; set; }
}
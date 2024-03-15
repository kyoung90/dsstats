using dsstats.db8;
using dsstats.db8.Ratings;
using dsstats.shared;
using dsstats.shared.Calc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Diagnostics;

namespace dsstats.ratings;

public abstract partial class RatingCalcService(ReplayContext context,
                                                IServiceScopeFactory scopeFactory,
                                                IOptions<DbImportOptions> importOptions,
                                                ILogger logger)
{
    protected readonly ReplayContext context = context;
    protected readonly IOptions<DbImportOptions> importOptions = importOptions;

    protected abstract Task<List<CalcDto>> GetCalcDtosAsync(CalcRequest calcRequest);
    protected abstract Task<List<CalcDto>> GetPreRatingCalcDtosAsync(CalcRequest calcRequest);
    protected abstract Task<List<CalcDto>> GetContinueRatingCalcDtosAsync(CalcRequest calcRequest);
    protected abstract Task<CalcRatingNgRequest> GetCalcRatingRequestAsync(List<CalcDto> calcDtos);

    public async Task ProduceRatings(CalcRequest request)
    {
        Stopwatch sw = Stopwatch.StartNew();
        var calcDtos = await GetCalcDtosAsync(request);
        var ratingRequest = await GetCalcRatingRequestAsync([]);

        List<ReplayNgRatingResult> replayRatings = [];
        while (calcDtos.Count > 0)
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

            request.Skip += request.Take;
            calcDtos = await GetCalcDtosAsync(request);
        }
        await SaveResult(ratingRequest);
        await SetPlayerRatingPos();
        await SetPlayerRatingChanges();
        sw.Stop();
        logger.LogWarning("Ratings produced in {time} min.", sw.Elapsed.ToString(@"mm\:ss"));
    }

    protected virtual async Task SaveResult(CalcRatingNgRequest request)
    {
        var connectionString = importOptions.Value.ImportConnectionString;

        await SavePlayerRatings(request.MmrIdRatings);

        await Csv2Mysql(GetFileName("Players"), nameof(context.PlayerNgRatings), importOptions.Value.ImportConnectionString);
        await Csv2Mysql(GetFileName("Replays"), nameof(context.ReplayNgRatings), importOptions.Value.ImportConnectionString);
        await Csv2Mysql(GetFileName("ReplayPlayers"), nameof(context.ReplayPlayerNgRatings), importOptions.Value.ImportConnectionString);
    }

    private async Task SavePlayerRatings(Dictionary<int, Dictionary<PlayerId, CalcRating>> playerRatings)
    {
        using var scope = scopeFactory.CreateScope();
        var importService = scope.ServiceProvider.GetRequiredService<IImportService>();
        var playerIds = await importService.GetPlayerIdDictionary();

        List<PlayerNgRatingCsv> ratings = [];
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
        List<ReplayPlayerNgRatingCsv> replayPlayerCsvs = [];
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
                    RatingNgType = (int)rating.RatingNgType,
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

    private async Task SetPlayerRatingPos()
    {
        try
        {
            using var connection = new MySqlConnection(importOptions.Value.ImportConnectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "CALL SetPlayerRatingNgPos();";
            command.CommandTimeout = 120;
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("failed setting player rating pos: {error}", ex.Message);
        }
    }

    private async Task SetPlayerRatingChanges()
    {
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            DateTime currentDate = DateTime.UtcNow;
            DateTime thirtyDaysAgo = currentDate.AddDays(-30);

            var replays = await context.Replays
                .Include(i => i.ReplayPlayers)
                    .ThenInclude(i => i.Player)
                        .ThenInclude(i => i.PlayerNgRatings)
                .Include(i => i.ReplayPlayers)
                    .ThenInclude(i => i.ReplayPlayerNgRatings)
                .Include(i => i.ReplayNgRatings)
                .Where(x => x.GameTime > thirtyDaysAgo)
                .OrderBy(x => x.GameTime)
                .AsSplitQuery()
                .ToListAsync();

            Dictionary<int, Dictionary<RatingNgType, PlayerNgRatingChange>> ratingChanges = [];

            foreach (var replay in replays)
            {
                bool isWithin24Hours = (currentDate - replay.GameTime) < TimeSpan.FromHours(24);
                bool isWithin10Days = (currentDate - replay.GameTime) < TimeSpan.FromDays(10);

                foreach (var replayRating in replay.ReplayNgRatings)
                {
                    foreach (var replayPlayer in replay.ReplayPlayers)
                    {
                        var playerRating = replayPlayer.Player.PlayerNgRatings
                            .FirstOrDefault(f => f.RatingNgType == replayRating.RatingNgType);
                        var replayPlayerRating = replayPlayer.ReplayPlayerNgRatings
                            .FirstOrDefault(f => f.RatingNgType == replayRating.RatingNgType);

                        if (playerRating is null || replayPlayerRating is null)
                        {
                            continue;
                        }

                        if (!ratingChanges.TryGetValue(replayPlayer.PlayerId, out var playerRatingChanges))
                        {
                            playerRatingChanges = ratingChanges[replayPlayer.PlayerId] = [];
                        }
                        if (!playerRatingChanges.TryGetValue(replayRating.RatingNgType, out var ratingChange))
                        {
                            ratingChange = playerRatingChanges[replayRating.RatingNgType] = new()
                            {
                                PlayerNgRatingId = playerRating.PlayerNgRatingId
                            };
                        }

                        ratingChange.Change30d += replayPlayerRating.Change;
                        if (isWithin24Hours) { ratingChange.Change24h += replayPlayerRating.Change; }
                        if (isWithin10Days) { ratingChange.Change10d += replayPlayerRating.Change; }
                    }
                }
            }
            await SaveCsvFile<PlayerNgRatingChangeCsv>(ratingChanges.SelectMany(s => s.Value.Values)
                .Select((s, index) => new PlayerNgRatingChangeCsv()
                {
                    PlayerNgRatingChangeId = index + 1,
                    Change24h = MathF.Round(s.Change24h, 2),
                    Change10d = MathF.Round(s.Change10d, 2),
                    Change30d = MathF.Round(s.Change30d, 2),
                    PlayerNgRatingId = s.PlayerNgRatingId
                }).ToList(), GetFileName("Changes"), FileMode.Create);
            await Csv2Mysql(GetFileName("Changes"), nameof(context.PlayerNgRatingChanges), importOptions.Value.ImportConnectionString);
        }
        catch (Exception ex)
        {
            logger.LogError("failed setting rating changes: {error}", ex.Message);
        }
        sw.Stop();
        logger.LogWarning("player rating changes set in {time} ms", sw.ElapsedMilliseconds);
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




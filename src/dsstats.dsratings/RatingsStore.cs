using CsvHelper;
using CsvHelper.Configuration;
using dsstats.db8;
using dsstats.db8.Ratings;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Globalization;

namespace dsstats.dsratings;

public static class RatingsStore
{
    private static readonly int commandTimeout = 600;

    public static async Task StoreStepResult(List<ReplayDsRatingResult> replayRatingResults, CalcDsRatingRequest request)
    {
        List<ReplayDsRating> replayRatings = [];
        List<ReplayPlayerDsRating> replayPlayerDsRatings = [];
        bool append = request.ReplayRatingAppendId != 0;

        foreach (var replayRatingResult in replayRatingResults)
        {
            request.ReplayRatingAppendId++;
            ReplayDsRating replayRating = new()
            {
                ReplayDsRatingId = request.ReplayRatingAppendId,
                RatingType = replayRatingResult.RatingType,
                LeaverType = replayRatingResult.LeaverType,
                ExpectationToWin = MathF.Round((float)replayRatingResult.ExpectationToWin, 2),
                IsPreRating = false,
                AvgRating = replayRatingResult.PlayerRatings.Count == 0 ? 0 
                    : Convert.ToInt32(replayRatingResult.PlayerRatings.Average(a => a.Rating)),
                ReplayId = replayRatingResult.ReplayId,
            };
            replayRatings.Add(replayRating);
            foreach (var replayPlayerRatingResult in replayRatingResult.PlayerRatings)
            {
                request.ReplayPlayerRatingAppendId++;
                ReplayPlayerDsRating replayPlayerDsRating = new()
                {
                    ReplayPlayerDsRatingId = request.ReplayPlayerRatingAppendId,
                    Rating = replayPlayerRatingResult.Rating,
                    RatingChange = replayPlayerRatingResult.RatingChange,
                    Games = replayPlayerRatingResult.Games,
                    Consistency = replayPlayerRatingResult.Consistency,
                    Confidence = replayPlayerRatingResult.Confidence,
                    ReplayPlayerId = replayPlayerRatingResult.ReplayPlayerId,
                };
                replayPlayerDsRatings.Add(replayPlayerDsRating);
            }
        }
        var replayRatingsFileName = "/data/mysqlfiles/ReplayDsRatings.csv";
        var replayPlayerRatingsFileName = "/data/mysqlfiles/ReplayPlayerDsRatings.csv";
        await CreateOrAppendCsv(replayRatings.Select(s => new ReplayDsRatingCsv(s)).ToList(), replayRatingsFileName, append);
        await CreateOrAppendCsv(replayPlayerDsRatings.Select(s => new ReplayPlayerDsRatingCsv(s)).ToList(), replayPlayerRatingsFileName, append);
    }

    private static async Task CreateOrAppendCsv<T>(List<T> records, string fileName, bool append) where T : CsvType, new()
    {
        using var stream = File.Open(fileName, append ? FileMode.Append : FileMode.Create);
        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false });
        await csv.WriteRecordsAsync(records);
    }

    public static async Task StorePlayerDsRatingsCsv(CalcDsRatingRequest request, ReplayContext context, string connectionString)
    {
        var playerIds = (await context.Players
            .Select(s => new { PlayerId = new PlayerId(s.ToonId, s.RealmId, s.RegionId), Id = s.PlayerId })
            .ToListAsync())
            .ToDictionary(k => k.PlayerId, v => v.Id);

        List<PlayerDsRating> playerDsRatings = [];
        int i = 0;
        foreach (var ent in request.MmrIdRatings)
        {
            RatingType ratingType = (RatingType)ent.Key;
            foreach (var ratingEnt in request.MmrIdRatings[ent.Key])
            {
                if (!playerIds.TryGetValue(ratingEnt.Key, out var playerId)
                    || playerId == 0)
                {
                    continue;
                }
                var playerRating = ratingEnt.Value;

                (Commander mainCmdr, double mainPercentage) = GetMainInfo(playerRating.CmdrCounts);
                i++;
                PlayerDsRating rating = new()
                {
                    PlayerDsRatingId = i,
                    PlayerId = playerId,
                    RatingType = ratingType,
                    Games = playerRating.Games,
                    Wins = playerRating.Wins,
                    Mvps = playerRating.Mvps,
                    Mmr = playerRating.Mmr,
                    Consistency = playerRating.Consistency,
                    Confidence = playerRating.Confidence,
                    PeakRating = playerRating.PeakRating,
                    RecentRatingGain = playerRating.RecentRatingGain.Count == 0 ? 0
                        : playerRating.RecentRatingGain.Average(),
                    MainCmdr = mainCmdr,
                    MainPercentage = mainPercentage,
                    WinStreak = playerRating.WinStreak,
                    LoseStreak = playerRating.LoseStreak,
                    CurrentStreak = playerRating.CurrentStreak,
                    Duration = playerRating.Duration,
                    LatestReplay = playerRating.LatestReplay,
                };
                playerDsRatings.Add(rating);
            }
        }

        var tableName = "PlayerDsRatings";
        var fileName = "/data/mysqlfiles/PlayerDsRatings.csv";
        await CreateOrAppendCsv(playerDsRatings.Select(s => new PlayerDsRatingCsv(s)).ToList(), fileName, false);

        var replayRatingsFileName = "/data/mysqlfiles/ReplayDsRatings.csv";
        var replayRatingsTableName = nameof(ReplayContext.ReplayDsRatings);
        var replayPlayerRatingsFileName = "/data/mysqlfiles/ReplayPlayerDsRatings.csv";
        var replayPlayerRatingsTableName = nameof(ReplayContext.ReplayPlayerDsRatings);

        await Csv2Mysql(fileName, tableName, connectionString);
        await Csv2Mysql(replayRatingsFileName, replayRatingsTableName, connectionString);
        await Csv2Mysql(replayPlayerRatingsFileName, replayPlayerRatingsTableName, connectionString);
    }

    public static async Task StorePlayerDsRatings(CalcDsRatingRequest request, ReplayContext context)
    {
        var playerIds = (await context.Players
            .Select(s => new { PlayerId = new PlayerId(s.ToonId, s.RealmId, s.RegionId), Id = s.PlayerId })
            .ToListAsync())
            .ToDictionary(k => k.PlayerId, v => v.Id);

        var ratingIds = (await context.PlayerDsRatings.Select(s => new { s.PlayerId, s.RatingType, s.PlayerDsRatingId })
            .ToListAsync()).ToDictionary(k => new RatingKey(k.PlayerId, k.RatingType), v => v.PlayerDsRatingId);

        int i = 0;
        foreach (var ent in request.MmrIdRatings)
        {
            RatingType ratingType = (RatingType)ent.Key;
            foreach (var ratingEnt in request.MmrIdRatings[ent.Key])
            {
                if (!playerIds.TryGetValue(ratingEnt.Key, out var playerId)
                    || playerId == 0)
                {
                    continue;
                }
                var playerRating = ratingEnt.Value;

                (Commander mainCmdr, double mainPercentage) = GetMainInfo(playerRating.CmdrCounts);

                if (!ratingIds.TryGetValue(new RatingKey(playerId, ratingType), out var playerRatingId))
                {
                    playerRatingId = 0;
                }

                PlayerDsRating rating = new()
                {
                    PlayerDsRatingId = playerRatingId,
                    PlayerId = playerId,
                    RatingType = ratingType,
                    Games = playerRating.Games,
                    Wins = playerRating.Wins,
                    Mvps = playerRating.Mvps,
                    Mmr = playerRating.Mmr,
                    Consistency = playerRating.Consistency,
                    Confidence = playerRating.Confidence,
                    PeakRating = playerRating.PeakRating,
                    RecentRatingGain = playerRating.RecentRatingGain.Count == 0 ? 0
                        : Math.Round(playerRating.RecentRatingGain.Average(), 2),
                    MainCmdr = mainCmdr,
                    MainPercentage = mainPercentage,
                    WinStreak = playerRating.WinStreak,
                    LoseStreak = playerRating.LoseStreak,
                    CurrentStreak = playerRating.CurrentStreak,
                    Duration = playerRating.Duration,
                    LatestReplay = playerRating.LatestReplay,
                };
                context.Update(rating);
                i++;
                if (i % 1000 == 0)
                {
                    await context.SaveChangesAsync();
                }
            }
        }
        await context.SaveChangesAsync();
    }

    private static (Commander mainCmdr, double mainPercentage) GetMainInfo(Dictionary<Commander, int> cmdrCounts)
    {
        if (cmdrCounts.Count == 0)
        {
            return (Commander.None, 0);
        }

        double sum = cmdrCounts.Values.Sum();
        var max = cmdrCounts.OrderByDescending(o => o.Value).First();
        return (max.Key, Math.Round(max.Value * 100.0 / sum, 2));
    }

    private static async Task Csv2Mysql(string fileName,
                        string tableName,
                        string connectionString)
    {
        if (!File.Exists(fileName))
        {
            return;
        }

        var tempTable = tableName + "_temp";
        var oldTable = tempTable + "_old";

        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandTimeout = commandTimeout;

            command.CommandText = @$"
DROP TABLE IF EXISTS {tempTable};
DROP TABLE IF EXISTS {oldTable};
CREATE TABLE {tempTable} LIKE {tableName};
SET SQL_LOG_BIN=0;
SET FOREIGN_KEY_CHECKS = 0;
ALTER TABLE {tempTable} DISABLE KEYS;
LOAD DATA INFILE '{fileName}' INTO TABLE {tempTable}
COLUMNS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '""' ESCAPED BY '""' LINES TERMINATED BY '\r\n';
ALTER TABLE {tempTable} ENABLE KEYS;

RENAME TABLE {tableName} TO {oldTable}, {tempTable} TO {tableName};
DROP TABLE {oldTable};
SET FOREIGN_KEY_CHECKS = 1;
SET SQL_LOG_BIN=1;";

            await command.ExecuteNonQueryAsync();

            File.Delete(fileName);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}

internal interface CsvType { }

internal record PlayerDsRatingCsv : CsvType
{
    public PlayerDsRatingCsv() { }
    public PlayerDsRatingCsv(PlayerDsRating rating)
    {
        PlayerDsRatingId = rating.PlayerDsRatingId;
        RatingType = (int)rating.RatingType;
        Games = rating.Games;
        Wins = rating.Wins;
        Mvps = rating.Mvps;
        Mmr = rating.Mmr;
        Consistency = rating.Consistency;
        Confidence = rating.Confidence;
        PeakRating = rating.PeakRating;
        RecentRatingGain = rating.RecentRatingGain;
        MainCmdr = (int)rating.MainCmdr;
        MainPercentage = rating.MainPercentage;
        WinStreak = rating.WinStreak;
        LoseStreak = rating.LoseStreak;
        CurrentStreak = rating.CurrentStreak;
        Duration = rating.Duration;
        LatestReplay = rating.LatestReplay.ToString("yyyy-MM-dd HH:mm:ss");
        PlayerId = rating.PlayerId;
    }
    public int PlayerDsRatingId { get; init; }
    public int RatingType { get; init; }
    public int Games { get; init; }
    public int Wins { get; init; }
    public int Mvps { get; init; }
    public double Mmr { get; init; }
    public double Consistency { get; init; }
    public double Confidence { get; init; }
    public double PeakRating { get; init; }
    public double RecentRatingGain { get; init; }
    public int MainCmdr { get; init; }
    public double MainPercentage { get; init; }
    public int WinStreak { get; init; }
    public int LoseStreak { get; init; }
    public int CurrentStreak { get; init; }
    public int Duration { get; init; }
    public string LatestReplay { get; init; } = string.Empty;
    public int PlayerId { get; init; }
}

public record ReplayPlayerDsRatingCsv : CsvType
{
    public ReplayPlayerDsRatingCsv() { }
    public ReplayPlayerDsRatingCsv(ReplayPlayerDsRating rating)
    {
        ReplayPlayerDsRatingId = rating.ReplayPlayerDsRatingId;
        Rating = rating.Rating;
        RatingChange = rating.RatingChange;
        Games = rating.Games;
        CmdrGames = rating.CmdrGames;
        Consistency = rating.Consistency;
        Confidence = rating.Confidence;
        ReplayPlayerId = rating.ReplayPlayerId;
    }
    public int ReplayPlayerDsRatingId { get; set; }
    public float Rating { get; set; }
    public float RatingChange { get; set; }
    public int Games { get; set; }
    public int CmdrGames { get; set; }
    public float Consistency { get; set; }
    public float Confidence { get; set; }
    public int ReplayPlayerId { get; set; }
}

public record ReplayDsRatingCsv : CsvType
{
    public ReplayDsRatingCsv() { }
    public ReplayDsRatingCsv(ReplayDsRating rating)
    {
        ReplayDsRatingId = rating.ReplayDsRatingId;
        RatingType = (int)rating.RatingType;
        LeaverType = (int)rating.LeaverType;
        ExpectationToWin = rating.ExpectationToWin;
        IsPreRating = rating.IsPreRating ? 1 : 0;
        AvgRating = rating.AvgRating;
        ReplayId = rating.ReplayId;
    }
    public int ReplayDsRatingId { get; set; }
    public int RatingType { get; set; }
    public int LeaverType { get; set; }
    public float ExpectationToWin { get; set; }
    public int IsPreRating { get; set; }
    public int AvgRating { get; set; }
    public int ReplayId { get; set; }
}
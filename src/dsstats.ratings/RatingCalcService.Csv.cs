using dsstats.shared.Calc;
using dsstats.shared;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

namespace dsstats.ratings;

public abstract partial class RatingCalcService
{
    private static readonly string mysqlDir = "/data/mysqlfiles";

    private static PlayerNgRatingCsv GetPlayerRatingCsvLine(CalcRating calcRating,
                                        RatingNgType ratingType,
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
            RatingNgType = (int)ratingType,
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

    private string GetFileName(string job)
    {
        string name = this.GetType().ToString().Replace("RatingCalcService", "");
        var path = Path.Combine(mysqlDir, $"{name}_{job}.csv");
        return path.Replace("\\", "/");
    }
}

internal interface ICsvRecord { };

internal record ReplayNgRatingCsv : ICsvRecord
{
    public int ReplayNgRatingId { get; set; }
    public int RatingNgType { get; set; }
    public int LeaverType { get; set; }
    public float Exp2Win { get; set; }
    public int AvgRating { get; set; }
    public int IsPreRating { get; set; }
    public int ReplayId { get; set; }
}

internal record ReplayPlayerNgRatingCsv : ICsvRecord
{
    public int ReplayPlayerNgRatingId { get; set; }
    public int RatingNgType { get; set; }
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

internal record PlayerNgRatingChangeCsv : ICsvRecord
{
    public int PlayerNgRatingChangeId { get; set; }
    public float Change24h { get; set; }
    public float Change10d { get; set; }
    public float Change30d { get; set; }
    public int PlayerNgRatingId { get; set; }
}
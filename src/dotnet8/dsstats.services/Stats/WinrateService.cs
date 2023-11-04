﻿
using Microsoft.Extensions.Caching.Memory;
using dsstats.shared;
using dsstats.shared.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace dsstats.services;

public partial class WinrateService : IWinrateService
{
    private readonly string connectionString;
    private readonly bool isSqlite;
    private readonly IMemoryCache memoryCache;
    private readonly ILogger<WinrateService> logger;

    public WinrateService(IOptions<DbImportOptions> dbOptions, IMemoryCache memoryCache, ILogger<WinrateService> logger)
    {
        this.connectionString = dbOptions.Value.ImportConnectionString;
        this.isSqlite = dbOptions.Value.IsSqlite;
        this.memoryCache = memoryCache;
        this.logger = logger;
    }

    public async Task<WinrateResponse> GetWinrate(StatsRequest request, CancellationToken token)
    {
        logger.LogInformation("getting winrate from Server - {date}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        var memKey = request.GenMemKey();

        if (!memoryCache.TryGetValue(memKey, out WinrateResponse? response)
            || response is null)
        {
            try
            {
                response = request.ComboRating ?
                    await ProduceComboWinrate(request, token) 
                    : await ProduceWinrate(request, token);
                if (response is not null)
                {
                    memoryCache.Set(memKey, response, TimeSpan.FromHours(3));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.LogError("failed producing winrate: {error}", ex.Message);
            }
        }
        return response ?? new();
    }

    public async Task<WinrateResponse> GetWinrate(WinrateRequest request, CancellationToken token)
    {
        logger.LogInformation("getting winrate from Server - {date}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        var memKey = request.GenMemKey();

        if (!memoryCache.TryGetValue(memKey, out WinrateResponse? response)
            || response is null)
        {
            try
            {
                response = request.ComboRating ?
                    await ProduceComboWinrate(request, token)
                    : await ProduceWinrate(request, token);
                if (response is not null)
                {
                    memoryCache.Set(memKey, response, TimeSpan.FromHours(3));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.LogError("failed producing winrate: {error}", ex.Message);
            }
        }
        return response ?? new();
    }

    private async Task<WinrateResponse?> ProduceWinrate(StatsRequest request, CancellationToken token)
    {
        var data = isSqlite ? await GetDataFromSqlite(request, token) 
            : await GetDataFromRaw(request, token);

        if (data is null)
        {
            return null;
        }

        if (request.RatingType == RatingType.Std || request.RatingType == RatingType.StdTE)
        {
            data = data.Where(x => x.Commander != Commander.None && (int)x.Commander <= 3).ToList();
        }
        else
        {
            data = data.Where(x => (int)x.Commander > 3).ToList();
        }

        return new()
        {
            Interest = request.Interest,
            WinrateEnts = data,
        };
    }

    private async Task<List<WinrateEnt>?> GetDataFromRaw(StatsRequest request, CancellationToken token)
    {
        if (request.Filter.Exp2Win is not null && (request.Filter.Exp2Win.FromExp2Win > 0 || request.Filter.Exp2Win.ToExp2Win > 0))
        {
            return await GetDataFromVeryRawWithExp2Win(request);
        }

        (var fromDate, var toDate) = Data.TimeperiodSelected(request.TimePeriod);

        var sql = request.Interest == Commander.None ?
$@"
SELECT 
    rp.Race as commander,
	count(*) as count,
    round(avg(rpr.Rating), 2) as avgrating,
    round(avg(rpr.RatingChange), 2) as avggain,
    sum(CASE WHEN rp.PlayerResult = 1 THEN 1 ELSE 0 END) as wins
FROM Replays as r
INNER JOIN ReplayRatings as rr on rr.ReplayId = r.ReplayId
INNER JOIN ReplayPlayers AS rp on rp.ReplayId = r.ReplayId
INNER JOIN RepPlayerRatings AS rpr on rpr.ReplayPlayerId = rp.ReplayPlayerId
WHERE rr.RatingType = {(int)request.RatingType}
    AND r.GameTime > '{fromDate.ToString("yyyy-MM-dd")}'
    {(toDate < DateTime.Today.AddDays(-2) ? $"AND r.GameTime < '{toDate.ToString("yyyy-MM-dd")}'" : "")}
    AND rp.Duration > 300
    {(request.Filter.Rating?.FromRating > Data.MinBuildRating ? $"AND rpr.Rating >= {request.Filter.Rating.FromRating}" : "")}
    {(request.Filter.Rating?.ToRating != 0 && request.Filter.Rating?.ToRating < Data.MaxBuildRating ? $"AND rpr.Rating <= {request.Filter.Rating.ToRating}" : "")}
GROUP BY rp.Race;
"
            :
$@"
SELECT 
    rp.OppRace as commander,
	count(*) as count,
    round(avg(rpr.Rating), 2) as avgrating,
    round(avg(rpr.RatingChange), 2) as avggain,
    sum(CASE WHEN rp.PlayerResult = 1 THEN 1 ELSE 0 END) as wins
FROM Replays as r
INNER JOIN ReplayRatings as rr on rr.ReplayId = r.ReplayId
INNER JOIN ReplayPlayers AS rp on rp.ReplayId = r.ReplayId
INNER JOIN RepPlayerRatings AS rpr on rpr.ReplayPlayerId = rp.ReplayPlayerId
WHERE rr.RatingType = {(int)request.RatingType}
    AND r.GameTime > '{fromDate:yyyy-MM-dd}'
    {(toDate < DateTime.Today.AddDays(-2) ? $"AND r.GameTime < '{toDate:yyyy-MM-dd}'" : "")}
    AND rp.Duration > 300
    AND rp.Race = {(int)request.Interest}
    {(request.Filter.Rating?.FromRating > Data.MinBuildRating ? $"AND rpr.Rating >= {request.Filter.Rating.FromRating}" : "")}
    {(request.Filter.Rating?.ToRating != 0 && request.Filter.Rating?.ToRating < Data.MaxBuildRating ? $"AND rpr.Rating <= {request.Filter.Rating.ToRating}" : "")}
GROUP BY rp.OppRace;
";

        try
        {
            var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(token);

            var command = new MySqlCommand(sql, connection);

            var reader = await command.ExecuteReaderAsync(token);

            List<WinrateEnt> ents = new();
            while (await reader.ReadAsync(token))
            {
                ents.Add(new()
                {
                    Commander = (Commander)reader.GetInt32(0),
                    Count = reader.GetInt32(1),
                    AvgRating = reader.GetDouble(2),
                    AvgGain = reader.GetDouble(3),
                    Wins = reader.GetInt32(4)
                });
            }
            return ents;
        }
        catch (Exception ex)
        {
            logger.LogError("failed getting winrate from db: {error}", ex.Message);
        }
        return null;
    }

    private async Task<List<WinrateEnt>?> GetDataFromVeryRawWithExp2Win(StatsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Filter.Exp2Win);
        try
        {
            (var fromDate, var toDate) = Data.TimeperiodSelected(request.TimePeriod);

            var fromRating = request.Filter.Rating?.FromRating <= Data.MinBuildRating ? 0 : request.Filter.Rating?.FromRating ?? 0;
            var toRating = request.Filter.Rating?.ToRating >= Data.MaxBuildRating ? 10000 : request.Filter.Rating?.ToRating ?? 10000;
            var tempTableName = Guid.NewGuid().ToString();


            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var createSql = $@"
            CREATE TEMPORARY TABLE `{tempTableName}` AS
            SELECT rp.ReplayId, MIN(rpr.Rating) AS minRating, MAX(rpr.Rating) AS maxRating
            FROM ReplayPlayers AS rp
            INNER JOIN RepPlayerRatings AS rpr ON rp.ReplayPlayerId = rpr.ReplayPlayerId
            INNER JOIN Replays AS r ON r.ReplayId = rp.ReplayId
            WHERE r.GameTime > @FromDate
            {(toDate < DateTime.Today.AddDays(-2) ? "AND r.GameTime < @ToDate" : "")}
            GROUP BY rp.ReplayId;
        ";

            using var createCommand = new MySqlCommand(createSql, connection);

            createCommand.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            if (toDate < DateTime.Today.AddDays(-2))
            {
                createCommand.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            }

            await createCommand.ExecuteNonQueryAsync();

            logger.LogInformation("Creating temp table: {sql}", createSql);

            var sql = request.Interest == Commander.None ?
                $@"
                SELECT
                  rp.Race AS commander,
                  COUNT(*) AS count,
                  ROUND(AVG(rpr.Rating), 2) AS avgrating,
                  ROUND(AVG(rpr.RatingChange), 2) AS avggain,
                  SUM(CASE WHEN rp.PlayerResult = 1 THEN 1 ELSE 0 END) AS wins
                FROM Replays AS r
                INNER JOIN ReplayRatings AS rr ON rr.ReplayId = r.ReplayId
                INNER JOIN ReplayPlayers AS rp ON rp.ReplayId = r.ReplayId
                INNER JOIN RepPlayerRatings AS rpr ON rpr.ReplayPlayerId = rp.ReplayPlayerId
                INNER JOIN `{tempTableName}` AS tpr ON r.ReplayId = tpr.ReplayId
                WHERE rr.RatingType = @RatingType
                  AND rp.Duration > 300
                  AND rr.ExpectationToWin >= @ExpectationMin AND rr.ExpectationToWin <= @ExpectationMax
                  AND tpr.minRating >= @FromRating AND tpr.maxRating <= @ToRating
                GROUP BY rp.Race;
            "
                :
                $@"
                SELECT
                  rp.OppRace AS commander,
                  COUNT(*) AS count,
                  ROUND(AVG(rpr.Rating), 2) AS avgrating,
                  ROUND(AVG(rpr.RatingChange), 2) AS avggain,
                  SUM(CASE WHEN rp.PlayerResult = 1 THEN 1 ELSE 0 END) AS wins
                FROM Replays AS r
                INNER JOIN ReplayRatings AS rr ON rr.ReplayId = r.ReplayId
                INNER JOIN ReplayPlayers AS rp ON rp.ReplayId = r.ReplayId
                INNER JOIN RepPlayerRatings AS rpr ON rpr.ReplayPlayerId = rp.ReplayPlayerId
                INNER JOIN `{tempTableName}` AS tpr ON r.ReplayId = tpr.ReplayId
                WHERE rr.RatingType = @RatingType
                  AND rp.Duration > 300
                  AND rr.ExpectationToWin >= @ExpectationMin AND rr.ExpectationToWin <= @ExpectationMax
                  AND tpr.minRating >= @FromRating AND tpr.maxRating <= @ToRating
                GROUP BY rp.OppRace;
            ";

            var fromExp2Win = request.Filter.Exp2Win.FromExp2Win / 100.0;
            var toExp2Win = request.Filter.Exp2Win.ToExp2Win / 100.0;

            logger.LogInformation("fromExp2Win: {from}({f}), toExp2Win: {to}({t})", fromExp2Win, request.Filter.Exp2Win.FromExp2Win, toExp2Win, request.Filter.Exp2Win.ToExp2Win);
            logger.LogInformation("fromRating: {from}, toRating: {to}", fromRating, toRating);

            using var mainQueryCommand = new MySqlCommand(sql, connection);
            mainQueryCommand.Parameters.AddWithValue("@RatingType", (int)request.RatingType);
            mainQueryCommand.Parameters.AddWithValue("@ExpectationMin", fromExp2Win);
            mainQueryCommand.Parameters.AddWithValue("@ExpectationMax", toExp2Win);
            mainQueryCommand.Parameters.AddWithValue("@FromRating", fromRating);
            mainQueryCommand.Parameters.AddWithValue("@ToRating", toRating);

            List<WinrateEnt> results = new();

            using (var reader = await mainQueryCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int cmdr = reader.GetInt32(0);
                    int count = reader.GetInt32(1);
                    double avgRating = reader.GetDouble(2);
                    double avgGain = reader.GetDouble(3);
                    int wins = reader.GetInt32(4);
                    results.Add(new()
                    {
                        Commander = (Commander)cmdr,
                        Count = count,
                        AvgRating = avgRating,
                        AvgGain = avgGain,
                        Wins = wins
                    });
                }
            }

            LogCommand(mainQueryCommand);

            using var cleanupCommand = new MySqlCommand($"DROP TEMPORARY TABLE `{tempTableName}`;", connection);
            await cleanupCommand.ExecuteNonQueryAsync();


            logger.LogInformation("drop table: {sql}", $"DROP TEMPORARY TABLE `{tempTableName}`;");

            return results;
        }
        catch (Exception ex)
        {
            logger.LogError("failed getting exp2win winrate: {error}", ex.Message);
        }
        return null;
    }

    private void LogCommand(MySqlCommand cmd)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            string cmdText = cmd.CommandText;

            foreach (MySqlParameter p in cmd.Parameters)
            {
                cmdText = cmdText.Replace(p.ParameterName, p.Value?.ToString());
            }

            logger.LogInformation(cmdText);
        }
    }
}


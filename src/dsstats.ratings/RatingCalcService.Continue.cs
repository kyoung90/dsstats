using dsstats.shared.Calc;
using dsstats.shared;
using dsstats.db8.Ratings;
using Microsoft.EntityFrameworkCore;
using dsstats.db8;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Microsoft.Extensions.DependencyInjection;

namespace dsstats.ratings;

public partial class RatingCalcService
{
    public async Task ProducePreRatings(CalcRequest request)
    {
        var calcDtos = await GetPreRatingCalcDtosAsync(request);

        if (calcDtos.Count == 0)
        {
            return;
        }

        var ratingRequest = await GetCalcRatingRequestAsync(calcDtos);
        List<ReplayNgRatingResult> replayRatings = [];

        for (int i = 0; i < calcDtos.Count; i++)
        {
            var calcDto = calcDtos[i];
            var ratings = lib.RatingsNg.ProcessReplayNg(calcDto, ratingRequest);

            replayRatings.AddRange(ratings);
        }

        await SaveRatings(replayRatings, preRating: true);
    }

    private async Task SaveRatings(List<ReplayNgRatingResult> ratings, bool preRating)
    {
        if (ratings.Count == 0)
        {
            return;
        }

        List<ReplayNgRating> dbRatings = [];
        List<ReplayPlayerNgRating> playerRatings = [];

        foreach (var rating in ratings)
        {
            dbRatings.Add(new()
            {
                ReplayId = rating.ReplayId,
                RatingNgType = rating.RatingNgType,
                LeaverType = rating.LeaverType,
                Exp2Win = rating.Exp2Win,
                IsPreRating = preRating,
            });
            foreach (var player in rating.ReplayPlayerNgRatingResults)
            {
                if (player.ReplayPlayerId is null)
                {
                    continue;
                }

                playerRatings.Add(new()
                {
                    RatingNgType = rating.RatingNgType,
                    Rating = player.Rating,
                    Change = player.Change,
                    Games = player.Games,
                    Consistency = player.Consistency,
                    Confidence = player.Confidence,
                    ReplayPlayerId = player.ReplayPlayerId.Value
                });
            }
        }
        context.ReplayNgRatings.AddRange(dbRatings);
        context.ReplayPlayerNgRatings.AddRange(playerRatings);
        await context.SaveChangesAsync();
    }

    public async Task PrdoduceContinueRatings(CalcRequest request)
    {
        var calcDtos = await GetContinueRatingCalcDtosAsync(request);

        if (calcDtos.Count == 0)
        {
            return;
        }

        await ClearPreRatings(calcDtos);

        var ratingRequest = await GetCalcRatingRequestAsync(calcDtos);
        List<ReplayNgRatingResult> replayRatings = [];

        for (int i = 0; i < calcDtos.Count; i++)
        {
            var calcDto = calcDtos[i];
            var ratings = lib.RatingsNg.ProcessReplayNg(calcDto, ratingRequest);

            replayRatings.AddRange(ratings);
        }

        await SaveRatings(replayRatings, preRating: false);
        await ContinuePlayerRatings(ratingRequest.MmrIdRatings);

        await SetPlayerRatingPos();
        await SetPlayerRatingChanges();
    }

    private async Task ClearPreRatings(List<CalcDto> calcDtos)
    {
        var replayIds = calcDtos.Select(s => s.ReplayId).ToList();

        await context.ReplayNgRatings
            .Where(x => replayIds.Contains(x.ReplayId))
            .ExecuteDeleteAsync();

        await context.ReplayPlayerNgRatings
            .Where(x => replayIds.Contains(x.ReplayPlayer!.ReplayId))
            .ExecuteDeleteAsync();
    }

    private async Task ContinuePlayerRatings(Dictionary<int, Dictionary<PlayerId, CalcRating>> mmrIdRatings)
    {
        var connectionString = importOptions.Value.ImportConnectionString;
        using var scope = scopeFactory.CreateScope();
        var importService = scope.ServiceProvider.GetRequiredService<IImportService>();

        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            var command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
$@"INSERT INTO {nameof(ReplayContext.PlayerNgRatings)}
    ({nameof(PlayerNgRating.PlayerNgRatingId)},
    {nameof(PlayerNgRating.RatingNgType)},
    {nameof(PlayerNgRating.Rating)},
    {nameof(PlayerNgRating.Pos)},
    {nameof(PlayerNgRating.Games)},
    {nameof(PlayerNgRating.Wins)},
    {nameof(PlayerNgRating.Consistency)},
    {nameof(PlayerNgRating.Confidence)},
    {nameof(PlayerNgRating.PlayerId)})
VALUES ((SELECT t.{nameof(PlayerNgRating.PlayerNgRatingId)} 
    FROM (SELECT * from {nameof(ReplayContext.PlayerNgRatings)} WHERE {nameof(PlayerNgRating.RatingNgType)} = @value1 
        AND {nameof(PlayerNgRating.PlayerId)} = @value8) as t),
    @value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8)
ON DUPLICATE KEY UPDATE {nameof(PlayerNgRating.Rating)}=@value2,
                        {nameof(PlayerNgRating.Games)}=@value4,
                        {nameof(PlayerNgRating.Wins)}=@value5,
                        {nameof(PlayerNgRating.Consistency)}=@value6,
                        {nameof(PlayerNgRating.Confidence)}=@value7
            ";
            command.Transaction = transaction;

            List<MySqlParameter> parameters = [];
            for (int i = 1; i <= 8; i++)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@value{i}";
                command.Parameters.Add(parameter);
                parameters.Add(parameter);
            }

            foreach (var ent in mmrIdRatings)
            {
                foreach (var calcEnt in ent.Value.Values)
                {
                    int playerId = await importService.GetPlayerIdAsync(calcEnt.PlayerId, "Anonymous");

                    parameters[0].Value = ent.Key;
                    parameters[1].Value = calcEnt.Mmr;
                    parameters[2].Value = 0;
                    parameters[3].Value = calcEnt.Games;
                    parameters[4].Value = calcEnt.Wins;
                    parameters[5].Value = calcEnt.Consistency;
                    parameters[6].Value = calcEnt.Confidence;
                    parameters[7].Value = playerId;
                    command.CommandTimeout = 240;
                    await command.ExecuteNonQueryAsync();
                }
            }
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("failed continue player ratings: {error}", ex.Message);
        }
    }
}

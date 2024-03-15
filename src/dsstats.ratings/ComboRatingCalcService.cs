using dsstats.db8;
using dsstats.ratings.lib;
using dsstats.shared;
using dsstats.shared.Calc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Diagnostics;

namespace dsstats.ratings;

public class ComboRatingCalcService(ReplayContext context,
                                     IServiceScopeFactory scopeFactory,
                                     IOptions<DbImportOptions> importOptions,
                                     ILogger<ComboRatingCalcService> logger)
    : RatingCalcService(context, scopeFactory, importOptions, logger)
{
    private Dictionary<int, bool> processedDsstatsReplayIds = [];

    protected override async Task<List<CalcDto>> GetCalcDtosAsync(CalcRequest calcRequest)
    {
        var query = from r in context.MaterializedArcadeReplays
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
            .Where(x => x.Duration >= 300
             && x.WinnerTeam > 0
             && x.GameTime >= fromDate
             && x.GameTime <= toDate)
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

    protected override async Task<List<CalcDto>> GetPreRatingCalcDtosAsync(CalcRequest calcRequest)
    {
        var query = context.Replays
            .Where(x => x.Duration >= 300
             && x.WinnerTeam > 0
             && x.GameTime >= calcRequest.FromDate
             && x.ReplayNgRatings.Count == 0)
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

        return rawDtos.Select(s => s.GetCalcDto()).ToList();
    }

    protected override async Task<List<CalcDto>> GetContinueRatingCalcDtosAsync(CalcRequest calcRequest)
    {
        var query = context.Replays
            .Where(x => x.Duration >= 300
             && x.WinnerTeam > 0
             && x.GameTime >= calcRequest.FromDate
             && x.ReplayNgRatings.All(a => a.IsPreRating))
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

        return rawDtos.Select(s => s.GetCalcDto()).ToList();
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

    protected override async Task<CalcRatingNgRequest> GetCalcRatingRequestAsync(List<CalcDto> calcDtos)
    {
        if (calcDtos.Count == 0)
        {
            return new CalcRatingNgRequest()
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
                        { (int)RatingNgType.CmdrWithTE, new() },
                        { (int)RatingNgType.StdWithTE, new() }
                    },
            };
        }
        else
        {
            return await GetCalcRatingRequestFromCalcDtos(calcDtos);
        }
    }

    private async Task<CalcRatingNgRequest> GetCalcRatingRequestFromCalcDtos(List<CalcDto> calcDtos)
    {
        var request = new CalcRatingNgRequest()
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
                        { (int)RatingNgType.CmdrWithTE, new() },
                        { (int)RatingNgType.StdWithTE, new() }
                    },
        };

        var ratingTypes = calcDtos.SelectMany(s => s.GetRatingNgType().GetUniqueFlags())
            .ToHashSet();

        var intRatingTypes = ratingTypes.Select(s => (int)s).ToList();

        var playerIds = calcDtos.SelectMany(s => s.Players).Select(s => s.PlayerId)
            .Distinct()
            .ToList();

        var toonIds = playerIds.Select(s => s.ToonId)
            .Distinct()
            .ToList();

        var query = from pr in context.PlayerNgRatings
                    join p in context.Players on pr.PlayerId equals p.PlayerId
                    where intRatingTypes.Contains((int)pr.RatingNgType)
                        && toonIds.Contains(p.ToonId)
                    select new
                    {
                        pr.RatingNgType,
                        PlayerId = new PlayerId(p.ToonId, p.RealmId, p.RegionId),
                        pr.Games,
                        pr.Wins,
                        pr.Mvp,
                        Mmr = pr.Rating,
                        pr.Consistency,
                        pr.Confidence,
                        pr.MainCmdr,
                        pr.MainCount
                    };

        var ratings = await query.ToListAsync();

        foreach (var playerId in playerIds)
        {
            var plRatings = ratings.Where(s => s.PlayerId == playerId).ToList();

            foreach (var plRating in plRatings)
            {
                request.MmrIdRatings[(int)plRating.RatingNgType][playerId] = new()
                {
                    PlayerId = playerId,
                    Games = plRating.Games,
                    Wins = plRating.Wins,
                    Mvps = plRating.Mvp,
                    Mmr = plRating.Mmr,
                    Consistency = plRating.Consistency,
                    Confidence = plRating.Confidence,
                    CmdrCounts = RatingService.GetFakeCmdrDic(plRating.MainCmdr, plRating.MainCount, plRating.Games)
                };
            }
        }

        return request;
    }
}

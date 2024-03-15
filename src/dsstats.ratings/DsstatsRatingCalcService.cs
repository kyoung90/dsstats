
using dsstats.db8;
using dsstats.shared;
using dsstats.shared.Calc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace dsstats.ratings;

public class DsstatsRatingCalcService(ReplayContext context,
                                      IServiceScopeFactory scopeFactory,
                                      IOptions<DbImportOptions> importOptions,
                                      ILogger<DsstatsRatingCalcService> logger) 
    : RatingCalcService(context, scopeFactory, importOptions, logger)
{
    protected override async Task<List<CalcDto>> GetCalcDtosAsync(CalcRequest calcRequest)
    {
        var query = context.Replays
            .Where(x => x.Duration >= 300
             && x.WinnerTeam > 0
             && x.GameTime >= calcRequest.FromDate
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
            .Skip(calcRequest.Skip)
            .Take(calcRequest.Take)
            .ToListAsync();

        return rawDtos.Select(s => s.GetCalcDto()).ToList();
    }

    protected override async Task<CalcRatingNgRequest> GetCalcRatingRequestAsync(List<CalcDto> calcDtos)
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
                        { (int)RatingNgType.CmdrWithTE, new() },
                        { (int)RatingNgType.StdWithTE, new() }
                    },
        });
    }

    protected override Task<List<CalcDto>> GetContinueRatingCalcDtosAsync(CalcRequest calcRequest)
    {
        throw new NotImplementedException();
    }

    protected override Task<List<CalcDto>> GetPreRatingCalcDtosAsync(CalcRequest calcRequest)
    {
        throw new NotImplementedException();
    }
}

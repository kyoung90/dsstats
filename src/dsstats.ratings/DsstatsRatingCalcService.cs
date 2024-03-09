
using dsstats.db8;
using dsstats.shared;
using dsstats.shared.Calc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace dsstats.ratings;

public class DsstatsRatingCalcService(ReplayContext context, IOptions<DbImportOptions> importOptions) 
    : RatingCalcService(context, importOptions)
{
    protected override async Task<List<CalcDto>> GetCalcDtosAsync(CalcRequest calcRequest)
    {
        var query = context.Replays
            .Where(x => x.Playercount == 6
             && x.Duration >= 300
             && x.WinnerTeam > 0
             && x.GameTime >= calcRequest.FromDate
             && calcRequest.GameModes.Contains(x.GameMode)
             && (!calcRequest.Continue || x.ReplayRatingInfo == null))
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

    protected override async Task<CalcRatingRequest> GetCalcRatingRequestAsync(DateTime fromDate)
    {
        return await Task.FromResult(new CalcRatingRequest()
        {
            RatingCalcType = RatingCalcType.Dsstats,
            StarTime = fromDate,
            MmrIdRatings = new()
                    {
                        { 1, new() },
                        { 2, new() },
                        { 3, new() },
                        { 4, new() }
                    },
        });
    }
}


using dsstats.db8;
using dsstats.shared;
using dsstats.shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace dsstats.db8services;



public partial class WinrateNgService(IServiceScopeFactory scopeFactory,
                                      IMemoryCache memoryCache,
                                      ILogger<WinrateNgService> logger) : IWinrateNgService
{
    public async Task<WinrateResponse> GetWinrate(WinrateNgRequest request, CancellationToken token)
    {
        var memKey = request.GenMemKey("WinrateNg" + request.RatingNgType);

        if (!memoryCache.TryGetValue(memKey, out WinrateResponse? response)
            || response is null)
        {
            try
            {
                response = await ProduceWinrate(request, token);
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

    private async Task<WinrateResponse?> ProduceWinrate(WinrateNgRequest request, CancellationToken token)
    {
        var data = await GetData(request, token);

        if (data is null)
        {
            return null;
        }

        return new()
        {
            Interest = request.Interest,
            WinrateEnts = data,
        };
    }

    private async Task<List<WinrateEnt>?> GetData(WinrateNgRequest request, CancellationToken token)
    {
        (var fromDate, var toDate) = request.GetTimeLimits();
        var tillDate = toDate.AddDays(-2);

        var limits = request.GetFilterLimits();

        using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();

        var group = request.Interest == Commander.None ?
                    from r in context.Replays
                    from rp in r.ReplayPlayers
                    join rr in context.ReplayNgRatings on r.ReplayId equals rr.ReplayId
                    join rpr in context.ReplayPlayerNgRatings on rp.ReplayPlayerId equals rpr.ReplayPlayerId
                    where r.GameTime > fromDate
                     && (toDate > tillDate || r.GameTime <= toDate)
                     && rr.RatingNgType == request.RatingNgType
                     && (limits.FromRating <= 0 || rpr.Rating >= limits.FromRating)
                     && (limits.ToRating <= 0 || rpr.Rating <= limits.ToRating)
                     && (limits.FromExp2Win <= 0 || rr.Exp2Win >= limits.FromExp2Win)
                     && (limits.ToExp2Win <= 0 || rr.Exp2Win <= limits.ToExp2Win)
                     && (!request.WithoutLeavers || rr.LeaverType == LeaverType.None)
                    group new { rp, rr, rpr, r } by rp.Race into g
                    select new WinrateEnt()
                    {
                        Commander = g.Key,
                        Count = g.Count(),
                        AvgRating = Math.Round(g.Average(a => a.rpr.Rating), 2),
                        AvgGain = Math.Round(g.Average(a => a.rpr.Change), 2),
                        Wins = g.Sum(s => s.rp.PlayerResult == PlayerResult.Win ? 1 : 0),
                        Replays = g.Select(s => s.r.ReplayId).Distinct().Count()
                    }
                    :
                    from r in context.Replays
                    from rp in r.ReplayPlayers
                    join rr in context.ReplayNgRatings on r.ReplayId equals rr.ReplayId
                    join rpr in context.ReplayPlayerNgRatings on rp.ReplayPlayerId equals rpr.ReplayPlayerId
                    where r.GameTime > fromDate
                     && (toDate > tillDate || r.GameTime <= toDate)
                     && rr.RatingNgType == request.RatingNgType
                     && (limits.FromRating <= 0 || rpr.Rating >= limits.FromRating)
                     && (limits.ToRating <= 0 || rpr.Rating <= limits.ToRating)
                     && (limits.FromExp2Win <= 0 || rr.Exp2Win >= limits.FromExp2Win)
                     && (limits.ToExp2Win <= 0 || rr.Exp2Win <= limits.ToExp2Win)
                     && rp.Race == request.Interest
                     && (!request.WithoutLeavers || rr.LeaverType == LeaverType.None)
                    group new { rp, rr, rpr, r } by rp.OppRace into g
                    select new WinrateEnt()
                    {
                        Commander = g.Key,
                        Count = g.Count(),
                        AvgRating = Math.Round(g.Average(a => a.rpr.Rating), 2),
                        AvgGain = Math.Round(g.Average(a => a.rpr.Change), 2),
                        Wins = g.Sum(s => s.rp.PlayerResult == PlayerResult.Win ? 1 : 0),
                        Replays = g.Select(s => s.r.ReplayId).Distinct().Count()
                    };

        return await group.ToListAsync();
    }
}


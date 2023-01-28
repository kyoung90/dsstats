﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using pax.dsstats.dbng.Extensions;
using pax.dsstats.shared;

namespace pax.dsstats.dbng.Services;

public partial class StatsService
{
    public async Task<CmdrStrengthResult> GetCmdrStrengthResults(CmdrStrengthRequest request, CancellationToken token)
    {
        var memKey = request.GenMemKey();
        if (!memoryCache.TryGetValue(memKey, out CmdrStrengthResult result))
        {
            result = await ProduceCmdrStrengthResult(request, token);
            memoryCache.Set(memKey, result, TimeSpan.FromHours(24));
        }
        return result;
    }

    private async Task<CmdrStrengthResult> ProduceCmdrStrengthResult(CmdrStrengthRequest request, CancellationToken token)
    {
        (var startDate, var endDate) = Data.TimeperiodSelected(request.TimePeriod);

        var replays = context.Replays
            .Where(x => x.GameTime > startDate
                && x.ReplayRatingInfo != null
                && x.ReplayRatingInfo.LeaverType == LeaverType.None
                && x.ReplayRatingInfo.RatingType == request.RatingType);

        if (endDate != DateTime.MinValue && (DateTime.Today - endDate).TotalDays > 2)
        {
            replays = replays.Where(x => x.GameTime < endDate);
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var group = from r in replays
                    from rp in r.ReplayPlayers
                    group rp by rp.Race into g
                    select new CmdrStrengthItem()
                    {
                        Commander = g.Key,
                        Matchups = g.Count(),
                        AvgRating = Math.Round(g.Sum(s => s.ReplayPlayerRatingInfo.Rating) / g.Count(), 2),
                        Wins = g.Count(c => c.PlayerResult == PlayerResult.Win)
                    };
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        return new()
        {
            Items = await group.ToListAsync(token)
        };
    }
}

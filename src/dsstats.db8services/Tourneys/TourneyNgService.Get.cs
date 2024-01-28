using dsstats.shared;
using dsstats.shared.Extensions;
using dsstats.shared.Tourneys;
using Microsoft.EntityFrameworkCore;

namespace dsstats.db8services.Tourneys;

public partial class TourneyNgService
{
    public async Task<List<TourneyDto>> GetTournaments()
    {
        return await context.Tourneys
            .Select(s => new TourneyDto()
            {
                Name = s.Name,
                TourneyGuid = s.TourneyGuid,
                StartDate = s.StartDate,
                GameMode = s.GameMode,
                WinnerTeam = s.WinnerTeam == null ? null : context.TourneyTeams.First(x => x.TeamGuid == s.WinnerTeam).Name
            }).ToListAsync();
    }

    public async Task<int> GetTourneyReplaysCount(TourneysReplaysRequest request, CancellationToken token)
    {
        var replays = GetReplayQueriable(request);
        return await replays.CountAsync(token);
    }

    public async Task<List<TourneyReplayListDto>> GetTourneyReplays(TourneysReplaysRequest request, CancellationToken token)
    {
        var replays = GetReplayQueriable(request);
        replays = SortReplays(request, replays);

        return await replays
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(token);
    }

    private IQueryable<TourneyReplayListDto> SortReplays(TourneysReplaysRequest request, IQueryable<TourneyReplayListDto> replays)
    {
        if (request.Orders.Count == 0)
        {
            return replays.OrderByDescending(o => o.GameTime);
        }

        foreach (var order in request.Orders)
        {
            if (order.Ascending)
            {
                replays = replays.AppendOrderBy(order.Property);
            }
            else
            {
                replays = replays.AppendOrderByDescending(order.Property);
            }
        }
        return replays;
    }

    private IQueryable<TourneyReplayListDto> GetReplayQueriable(TourneysReplaysRequest request)
    {
        var replays = context.Replays.AsQueryable();

        if (request.EventGuid == Guid.Empty)
        {
            replays = replays.Where(x => x.TourneyMatchId != null);
        }
        else
        {
            replays = replays.Where(x => x.TourneyMatch!.Tourney!.TourneyGuid == request.EventGuid);
        }

        return replays.Select(s => new TourneyReplayListDto()
        {
            GameTime = s.GameTime,
            Duration = s.Duration,
            WinnerTeam = s.WinnerTeam,
            GameMode = s.GameMode,
            TournamentEdition = s.TournamentEdition,
            ReplayHash = s.ReplayHash,
            CommandersTeam1 = s.CommandersTeam1,
            CommandersTeam2 = s.CommandersTeam2,
            TournamentName = s.TourneyMatch!.Tourney!.Name
        });
    }
}


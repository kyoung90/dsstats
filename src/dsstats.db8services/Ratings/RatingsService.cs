using AutoMapper;
using AutoMapper.QueryableExtensions;
using dsstats.db8;
using dsstats.db8.Ratings;
using dsstats.shared;
using dsstats.shared.Interfaces;
using dsstats.shared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace dsstats.db8services.Ratings;

public class RatingsService(ReplayContext context, IMapper mapper) : IRatingsService
{
    public async Task<int> GetRatingsCount(RatingsNgRequest request, CancellationToken token = default)
    {
        var ratings = GetRatingsQueriable(request);
        return await ratings.CountAsync(token);
    }

    public async Task<RatingsNgResult> GetRatings(RatingsNgRequest request, CancellationToken token)
    {
        var ratings = GetRatingsQueriable(request);
        var list = OrderRatings(ratings, request);

        var listDtos = await list
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(token);

        return new()
        {
            Ratings = listDtos
        };
    }

    private IQueryable<PlayerRatingNgListDto> OrderRatings(IQueryable<PlayerNgRating> ratings, RatingsNgRequest request)
    {
        bool hasOrders = false;

        foreach (var order in request.Orders)
        {
            var prop = typeof(PlayerRatingNgListDto).GetProperty(order.Property);

            if (prop is null)
            {
                continue;
            }

            hasOrders = true;

            if (order.Ascending)
            {
                ratings = ratings.AppendOrderBy(order.Property);
            }
            else
            {
                ratings = ratings.AppendOrderByDescending(order.Property);
            }
        }

        if (!hasOrders)
        {
            return ratings
                .OrderByDescending(o => o.Rating)
                .ProjectTo<PlayerRatingNgListDto>(mapper.ConfigurationProvider);
        }
        else
        {
            return ratings.ProjectTo<PlayerRatingNgListDto>(mapper.ConfigurationProvider);
        }
    }


    private IQueryable<PlayerNgRating> GetRatingsQueriable(RatingsNgRequest request)
    {
        return context.PlayerNgRatings
            .Where(x => x.RatingNgType == request.RatingNgType);
    }
}




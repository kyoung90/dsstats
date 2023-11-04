
using dsstats.shared;

namespace dsstats.services;

public partial class PlayerService
{
    public async Task<int> GetRatingsCount(RatingsRequest request, CancellationToken token)
    {
        if (request.ComboRating)
        {
            return await GetComboRatingsListCount(request, token);
        }

        if (request.Arcade)
        {
            return await GetArcadeRatingsListCount(request, token);
        }

        return await GetDsstatsRatingsListCount(request, token);
    }

    public async Task<List<ComboPlayerRatingDto>> GetRatings(RatingsRequest request, CancellationToken token)
    {
        if (request.ComboRating)
        {
            return await GetComboRatingsList(request, token);
        }

        if (request.Arcade)
        {
            return await GetArcadeRatingsList(request, token);
        }

        return await GetDsstatsRatingsList(request, token);
    }
}
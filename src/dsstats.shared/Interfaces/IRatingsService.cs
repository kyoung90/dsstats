namespace dsstats.shared.Interfaces;

public interface IRatingsService
{
    Task<RatingsNgResult> GetRatings(RatingsNgRequest request, CancellationToken token);
    Task<int> GetRatingsCount(RatingsNgRequest request, CancellationToken token = default);
}
using dsstats.shared;
using dsstats.shared.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace dsstats.apiServices;

public class RatingsService(HttpClient httpClient, ILogger<RatingsService> logger) : IRatingsService
{
    private readonly string ratingsController = "api8/v1/ratings";

    public async Task<RatingsNgResult> GetRatings(RatingsNgRequest request, CancellationToken token)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{ratingsController}/ratings", request, token);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<RatingsNgResult>() ?? new();
        }
        catch (Exception ex)
        {
            logger.LogError("failed getting ratings: {error}", ex.Message);
        }
        return new();
    }

    public async Task<int> GetRatingsCount(RatingsNgRequest request, CancellationToken token = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{ratingsController}/ratingscount", request, token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>(token);
        }
        catch (Exception ex)
        {
            logger.LogError("failed getting ratings count: {error}", ex.Message);
        }
        return 0;
    }
}

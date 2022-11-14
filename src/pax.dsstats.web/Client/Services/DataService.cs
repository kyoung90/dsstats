﻿using pax.dsstats.shared;
using System.Net.Http.Json;

namespace pax.dsstats.web.Client.Services;

public class DataService : IDataService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<DataService> logger;
    private readonly string statsController = "api/Stats/";
    private readonly string buildsController = "api/Builds/";

    public DataService(HttpClient httpClient, ILogger<DataService> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    public async Task<ReplayDto?> GetReplay(string replayHash, CancellationToken token = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"{statsController}GetReplay/{replayHash}", token);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ReplayDto>();
            }
            else
            {
                logger.LogError($"failed getting replay: {response.StatusCode}");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            logger.LogError($"failed getting replay: {e.Message}");
        }
        return null;
    }

    public async Task<int> GetReplaysCount(ReplaysRequest request, CancellationToken token = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{statsController}GetReplaysCount", request, token);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<int>();
            }
            else
            {
                logger.LogError($"failed getting replay count: {response.StatusCode}");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            logger.LogError($"failed getting replay count: {e.Message}");
        }
        return 0;
    }

    public async Task<ICollection<ReplayListDto>> GetReplays(ReplaysRequest request, CancellationToken token = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{statsController}GetReplays", request, token);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ICollection<ReplayListDto>>() ?? new List<ReplayListDto>();
            }
            else
            {
                logger.LogError($"failed getting replays: {response.StatusCode}");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            logger.LogError($"failed getting replays: {e.Message}");
        }
        return new List<ReplayListDto>();
    }


    public async Task<StatsResponse> GetStats(StatsRequest request, CancellationToken token = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{statsController}GetStats", request, token);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<StatsResponse>() ?? new();
            }
            else
            {
                logger.LogError($"failed getting stats: {response.StatusCode}");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            logger.LogError($"failed getting stats: {e.Message}");
        }
        return new();
    }

    public async Task<BuildResponse> GetBuild(BuildRequest request, CancellationToken token = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{statsController}GetBuild", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BuildResponse>() ?? new();
            }
            else
            {
                logger.LogError($"failed getting build: {response.StatusCode}");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            logger.LogError($"failed getting build: {e.Message}");
        }
        return new();
    }

    public async Task<int> GetRatingsCount(RatingsRequest request, CancellationToken token = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{statsController}GetRatingsCount", request, token);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<int>(cancellationToken: token);
            }
            else
            {
                logger.LogError($"failed getting ratings count: {response.StatusCode}");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            logger.LogError($"failed getting ratings count: {e.Message}");
        }
        return 0;

    }

    public async Task<List<PlayerRatingDto>> GetRatings(RatingsRequest request, CancellationToken token = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{statsController}GetRatings", request, token);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<PlayerRatingDto>>(cancellationToken: token) ?? new();
            }
            else
            {
                logger.LogError($"failed getting ratings: {response.StatusCode}");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            logger.LogError($"failed getting ratings: {e.Message}");
        }
        return new();
    }

    public async Task<string?> GetPlayerRatings(int toonId)
    {
        try
        {
            var response = await httpClient.GetAsync($"{statsController}GetPlayerRatings/{toonId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception e)
        {
            logger.LogError($"failed getting player ratings: {e.Message}");
        }
        return null;
    }

    public async Task<List<MmrDevDto>> GetRatingsDeviation()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<MmrDevDto>>($"{statsController}GetRatingsDeviation") ?? new();
        }
        catch (Exception e)
        {
            logger.LogError($"failed getting rating deviation: {e.Message}");
        }
        return new();
    }

    public async Task<List<MmrDevDto>> GetRatingsDeviationStd()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<MmrDevDto>>($"{statsController}GetRatingsDeviationStd") ?? new();
        }
        catch (Exception e)
        {
            logger.LogError($"failed getting rating deviationstd: {e.Message}");
        }
        return new();
    }

    public async Task<ICollection<PlayerMatchupInfo>> GetPlayerDetailInfo(int toonId)
    {
        try
        {
            var response = await httpClient.GetAsync($"{statsController}GetPlayerDetails/{toonId}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ICollection<PlayerMatchupInfo>>();
                return result ?? new List<PlayerMatchupInfo>();
            }
            else
            {
                logger.LogError($"failed getting playerDetails: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"failed getting playerDetails: {ex.Message}");
        }
        return new List<PlayerMatchupInfo>();
    }

    public async Task<PlayerRatingDto?> GetPlayerRating(int toonId)
    {
        try
        {
            var response = await httpClient.GetAsync($"{statsController}PlayerRating/{toonId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PlayerRatingDto?>();
            }
            else
            {
                logger.LogError($"failed getting playerRating: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"failed getting playerRating: {ex.Message}");
        }
        return null;
    }

    public async Task<List<RequestNames>> GetTopPlayers()
    {
        try
        {
            var topPlayers = await httpClient.GetFromJsonAsync<List<RequestNames>>($"{buildsController}topplayers");
            if (topPlayers != null)
            {
                return topPlayers;
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"failed getting topPlayers: {ex.Message}");
        }
        return Data.GetDefaultRequestNames();
    }

    public async Task<ICollection<string>> GetReplayPaths()
    {
        return await Task.FromResult(new List<string>());
    }

    public async Task<List<string>> GetTournaments()
    {
        return await Task.FromResult(new List<string>());
    }
}

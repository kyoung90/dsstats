using dsstats.shared;
using dsstats.shared.Interfaces;
using dsstats.shared.Tourneys;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace dsstats.apiServices;

public class TourneyNgService(HttpClient httpClient, ILogger<TourneyNgService> logger) : ITourneyNgService
{
    private readonly string tourneyController = "api8/v1/Tourney";

    public Task<bool> AddTournamentPlayers(TourneyPlayersDto playersDto)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> AddTourneyMatch(TourneyMatchCreateDto createDto)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> AddTourneyTeam(TourneyTeamCreateDto createDto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateNewSwissRound(Guid tourneyGuid)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateRandomTeams(Guid tourneyGuid, RatingType ratingType)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateRoundRobinBracket(Guid tourneyGuid)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> CreateTournament(TourneyCreateDto createDto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ReportMatchResult(TourneyMatchResult result)
    {
        throw new NotImplementedException();
    }

    public async Task<List<TourneyDto>> GetTournaments()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<TourneyDto>>($"{tourneyController}") ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError("failed getting tournaments: {error}", ex.Message);
        }
        return [];
    }

    public async Task<List<TourneyReplayListDto>> GetTourneyReplays(TourneysReplaysRequest request, CancellationToken token)
    {
        try
        {
            var result = await httpClient.PostAsJsonAsync($"{tourneyController}/replays", request, token);
            result.EnsureSuccessStatusCode();

            var replays = await result.Content.ReadFromJsonAsync<List<TourneyReplayListDto>>();
            return replays ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError("failed getting tourney replays: {error}", ex.Message);
        }
        return [];
    }

    public async Task<int> GetTourneyReplaysCount(TourneysReplaysRequest request, CancellationToken token)
    {
        try
        {
            var result = await httpClient.PostAsJsonAsync($"{tourneyController}/replayscount", request, token);
            result.EnsureSuccessStatusCode();

            return await result.Content.ReadFromJsonAsync<int>();
        }
        catch (Exception ex)
        {
            logger.LogError("failed getting tourney replays count: {error}", ex.Message);
        }
        return 0;
    }

    public async Task<TourneyStatsResponse> GetStats(TourneyStatsRequest request, CancellationToken token = default)
    {
        try
        {
            var result = await httpClient.PostAsJsonAsync($"{tourneyController}/stats", request, token);
            result.EnsureSuccessStatusCode();

            return await result.Content.ReadFromJsonAsync<TourneyStatsResponse>() ?? new();
        }
        catch (Exception ex)
        {
            logger.LogError("failed getting tourney stats: {error}", ex.Message);
        }
        return new();
    }
}

using MathNet.Numerics.Financial;
using Microsoft.AspNetCore.Mvc;
using pax.dsstats.dbng;
using pax.dsstats.dbng.Services;
using pax.dsstats.shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace pax.dsstats.web.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingsController
{
    private readonly IRatingRepository ratingRepository;
    private readonly PlayerService playerService;

    public RatingsController(IRatingRepository ratingRepository, PlayerService playerService)
    {
        this.ratingRepository = ratingRepository;
        this.playerService = playerService;
    }

    [HttpPost]
    [Route("GetRatingsCount")]
    public async Task<ActionResult<int>> GetRatingsCount(RatingsRequest request, CancellationToken token = default)
    {
        try
        {
            return await ratingRepository.GetRatingsCount(request, token);
        }
        catch (OperationCanceledException) { }
        return new NoContentResult();
    }

    [HttpPost]
    [Route("GetRatings")]
    public async Task<ActionResult<RatingsResult>> GetRatings(RatingsRequest request, CancellationToken token = default)
    {
        try
        {
            return await ratingRepository.GetRatings(request, token);
        }
        catch (OperationCanceledException) { }
        return new NoContentResult();
    }

    [HttpGet]
    [Route("GetRatingsDeviation")]
    public async Task<List<MmrDevDto>> GetRatingsDeviation()
    {
        return await ratingRepository.GetRatingsDeviation();
    }

    [HttpGet]
    [Route("GetRatingsDeviationStd")]
    public async Task<List<MmrDevDto>> GetRatingsDeviationStd()
    {
        return await ratingRepository.GetRatingsDeviationStd();
    }

    [HttpGet]
    [Route("PlayerRating/{toonId}")]
    public async Task<RavenPlayerDetailsDto> GetPlayerRating(int toonId)
    {
        return await ratingRepository.GetPlayerDetails(toonId);
    }

    [HttpPost]
    [Route("GetToonIdRatings")]
    public async Task<ToonIdRatingResponse> GetToonIdRatings(ToonIdRatingRequest request, CancellationToken token)
    {
        return await ratingRepository.GetToonIdRatings(request, token);
    }

    [HttpPost]
    [Route("GetToonIdCalcRatings")]
    public async Task<List<PlayerRatingReplayCalcDto>> GetToonIdCalcRatings(ToonIdRatingRequest request, CancellationToken token)
    {
        return await ratingRepository.GetToonIdCalcRatings(request, token);
    }

    //[HttpPost]
    //[Route("GetRatingChangesCount")]
    //public async Task<int> GetRatingChangesCount(RatingChangesRequest request, CancellationToken token)
    //{
    //    return await ratingRepository.GetRatingChangesCount(request, token);
    //}

    //[HttpPost]
    //[Route("GetRatingChanges")]
    //public async Task<RatingChangesResult> GetRatingChanges(RatingChangesRequest request, CancellationToken token)
    //{
    //    return await ratingRepository.GetRatingChanges(request, token);
    //}

    [HttpPost]
    [Route("GetDistribution")]
    public async Task<DistributionResponse> GetDistribution(DistributionRequest request, CancellationToken token)
    {
        return await ratingRepository.GetDistribution(request, token);
    }

    [HttpPost]
    [Route("GetPlayerDetails")]
    public async Task<PlayerDetailResponse> GetPlayerDetails(PlayerDetailRequest request, CancellationToken token)
    {
        return await playerService.GetPlayerDetails(request, token);
    }

    [HttpGet]
    [Route("GetPlayerDatailSummary/{toonId}")]
    public async Task<PlayerDetailSummaryV5> GetPlayerSummary(int toonId, CancellationToken token = default)
    {
        var summary = await playerService.GetPlayerSummary(toonId, token);
        return new(summary);
    }

    [HttpGet]
    [Route("GetPlayerRatingDetails/{toonId}/{ratingType}")]
    public async Task<PlayerRatingDetails> GetPlayerRatingDetails(int toonId, int ratingType, CancellationToken token = default)
    {
        return await playerService.GetPlayerRatingDetails(toonId, (RatingType)ratingType, token);
    }

    [HttpGet]
    [Route("GetPlayerCmdrAvgGain/{toonId}/{ratingType}/{timePeriod}")]
    public async Task<List<PlayerCmdrAvgGain>> GetPlayerCmdrAvgGain(int toonId, int ratingType, int timePeriod, CancellationToken token)
    {
        return await playerService.GetPlayerCmdrAvgGain(toonId, (RatingType)ratingType, (TimePeriod)timePeriod, token);
    }
}


public record PlayerDetailSummaryV5
{
    public PlayerDetailSummaryV5(PlayerDetailSummary detailSummary)
    {
        GameModesPlayed = detailSummary.GameModesPlayed;
        Ratings = detailSummary.Ratings.Select(s => new PlayerRatingDetailDtoV5(s)).ToList();
        Commanders = detailSummary.Commanders;
    }

    public List<PlayerGameModeResult> GameModesPlayed { get; set; } = new();
    public List<PlayerRatingDetailDtoV5> Ratings { get; set; } = new();
    public List<CommanderInfo> Commanders { get; set; } = new();
}

public record PlayerRatingDetailDtoV5
{
    public PlayerRatingDetailDtoV5(PlayerRatingDetailDto playerRatingDto)
    {
        RatingType = playerRatingDto.RatingType;
        Rating = playerRatingDto.Rating;
        Pos = playerRatingDto.Pos;
        Games = playerRatingDto.Games;
        Wins = playerRatingDto.Wins;
        Mvp = playerRatingDto.Mvp;
        TeamGames = playerRatingDto.TeamGames;
        MainCount = playerRatingDto.MainCount;
        Main = playerRatingDto.Main;
        Consistency = playerRatingDto.Consistency;
        Confidence = playerRatingDto.Confidence;
        IsUploader = playerRatingDto.IsUploader;
        MmrOverTime = "";
        Player = playerRatingDto.Player;
        PlayerRatingChange = playerRatingDto.PlayerRatingChange;
    }

    public RatingType RatingType { get; init; }
    public double Rating { get; init; }
    public int Pos { get; init; }
    public int Games { get; init; }
    public int Wins { get; init; }
    public int Mvp { get; init; }
    public int TeamGames { get; init; }
    public int MainCount { get; init; }
    public Commander Main { get; init; }
    public double Consistency { get; set; }
    public double Confidence { get; set; }
    public bool IsUploader { get; set; }
    public string MmrOverTime { get; set; } = "";
    public PlayerRatingPlayerDto Player { get; init; } = null!;
    public PlayerRatingChangeDto? PlayerRatingChange { get; init; }
    [NotMapped]
    public double MmrChange { get; set; }
    [NotMapped]
    public double FakeDiff { get; set; }
}
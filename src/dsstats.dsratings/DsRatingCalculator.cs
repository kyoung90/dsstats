using dsstats.shared;
using dsstats.shared.Calc;
using System.Collections.Frozen;

namespace dsstats.dsratings;

public abstract class DsRatingCalculator
{
    public async Task CalculateRatings()
    {
        var replaysRequest = new CalcReplaysRequest() { Skip = 0, Take = 5000 };
        var ratingRequest = new CalcRatingRequest()
        {
            RatingCalcType = RatingCalcType.Dsstats,
            MmrIdRatings = new()
                    {
                        { 1, new() },
                        { 2, new() },
                        { 3, new() },
                    { 4, new() }
                    },
            BannedPlayers = new Dictionary<PlayerId, bool>().ToFrozenDictionary()
        };

        var replays = await GetReplays(replaysRequest);

        while (replays.Count > 0)
        {
            List<ReplayDsRatingResult> replayRatings = [];
            foreach (var replay in replays)
            {
                var replayRatingDto = ProcessReplay(replay, ratingRequest);
                if (replayRatingDto is not null && !replay.IsArcade)
                {
                    replayRatings.Add(replayRatingDto);
                }
            }

            await SaveStepResult(replayRatings, ratingRequest);

            replaysRequest.Skip += replaysRequest.Take;
            replays = await GetReplays(replaysRequest);
        }

        await SavePlayerRatings(ratingRequest);
    }


    public abstract Task<List<CalcDto>> GetReplays(CalcReplaysRequest request);
    public abstract ReplayDsRatingResult? ProcessReplay(CalcDto replay, CalcRatingRequest request);
    public abstract Task SavePlayerRatings(CalcRatingRequest request);
    public abstract Task SaveStepResult(List<ReplayDsRatingResult> replayRatings, CalcRatingRequest request);

}

public record CalcReplaysRequest
{
    public DateTime Start { get; set; } = new DateTime(2021, 2, 1);
    public List<GameMode> GameModes { get; set; } = [GameMode.Standard];
    public int Skip { get; set; }
    public int Take { get; set; }
}

public record ReplayDsRatingResult
{
    public RatingType RatingType { get; set; }
    public LeaverType LeaverType { get; set; }
    public double ExpectationToWin { get; set; } // WinnerTeam
    public int ReplayId { get; set; }
    public bool IsPreRating { get; set; }
    public List<ReplayPlayerDsRatingResult> PlayerRatings { get; init; } = [];
}

public class ReplayPlayerDsRatingResult
{
    public int Games { get; set; }
    public int Wins { get; set; }
    public int Duration { get; set; }
    public double Rating { get; set; }
    public double Consistency { get; set; }
    public double Confidence { get; set; }
    public double RecentRatingGain { get; set; }
    public double PeakRating { get; set; }
    public int WinStreak { get; set; }
    public int LoseStreak { get; set; }
    public int CurrentStreak { get; set; }
}
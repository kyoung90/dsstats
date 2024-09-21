using dsstats.shared;
using dsstats.shared.Calc;

namespace dsstats.dsratings;

public static class DsstatsReplayProcessor
{
    public static ReplayDsRatingResult? ProcessReplay(CalcDto replay, CalcDsRatingRequest request)
    {
        ReplayDsRatingResult result = new ReplayDsRatingResult()
        {
            RatingType = (RatingType)replay.GetRatingType(),
            LeaverType = (LeaverType)replay.GetLeaverTyp(),
            ReplayId = replay.ReplayId
        };

        if (result.RatingType == RatingType.None || replay.Duration < 300 || replay.WinnerTeam == 0)
        {
            return null;
        }

        var teamInfos = GetTeamInfos(replay, request, result);

        result.ExpectationToWin = EloExpectationToWin(
            teamInfos.WinnerTeam.Rating / teamInfos.WinnerTeam.Count,
            teamInfos.LoserTeam.Rating / teamInfos.LoserTeam.Count,
            request.MmrOptions.Clip);

        var leaverImpact = GetLeaverImpact(result.LeaverType);

        var winnerResults = ProcessTeamPlayers(teamInfos.WinnerTeam, result.ExpectationToWin, 1, request, leaverImpact);
        var loserResults = ProcessTeamPlayers(teamInfos.WinnerTeam, 1 - result.ExpectationToWin, 0, request, leaverImpact);

        result.PlayerRatings.AddRange(winnerResults);
        result.PlayerRatings.AddRange(loserResults);

        return result;
    }

    private static List<ReplayPlayerDsRatingResult> ProcessTeamPlayers(TeamInfo teamInfo,
                                           double expToWin,
                                           int playerResult,
                                           CalcDsRatingRequest request,
                                           double leaverImpact)
    {
        List<ReplayPlayerDsRatingResult> resutls = [];
        var confidencePerTeam = teamInfo.Confidence / teamInfo.Count;

        foreach (var teamPlayer in teamInfo.Players)
        {
            var confidence = confidencePerTeam;
            var playerImpact = GetPlayerImpact(teamPlayer.Rating, confidence, request.MmrOptions);
            var mmrDelta = 0.0;
            var consistencyDelta = 0.0;
            var confidenceDelta = 0.0;

            if (teamPlayer.Player.IsLeaver)
            {
                mmrDelta = -1 * CalculateMmrDelta(expToWin, playerImpact, request.MmrOptions.EloK);
            }
            else
            {
                playerImpact *= leaverImpact;
                mmrDelta = CalculateMmrDelta(expToWin, playerImpact, request.MmrOptions.EloK);
                consistencyDelta = Math.Abs(expToWin - playerResult) < 0.50 ? 1.0 : 0.0;
                confidenceDelta = 1 - Math.Abs(expToWin - playerResult);
            }

            var result = UpdatePlayerRating(teamPlayer, mmrDelta, consistencyDelta, confidenceDelta, request);
            resutls.Add(result);
        }
        return resutls;
    }

    private static ReplayPlayerDsRatingResult UpdatePlayerRating(TeamPlayer teamPlayer,
                                           double mmrDelta,
                                           double consistencyDelta,
                                           double confidenceDelta,
                                           CalcDsRatingRequest request)
    {
        var rating = teamPlayer.Rating;
        double mmrAfter = rating.Mmr + mmrDelta;
        double consistencyAfter = ((rating.Consistency * request.MmrOptions.consistencyBeforePercentage)
            + (consistencyDelta * (1 - request.MmrOptions.consistencyBeforePercentage)));
        double confidenceAfter = ((rating.Confidence * request.MmrOptions.confidenceBeforePercentage)
            + (confidenceDelta * (1 - request.MmrOptions.confidenceBeforePercentage)));

        rating.Consistency = Math.Clamp(consistencyAfter, 0, 1);
        rating.Confidence = Math.Clamp(confidenceAfter, 0, 1);
        rating.Mmr = mmrAfter;
        if (rating.Mmr > rating.PeakRating)
        {
            rating.PeakRating = rating.Mmr;
        }
        rating.Games++;

        SetCmdr(rating, teamPlayer.Player.Race);

        if (teamPlayer.Player.IsLeaver)
        {
            teamPlayer.Player.PlayerResult = (int)PlayerResult.Los;
        }

        UpdateStreakAndWins(rating, teamPlayer.Player.PlayerResult);
        if (teamPlayer.Player.IsMvp)
        {
            rating.Mvps++;
        }

        return new()
        {
            GamePos = teamPlayer.Player.GamePos,
            Rating = (float)rating.Mmr,
            RatingChange = (float)mmrDelta,
            Games = rating.Games,
            Consistency = (float)rating.Consistency,
            Confidence = (float)rating.Confidence,
            ReplayPlayerId = teamPlayer.Player.ReplayPlayerId,
        };
    }

    private static void UpdateRecentRatingGain(CalcDsRating rating, double mmrDelta)
    {
        if (rating.RecentRatingGain == null)
        {
            rating.RecentRatingGain = new List<double>();
        }

        // Ensure the RecentRatingGain stores at most 10 entries
        if (rating.RecentRatingGain.Count >= 10)
        {
            rating.RecentRatingGain.RemoveAt(0); // Remove the oldest entry
        }

        // Add the latest rating change
        rating.RecentRatingGain.Add(mmrDelta);
    }

    private static void UpdateStreakAndWins(CalcDsRating rating, int playerResult)
    {
        // Win scenario
        if (playerResult == (int)PlayerResult.Win)
        {
            rating.Wins++;
            if (rating.CurrentStreak > 0)
            {
                // Continue win streak
                rating.CurrentStreak++;
            }
            else
            {
                // Reset to start a new win streak
                rating.CurrentStreak = 1;
            }

            // Update maximum win streak
            if (rating.CurrentStreak > rating.WinStreak)
            {
                rating.WinStreak = rating.CurrentStreak;
            }
        }
        // Lose scenario
        else
        {
            if (rating.CurrentStreak < 0)
            {
                // Continue losing streak
                rating.CurrentStreak--;
            }
            else
            {
                // Reset to start a new losing streak
                rating.CurrentStreak = -1;
            }

            // Update maximum losing streak
            if (Math.Abs(rating.CurrentStreak) > rating.LoseStreak)
            {
                rating.LoseStreak = Math.Abs(rating.CurrentStreak);
            }
        }
    }

    private static void SetCmdr(CalcDsRating calcRating, Commander cmdr)
    {
        if (calcRating.CmdrCounts.TryGetValue(cmdr, out int count))
        {
            calcRating.CmdrCounts[cmdr] = count + 1;
        }
        else
        {
            calcRating.CmdrCounts[cmdr] = 1;
        }
    }

    public static TeamInfos GetTeamInfos(CalcDto replay, CalcDsRatingRequest request, ReplayDsRatingResult result)
    {
        var winnerTeamInfo = new TeamInfo();
        var loserTeamInfo = new TeamInfo();

        foreach (var player in replay.Players)
        {
            var calcRating = GetOrCreatePlayerRating(player, request, result);
            calcRating.LatestReplay = replay.GameTime;
            calcRating.Duration += Convert.ToInt32(TimeSpan.FromSeconds(replay.Duration).TotalMinutes);

            if (player.PlayerResult == (int)PlayerResult.Win)
            {
                winnerTeamInfo.Update(player, calcRating);
            }
            else
            {
                loserTeamInfo.Update(player, calcRating);
            }
        }

        return new TeamInfos(winnerTeamInfo, loserTeamInfo);
    }

    private static CalcDsRating GetOrCreatePlayerRating(PlayerCalcDto player, CalcDsRatingRequest request, ReplayDsRatingResult result)
    {
        if (!request.MmrIdRatings[(int)result.RatingType].TryGetValue(player.PlayerId, out var calcRating))
        {
            calcRating = new CalcDsRating
            {
                PlayerId = player.PlayerId,
                Mmr = request.MmrOptions.StartMmr,
                IsUploader = player.IsUploader
            };
            request.MmrIdRatings[(int)result.RatingType][player.PlayerId] = calcRating;
        }

        return calcRating;
    }

    private static double EloExpectationToWin(double ratingOne, double ratingTwo, double clip)
    {
        return 1.0 / (1.0 + Math.Pow(10.0, (2.0 / clip) * (ratingTwo - ratingOne)));
    }

    private static double GetLeaverImpact(LeaverType leaverType)
    {
        return leaverType switch
        {
            LeaverType.None => 1,
            LeaverType.OneLeaver => 0.5,
            LeaverType.TwoSameTeam => 0.5,
            _ => 0.25
        };
    }

    private static double GetPlayerImpact(CalcDsRating calcRating, double teamConfidence, MmrOptions mmrOptions)
    {
        double factor_consistency =
            GetCorrectedRevConsistency(1 - calcRating.Consistency, mmrOptions.consistencyImpact);
        double factor_confidence = GetCorrectedConfidenceFactor(calcRating.Confidence,
                                                                teamConfidence,
                                                                mmrOptions.distributionMult,
                                                                mmrOptions.confidenceImpact);

        return 1
            * (mmrOptions.UseConsistency ? factor_consistency : 1.0)
            * (mmrOptions.UseConfidence ? factor_confidence : 1.0);
    }

    private static double GetCorrectedRevConsistency(double raw_revConsistency, double consistencyImpact)
    {
        return 1 + consistencyImpact * (raw_revConsistency - 1);
    }

    private static double GetCorrectedConfidenceFactor(double playerConfidence,
                                               double replayConfidence,
                                               double distributionMult,
                                               double confidenceImpact)
    {
        double totalConfidenceFactor =
            (0.5 * (1 - GetConfidenceFactor(playerConfidence, distributionMult)))
            + (0.5 * GetConfidenceFactor(replayConfidence, distributionMult));

        return 1 + confidenceImpact * (totalConfidenceFactor - 1);
    }

    private static double GetConfidenceFactor(double confidence, double distributionMult)
    {
        double variance = ((distributionMult * 0.4) + (1 - confidence));

        return distributionMult * (1 / (Math.Sqrt(2 * Math.PI) * Math.Abs(variance)));
    }

    private static double CalculateMmrDelta(double elo, double playerImpact, double eloK)
    {
        return (double)(eloK * (1 - elo) * playerImpact);
    }
}

public record TeamInfos(TeamInfo WinnerTeam, TeamInfo LoserTeam);
public record TeamInfo()
{
    public int Count { get; set; }
    public double Rating { get; set; }
    public double Consistency { get; set; }
    public double Confidence { get; set; }
    public List<TeamPlayer> Players { get; set; } = [];
    public void Update(PlayerCalcDto player, CalcDsRating calcRating)
    {
        Players.Add(new(player, calcRating));
        Rating += calcRating.Mmr;
        Consistency += calcRating.Consistency;
        Confidence += calcRating.Confidence;
        Count++;
    }
};

public record TeamPlayer(PlayerCalcDto Player, CalcDsRating Rating);

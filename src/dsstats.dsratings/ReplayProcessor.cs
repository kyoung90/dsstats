using dsstats.shared.Calc;
using dsstats.shared;

namespace dsstats.dsratings;

public static class ReplayProcessor
{
    public static ReplayDsRatingResult? ProcessReplay(CalcDto calcDto, CalcDsRatingRequest request)
    {
        var calcData = GetCalcData(calcDto, request);

        if (calcData is null)
        {
            return null;
        }

        List<ReplayPlayerDsRatingResult> playerRatings = new();

        foreach (var player in calcData.WinnerTeam)
        {
            var playerRating = ProcessPlayer(player, calcData, request, isWinner: true);
            playerRatings.Add(playerRating);
        }

        foreach (var player in calcData.LoserTeam)
        {
            var playerRating = ProcessPlayer(player, calcData, request, isWinner: false);
            playerRatings.Add(playerRating);
        }

        var result = new ReplayDsRatingResult()
        {
            RatingType = (RatingType)calcData.RatingType,
            LeaverType = (LeaverType)calcData.LeaverType,
            ExpectationToWin = MathF.Round((float)calcData.WinnerTeamExpecationToWin, 2),
            PlayerRatings = playerRatings,
            ReplayId = calcDto.ReplayId,
        };

        return result;
    }

    private static ReplayPlayerDsRatingResult ProcessPlayer(TeamPlayer teamPlayer,
                                                CalcData calcData,
                                                CalcDsRatingRequest request,
                                                bool isWinner)
    {
        var teamConfidence = isWinner ? calcData.WinnerTeamConfidence : calcData.LoserTeamConfidence;
        var playerImpact = GetPlayerImpact(teamPlayer.Rating, teamConfidence, request);

        var mmrDelta = 0.0;
        var consistencyDelta = 0.0;
        var confidenceDelta = 0.0;

        var exp2win = isWinner ? calcData.WinnerTeamExpecationToWin : 1.0 - calcData.WinnerTeamExpecationToWin;
        var result = isWinner ? 1 : 0;
        if (teamPlayer.Player.IsLeaver)
        {
            mmrDelta =
             -1 * CalculateMmrDelta(isWinner ? exp2win : 1.0 - exp2win, playerImpact, request.MmrOptions.EloK);
        }
        else
        {
            playerImpact *= calcData.LeaverImpact;
            mmrDelta = CalculateMmrDelta(calcData.WinnerTeamExpecationToWin, playerImpact, request.MmrOptions.EloK);
            consistencyDelta = Math.Abs(exp2win - result) < 0.50 ? 1.0 : 0.0;
            confidenceDelta = 1 - Math.Abs(exp2win - result);

            if (!isWinner)
            {
                mmrDelta *= -1;
            }
        }

        double mmrAfter = teamPlayer.Rating.Mmr + mmrDelta;
        double consistencyAfter = ((teamPlayer.Rating.Consistency * request.MmrOptions.consistencyBeforePercentage)
            + (consistencyDelta * (1 - request.MmrOptions.consistencyBeforePercentage)));
        double confidenceAfter = ((teamPlayer.Rating.Confidence * request.MmrOptions.confidenceBeforePercentage)
            + (confidenceDelta * (1 - request.MmrOptions.confidenceBeforePercentage)));

        consistencyAfter = Math.Clamp(consistencyAfter, 0, 1);
        confidenceAfter = Math.Clamp(confidenceAfter, 0, 1);

        teamPlayer.Rating.Consistency = consistencyAfter;
        teamPlayer.Rating.Confidence = confidenceAfter;
        teamPlayer.Rating.Games++;

        if (!teamPlayer.Player.IsLeaver)
        {
            if (isWinner)
            {
                teamPlayer.Rating.Wins++;
            }
            if (teamPlayer.Player.IsMvp)
            {
                teamPlayer.Rating.Mvps++;
            }
        }

        SetCmdr(teamPlayer.Rating, teamPlayer.Player.Race);
        UpdateStreak(teamPlayer.Rating, teamPlayer.Player.PlayerResult);
        UpdateRecentRatingGain(teamPlayer.Rating, mmrDelta);

        var ratingChange = (float)(mmrAfter - teamPlayer.Rating.Mmr);
        teamPlayer.Rating.Mmr = mmrAfter;
        if (teamPlayer.Rating.Mmr > teamPlayer.Rating.PeakRating)
        {
            teamPlayer.Rating.PeakRating = teamPlayer.Rating.Mmr;
        }

        return new()
        {
            ReplayPlayerId = teamPlayer.Player.ReplayPlayerId,
            GamePos = teamPlayer.Player.GamePos,
            Rating = (float)mmrAfter,
            RatingChange = ratingChange,
            Games = teamPlayer.Rating.Games,
            Consistency = (float)teamPlayer.Rating.Consistency,
            Confidence = (float)teamPlayer.Rating.Confidence,
        };
    }

    private static void UpdateRecentRatingGain(CalcDsRating rating, double mmrDelta)
    {
        if (rating.RecentRatingGain.Count >= 10)
        {
            rating.RecentRatingGain.RemoveAt(0);
        }
        rating.RecentRatingGain.Add(mmrDelta);
    }

    private static void UpdateStreak(CalcDsRating rating, int playerResult)
    {
        // Win scenario
        if (playerResult == (int)PlayerResult.Win)
        {
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

    private static double CalculateMmrDelta(double elo, double playerImpact, double eloK)
    {
        return (double)(eloK * (1 - elo) * playerImpact);
    }

    private static double GetPlayerImpact(CalcDsRating calcRating, double teamConfidence, CalcDsRatingRequest request)
    {
        double factor_consistency =
            GetCorrectedRevConsistency(1 - calcRating.Consistency, request.MmrOptions.consistencyImpact);
        double factor_confidence = GetCorrectedConfidenceFactor(calcRating.Confidence,
                                                                teamConfidence,
                                                                request.MmrOptions.distributionMult,
                                                                request.MmrOptions.confidenceImpact);

        return 1
            * (request.MmrOptions.UseConsistency ? factor_consistency : 1.0)
            * (request.MmrOptions.UseConfidence ? factor_confidence : 1.0);
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

    private static CalcData? GetCalcData(CalcDto calcDto, CalcDsRatingRequest request)
    {
        var ratingType = calcDto.GetRatingType();
        if (ratingType == (int)RatingType.None)
        {
            return null;
        }

        List<TeamPlayer> winnerTeam = new();
        List<TeamPlayer> loserTeam = new();

        foreach (var player in calcDto.Players)
        {
            bool isBanned = false;
            if (request.RatingCalcType == RatingCalcType.Dsstats
                && request.BannedPlayers.ContainsKey(player.PlayerId))
            {
                isBanned = true;
            }

            if (!request.MmrIdRatings[ratingType].TryGetValue(player.PlayerId, out var calcRating)
                || isBanned)
            {
                calcRating = request.MmrIdRatings[ratingType][player.PlayerId] = new()
                {
                    PlayerId = player.PlayerId,
                    Mmr = request.MmrOptions.StartMmr,
                    IsUploader = player.IsUploader
                };
            }

            calcRating.Duration += Convert.ToInt32(TimeSpan.FromSeconds(calcDto.Duration).TotalMinutes);
            calcRating.LatestReplay = calcDto.GameTime;

            if (player.PlayerResult == 1)
            {
                winnerTeam.Add(new(player, calcRating));
            }
            else
            {
                loserTeam.Add(new(player, calcRating));
            }
        }

        if (winnerTeam.Count == 0 || loserTeam.Count == 0)
        {
            return null;
        }

        var expectationToWin = EloExpectationToWin(winnerTeam.Sum(s => s.Rating.Mmr) / winnerTeam.Count,
            loserTeam.Sum(s => s.Rating.Mmr) / loserTeam.Count,
            request.MmrOptions.Clip);

        var leaverType = calcDto.GetLeaverTyp();

        return new()
        {
            RatingType = ratingType,
            LeaverType = leaverType,
            LeaverImpact = GetLeaverImpact(leaverType),
            WinnerTeam = winnerTeam,
            LoserTeam = loserTeam,
            WinnerTeamExpecationToWin = expectationToWin,
            WinnerTeamConfidence = winnerTeam.Sum(s => s.Rating.Confidence) / winnerTeam.Count,
            LoserTeamConfidence = loserTeam.Sum(s => s.Rating.Confidence) / loserTeam.Count,
        };
    }

    private static double EloExpectationToWin(double ratingOne, double ratingTwo, double clip)
    {
        return 1.0 / (1.0 + Math.Pow(10.0, (2.0 / clip) * (ratingTwo - ratingOne)));
    }

    private static double GetLeaverImpact(int leaverType)
    {
        return leaverType switch
        {
            0 => 1,
            1 => 0.5,
            2 => 0.5,
            _ => 0.25
        };
    }
}

public record CalcData
{
    public int RatingType { get; init; }
    public int LeaverType { get; init; }
    public double LeaverImpact { get; init; }
    public List<TeamPlayer> WinnerTeam { get; init; } = new();
    public List<TeamPlayer> LoserTeam { get; init; } = new();
    public double WinnerTeamExpecationToWin { get; init; }
    public double WinnerTeamConfidence { get; init; }
    public double LoserTeamConfidence { get; init; }
}

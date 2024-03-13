using dsstats.shared;
using dsstats.shared.Calc;

namespace dsstats.ratings.lib;

public static class RatingsNg
{
    public static List<ReplayNgRatingResult> ProcessReplayNg(CalcDto calcDto, CalcRatingNgRequest request)
    {
        var calcDatas = GetCalcDatas(calcDto, request);

        if (calcDatas.Count == 0)
        {
            return [];
        }

        List<ReplayNgRatingResult> results = [];


        foreach (var calcData in calcDatas)
        {
            List<ReplayPlayerNgRatingResult> playerRatings = [];

            foreach (var player in calcData.WinnerTeam)
            {
                var playerRating = ProcessPlayerNg(player, calcData, request, isWinner: true);
                playerRatings.Add(playerRating);
            }

            foreach (var player in calcData.LoserTeam)
            {
                var playerRating = ProcessPlayerNg(player, calcData, request, isWinner: false);
                playerRatings.Add(playerRating);
            }

            results.Add(new()
            {
                RatingNgType = (RatingNgType)calcData.RatingType,
                LeaverType = (LeaverType)calcData.LeaverType,
                Exp2Win = MathF.Round((float)calcData.WinnerTeamExpecationToWin, 2),
                ReplayId = calcDto.ReplayId,
                ReplayPlayerNgRatingResults = playerRatings
            });
        }

        return results;
    }

    private static ReplayPlayerNgRatingResult ProcessPlayerNg(PlayerCalcDto player,
                                            CalcData calcData,
                                            CalcRatingNgRequest request,
                                            bool isWinner)
    {
        var teamConfidence = isWinner ? calcData.WinnerTeamConfidence : calcData.LoserTeamConfidence;
        var playerImpact = GetPlayerImpact(player.CalcRating, teamConfidence, request);

        var mmrDelta = 0.0;
        var consistencyDelta = 0.0;
        var confidenceDelta = 0.0;

        var exp2win = isWinner ? calcData.WinnerTeamExpecationToWin : 1.0 - calcData.WinnerTeamExpecationToWin;
        var result = isWinner ? 1 : 0;
        if (player.IsLeaver)
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

        double mmrAfter = player.CalcRating.Mmr + mmrDelta;
        double consistencyAfter = ((player.CalcRating.Consistency * request.MmrOptions.consistencyBeforePercentage)
            + (consistencyDelta * (1 - request.MmrOptions.consistencyBeforePercentage)));
        double confidenceAfter = ((player.CalcRating.Confidence * request.MmrOptions.confidenceBeforePercentage)
            + (confidenceDelta * (1 - request.MmrOptions.confidenceBeforePercentage)));

        consistencyAfter = Math.Clamp(consistencyAfter, 0, 1);
        confidenceAfter = Math.Clamp(confidenceAfter, 0, 1);

        player.CalcRating.Consistency = consistencyAfter;
        player.CalcRating.Confidence = confidenceAfter;
        player.CalcRating.Games++;

        if (!player.IsLeaver)
        {
            if (isWinner)
            {
                player.CalcRating.Wins++;
            }
            if (player.IsMvp)
            {
                player.CalcRating.Mvps++;
            }
        }

        SetCmdr(player.CalcRating, player.Race);

        var ratingChange = (float)(mmrAfter - player.CalcRating.Mmr);
        player.CalcRating.Mmr = mmrAfter;

        return new()
        {
            ReplayPlayerId = player.ReplayPlayerId,
            Rating = (float)mmrAfter,
            Change = ratingChange,
            Games = player.CalcRating.Games,
            Consistency = (float)player.CalcRating.Consistency,
            Confidence = (float)player.CalcRating.Confidence,
        };
    }

    private static void SetCmdr(CalcRating calcRating, Commander cmdr)
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

    private static double GetPlayerImpact(CalcRating calcRating, double teamConfidence, CalcRatingNgRequest request)
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

    private static List<CalcData> GetCalcDatas(CalcDto calcDto, CalcRatingNgRequest request)
    {
        var ratingType = calcDto.GetRatingNgType();

        if (ratingType == RatingNgType.None)
        {
            return [];
        }

        List<CalcData> calcDatas = [];

        var ratingTypes = ratingType.GetUniqueFlags();
        foreach (RatingNgType ratingNgType in ratingTypes)
        {
            if (ratingNgType == RatingNgType.None)
            {
                continue;
            }

            var calcData = GetCalcData(calcDto, request, (int)ratingNgType);
            if (calcData is not null)
            {
                calcDatas.Add(calcData);
            }
        }

        return calcDatas;
    }

    private static CalcData? GetCalcData(CalcDto calcDto, CalcRatingNgRequest request, int ratingNgType)
    {
        List<PlayerCalcDto> winnerTeam = [];
        List<PlayerCalcDto> loserTeam = [];

        foreach (var player in calcDto.Players)
        {
            if (!request.MmrIdRatings[ratingNgType].TryGetValue(player.PlayerId, out var calcRating))
            {
                calcRating = request.MmrIdRatings[ratingNgType][player.PlayerId] = new()
                {
                    PlayerId = player.PlayerId,
                    Mmr = request.MmrOptions.StartMmr,
                    IsUploader = player.IsUploader
                };
            }

            if (player.PlayerResult == 1)
            {
                winnerTeam.Add(player with { CalcRating = calcRating });
            }
            else
            {
                loserTeam.Add(player with { CalcRating = calcRating });
            }
        }

        if (winnerTeam.Count != loserTeam.Count)
        {
            return null;
        }

        var expectationToWin = EloExpectationToWin(winnerTeam.Sum(s => s.CalcRating.Mmr) / winnerTeam.Count,
            loserTeam.Sum(s => s.CalcRating.Mmr) / loserTeam.Count,
            request.MmrOptions.Clip);

        var leaverType = calcDto.GetLeaverTyp();

        return new()
        {
            RatingType = ratingNgType,
            LeaverType = leaverType,
            LeaverImpact = GetLeaverImpact(leaverType),
            WinnerTeam = winnerTeam,
            LoserTeam = loserTeam,
            WinnerTeamExpecationToWin = expectationToWin,
            WinnerTeamConfidence = winnerTeam.Sum(s => s.CalcRating.Confidence) / winnerTeam.Count,
            LoserTeamConfidence = loserTeam.Sum(s => s.CalcRating.Confidence) / loserTeam.Count,
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

    public static IEnumerable<T> GetUniqueFlags<T>(this T flags)
        where T : Enum
    {
        foreach (Enum value in Enum.GetValues(flags.GetType()))
            if (flags.HasFlag(value))
                yield return (T)value;
    }
}



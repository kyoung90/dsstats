using dsstats.shared;
using dsstats.shared.Calc;

namespace dsstats.dsratings;

public static class DsstatsReplayProcessor
{
    public static ReplayDsRatingResult? ProcessReplay(CalcDto replay, CalcRatingRequest request)
    {
        ReplayDsRatingResult result = new ReplayDsRatingResult()
        {
            RatingType = (RatingType)replay.GetRatingType(),
            LeaverType = (LeaverType)replay.GetLeaverTyp(),
            ReplayId = replay.ReplayId
        };

        var teamInfos = InitPlayers(replay, request, result);

        result.ExpectationToWin = EloExpectationToWin(
            teamInfos.WinnerTeam.TeamRating / teamInfos.WinnerTeam.Count,
            teamInfos.LoserTeam.TeamRating / teamInfos.LoserTeam.Count,
            request.MmrOptions.Clip);

        var leaverImpact = GetLeaverImpact(result.LeaverType);
        foreach (var player in result.PlayerRatings)
        {

        }

        return result;
    }

    public static TeamInfos InitPlayers(CalcDto replay, CalcRatingRequest request, ReplayDsRatingResult result)
    {
        double winnerTeamRating = 0;
        double winnerTeamConsistency = 0;
        double winnerTeamConfidence = 0;
        double loserTeamRating = 0;
        double loserTeamConsistency = 0;
        double loserTeamConfidence = 0;
        int winnerCount = 0;
        int loserCount = 0; 
        foreach (var player in replay.Players)
        {
            if (!request.MmrIdRatings[(int)result.RatingType].TryGetValue(player.PlayerId, out var calcRating)
)
            {
                calcRating = request.MmrIdRatings[(int)result.RatingType][player.PlayerId] = new()
                {
                    PlayerId = player.PlayerId,
                    Mmr = request.MmrOptions.StartMmr,
                    IsUploader = player.IsUploader
                };
            }
            result.PlayerRatings.Add(new()
            {
                Games = calcRating.Games,
                Wins = calcRating.Wins,
                Rating = calcRating.Mmr,
                Consistency = calcRating.Consistency,
                Confidence = calcRating.Confidence,
            });
            if (player.PlayerResult == (int)PlayerResult.Win)
            {
                winnerTeamRating += calcRating.Mmr;
                winnerTeamConsistency += calcRating.Consistency;
                winnerTeamConfidence += calcRating.Confidence;
                winnerCount++;
            }
            else
            {
                loserTeamRating += calcRating.Mmr;
                loserTeamConsistency += calcRating.Consistency;
                loserTeamConfidence += calcRating.Confidence;
                loserCount++;
            }
        }
        TeamInfo winnerTeamInfo = new(winnerCount, winnerTeamRating, winnerTeamConsistency, winnerTeamConfidence);
        TeamInfo loserTeamInfo = new(loserCount, loserTeamRating, loserTeamConsistency, loserTeamConfidence);
        return new(winnerTeamInfo, loserTeamInfo);
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
}

public record TeamInfos(TeamInfo WinnerTeam, TeamInfo LoserTeam);
public record TeamInfo(int Count, double TeamRating, double TeamConsistency, double TeamConfidence);

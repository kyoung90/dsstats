using dsstats.db8;
using dsstats.shared;
using dsstats.shared.Calc;
using Microsoft.EntityFrameworkCore;
namespace dsstats.dsratings;

public class DsstatsRatings(ReplayContext context) : DsRatingCalculator
{
    public override async Task<List<CalcDto>> GetReplays(CalcReplaysRequest request)
    {
        var rawDtos = await context.Replays
            .Where(x => x.Playercount == 6
             && x.Duration >= 300
             && x.WinnerTeam > 0
             && request.GameModes.Contains(x.GameMode)
             && x.TournamentEdition == false
             && x.GameTime >= request.Start)
            .OrderBy(o => o.GameTime)
                .ThenBy(o => o.ReplayId)
            .Select(s => new RawCalcDto()
            {
                DsstatsReplayId = s.ReplayId,
                GameTime = s.GameTime,
                Duration = s.Duration,
                Maxkillsum = s.Maxkillsum,
                GameMode = (int)s.GameMode,
                TournamentEdition = false,
                Players = s.ReplayPlayers.Select(t => new RawPlayerCalcDto()
                {
                    ReplayPlayerId = t.ReplayPlayerId,
                    GamePos = t.GamePos,
                    PlayerResult = (int)t.PlayerResult,
                    Race = t.Race,
                    Duration = t.Duration,
                    Kills = t.Kills,
                    Team = t.Team,
                    IsUploader = t.Player.UploaderId != null,
                    PlayerId = new(t.Player.ToonId, t.Player.RealmId, t.Player.RegionId)
                }).ToList()

            })
            .AsSplitQuery()
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync();

        return rawDtos.Select(s => s.GetCalcDto()).ToList();
    }

    public override ReplayDsRatingResult? ProcessReplay(CalcDto replay, CalcRatingRequest request)
    {
        throw new NotImplementedException();
    }

    public override async Task SavePlayerRatings(CalcRatingRequest request)
    {
        await Task.Delay(1000);
    }

    public override async Task SaveStepResult(List<ReplayDsRatingResult> replayRatings, CalcRatingRequest request)
    {
        await Task.Delay(1000);
    }
}

public record RawCalcDto
{
    public int DsstatsReplayId { get; set; }
    public int Sc2ArcadeReplayId { get; set; }
    public DateTime GameTime { get; init; }
    public int GameMode { get; set; }
    public int Duration { get; init; }
    public int Maxkillsum { get; init; }
    public bool TournamentEdition { get; init; }
    public List<RawPlayerCalcDto> Players { get; init; } = new();

    public CalcDto GetCalcDto()
    {
        return new()
        {
            ReplayId = Math.Max(DsstatsReplayId, Sc2ArcadeReplayId),
            GameTime = GameTime,
            GameMode = GameMode,
            Duration = Duration,
            TournamentEdition = TournamentEdition,
            Players = Players.Select(s => new PlayerCalcDto()
            {
                ReplayPlayerId = s.ReplayPlayerId,
                GamePos = s.GamePos,
                PlayerResult = s.PlayerResult,
                IsLeaver = s.Duration < Duration - 90,
                IsMvp = s.Kills == Maxkillsum,
                Team = s.Team,
                Race = s.Race,
                PlayerId = s.PlayerId,
                IsUploader = s.IsUploader
            }).ToList(),
        };
    }
}

public record RawPlayerCalcDto
{
    public int ReplayPlayerId { get; init; }
    public int GamePos { get; init; }
    public int PlayerResult { get; init; }
    public int Duration { get; init; }
    public int Kills { get; init; }
    public int Team { get; init; }
    public Commander Race { get; init; }
    public PlayerId PlayerId { get; init; } = null!;
    public bool IsUploader { get; set; }
}
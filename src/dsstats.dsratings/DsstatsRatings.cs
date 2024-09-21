using dsstats.db8;
using dsstats.shared;
using dsstats.shared.Calc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;
using System.Text.Json;
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

    public override ReplayDsRatingResult? ProcessReplay(CalcDto replay, CalcDsRatingRequest request)
    {
        var result = DsstatsReplayProcessor.ProcessReplay(replay, request);
        return result;
    }

    public override async Task SavePlayerRatings(CalcDsRatingRequest request)
    {
        await Task.Delay(1000);
        var topRatings = request.MmrIdRatings[(int)RatingType.Std].Values.OrderByDescending(o => o.Mmr).ToList();
        var json = JsonSerializer.Serialize(topRatings, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText("/data/ds/dsratings.json", json);
    }

    public override async Task SaveStepResult(List<ReplayDsRatingResult> replayRatings, CalcDsRatingRequest request)
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

public record CalcDsRatingRequest
{
    public RatingCalcType RatingCalcType { get; init; }
    public bool Continue { get; set; }
    public DateTime StarTime { get; set; }
    public MmrOptions MmrOptions { get; set; } = new();
    public List<CalcDto> CalcDtos { get; set; } = new();
    public int ReplayRatingAppendId { get; set; }
    public int ReplayPlayerRatingAppendId { get; set; }
    public Dictionary<int, Dictionary<PlayerId, CalcDsRating>> MmrIdRatings { get; set; } = [];
    public FrozenDictionary<PlayerId, bool> BannedPlayers { get; init; } = new Dictionary<PlayerId, bool>()
            {
                { new(466786, 2, 2), true }, // SabreWolf
                { new(9774911, 1, 2), true }, // Baka
                { new(3768192, 1, 1), true } // Henz
            }.ToFrozenDictionary();
    public FrozenDictionary<PlayerId, bool> SoftBannedPlayers { get; init; } = new Dictionary<PlayerId, bool>()
    {

    }.ToFrozenDictionary();
}

public record CalcDsRating
{
    public PlayerId PlayerId { get; set; } = new();
    public int Games { get; set; }
    public int Wins { get; set; }
    public int Mvps { get; set; }
    public double Mmr { get; set; }
    public double Consistency { get; set; }
    public double Confidence { get; set; }
    public bool IsUploader { get; set; }
    public double PeakRating { get; set; }
    public int WinStreak { get; set; }
    public int LoseStreak { get; set; }
    public int CurrentStreak { get; set; }
    public int Duration { get; set; }
    public List<double> RecentRatingGain { get; set; } = [];
    public Dictionary<Commander, int> CmdrCounts { get; set; } = [];
    public DateTime LatestReplay { get; set; }
}
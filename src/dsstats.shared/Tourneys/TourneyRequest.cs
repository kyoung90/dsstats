namespace dsstats.shared.Tourneys;

public record TourneyRequest
{

}

public record TourneyReplayListDto
{
    public DateTime GameTime { get; init; }
    public int Duration { get; init; }
    public int WinnerTeam { get; init; }
    public GameMode GameMode { get; init; }
    public bool TournamentEdition { get; init; }
    public string ReplayHash { get; init; } = string.Empty;
    public string CommandersTeam1 { get; init; } = string.Empty;
    public string CommandersTeam2 { get; init; } = string.Empty;
    public string TournamentName { get; init; } = string.Empty;
}
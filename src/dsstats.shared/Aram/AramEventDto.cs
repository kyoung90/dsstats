namespace dsstats.shared.Aram;

public record AramEventDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public GameMode GameMode { get; set; }
}


public record AramPlayerDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? AmPlayerId { get; set; }
    public int? EuPlayerId { get; set; }
    public int StartRating { get; set; }
}

public record MatchReport
{
    public Guid MatchGuid { get; set; }
    public MatchResult Result { get; set; }
    public string? ReplayHash1 { get; set; }
    public string? ReplayHash2 { get; set; }
}
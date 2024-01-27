namespace dsstats.shared;

public record TourneyDto
{
    public string Name { get; set; } = string.Empty;
    public Guid TourneyGuid { get; set; }
    public DateTime StartDate { get; set; }
    public GameMode GameMode { get; set; }
    public string? WinnerTeam { get; set; }
}

public record TourneyCreateDto
{
    public string Name { get; set; } = "unknown";
    public DateTime EventStart { get; set; }
    public GameMode GameMode { get; set; }
}

public record TourneyPlayersDto
{
    public Guid TourneyGuid {  set; get; }
    public List<PlayerId> PlayerIds { get; set; } = [];
}

public record TourneyTeamCreateDto
{
    public Guid TourneyGuid { get; set; }
    public string Name {  set; get; } = string.Empty;
    public List<PlayerId> Players { get; set; } = [];
}

public record TourneyMatchCreateDto
{
    public Guid TourneyGuid { set; get; }
    public int Round { get; set; }
    public int Group { get; set; }
    public bool IsLowerBracket { get; set; }
    public Guid TeamAGuid { get; set; }
    public Guid TeamBGuid { get; set; }
    public Commander Ban1 { get; set; }
    public Commander Ban2 { get; set; }
    public Commander Ban3 { get; set; }
}

public record TourneyMatchResult
{
    public Guid TourneyMatchGuid { set; get; }
    public MatchResult MatchResult { get; set; }
    public Commander Ban1 { get; set; }
    public Commander Ban2 { get; set; }
    public Commander Ban3 { get; set; }
    public List<string> ReplayHashes { get; set; } = [];
}
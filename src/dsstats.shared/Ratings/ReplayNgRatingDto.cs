namespace dsstats.shared;

public record ReplayNgRatingResult
{
    public RatingNgType RatingNgType { get; set; }
    public LeaverType LeaverType { get; set; }
    public float Exp2Win { get; set; }
    public int AvgRating { get; set; }
    public bool IsPreRating { get; set; }
    public int ReplayId { get; set; }
    public List<ReplayPlayerNgRatingResult> ReplayPlayerNgRatingResults { get; set; } = [];
}

public record ReplayPlayerNgRatingResult
{
    public float Rating { get; set; }
    public float Change { get; set; }
    public int Games { get; set; }
    public float Consistency { get; set; }
    public float Confidence { get; set; }
    public int? ReplayPlayerId { get; set; }
}

public record RatingsNgRequest
{
    public RatingNgType RatingNgType { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    public List<TableOrder> Orders { get; set; } = [];
}

public record RatingsNgResult
{
    public List<PlayerRatingNgListDto> Ratings { get; init; } = [];
}

public record PlayerRatingNgListDto
{
    public double Rating { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public int Pos { get; set; }
    public PlayerDto Player { get; set; } = new();
    public int Mvp { get; set; }
    public int MainCount { get; set; }
    public Commander MainCmdr { get; set; }
}
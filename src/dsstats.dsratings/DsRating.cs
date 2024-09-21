namespace dsstats.dsratings;

public class DsRating
{
    public int Pos { get; set; }
    public double PercentileRank { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public int Duration { get; set; }
    public double Rating {  get; set; }
    public double Consistency { get; set; }
    public double Confidence { get; set; }
    public double RecentRatingGain { get; set; }
    public double PeakRating {  get; set; }
    public int WinStreak { get; set; }
    public int LoseStreak { get; set; }
    public int CurrentStreak { get; set; }
}



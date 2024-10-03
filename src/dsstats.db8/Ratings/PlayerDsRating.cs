using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace dsstats.db8.Ratings;

public class PlayerDsRating
{
    public int PlayerDsRatingId { get; set; }
    public RatingType RatingType { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public int Mvps { get; set; }
    public double Mmr { get; set; }
    public double Consistency { get; set; }
    public double Confidence { get; set; }
    [Column(TypeName = "FLOAT(8, 2)")]
    public double PeakRating { get; set; }
    [Column(TypeName = "FLOAT(8, 2)")]
    public double RecentRatingGain { get; set; }
    public Commander MainCmdr { get; set; }
    [Column(TypeName = "FLOAT(8, 2)")]
    public double MainPercentage { get; set; }
    public int WinStreak { get; set; }
    public int LoseStreak { get; set; }
    public int CurrentStreak { get; set; }
    public int Duration { get; set; }
    [Precision(0)]
    public DateTime LatestReplay { get; set; }
    public int PlayerId { get; set; }
    public Player? Player { get; set; }
}

public class ReplayPlayerDsRating
{
    public int ReplayPlayerDsRatingId { get; set; }
    [Column(TypeName = "FLOAT(8, 2)")]
    public float Rating { get; set; }
    [Column(TypeName = "FLOAT(8, 2)")]
    public float RatingChange { get; set; }
    public int Games { get; set; }
    public int CmdrGames { get; set; }
    [Column(TypeName = "FLOAT(8, 2)")]
    public float Consistency { get; set; }
    [Column(TypeName = "FLOAT(8, 2)")]
    public float Confidence { get; set; }
    public int ReplayPlayerId { get; set; }
    public ReplayPlayer? ReplayPlayer { get; set; }
}

public class ReplayDsRating
{
    public int ReplayDsRatingId { get; set; }
    public RatingType RatingType { get; set; }
    public LeaverType LeaverType { get; set; }
    [Column(TypeName = "FLOAT(6, 2)")]
    public float ExpectationToWin { get; set; }
    public bool IsPreRating { get; set; }
    public int AvgRating { get; set; }
    public int ReplayId { get; set; }
    public Replay? Replay { get; set; }
}
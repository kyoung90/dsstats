using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dsstats.db8.Aram;

public class AramEvent
{
    public AramEvent()
    {
        AramPlayers = new HashSet<AramPlayer>();
        AramMatches = new HashSet<AramMatch>();
    }
    public int AramEventId { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Precision(0)]
    public DateTime StartTime { get; set; }
    [Precision(0)]
    public DateTime EndTime { get; set; }
    public GameMode GameMode { get; set; }
    public int OpenMatches { get; set; }
    public int ReportedMatches { get; set; }
    public virtual ICollection<AramPlayer> AramPlayers { get; set; }
    public virtual ICollection<AramMatch> AramMatches { get; set; }
}

public class AramPlayer
{
    public AramPlayer()
    {
        AramSlots = new HashSet<AramSlot>();
    }
    public int AramPlayerId { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public PlayerStatus Status { get; set; }
    public int StartRating { get; set; }
    public int Wins { get; set; }
    public int Performance { get; set; }
    public int Matches { get; set; }
    public int? AmPlayerId { get; set; }
    [ForeignKey(nameof(AmPlayerId))]
    public Player? AmPlayer { get; set; }
    public int? EuPlayerId { get; set; }
    [ForeignKey(nameof(EuPlayerId))]
    public Player? EuPlayer { get; set; }
    public int AramEventId { get; set; }
    public virtual AramEvent? AramEvent { get; set; }
    public virtual ICollection<AramSlot> AramSlots { get; set; }
}

public class AramMatch
{
    public AramMatch()
    {
        AramSlots = new HashSet<AramSlot>();
    }
    public int AramMatchId { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public int Team1Rating { get; set; }
    public int Team2Rating { get; set; }
    public MatchResult MatchResult{ get; set; }
    public float MatchHistoryScore { get; set; }
    public virtual ICollection<AramSlot> AramSlots { get; set; }
    public int? Replay1Id { get; set; }
    [ForeignKey(nameof(Replay1Id))]
    public virtual Replay? Replay1 { get; set; }
    public int? Replay2Id { get; set; }
    [ForeignKey(nameof(Replay2Id))]
    public virtual Replay? Replay2 { get; set; }
    public int AramEventId { get; set; }
    public virtual AramEvent? AramEvent { get; set; }
}

public class AramSlot
{
    public int AramSlotId { get; set; }
    public int Pos { get; set; }
    public int Team { get; set; }
    public Commander Commander { get; set; }
    public int AramPlayerId { get; set; }
    public virtual AramPlayer? AramPlayer { get; set; }
    public int AramMatchId { get; set; }
    public virtual AramMatch? AramMatch { get; set; }
}



using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace dsstats.db8;

public class Tourney
{
    public Tourney()
    {
        TourneyPlayers = new HashSet<TourneyPlayer>();
        TourneyMatches = new HashSet<TourneyMatch>();
        TourneyTeams = new HashSet<TourneyTeam>();
    }
    public int TourneyId { get; set; }
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public Guid TourneyGuid { get; set; } = Guid.NewGuid();
    public int? WinnerTeamId { get; set; }
    [Precision(0)]
    public DateTime StartDate { get; set; }
    public GameMode GameMode { get; set; }
    public Guid? WinnerTeam { get; set; }
    public ICollection<TourneyPlayer> TourneyPlayers { get; set; }
    public ICollection<TourneyMatch> TourneyMatches { get; set; }
    public ICollection<TourneyTeam> TourneyTeams { get; set; }
}


public class TourneyPlayer
{
    public int TourneyPlayerId { get; set; }
    public Guid TourneyPlayerGuid { get; set; } = Guid.NewGuid();
    public int TourneyId { get; set; }
    public Tourney? Tourney { get; set; }
    public int PlayerId { get; set; }
    public Player? Player { get; set; }
    public int? TourneyTeamId { get; set; }
    public TourneyTeam? TourneyTeam { get; set; }
}

public class TourneyTeam
{
    public TourneyTeam()
    {
        TourneyPlayers = new HashSet<TourneyPlayer>();
        TourneyMatches = new HashSet<TourneyMatch>();
    }

    public int TourneyTeamId { get; set; }
    public Guid TeamGuid {  get; set; } = Guid.NewGuid();
    [MaxLength(100)]
    public string Name { get; set; } = "unknown";
    public int TourneyId { get; set; }
    public Tourney? Tourney { get; set; }
    public ICollection<TourneyPlayer> TourneyPlayers { get; set; }
    public ICollection<TourneyMatch> TourneyMatches { get; set; }
}

public class TourneyMatch
{
    public TourneyMatch()
    {
        TourneyTeams = new HashSet<TourneyTeam>();
        Replays = new HashSet<Replay>();
    }

    public int TourneyMatchId { get; set; }
    public Guid TourneyMatchGuid { get; set; } = Guid.NewGuid();
    public int Round {  get; set; }
    public int Group {  get; set; }
    public bool IsLowerBracket { get; set; }
    public int TourneyId { get; set; }
    public Tourney? Tourney { get; set; }
    public Commander Ban1 { get; set; }
    public Commander Ban2 { get; set; }
    public Commander Ban3 { get; set; }
    public MatchResult MatchResult { get; set; }
    public Guid? TeamAGuid { get; set; }
    public ICollection<TourneyTeam> TourneyTeams { get; set; }
    public ICollection<Replay> Replays { get; set; }
}




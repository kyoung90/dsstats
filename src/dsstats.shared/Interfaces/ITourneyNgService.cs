using dsstats.shared.Tourneys;

namespace dsstats.shared.Interfaces;

public interface ITourneyNgService
{
    Task<bool> AddTournamentPlayers(TourneyPlayersDto playersDto);
    Task<Guid> AddTourneyMatch(TourneyMatchCreateDto createDto);
    Task<Guid> AddTourneyTeam(TourneyTeamCreateDto createDto);
    Task<bool> CreateNewSwissRound(Guid tourneyGuid);
    Task<bool> CreateRandomTeams(Guid tourneyGuid, RatingType ratingType);
    Task<bool> CreateRoundRobinBracket(Guid tourneyGuid);
    Task<Guid> CreateTournament(TourneyCreateDto createDto);
    Task<List<TourneyDto>> GetTournaments();
    Task<List<TourneyReplayListDto>> GetTourneyReplays(TourneysReplaysRequest request, CancellationToken token);
    Task<int> GetTourneyReplaysCount(TourneysReplaysRequest request, CancellationToken token);
    Task<bool> ReportMatchResult(TourneyMatchResult result);
}
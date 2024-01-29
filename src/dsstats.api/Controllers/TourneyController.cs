using dsstats.shared;
using dsstats.shared.Interfaces;
using dsstats.shared.Tourneys;
using Microsoft.AspNetCore.Mvc;

namespace dsstats.api.Controllers;

[ApiController]
[Route("api8/v1/[controller]")]
public class TourneyController(ITourneyNgService tourneyService) : Controller
{
    [HttpGet]
    public async Task<ActionResult<List<TourneyDto>>> GetTournaments()
    {
        return await tourneyService.GetTournaments();
    }

    [HttpPost]
    [Route("replays")]
    public async Task<ActionResult<List<TourneyReplayListDto>>> GetTourneyReplays(TourneysReplaysRequest request, CancellationToken token)
    {
        return await tourneyService.GetTourneyReplays(request, token);
    }

    [HttpPost]
    [Route("replayscount")]
    public async Task<ActionResult<int>> GetTourneyReplaysCount(TourneysReplaysRequest request, CancellationToken token)
    {
        return await tourneyService.GetTourneyReplaysCount(request, token);
    }
}
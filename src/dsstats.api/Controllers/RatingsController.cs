
using dsstats.shared;
using dsstats.shared.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace dsstats.api.Controllers;

[EnableCors("dsstatsOrigin")]
[ApiController]
[Route("api8/v1/[controller]")]
public class RatingsController(IRatingsService ratingsService) : Controller
{

    [HttpPost]
    [Route("ratingscount")]
    public async Task<ActionResult<int>> GetRatingsCount(RatingsNgRequest request, CancellationToken token = default)
    {
        return await ratingsService.GetRatingsCount(request, token);
    }

    [HttpPost]
    [Route("ratings")]
    public async Task<ActionResult<RatingsNgResult>> GetRatings(RatingsNgRequest request, CancellationToken token)
    {
        return await ratingsService.GetRatings(request, token);
    }
}

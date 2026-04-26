using System.Security.Claims;
using Haishabet.Application.Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haishabet.Api.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController : ControllerBase
{
    private readonly IGameService _games;

    public GamesController(IGameService games) => _games = games;

    [HttpGet]
    public async Task<IActionResult> Catalog(CancellationToken ct)
    {
        var result = await _games.GetCatalogAsync(ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("spin")]
    [Authorize]
    public async Task<IActionResult> Spin([FromBody] SpinRequest body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userId, out var id)) return Unauthorized();

        var result = await _games.SpinAsync(id, body, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}

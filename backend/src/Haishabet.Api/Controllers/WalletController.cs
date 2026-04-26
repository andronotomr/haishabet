using System.Security.Claims;
using Haishabet.Application.Wallet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haishabet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/wallet")]
public sealed class WalletController : ControllerBase
{
    private readonly IWalletService _wallet;

    public WalletController(IWalletService wallet) => _wallet = wallet;

    [HttpGet("balance")]
    public async Task<IActionResult> Balance(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userId, out var id)) return Unauthorized();

        var result = await _wallet.GetBalanceAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Crédito manual de saldo — APENAS PARA DEMO/MVP.
    /// Em produção, saldo só aparece via webhook do PSP confirmando PIX recebido.
    /// </summary>
    [HttpPost("deposit/manual")]
    public async Task<IActionResult> ManualDeposit([FromBody] DepositRequest body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userId, out var id)) return Unauthorized();

        var result = await _wallet.CreditDepositAsync(id, body.AmountCents, Guid.NewGuid().ToString("N"), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("extract")]
    public async Task<IActionResult> Extract([FromQuery] int take = 50, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userId, out var id)) return Unauthorized();

        var result = await _wallet.GetExtractAsync(id, take, ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }
}

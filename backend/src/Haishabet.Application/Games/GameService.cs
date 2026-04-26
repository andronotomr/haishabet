using Haishabet.Application.Common;
using Haishabet.Application.Persistence;
using Haishabet.Domain;
using Haishabet.Domain.Entities;
using Haishabet.GameEngine.Rng;
using Haishabet.GameEngine.Slot;

namespace Haishabet.Application.Games;

/// <summary>
/// Orquestra: catálogo de jogos + execução de spin.
/// Hoje só Slot in-house é suportado. Crash/Mines virão em fase 2.
/// </summary>
public sealed class GameService : IGameService
{
    private readonly IGameRepository _games;
    private readonly IWalletRepository _wallets;
    private readonly IBetRepository _bets;
    private readonly IClock _clock;
    private readonly IReadOnlyDictionary<string, SlotConfig> _slotConfigs;
    private long _nonceCounter;     // simples para MVP em memória (multi-process precisa Redis INCR)

    public GameService(
        IGameRepository games,
        IWalletRepository wallets,
        IBetRepository bets,
        IClock clock,
        IReadOnlyDictionary<string, SlotConfig> slotConfigs)
    {
        _games = games;
        _wallets = wallets;
        _bets = bets;
        _clock = clock;
        _slotConfigs = slotConfigs;
    }

    public async Task<Result<IReadOnlyList<GameSummaryDto>>> GetCatalogAsync(CancellationToken ct = default)
    {
        var list = await _games.GetActiveAsync(ct);
        var dtos = list.Select(g => new GameSummaryDto(
            g.Id, g.Slug, g.Name, g.Type.ToString(), g.Provider,
            g.MinBet.Cents, g.MaxBet.Cents, g.RtpTarget, g.Volatility.ToString(),
            g.ThumbnailUrl
        )).ToList();
        return Result<IReadOnlyList<GameSummaryDto>>.Ok(dtos);
    }

    public async Task<Result<SpinResponseDto>> SpinAsync(Guid userId, SpinRequest request, CancellationToken ct = default)
    {
        var game = await _games.FindBySlugAsync(request.GameSlug, ct);
        if (game is null || !game.Active)
            return Result<SpinResponseDto>.Fail("Jogo não encontrado.");

        if (!_slotConfigs.TryGetValue(request.GameSlug, out var slotConfig))
            return Result<SpinResponseDto>.Fail($"Configuração matemática do jogo '{request.GameSlug}' não está registrada.");

        if (request.BetCents < game.MinBet.Cents || request.BetCents > game.MaxBet.Cents)
            return Result<SpinResponseDto>.Fail($"Aposta deve estar entre R$ {game.MinBet.Reais:0.00} e R$ {game.MaxBet.Reais:0.00}.");

        var wallet = await _wallets.FindByUserIdAsync(userId, ct);
        if (wallet is null) return Result<SpinResponseDto>.Fail("Carteira não encontrada.");
        if (wallet.BalanceReal.Cents < request.BetCents)
            return Result<SpinResponseDto>.Fail("Saldo insuficiente.");

        // --- DEBITA APOSTA (transação) ---
        var bet = Money.FromCents(request.BetCents);
        wallet.BalanceReal -= bet;
        wallet.UpdatedAt = _clock.UtcNow;
        await _wallets.AddTransactionAsync(new WalletTransaction
        {
            WalletId = wallet.Id,
            Type = WalletTransactionType.Bet,
            Direction = TransactionDirection.Debit,
            Bucket = WalletBucket.Real,
            Amount = bet,
            BalanceAfter = wallet.BalanceReal,
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            RefType = "bet",
            CreatedAt = _clock.UtcNow,
        }, ct);

        // --- ROLA O SPIN ---
        var engine = new SlotEngine(slotConfig);
        var serverSeed = HmacRngStream.GenerateServerSeed();
        var clientSeed = string.IsNullOrWhiteSpace(request.ClientSeed) ? Guid.NewGuid().ToString("N")[..16] : request.ClientSeed;
        var nonce = (ulong)Interlocked.Increment(ref _nonceCounter);
        var spin = engine.Spin(bet, serverSeed, clientSeed, nonce);

        // --- CREDITA PRÊMIO (se houver) ---
        if (spin.TotalPayout.Cents > 0)
        {
            wallet.BalanceReal += spin.TotalPayout;
            wallet.UpdatedAt = _clock.UtcNow;
            await _wallets.AddTransactionAsync(new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = WalletTransactionType.Payout,
                Direction = TransactionDirection.Credit,
                Bucket = WalletBucket.Real,
                Amount = spin.TotalPayout,
                BalanceAfter = wallet.BalanceReal,
                IdempotencyKey = Guid.NewGuid().ToString("N"),
                RefType = "bet_payout",
                CreatedAt = _clock.UtcNow,
            }, ct);
        }
        await _wallets.UpdateAsync(wallet, ct);

        // --- REGISTRA APOSTA ---
        var betEntity = new Bet
        {
            UserId = userId,
            GameId = game.Id,
            Amount = bet,
            Bucket = WalletBucket.Real,
            Result = spin.TotalPayout.Cents > 0 ? BetResult.Win : BetResult.Lose,
            Payout = spin.TotalPayout,
            Multiplier = bet.Cents > 0 ? (decimal)spin.TotalPayout.Cents / bet.Cents : 0m,
            ServerSeed = Convert.ToHexString(serverSeed),
            ClientSeed = clientSeed,
            Nonce = nonce,
            CreatedAt = _clock.UtcNow,
        };
        await _bets.AddAsync(betEntity, ct);

        var dto = new SpinResponseDto(
            BetId: betEntity.Id,
            GameSlug: game.Slug,
            BetCents: bet.Cents,
            PayoutCents: spin.TotalPayout.Cents,
            Result: spin.TotalPayout.Cents > 0 ? "win" : "lose",
            Multiplier: betEntity.Multiplier,
            Grid: spin.Grid,
            WinningLines: spin.WinningLines.Select(w => new WinningLineDto(w.LineIndex, w.SymbolId, w.MatchedCount, w.PayoutCents)).ToList(),
            BalanceAfterCents: wallet.BalanceReal.Cents,
            ServerSeedHash: spin.ServerSeedHash,
            ClientSeed: spin.ClientSeed,
            Nonce: spin.Nonce
        );
        return Result<SpinResponseDto>.Ok(dto);
    }
}

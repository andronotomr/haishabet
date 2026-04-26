using Haishabet.Application.Common;
using Haishabet.Application.Persistence;
using Haishabet.Domain;
using Haishabet.Domain.Entities;

namespace Haishabet.Application.Wallet;

public sealed class WalletService : IWalletService
{
    private readonly IWalletRepository _wallets;
    private readonly IClock _clock;

    public WalletService(IWalletRepository wallets, IClock clock)
    {
        _wallets = wallets;
        _clock = clock;
    }

    public async Task<Result<WalletBalanceDto>> GetBalanceAsync(Guid userId, CancellationToken ct = default)
    {
        var wallet = await _wallets.FindByUserIdAsync(userId, ct);
        if (wallet is null) return Result<WalletBalanceDto>.Fail("Carteira não encontrada.");
        return Result<WalletBalanceDto>.Ok(ToDto(wallet));
    }

    public async Task<Result<WalletBalanceDto>> CreditDepositAsync(
        Guid userId, long amountCents, string idempotencyKey, CancellationToken ct = default)
    {
        if (amountCents <= 0) return Result<WalletBalanceDto>.Fail("Valor deve ser positivo.");
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return Result<WalletBalanceDto>.Fail("Idempotency key é obrigatório.");

        var wallet = await _wallets.FindByUserIdAsync(userId, ct);
        if (wallet is null) return Result<WalletBalanceDto>.Fail("Carteira não encontrada.");

        wallet.BalanceReal += Money.FromCents(amountCents);
        wallet.UpdatedAt = _clock.UtcNow;

        await _wallets.AddTransactionAsync(new WalletTransaction
        {
            WalletId = wallet.Id,
            Type = WalletTransactionType.Deposit,
            Direction = TransactionDirection.Credit,
            Bucket = WalletBucket.Real,
            Amount = Money.FromCents(amountCents),
            BalanceAfter = wallet.BalanceReal,
            IdempotencyKey = idempotencyKey,
            RefType = "manual_deposit",
            CreatedAt = _clock.UtcNow,
        }, ct);

        await _wallets.UpdateAsync(wallet, ct);
        return Result<WalletBalanceDto>.Ok(ToDto(wallet));
    }

    public async Task<Result<IReadOnlyList<WalletTransactionDto>>> GetExtractAsync(
        Guid userId, int take = 50, CancellationToken ct = default)
    {
        var wallet = await _wallets.FindByUserIdAsync(userId, ct);
        if (wallet is null) return Result<IReadOnlyList<WalletTransactionDto>>.Fail("Carteira não encontrada.");

        var txs = await _wallets.GetRecentTransactionsAsync(wallet.Id, take, ct);
        var dtos = txs.Select(t => new WalletTransactionDto(
            t.Id,
            t.Type.ToString(),
            t.Direction.ToString(),
            t.Amount.Cents,
            t.BalanceAfter.Cents,
            t.RefType,
            t.RefId,
            t.CreatedAt
        )).ToList();
        return Result<IReadOnlyList<WalletTransactionDto>>.Ok(dtos);
    }

    private static WalletBalanceDto ToDto(Domain.Entities.Wallet wallet) => new(
        wallet.BalanceReal.Cents,
        wallet.BalanceBonus.Cents,
        wallet.TotalAvailable.Cents,
        wallet.UpdatedAt
    );
}

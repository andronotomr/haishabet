using Haishabet.Application.Common;

namespace Haishabet.Application.Wallet;

public interface IWalletService
{
    Task<Result<WalletBalanceDto>> GetBalanceAsync(Guid userId, CancellationToken ct = default);
    Task<Result<WalletBalanceDto>> CreditDepositAsync(Guid userId, long amountCents, string idempotencyKey, CancellationToken ct = default);
    Task<Result<IReadOnlyList<WalletTransactionDto>>> GetExtractAsync(Guid userId, int take = 50, CancellationToken ct = default);
}

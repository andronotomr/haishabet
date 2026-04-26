using Haishabet.Domain.Entities;
using WalletEntity = Haishabet.Domain.Entities.Wallet;

namespace Haishabet.Application.Persistence;

public interface IWalletRepository
{
    Task<WalletEntity?> FindByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(WalletEntity wallet, CancellationToken ct = default);
    Task UpdateAsync(WalletEntity wallet, CancellationToken ct = default);
    Task AddTransactionAsync(WalletTransaction tx, CancellationToken ct = default);
    Task<IReadOnlyList<WalletTransaction>> GetRecentTransactionsAsync(Guid walletId, int take = 50, CancellationToken ct = default);
}

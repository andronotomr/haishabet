using System.Collections.Concurrent;
using Haishabet.Application.Persistence;
using Haishabet.Domain.Entities;

namespace Haishabet.Infrastructure.Persistence;

public sealed class InMemoryWalletRepository : IWalletRepository
{
    private readonly ConcurrentDictionary<Guid, Wallet> _walletsById = new();
    private readonly ConcurrentDictionary<Guid, Guid> _walletByUserId = new();   // userId → walletId
    private readonly ConcurrentDictionary<Guid, List<WalletTransaction>> _txByWalletId = new();
    private readonly object _txLock = new();

    public Task<Wallet?> FindByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        if (_walletByUserId.TryGetValue(userId, out var wid) && _walletsById.TryGetValue(wid, out var w))
            return Task.FromResult<Wallet?>(w);
        return Task.FromResult<Wallet?>(null);
    }

    public Task AddAsync(Wallet wallet, CancellationToken ct = default)
    {
        _walletsById[wallet.Id] = wallet;
        _walletByUserId[wallet.UserId] = wallet.Id;
        _txByWalletId[wallet.Id] = [];
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Wallet wallet, CancellationToken ct = default)
    {
        _walletsById[wallet.Id] = wallet;
        return Task.CompletedTask;
    }

    public Task AddTransactionAsync(WalletTransaction tx, CancellationToken ct = default)
    {
        lock (_txLock)
        {
            if (!_txByWalletId.TryGetValue(tx.WalletId, out var list))
            {
                list = [];
                _txByWalletId[tx.WalletId] = list;
            }
            list.Add(tx);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<WalletTransaction>> GetRecentTransactionsAsync(
        Guid walletId, int take = 50, CancellationToken ct = default)
    {
        if (!_txByWalletId.TryGetValue(walletId, out var list))
            return Task.FromResult<IReadOnlyList<WalletTransaction>>([]);

        IReadOnlyList<WalletTransaction> snapshot;
        lock (_txLock)
        {
            snapshot = list.OrderByDescending(t => t.CreatedAt).Take(take).ToList();
        }
        return Task.FromResult(snapshot);
    }
}

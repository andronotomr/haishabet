using System.Collections.Concurrent;
using Haishabet.Application.Persistence;
using Haishabet.Domain.Entities;

namespace Haishabet.Infrastructure.Persistence;

public sealed class InMemoryBetRepository : IBetRepository
{
    private readonly ConcurrentDictionary<Guid, Bet> _bets = new();
    private readonly ConcurrentDictionary<Guid, List<Guid>> _byUser = new();
    private readonly object _lock = new();

    public Task AddAsync(Bet bet, CancellationToken ct = default)
    {
        _bets[bet.Id] = bet;
        lock (_lock)
        {
            if (!_byUser.TryGetValue(bet.UserId, out var ids))
            {
                ids = [];
                _byUser[bet.UserId] = ids;
            }
            ids.Add(bet.Id);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Bet>> GetByUserAsync(Guid userId, int take = 50, CancellationToken ct = default)
    {
        if (!_byUser.TryGetValue(userId, out var ids))
            return Task.FromResult<IReadOnlyList<Bet>>([]);

        IReadOnlyList<Bet> snapshot;
        lock (_lock)
        {
            snapshot = ids
                .Select(id => _bets[id])
                .OrderByDescending(b => b.CreatedAt)
                .Take(take)
                .ToList();
        }
        return Task.FromResult(snapshot);
    }
}

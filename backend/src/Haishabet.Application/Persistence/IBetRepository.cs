using Haishabet.Domain.Entities;

namespace Haishabet.Application.Persistence;

public interface IBetRepository
{
    Task AddAsync(Bet bet, CancellationToken ct = default);
    Task<IReadOnlyList<Bet>> GetByUserAsync(Guid userId, int take = 50, CancellationToken ct = default);
}

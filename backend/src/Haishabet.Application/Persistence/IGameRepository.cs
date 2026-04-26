using Haishabet.Domain.Entities;

namespace Haishabet.Application.Persistence;

public interface IGameRepository
{
    Task<IReadOnlyList<GameInfo>> GetActiveAsync(CancellationToken ct = default);
    Task<GameInfo?> FindBySlugAsync(string slug, CancellationToken ct = default);
    Task<GameInfo?> FindByIdAsync(Guid id, CancellationToken ct = default);
}

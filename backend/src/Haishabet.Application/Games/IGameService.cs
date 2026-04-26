using Haishabet.Application.Common;

namespace Haishabet.Application.Games;

public interface IGameService
{
    Task<Result<IReadOnlyList<GameSummaryDto>>> GetCatalogAsync(CancellationToken ct = default);
    Task<Result<SpinResponseDto>> SpinAsync(Guid userId, SpinRequest request, CancellationToken ct = default);
}

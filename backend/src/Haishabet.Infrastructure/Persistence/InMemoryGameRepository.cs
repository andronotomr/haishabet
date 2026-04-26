using System.Collections.Concurrent;
using Haishabet.Application.Persistence;
using Haishabet.Domain.Entities;

namespace Haishabet.Infrastructure.Persistence;

/// <summary>
/// Catálogo in-memory pré-populado com os jogos do MVP.
/// </summary>
public sealed class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, GameInfo> _byId = new();
    private readonly ConcurrentDictionary<string, Guid> _bySlug = new();

    public InMemoryGameRepository()
    {
        Seed();
    }

    private void Seed()
    {
        var tigre = new GameInfo
        {
            Slug = "tigre-da-fortuna",
            Name = "Tigre da Fortuna",
            Type = GameType.Slot,
            Provider = "inhouse",
            MinBet = Domain.Money.FromReais(1),
            MaxBet = Domain.Money.FromReais(500),
            RtpTarget = 0.96m,
            Volatility = Volatility.High,
            Active = true,
            ThumbnailUrl = "/games/tigre-da-fortuna.png",
        };
        _byId[tigre.Id] = tigre;
        _bySlug[tigre.Slug] = tigre.Id;
    }

    public Task<IReadOnlyList<GameInfo>> GetActiveAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<GameInfo>>(_byId.Values.Where(g => g.Active).ToList());

    public Task<GameInfo?> FindBySlugAsync(string slug, CancellationToken ct = default)
        => Task.FromResult(_bySlug.TryGetValue(slug, out var id) && _byId.TryGetValue(id, out var g) ? g : null);

    public Task<GameInfo?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_byId.TryGetValue(id, out var g) ? g : null);
}

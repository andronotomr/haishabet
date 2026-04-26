using System.Collections.Concurrent;
using Haishabet.Application.Persistence;
using Haishabet.Domain.Entities;

namespace Haishabet.Infrastructure.Persistence;

/// <summary>
/// Storage in-memory para MVP. Substituir por implementação EF Core quando Postgres
/// estiver disponível. Thread-safe via ConcurrentDictionary.
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _byId = new();
    private readonly ConcurrentDictionary<string, Guid> _byEmail = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Guid> _byCpf = new();

    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        if (_byEmail.TryGetValue(email, out var id) && _byId.TryGetValue(id, out var user))
            return Task.FromResult<User?>(user);
        return Task.FromResult<User?>(null);
    }

    public Task<User?> FindByCpfAsync(string cpf, CancellationToken ct = default)
    {
        if (_byCpf.TryGetValue(cpf, out var id) && _byId.TryGetValue(id, out var user))
            return Task.FromResult<User?>(user);
        return Task.FromResult<User?>(null);
    }

    public Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_byId.TryGetValue(id, out var u) ? u : null);

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        _byId[user.Id] = user;
        _byEmail[user.Email] = user.Id;
        _byCpf[user.Cpf.Value] = user.Id;
        return Task.CompletedTask;
    }
}

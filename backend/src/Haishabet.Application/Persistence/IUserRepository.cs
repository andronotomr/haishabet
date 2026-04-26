using Haishabet.Domain.Entities;

namespace Haishabet.Application.Persistence;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> FindByCpfAsync(string cpf, CancellationToken ct = default);
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}

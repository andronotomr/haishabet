using Haishabet.Application.Auth;

namespace Haishabet.Infrastructure.Auth;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;   // ~250ms hash, balanceia segurança × UX

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch { return false; }
    }
}

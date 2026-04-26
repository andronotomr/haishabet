using Haishabet.Domain.Entities;

namespace Haishabet.Application.Auth;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) Issue(User user);
}

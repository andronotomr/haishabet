using Haishabet.Application.Common;

namespace Haishabet.Application.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

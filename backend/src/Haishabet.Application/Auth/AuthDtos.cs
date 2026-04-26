namespace Haishabet.Application.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string Cpf,
    string FullName,
    DateOnly BirthDate
);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    UserDto User
);

public sealed record UserDto(
    Guid Id,
    string Email,
    string FullName,
    string CpfMasked
);

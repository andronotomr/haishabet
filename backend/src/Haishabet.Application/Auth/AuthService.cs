using Haishabet.Application.Common;
using Haishabet.Application.Persistence;
using Haishabet.Domain;
using Haishabet.Domain.Entities;

namespace Haishabet.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IWalletRepository _wallets;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly IClock _clock;

    public AuthService(
        IUserRepository users,
        IWalletRepository wallets,
        IPasswordHasher hasher,
        IJwtTokenService jwt,
        IClock clock)
    {
        _users = users;
        _wallets = wallets;
        _hasher = hasher;
        _jwt = jwt;
        _clock = clock;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<AuthResponse>.Fail("E-mail é obrigatório.");
        if (request.Password is null || request.Password.Length < 8)
            return Result<AuthResponse>.Fail("Senha precisa de no mínimo 8 caracteres.");
        if (!Cpf.TryParse(request.Cpf, out var cpf))
            return Result<AuthResponse>.Fail("CPF inválido.");
        if (string.IsNullOrWhiteSpace(request.FullName))
            return Result<AuthResponse>.Fail("Nome completo é obrigatório.");

        var emailNorm = request.Email.Trim().ToLowerInvariant();

        if (await _users.FindByEmailAsync(emailNorm, ct) is not null)
            return Result<AuthResponse>.Fail("E-mail já cadastrado.");
        if (await _users.FindByCpfAsync(cpf.Value, ct) is not null)
            return Result<AuthResponse>.Fail("CPF já cadastrado.");

        var user = new User
        {
            Email = emailNorm,
            PasswordHash = _hasher.Hash(request.Password),
            Cpf = cpf,
            FullName = request.FullName.Trim(),
            BirthDate = request.BirthDate,
        };

        if (!user.IsAdult(_clock.Today))
            return Result<AuthResponse>.Fail("Cadastro permitido apenas para maiores de 18 anos.");

        await _users.AddAsync(user, ct);
        await _wallets.AddAsync(new Domain.Entities.Wallet { UserId = user.Id }, ct);

        return Result<AuthResponse>.Ok(BuildAuthResponse(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<AuthResponse>.Fail("Credenciais inválidas.");

        var user = await _users.FindByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Fail("Credenciais inválidas.");

        if (user.Status != UserStatus.Active)
            return Result<AuthResponse>.Fail("Conta inativa ou bloqueada.");

        user.LastLoginAt = _clock.UtcNow;
        return Result<AuthResponse>.Ok(BuildAuthResponse(user));
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var (token, exp) = _jwt.Issue(user);
        var dto = new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Cpf.Value[..3] + "*****" + user.Cpf.Value[^2..]
        );
        return new AuthResponse(token, exp, dto);
    }
}

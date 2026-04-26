using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Haishabet.Application.Auth;
using Haishabet.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Haishabet.Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings) => _settings = settings.Value;

    public (string Token, DateTimeOffset ExpiresAt) Issue(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.AccessSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_settings.AccessMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

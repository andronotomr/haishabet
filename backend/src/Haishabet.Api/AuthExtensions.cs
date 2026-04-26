using System.Text;
using Haishabet.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Haishabet.Api;

public static class AuthExtensions
{
    public static IServiceCollection AddHaishabetJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var jwt = config.GetSection("Jwt").Get<JwtSettings>()
                  ?? throw new InvalidOperationException("Configuração 'Jwt' ausente em appsettings.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.AccessSecret));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization();
        return services;
    }
}

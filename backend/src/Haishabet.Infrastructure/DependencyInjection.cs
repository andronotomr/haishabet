using Haishabet.Application.Auth;
using Haishabet.Application.Common;
using Haishabet.Application.Games;
using Haishabet.Application.Persistence;
using Haishabet.Application.Wallet;
using Haishabet.GameEngine.Slot;
using Haishabet.GameEngine.Slot.Catalog;
using Haishabet.Infrastructure.Auth;
using Haishabet.Infrastructure.Persistence;
using Haishabet.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Haishabet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHaishabet(this IServiceCollection services, IConfiguration config)
    {
        // --- Settings ---
        services.Configure<JwtSettings>(config.GetSection("Jwt"));

        // --- Repos in-memory (TODO: trocar por EF Core quando Postgres estiver no ar) ---
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<IWalletRepository, InMemoryWalletRepository>();
        services.AddSingleton<IBetRepository, InMemoryBetRepository>();
        services.AddSingleton<IGameRepository, InMemoryGameRepository>();

        // --- Auth ---
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // --- Common ---
        services.AddSingleton<IClock, SystemClock>();

        // --- Slot configs (catálogo de matemática dos jogos) ---
        var slotConfigs = new Dictionary<string, SlotConfig>(StringComparer.OrdinalIgnoreCase)
        {
            [TigreDaFortunaConfig.Slug] = TigreDaFortunaConfig.Create(),
        };
        services.AddSingleton<IReadOnlyDictionary<string, SlotConfig>>(slotConfigs);

        // --- Application services ---
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IGameService, GameService>();

        return services;
    }
}

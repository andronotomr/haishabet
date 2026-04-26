using Haishabet.Domain;
using Haishabet.GameEngine.Rng;

namespace Haishabet.GameEngine.Slot;

/// <summary>
/// Roda N spins simulados com o mesmo bet e mede RTP, hit rate, max win, distribuição.
/// Usado em testes pra validar que pesos e paytable batem com RTP alvo.
/// </summary>
public sealed class SlotSimulator
{
    private readonly SlotEngine _engine;

    public SlotSimulator(SlotEngine engine) => _engine = engine;

    public SimulationResult Run(Money bet, int spins, byte[]? serverSeed = null, string clientSeed = "sim")
    {
        if (spins <= 0) throw new ArgumentOutOfRangeException(nameof(spins));
        serverSeed ??= HmacRngStream.GenerateServerSeed();

        long totalBet = 0;
        long totalPayout = 0;
        long maxPayout = 0;
        int hits = 0;
        var payoutBuckets = new Dictionary<int, int>(); // multiplier (rounded) → count

        for (ulong nonce = 0; nonce < (ulong)spins; nonce++)
        {
            var result = _engine.Spin(bet, serverSeed, clientSeed, nonce);
            totalBet += bet.Cents;
            totalPayout += result.TotalPayout.Cents;
            if (result.TotalPayout.Cents > 0)
            {
                hits++;
                if (result.TotalPayout.Cents > maxPayout) maxPayout = result.TotalPayout.Cents;
                int mult = (int)(result.TotalPayout.Cents / bet.Cents);
                payoutBuckets[mult] = payoutBuckets.GetValueOrDefault(mult) + 1;
            }
        }

        return new SimulationResult(
            Spins: spins,
            TotalBet: Money.FromCents(totalBet),
            TotalPayout: Money.FromCents(totalPayout),
            Rtp: (decimal)totalPayout / totalBet,
            HitRate: (decimal)hits / spins,
            MaxPayout: Money.FromCents(maxPayout),
            MaxPayoutMultiplier: bet.Cents > 0 ? (decimal)maxPayout / bet.Cents : 0m,
            PayoutMultiplierDistribution: payoutBuckets
        );
    }
}

public sealed record SimulationResult(
    int Spins,
    Money TotalBet,
    Money TotalPayout,
    decimal Rtp,
    decimal HitRate,
    Money MaxPayout,
    decimal MaxPayoutMultiplier,
    IReadOnlyDictionary<int, int> PayoutMultiplierDistribution
);

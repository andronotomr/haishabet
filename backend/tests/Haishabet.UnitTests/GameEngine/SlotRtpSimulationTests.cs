using FluentAssertions;
using Haishabet.Domain;
using Haishabet.GameEngine.Slot;
using Haishabet.GameEngine.Slot.Catalog;
using Xunit;
using Xunit.Abstractions;

namespace Haishabet.UnitTests.GameEngine;

/// <summary>
/// Roda simulações em massa pra validar RTP, hit rate e distribuição da paytable.
/// Esses testes substituem (em parte) o que uma certificadora externa faria.
/// </summary>
public class SlotRtpSimulationTests
{
    private readonly ITestOutputHelper _out;

    public SlotRtpSimulationTests(ITestOutputHelper output) => _out = output;

    [Fact]
    public void TigreDaFortuna_200k_spins_RTP_within_target_band()
    {
        var cfg = TigreDaFortunaConfig.Create();
        var engine = new SlotEngine(cfg);
        var sim = new SlotSimulator(engine);

        var result = sim.Run(bet: Money.FromReais(1), spins: 200_000);

        _out.WriteLine($"Spins:              {result.Spins:N0}");
        _out.WriteLine($"Total bet:          {result.TotalBet}");
        _out.WriteLine($"Total payout:       {result.TotalPayout}");
        _out.WriteLine($"RTP medido:         {result.Rtp:P3}");
        _out.WriteLine($"RTP alvo (config):  {cfg.RtpTarget:P2}");
        _out.WriteLine($"Hit rate:           {result.HitRate:P2}");
        _out.WriteLine($"Max payout:         {result.MaxPayout}  ({result.MaxPayoutMultiplier}x)");
        _out.WriteLine("");
        _out.WriteLine("Distribuição de prêmios (multiplier sobre bet → contagem):");
        foreach (var (mult, count) in result.PayoutMultiplierDistribution.OrderBy(k => k.Key))
            _out.WriteLine($"  {mult,5}x → {count,7:N0}");

        // RTP deve estar dentro de ±3% do alvo (variância natural com 200k giros).
        // Em produção com 100M giros, banda apertaria pra ±0.5%.
        result.Rtp.Should().BeInRange(cfg.RtpTarget - 0.03m, cfg.RtpTarget + 0.03m,
            "RTP medido deve convergir para o alvo configurado");

        // Hit rate de slot saudável fica entre 15-40%.
        result.HitRate.Should().BeInRange(0.10m, 0.50m,
            "hit rate fora do esperado — paytable pode estar quebrada");
    }

    [Fact]
    public void Same_seeds_produce_same_RTP()
    {
        var cfg = TigreDaFortunaConfig.Create();
        var engine = new SlotEngine(cfg);
        var sim = new SlotSimulator(engine);
        var seed = new byte[32];
        for (int i = 0; i < 32; i++) seed[i] = (byte)i;

        var r1 = sim.Run(Money.FromReais(1), spins: 5000, serverSeed: seed, clientSeed: "fixed");
        var r2 = sim.Run(Money.FromReais(1), spins: 5000, serverSeed: seed, clientSeed: "fixed");

        r1.TotalPayout.Should().Be(r2.TotalPayout);
        r1.Rtp.Should().Be(r2.Rtp);
    }
}

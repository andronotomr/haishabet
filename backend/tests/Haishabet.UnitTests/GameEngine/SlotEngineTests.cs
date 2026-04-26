using System.Text;
using FluentAssertions;
using Haishabet.Domain;
using Haishabet.GameEngine.Rng;
using Haishabet.GameEngine.Slot;
using Haishabet.GameEngine.Slot.Catalog;
using Xunit;

namespace Haishabet.UnitTests.GameEngine;

public class SlotEngineTests
{
    private static byte[] Seed(string s) => Encoding.UTF8.GetBytes(s);

    [Fact]
    public void Spin_with_same_inputs_is_deterministic()
    {
        var engine = new SlotEngine(TigreDaFortunaConfig.Create());
        var seed = Seed("server-deterministico-32-bytes-xxx");

        var r1 = engine.Spin(Money.FromReais(1), seed, "client", nonce: 99);
        var r2 = engine.Spin(Money.FromReais(1), seed, "client", nonce: 99);

        r1.TotalPayout.Should().Be(r2.TotalPayout);
        for (int col = 0; col < r1.Grid.Length; col++)
            r1.Grid[col].Should().BeEquivalentTo(r2.Grid[col]);
    }

    [Fact]
    public void Spin_grid_has_correct_dimensions()
    {
        var cfg = TigreDaFortunaConfig.Create();
        var engine = new SlotEngine(cfg);
        var r = engine.Spin(Money.FromReais(1), Seed("k"), "c", 0);

        r.Grid.Length.Should().Be(cfg.Reels);
        foreach (var col in r.Grid)
            col.Length.Should().Be(cfg.Rows);
    }

    [Fact]
    public void Spin_payout_never_exceeds_max_win_cap()
    {
        var cfg = TigreDaFortunaConfig.Create();
        var engine = new SlotEngine(cfg);
        long capCents = Money.FromReais(1).Cents * cfg.MaxWinMultiplier;

        for (ulong nonce = 0; nonce < 1000; nonce++)
        {
            var r = engine.Spin(Money.FromReais(1), Seed("test"), "c", nonce);
            r.TotalPayout.Cents.Should().BeLessThanOrEqualTo(capCents);
        }
    }

    [Fact]
    public void Spin_payout_uses_paytable_correctly()
    {
        // Config minimal: 3 colunas, 1 linha, 1 payline, 1 símbolo (só ele) → todo spin paga
        var cfg = new SlotConfig(
            Slug: "test",
            Reels: 3,
            Rows: 1,
            Symbols: [
                new SlotSymbol("X", "X", Weight: 1, Payouts: new Dictionary<int, int> { [3] = 10 })
            ],
            Paylines: [[0, 0, 0]],
            RtpTarget: 0m,
            MaxWinMultiplier: 1000
        );
        var engine = new SlotEngine(cfg);
        var bet = Money.FromReais(2);    // 200 cents
        var result = engine.Spin(bet, Seed("k"), "c", 0);

        // 3 X em linha → paga 10x bet → 2000 cents
        result.TotalPayout.Cents.Should().Be(2000);
        result.WinningLines.Should().HaveCount(1);
        result.WinningLines[0].SymbolId.Should().Be("X");
        result.WinningLines[0].MatchedCount.Should().Be(3);
    }

    [Fact]
    public void Wild_substitutes_other_symbols()
    {
        // Config: 3 colunas, 1 linha. Wild W, símbolo S paga 5x quando 3.
        var cfg = new SlotConfig(
            Slug: "test-wild",
            Reels: 3,
            Rows: 1,
            Symbols: [
                new SlotSymbol("W", "Wild", Weight: 1, IsWild: true),
                new SlotSymbol("S", "S",    Weight: 1, Payouts: new Dictionary<int, int> { [3] = 5 }),
            ],
            Paylines: [[0, 0, 0]],
            RtpTarget: 0m,
            MaxWinMultiplier: 1000
        );

        // Roda muitos spins: deve ter casos onde wild substitui S.
        var engine = new SlotEngine(cfg);
        var bet = Money.FromReais(1);
        bool sawWildAssistedWin = false;
        for (ulong n = 0; n < 5000; n++)
        {
            var r = engine.Spin(bet, Seed("wildtest"), "c", n);
            if (r.WinningLines.Count > 0 && r.WinningLines[0].SymbolId == "S")
            {
                // Se houve win em S e o grid tem pelo menos um W, foi wild-assisted
                if (r.Grid.Any(col => col[0] == "W")) { sawWildAssistedWin = true; break; }
            }
        }
        sawWildAssistedWin.Should().BeTrue("wild deve substituir S em pelo menos uma vitória nas 5000 amostras");
    }
}

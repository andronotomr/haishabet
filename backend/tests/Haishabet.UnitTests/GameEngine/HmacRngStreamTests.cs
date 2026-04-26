using System.Text;
using FluentAssertions;
using Haishabet.GameEngine.Rng;
using Xunit;

namespace Haishabet.UnitTests.GameEngine;

public class HmacRngStreamTests
{
    private static byte[] Seed(string s) => Encoding.UTF8.GetBytes(s);

    [Fact]
    public void Same_seed_and_nonce_give_identical_stream()
    {
        var s1 = new HmacRngStream(Seed("server-x"), "client-y", nonce: 42);
        var s2 = new HmacRngStream(Seed("server-x"), "client-y", nonce: 42);

        for (int i = 0; i < 100; i++)
            s1.NextUInt32().Should().Be(s2.NextUInt32(), "mesma seed/nonce devem dar mesma sequência");
    }

    [Fact]
    public void Different_nonce_gives_different_stream()
    {
        var s1 = new HmacRngStream(Seed("server"), "client", nonce: 1);
        var s2 = new HmacRngStream(Seed("server"), "client", nonce: 2);

        var firstA = s1.NextUInt32();
        var firstB = s2.NextUInt32();
        firstA.Should().NotBe(firstB);
    }

    [Fact]
    public void NextInt_in_range()
    {
        var rng = new HmacRngStream(Seed("k"), "c", 0);
        for (int i = 0; i < 1000; i++)
        {
            var v = rng.NextInt(10);
            v.Should().BeInRange(0, 9);
        }
    }

    [Fact]
    public void NextInt_distribution_is_roughly_uniform()
    {
        var rng = new HmacRngStream(Seed("dist"), "c", 0);
        var counts = new int[10];
        for (int i = 0; i < 100_000; i++)
            counts[rng.NextInt(10)]++;

        // Cada bucket deve estar dentro de ±10% do esperado (10000)
        foreach (var c in counts)
            c.Should().BeInRange(9000, 11000);
    }

    [Fact]
    public void NextDouble_in_unit_interval()
    {
        var rng = new HmacRngStream(Seed("d"), "c", 0);
        for (int i = 0; i < 1000; i++)
        {
            var d = rng.NextDouble();
            d.Should().BeGreaterThanOrEqualTo(0).And.BeLessThan(1);
        }
    }

    [Fact]
    public void GenerateServerSeed_is_32_bytes()
    {
        HmacRngStream.GenerateServerSeed().Length.Should().Be(32);
    }

    [Fact]
    public void HashServerSeed_is_64_hex_chars()
    {
        var seed = HmacRngStream.GenerateServerSeed();
        var hash = HmacRngStream.HashServerSeed(seed);
        hash.Length.Should().Be(64);
        hash.All("0123456789abcdef".Contains).Should().BeTrue();
    }
}

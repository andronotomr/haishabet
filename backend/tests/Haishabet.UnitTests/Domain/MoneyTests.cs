using FluentAssertions;
using Haishabet.Domain;
using Xunit;

namespace Haishabet.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void FromReais_converts_to_cents()
    {
        Money.FromReais(10.50m).Cents.Should().Be(1050);
        Money.FromReais(0.01m).Cents.Should().Be(1);
        Money.FromReais(0).Cents.Should().Be(0);
    }

    [Fact]
    public void Reais_property_round_trips()
    {
        Money.FromCents(2350).Reais.Should().Be(23.50m);
    }

    [Theory]
    [InlineData(100, 250, 350)]
    [InlineData(0, 1, 1)]
    [InlineData(-50, 100, 50)]
    public void Add_sums_cents(long a, long b, long expected)
    {
        (Money.FromCents(a) + Money.FromCents(b)).Cents.Should().Be(expected);
    }

    [Fact]
    public void Subtract_can_go_negative()
    {
        (Money.FromCents(100) - Money.FromCents(150)).Cents.Should().Be(-50);
    }

    [Fact]
    public void Multiply_by_long()
    {
        (Money.FromCents(100) * 7).Cents.Should().Be(700);
    }

    [Fact]
    public void Comparisons_work()
    {
        (Money.FromCents(100) > Money.FromCents(50)).Should().BeTrue();
        (Money.FromCents(50) < Money.FromCents(100)).Should().BeTrue();
        (Money.FromCents(100) >= Money.FromCents(100)).Should().BeTrue();
    }

    [Fact]
    public void ToString_formats_BR()
    {
        Money.FromReais(1234.56m).ToString().Should().Be("R$ 1234.56");
        Money.FromCents(50).ToString().Should().Be("R$ 0.50");
    }

    [Fact]
    public void Records_with_same_cents_are_equal()
    {
        Money.FromCents(100).Should().Be(Money.FromCents(100));
        Money.FromReais(1).Should().Be(Money.FromCents(100));
    }
}

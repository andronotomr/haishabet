using FluentAssertions;
using Haishabet.Domain;
using Xunit;

namespace Haishabet.UnitTests.Domain;

public class CpfTests
{
    // CPFs válidos (com checksum correto, públicos / dummies de teste)
    [Theory]
    [InlineData("11144477735")]   // CPF de teste comum
    [InlineData("111.444.777-35")]
    [InlineData("529.982.247-25")]
    public void Valid_cpf_parses(string raw)
    {
        Cpf.TryParse(raw, out var cpf).Should().BeTrue();
        cpf.Value.Length.Should().Be(11);
        cpf.Value.All(char.IsDigit).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123")]
    [InlineData("00000000000")]            // todos iguais
    [InlineData("11111111111")]
    [InlineData("12345678900")]            // checksum inválido
    [InlineData("111.444.777-30")]         // checksum inválido (ult dig errado)
    public void Invalid_cpf_rejected(string? raw)
    {
        Cpf.TryParse(raw, out _).Should().BeFalse();
    }

    [Fact]
    public void Formatted_output()
    {
        var cpf = Cpf.Parse("11144477735");
        cpf.Formatted().Should().Be("111.444.777-35");
    }
}

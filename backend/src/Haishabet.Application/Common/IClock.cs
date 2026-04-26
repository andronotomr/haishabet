namespace Haishabet.Application.Common;

/// <summary>Abstração de relógio para permitir testes determinísticos.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
    DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);
}

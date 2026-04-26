namespace Haishabet.Domain.Entities;

/// <summary>
/// Metadata do jogo no catálogo. A configuração matemática (símbolos, pesos, paytable)
/// vive na camada GameEngine — aqui só o catálogo.
/// </summary>
public sealed class GameInfo
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Slug { get; init; }      // "tigre-da-fortuna"
    public required string Name { get; init; }      // "Tigre da Fortuna"
    public required GameType Type { get; init; }
    public required string Provider { get; init; }  // "inhouse" ou nome do agregador
    public required Money MinBet { get; init; }
    public required Money MaxBet { get; init; }
    public decimal RtpTarget { get; init; }
    public Volatility Volatility { get; init; }
    public bool Active { get; set; } = true;
    public string? ThumbnailUrl { get; set; }
}

public enum GameType
{
    Slot = 0,
    Crash = 1,
    Mines = 2,
    Plinko = 3,
    Scratch = 4,
}

public enum Volatility
{
    Low = 0,
    Medium = 1,
    High = 2,
    VeryHigh = 3,
}

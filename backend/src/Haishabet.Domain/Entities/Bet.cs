namespace Haishabet.Domain.Entities;

public sealed class Bet
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
    public required Guid GameId { get; init; }
    public required Money Amount { get; init; }
    public required WalletBucket Bucket { get; init; }
    public required BetResult Result { get; init; }
    public required Money Payout { get; init; }
    public decimal Multiplier { get; init; }
    public string? ServerSeed { get; init; }
    public string? ClientSeed { get; init; }
    public ulong Nonce { get; init; }
    public string? DetailsJson { get; init; }   // resultado serializado (grid, paylines etc.)
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public enum BetResult
{
    Win = 0,
    Lose = 1,
    Push = 2,
}

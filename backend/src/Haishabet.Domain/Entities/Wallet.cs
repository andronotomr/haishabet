namespace Haishabet.Domain.Entities;

/// <summary>
/// Carteira do usuário com 3 buckets de saldo (real, bônus, locked).
/// Saldos são calculados a partir do ledger de wallet_transactions —
/// estes campos são projeção materializada para leitura rápida.
/// </summary>
public sealed class Wallet
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
    public Money BalanceReal { get; set; } = Money.Zero;
    public Money BalanceBonus { get; set; } = Money.Zero;
    public Money BalanceLocked { get; set; } = Money.Zero;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Money TotalAvailable => BalanceReal + BalanceBonus;
}

public enum WalletBucket
{
    Real = 0,
    Bonus = 1,
    Locked = 2,
}

namespace Haishabet.Domain.Entities;

/// <summary>
/// Lançamento no ledger da carteira. IMUTÁVEL — uma vez gravado, nunca alterado.
/// Saldo da Wallet é projeção da soma dessas transações por bucket.
/// </summary>
public sealed class WalletTransaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid WalletId { get; init; }
    public required WalletTransactionType Type { get; init; }
    public required WalletBucket Bucket { get; init; }
    public required Money Amount { get; init; }   // sempre positivo; tipo + direção decidem sinal
    public required TransactionDirection Direction { get; init; }
    public Money BalanceAfter { get; init; }
    public string? RefType { get; init; }     // "deposit", "bet", "withdrawal", "admin"
    public Guid? RefId { get; init; }
    public required string IdempotencyKey { get; init; }
    public string CreatedBy { get; init; } = "system";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public enum WalletTransactionType
{
    Deposit = 0,
    Withdrawal = 1,
    Bet = 2,
    Payout = 3,
    Bonus = 4,
    Adjustment = 5,
    Refund = 6,
    Rollback = 7,
}

public enum TransactionDirection
{
    Credit = 0,   // entra dinheiro no bucket
    Debit = 1,    // sai dinheiro do bucket
}

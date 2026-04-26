namespace Haishabet.Application.Wallet;

public sealed record WalletBalanceDto(
    long BalanceRealCents,
    long BalanceBonusCents,
    long TotalAvailableCents,
    DateTimeOffset UpdatedAt
);

public sealed record WalletTransactionDto(
    Guid Id,
    string Type,           // Deposit, Bet, Payout, etc.
    string Direction,      // Credit / Debit
    long AmountCents,
    long BalanceAfterCents,
    string? RefType,
    Guid? RefId,
    DateTimeOffset CreatedAt
);

public sealed record DepositRequest(long AmountCents);

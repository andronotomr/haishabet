namespace Haishabet.GameEngine.Slot;

/// <summary>
/// Configuração matemática de um slot. Define grid, símbolos, pesos, paylines,
/// RTP alvo e teto de prêmio. A tabela de pagamentos vive nos próprios símbolos.
/// </summary>
public sealed record SlotConfig(
    string Slug,
    int Reels,
    int Rows,
    IReadOnlyList<SlotSymbol> Symbols,
    IReadOnlyList<int[]> Paylines,    // cada payline = [linha em col0, linha em col1, ...]
    decimal RtpTarget,
    int MaxWinMultiplier              // teto: payout não passa de bet * MaxWinMultiplier
);

namespace Haishabet.GameEngine.Slot;

/// <summary>
/// Símbolo de um slot. Wild substitui qualquer outro ao formar linhas.
/// </summary>
public sealed record SlotSymbol(
    string Id,
    string Name,
    int Weight,
    IReadOnlyDictionary<int, int>? Payouts = null,   // count→multiplier (3→250 = paga 250x bet)
    bool IsWild = false
);

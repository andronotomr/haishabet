using Haishabet.Domain;

namespace Haishabet.GameEngine.Slot;

public sealed record SpinResult(
    string[][] Grid,                              // [col][row], strings = symbol IDs
    Money TotalPayout,
    IReadOnlyList<WinningLine> WinningLines,
    string ServerSeedHash,
    string ClientSeed,
    ulong Nonce
);

public sealed record WinningLine(
    int LineIndex,
    string SymbolId,
    int MatchedCount,
    long PayoutCents
);

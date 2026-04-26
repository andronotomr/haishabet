namespace Haishabet.Application.Games;

public sealed record GameSummaryDto(
    Guid Id,
    string Slug,
    string Name,
    string Type,
    string Provider,
    long MinBetCents,
    long MaxBetCents,
    decimal Rtp,
    string Volatility,
    string? ThumbnailUrl
);

public sealed record SpinRequest(string GameSlug, long BetCents, string? ClientSeed = null);

public sealed record SpinResponseDto(
    Guid BetId,
    string GameSlug,
    long BetCents,
    long PayoutCents,
    string Result,                    // "win" / "lose"
    decimal Multiplier,
    string[][] Grid,                  // [col][row]
    IReadOnlyList<WinningLineDto> WinningLines,
    long BalanceAfterCents,
    string ServerSeedHash,
    string ClientSeed,
    ulong Nonce
);

public sealed record WinningLineDto(int LineIndex, string SymbolId, int MatchedCount, long PayoutCents);

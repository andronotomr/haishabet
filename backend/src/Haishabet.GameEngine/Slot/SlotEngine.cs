using Haishabet.Domain;
using Haishabet.GameEngine.Rng;

namespace Haishabet.GameEngine.Slot;

/// <summary>
/// Motor de slot puro. Sem dependência de I/O. Recebe config + aposta + RNG e devolve resultado.
/// Determinístico para mesmo (seed, nonce). Pronto para simulação em massa e testes.
/// </summary>
public sealed class SlotEngine
{
    private readonly SlotConfig _config;
    private readonly int _totalWeight;
    private readonly int[] _cumulativeWeights;
    private readonly string? _wildId;

    public SlotEngine(SlotConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;

        int total = 0;
        _cumulativeWeights = new int[config.Symbols.Count];
        for (int i = 0; i < config.Symbols.Count; i++)
        {
            if (config.Symbols[i].Weight <= 0)
                throw new ArgumentException($"Símbolo {config.Symbols[i].Id} tem peso inválido.");
            total += config.Symbols[i].Weight;
            _cumulativeWeights[i] = total;
        }
        _totalWeight = total;
        _wildId = config.Symbols.FirstOrDefault(s => s.IsWild)?.Id;
    }

    public SlotConfig Config => _config;

    public SpinResult Spin(Money bet, byte[] serverSeed, string clientSeed, ulong nonce)
    {
        ArgumentNullException.ThrowIfNull(serverSeed);
        if (bet.Cents <= 0) throw new ArgumentException("Aposta deve ser positiva.", nameof(bet));

        var rng = new HmacRngStream(serverSeed, clientSeed, nonce);
        var grid = GenerateGrid(rng);

        long totalPayout = 0;
        var winningLines = new List<WinningLine>();

        for (int i = 0; i < _config.Paylines.Count; i++)
        {
            var line = _config.Paylines[i];
            var (symbolId, count) = EvaluateLine(grid, line);
            if (symbolId == null || count < 3) continue;

            var symbol = _config.Symbols.First(s => s.Id == symbolId);
            if (symbol.Payouts == null) continue;
            if (!symbol.Payouts.TryGetValue(count, out int multiplier)) continue;

            long linePayout = bet.Cents * multiplier;
            totalPayout += linePayout;
            winningLines.Add(new WinningLine(i, symbolId, count, linePayout));
        }

        // Cap em max win
        long cap = bet.Cents * _config.MaxWinMultiplier;
        if (totalPayout > cap) totalPayout = cap;

        return new SpinResult(
            Grid: grid,
            TotalPayout: Money.FromCents(totalPayout),
            WinningLines: winningLines,
            ServerSeedHash: HmacRngStream.HashServerSeed(serverSeed),
            ClientSeed: clientSeed,
            Nonce: nonce
        );
    }

    private string[][] GenerateGrid(IRngStream rng)
    {
        var grid = new string[_config.Reels][];
        for (int col = 0; col < _config.Reels; col++)
        {
            grid[col] = new string[_config.Rows];
            for (int row = 0; row < _config.Rows; row++)
                grid[col][row] = PickSymbol(rng);
        }
        return grid;
    }

    private string PickSymbol(IRngStream rng)
    {
        int roll = rng.NextInt(_totalWeight);
        for (int i = 0; i < _cumulativeWeights.Length; i++)
            if (roll < _cumulativeWeights[i]) return _config.Symbols[i].Id;
        return _config.Symbols[^1].Id;
    }

    /// <summary>
    /// Avalia uma payline da esquerda para a direita.
    /// Retorna o símbolo "ativo" (primeiro não-wild, ou wild se todos forem wild)
    /// e quantos símbolos consecutivos do início da linha batem (wild conta como qualquer).
    /// </summary>
    private (string? symbolId, int count) EvaluateLine(string[][] grid, int[] line)
    {
        if (line.Length != _config.Reels)
            throw new InvalidOperationException("Payline com tamanho diferente do número de colunas.");

        // Coleta os símbolos na linha
        var lineSymbols = new string[_config.Reels];
        for (int col = 0; col < _config.Reels; col++)
            lineSymbols[col] = grid[col][line[col]];

        // Acha o símbolo ativo (primeiro não-wild, ou wild se todos forem wild)
        string? active = null;
        for (int i = 0; i < _config.Reels; i++)
        {
            if (lineSymbols[i] != _wildId)
            {
                active = lineSymbols[i];
                break;
            }
        }
        active ??= _wildId;
        if (active == null) return (null, 0);

        // Conta a partir da esquerda, parando no primeiro mismatch.
        int count = 0;
        for (int col = 0; col < _config.Reels; col++)
        {
            if (lineSymbols[col] == active || lineSymbols[col] == _wildId) count++;
            else break;
        }
        return (active, count);
    }
}

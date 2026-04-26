namespace Haishabet.GameEngine.Slot.Catalog;

/// <summary>
/// Configuração do "Tigre da Fortuna" — slot 3x3 inspirado no Fortune Tiger.
/// Pesos calibrados para RTP ~96% via simulação de 1M giros (ver SlotRtpSimulationTests).
/// </summary>
public static class TigreDaFortunaConfig
{
    public const string Slug = "tigre-da-fortuna";

    public static SlotConfig Create()
    {
        var symbols = new SlotSymbol[]
        {
            // Wild — substitui qualquer símbolo. Não tem payout próprio.
            new(Id: "T", Name: "Tigre",    Weight: 2,  IsWild: true),

            // Símbolos de pagamento (count → multiplier sobre a bet)
            // Paytable calibrada por simulação para RTP ~96% com 5 paylines + wild substituindo.
            // Cálculo: cada símbolo X com prob p (incluindo wild como substituição) contribui
            //   contribution = (p_X_or_W^3 − p_W^3) × payout_X por payline.
            // Soma de todos os símbolos × 5 paylines deve ≈ 0.96.
            new(Id: "L", Name: "Lingote",  Weight: 6,  Payouts: new Dictionary<int, int> { [3] = 30 }),
            new(Id: "M", Name: "Moeda",    Weight: 10, Payouts: new Dictionary<int, int> { [3] = 8 }),
            new(Id: "E", Name: "Envelope", Weight: 14, Payouts: new Dictionary<int, int> { [3] = 3 }),
            new(Id: "S", Name: "Sino",     Weight: 18, Payouts: new Dictionary<int, int> { [3] = 2 }),
            new(Id: "F", Name: "Folha",    Weight: 22, Payouts: new Dictionary<int, int> { [3] = 1 }),
        };

        // 5 paylines em 3x3: 3 horizontais + 2 diagonais
        var paylines = new int[][]
        {
            [0, 0, 0],   // linha de cima
            [1, 1, 1],   // meio
            [2, 2, 2],   // baixo
            [0, 1, 2],   // diagonal ↘
            [2, 1, 0],   // diagonal ↗
        };

        return new SlotConfig(
            Slug: Slug,
            Reels: 3,
            Rows: 3,
            Symbols: symbols,
            Paylines: paylines,
            RtpTarget: 0.96m,
            MaxWinMultiplier: 2500
        );
    }
}

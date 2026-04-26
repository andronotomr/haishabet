# Jogos — catálogo, mecânica e regras de ganho

## Jogos mais usados e mais rentáveis no Brasil

> Levantamento do mercado BR 2024–2026 (rankings de operadoras, dados públicos de provedores como PG Soft / Pragmatic / Spribe, tendências de mídia especializada).

### Top 15 jogos por popularidade no Brasil

| # | Jogo | Provedor | Categoria | RTP oficial | Volatilidade | Por que viralizou |
|---|---|---|---|---|---|---|
| 1 | **Fortune Tiger** ("o tigrinho") | PG Soft | Slot 3×3 | 96.81% | Alta | Tema oriental + tigre, multiplicadores 4×/10×/40×/250×, animação cativante. **O slot mais jogado do BR.** |
| 2 | **Aviator** | Spribe | Crash | 97.00% | — | Round social rápido, decisão emocional de cash-out |
| 3 | **Fortune Ox** ("touro") | PG Soft | Slot 3×3 | 96.75% | Alta | Mesma engine do Tigre, tema boi |
| 4 | **Fortune Mouse** ("ratinho") | PG Soft | Slot 3×3 | 96.96% | Alta | Mesma família, tema rato/zodíaco |
| 5 | **Fortune Rabbit** ("coelhinho") | PG Soft | Slot 3×3 | 96.81% | Alta | Mesma família, lançado 2023 |
| 6 | **Fortune Dragon** ("dragão") | PG Soft | Slot 3×3 | 96.5% | Alta | Mesma família |
| 7 | **Sweet Bonanza** | Pragmatic Play | Slot tumble 6×5 | 96.51% | Muito alta | Doces caindo, multipliers acumulados nos free spins |
| 8 | **Gates of Olympus** | Pragmatic Play | Slot tumble 6×5 | 96.50% | Muito alta | "Pai Zeus", mesma engine do Sweet Bonanza |
| 9 | **Mines** | Spribe | Probabilidade | 97.00% | configurável | Decisão simples, tensão crescente |
| 10 | **Plinko** | Spribe / BGaming | Probabilidade | 97.00% | configurável | Bola caindo entre pinos, visual hipnótico |
| 11 | **Spaceman** | Pragmatic Play | Crash | 96.50% | — | Variante do Aviator com astronauta |
| 12 | **Dice** | Spribe | Probabilidade | 99.00% | baixa | Roda de dado, jogadores experientes |
| 13 | **JetX** | SmartSoft | Crash | 97.00% | — | Variante crash com jato |
| 14 | **Dog House Megaways** | Pragmatic | Slot Megaways | 96.55% | alta | Cachorrinho, Megaways (até 117 mil linhas) |
| 15 | **Bac Bo** | Evolution | Cassino ao vivo | 98.87% | baixa | Bacarat com dados, dealer ao vivo |

### Por que **Fortune Tiger** é o "rei" do Brasil
- **Simplicidade**: 3×3 é o slot mais fácil de entender.
- **Velocidade**: 3–4 segundos por giro → alta frequência → maior volume de aposta → mais GGR.
- **Tema**: tigre = poder/sorte na cultura asiática, virou meme no BR ("o jogo do tigrinho").
- **Multiplicadores grandes** (até 250×) → história de "ganhei mil reais com R$ 5".
- **Sons e animações** vibrantes e recompensadoras (até nas perdas, pelos quase-ganhos).
- **Viralização orgânica** em TikTok/Kwai com "lives jogando tigrinho" e "estratégia do tigrinho" (não existe estratégia — é RNG, mas vende sonho).

### Padrão dos jogos PG Soft (família Fortune)
Todos clones com tema diferente, **mesma engine, mesma matemática:**
- Grid 3×3, **5 paylines** (3 horizontais + 2 diagonais).
- Wild = símbolo principal, com multiplicador (4×/10×/40×/250×) ao completar paylines.
- 3 scatters → **10 free spins** com multiplicador progressivo.
- RTP ~96.7–96.8%, volatilidade alta, max win 2500×.

> **Para o nosso MVP, clonar essa fórmula com tema próprio cobre ~80% do interesse do mercado BR.**

---

## Tipos de jogo e esforço de implementação

| Tipo | Exemplo | Esforço dev | Margem | Retenção |
|---|---|---|---|---|
| **Slot 3×3** ("tigrinho") | Fortune Tiger | Médio | 3–5% | Altíssima |
| **Slot tumble 6×5** | Sweet Bonanza, Gates Olympus | Médio-alto | 3–5% | Sessões longas |
| **Crash** | Aviator, Spaceman | **Baixo** | 1–3% | Social, viral |
| **Mines** | Spribe Mines | **Baixo** | configurável | Boa |
| **Plinko** | Spribe Plinko | **Baixo** | configurável | Boa |
| **Roleta RNG** | Roleta europeia | Baixo-médio | 2,7% (zero único) | Média |
| **Cassino ao vivo** | Bac Bo, Crazy Time | **Inviável in-house** | — | Alta (ticket alto) |
| **Sportsbook** | qualquer aposta esportiva | **Alto** (feed de odds) | 5–8% | Sazonal |

---

## Catálogo MVP recomendado

**3 jogos in-house** que cobrem os 3 padrões mais lucrativos e ensinam a engine completa:

| # | Nome interno | Inspirado em | Tipo | RTP | Volatilidade | Por que |
|---|---|---|---|---|---|---|
| 1 | `tigre-da-fortuna` | Fortune Tiger | Slot 3×3 | 96.0% | Alta | Cobre toda a mecânica de slot (símbolos, paylines, wild com multiplicador, free spins) |
| 2 | `voa-aviao` | Aviator | Crash | 97.0% | — | Cobre WebSocket ao vivo, cash-out, multi-jogador no mesmo round |
| 3 | `mina-de-ouro` | Spribe Mines | Mines | 97.0% | configurável | Cobre jogo de probabilidade exata, decisão progressiva |

Opcionais se sobrar tempo:

| # | Nome | Inspirado em | Tipo | RTP |
|---|---|---|---|---|
| 4 | `plinko-bola` | Plinko | Plinko | 97.0% |
| 5 | `raspa-tubarao` | Raspadinha | Instant | 92.0% |

> **Por quê só 3 no MVP:** cada jogo certificável (futuro) custa caro. Para validar produto/aquisição/retenção, 3 jogos bons valem mais que 10 medianos.

---

## 1. SLOT — como funciona (estilo Fortune Tiger)

### Componentes
- **Reels (rolos)**: 3 colunas × 3 linhas visíveis.
- **Símbolos**: tipicamente 6–10 com pesos diferentes.
- **Paylines**: combinações que pagam (em 3×3, geralmente 5: horizontais + diagonais).
- **Wild**: substitui qualquer símbolo. Em Fortune Tiger é o próprio Tigre, e tem mecânica especial: **wild = multiplicador 4× / 10× / 40× / 250×** quando completa linha (sorteado por probabilidade).
- **Scatter / bônus**: 3 scatters disparam free spins.
- **Multiplicadores acumulados**: nos free spins o multiplier vai crescendo.

### Fortune Tiger — números reais (públicos)
- **RTP: 96.81%** (margem da casa: 3.19%)
- Volatilidade: alta
- Aposta mín: R$ 0,40 / Aposta máx: R$ 2.000
- Hit frequency: ~25%
- Max win: **2500× a aposta**

### Configuração do nosso `tigre-da-fortuna`

```json
{
  "slug": "tigre-da-fortuna",
  "type": "slot",
  "reels": 3,
  "rows": 3,
  "symbols": [
    { "id": "T", "name": "Tigre",    "weight":  2, "wild": true,  "multipliers":[4,10,40,250] },
    { "id": "L", "name": "Lingote",  "weight":  4, "payout": { "3": 250 } },
    { "id": "M", "name": "Moeda",    "weight":  8, "payout": { "3":  50 } },
    { "id": "E", "name": "Envelope", "weight": 12, "payout": { "3":  10 } },
    { "id": "S", "name": "Sino",     "weight": 16, "payout": { "3":   5 } },
    { "id": "F", "name": "Folha",    "weight": 20, "payout": { "3":   2 } }
  ],
  "paylines": [
    [0,0,0],[1,1,1],[2,2,2],
    [0,1,2],[2,1,0]
  ],
  "rtp_target": 0.96,
  "max_win_multiplier": 2500,
  "features": {
    "wild_multiplier": { "values":[4,10,40,250], "weights":[60,30,9,1] },
    "free_spins":      { "trigger":"3_scatters", "spins":10, "multiplier_progression":true }
  }
}
```

### Algoritmo de spin (pseudocódigo C#)

```csharp
public SpinResult Spin(SlotConfig cfg, long betAmountCents,
                        byte[] serverSeed, byte[] clientSeed, ulong nonce)
{
    var rng = new HmacRngStream(serverSeed, clientSeed, nonce);

    // 1. Gerar grid 3x3 sorteando símbolos por peso
    var grid = new string[cfg.Reels, cfg.Rows];
    int totalWeight = cfg.Symbols.Sum(s => s.Weight);
    for (int col = 0; col < cfg.Reels; col++)
        for (int row = 0; row < cfg.Rows; row++) {
            int pick = (int)(rng.NextUInt32() % (uint)totalWeight);
            grid[col, row] = SelectByWeight(cfg.Symbols, pick).Id;
        }

    // 2. Avaliar cada payline
    long totalPayoutCents = 0;
    foreach (var line in cfg.Paylines) {
        var symbols = Enumerable.Range(0, cfg.Reels)
                         .Select(c => grid[c, line[c]]).ToArray();
        var (paySymbol, count, isWild) = EvaluateLine(symbols, cfg);
        if (count >= 3) {
            long basePayout = cfg.Symbols.First(s => s.Id == paySymbol).Payout["3"]
                            * (betAmountCents / 100);  // ratio
            long mult = isWild ? RollWildMultiplier(cfg, rng) : 1;
            totalPayoutCents += basePayout * mult;
        }
    }

    // 3. Cap em max_win
    long cap = betAmountCents * cfg.MaxWinMultiplier;
    totalPayoutCents = Math.Min(totalPayoutCents, cap);

    // 4. Free spins (se 3+ scatters apareceram)
    var freeSpins = CheckScatterTrigger(grid, cfg);

    return new SpinResult(grid, totalPayoutCents, freeSpins, /* details */);
}
```

### Como o RTP é calibrado (passo a passo)
1. Define paytable e pesos iniciais "no chute".
2. Roda **simulação de 10–100 milhões de giros** (`SlotEngine.Simulator` no projeto).
3. Calcula RTP = `total_pago_em_premios / total_apostado`.
4. Ajusta pesos / payouts até atingir RTP alvo (ex: 0.96 ± 0.005).
5. Verifica também: volatilidade (desvio padrão), hit frequency, distribuição dos prêmios grandes.
6. Documenta paytable final + matemática numa "math sheet" (exigência das certificadoras GLI/BMM/eCOGRA).

**Exemplo manual simplificado** (1 payline, 2 símbolos):
- Símbolo A (peso 1): paga 100× quando 3 iguais.
- Símbolo B (peso 9): paga 5× quando 3 iguais.
- P(3A) = (1/10)³ = 0.001 → contribui `0.001 × 100 = 0.10` (10% RTP).
- P(3B) = (9/10)³ = 0.729 → contribui `0.729 × 5 = 3.645` (364% RTP — absurdo).
- Conclusão: payout B precisa cair muito, ou peso/payout de A subir. **Daí a necessidade de simulação numérica.**

---

## 2. CRASH — como funciona (estilo Aviator)

### Mecânica
- Round dura ~5–30 segundos. Multiplicador começa em 1.00× e cresce ao longo do tempo.
- Em algum momento aleatório o multiplicador **crasha** (avião voa pra fora da tela).
- Jogadores apostam **antes** do round.
- Durante o round, podem fazer **cash-out** a qualquer momento → ganham `aposta × multiplier_atual`.
- Se não fizerem cash-out até o crash → perdem tudo.

### Matemática do ponto de crash

Para garantir RTP de 97% (casa fica com 3%), o ponto de crash segue uma distribuição:

```
P(crash ≤ x) = 1 - 0.97 / x        (para x ≥ 1.00)
```

Ou seja: 3% dos rounds crasham exatamente em **1.00×** (ninguém ganha — esse é o "house edge"). Os outros 97% se distribuem inversamente: muito mais rounds em multiplicadores baixos (1.5×, 2×) e poucos em multiplicadores altos (50×, 100×).

### Algoritmo (C# pseudocódigo)

```csharp
public decimal GenerateCrashPoint(byte[] serverSeed, byte[] clientSeed,
                                   ulong nonce, decimal houseEdge = 0.03m)
{
    // HMAC-SHA256 → bytes
    var hash = HmacSha256(serverSeed, $"{Convert.ToHexString(clientSeed)}:{nonce}");
    ulong intVal = BitConverter.ToUInt64(hash, 0);

    // 3% dos rounds crasham instantâneo (house edge)
    if (intVal % 100 < (ulong)(houseEdge * 100))
        return 1.00m;

    // Outros 97%: distribuição inversa
    double u = (intVal & ((1UL << 52) - 1)) / (double)(1UL << 52);  // u ∈ (0,1)
    double crash = (1.0 - (double)houseEdge) / (1.0 - u);
    crash = Math.Floor(crash * 100) / 100;       // 2 casas decimais
    return Math.Max(1.00m, (decimal)crash);
}
```

### Provably fair (essencial para crash)
- Antes do round, servidor publica `hash(serverSeed)`.
- Após N rounds, revela `serverSeed` → jogador recalcula e confirma.
- Seeds em chain (cada round usa hash do próximo) — padrão clássico do Stake/BC.Game.

### Tempo real (SignalR)

```
[Cliente abre o jogo] → SignalR Hub.JoinAsync("crash")
[Servidor a cada round]:
  1. Aceita apostas por 8s → broadcast "betting_open"
  2. Sorteia crashPoint (server-side, secreto até o final)
  3. Inicia tick: a cada 100ms broadcast multiplier crescente
       multiplier(t) = 1.0024^t  (~30% por segundo)
  4. Quando atinge crashPoint → broadcast "crashed"
  5. Resolve cash-outs feitos antes do crash → paga em ledger
  6. Próximo round em 3s
```

Curva: em 10s chega a ~1.82×; em 30s, ~6×; em 60s, ~36×.

---

## 3. MINES — como funciona (estilo Spribe)

### Mecânica
- Grid 5×5 (25 casas). Jogador escolhe quantas minas (1 a 24, padrão 3).
- Sistema sorteia posição das minas no início do round (secreto).
- Jogador clica em casas:
  - **Diamante (segura)** → multiplica ganho potencial e libera próximo clique.
  - **Mina** → perde tudo.
- Pode **cash-out** a qualquer momento.

### Matemática

Multiplicador após `k` cliques seguros, com `m` minas em `n=25` casas:

```
multiplier(k, m) = (1 - houseEdge) × C(n, k) / C(n - m, k)
```

onde `C(n,k)` = combinação. Exemplo com **3 minas** (`m=3`, `houseEdge=0.03`):

| Cliques seguros | Multiplicador |
|---|---|
| 1 | 1.13× |
| 2 | 1.29× |
| 5 | 2.13× |
| 10 | 6.84× |
| 22 (todas seguras) | 33.96× |

### Algoritmo (C# pseudocódigo)

```csharp
public class MinesRound
{
    public HashSet<int> MinePositions { get; }   // sorteadas no início, secretas
    public List<int> Revealed { get; } = new();
    public decimal CurrentMultiplier { get; private set; } = 1m;
    public bool Ended { get; private set; }

    public ClickResult Click(int cellIndex, int mineCount, decimal houseEdge = 0.03m)
    {
        if (Ended) throw new InvalidOperationException();
        if (MinePositions.Contains(cellIndex)) {
            Ended = true;
            return ClickResult.Mine();        // perdeu tudo
        }
        Revealed.Add(cellIndex);
        int k = Revealed.Count;
        CurrentMultiplier = (1m - houseEdge)
                          * (decimal)Combination(25, k)
                          / (decimal)Combination(25 - mineCount, k);
        return ClickResult.Safe(CurrentMultiplier);
    }

    public CashOutResult CashOut(long betAmountCents)
    {
        Ended = true;
        return new CashOutResult(payout: (long)(betAmountCents * CurrentMultiplier));
    }
}
```

---

## RNG no servidor — princípios comuns a todos os jogos

1. **Crypto-secure**: `RandomNumberGenerator.Create()` (.NET) — **nunca** `System.Random` (não é seguro para criptografia).
2. **Provably fair (recomendado)**:
   - `serverSeed` (32 bytes random, secreto até reveal).
   - `clientSeed` (escolhido pelo jogador, visível).
   - `nonce` (contador do giro, incrementa a cada bet).
   - `result = HMAC-SHA256(serverSeed, $"{clientSeed}:{nonce}")` → mapeia para o resultado do jogo.
   - `hash(serverSeed)` é publicado antes; após N giros (ou ao trocar seed), revela e jogador audita.
3. **Sem RNG no cliente**. Cliente só envia "girei". Resultado completo vem do servidor; animação visualiza o resultado já decidido.
4. **Idempotência**: mesmo `nonce` gera mesmo resultado → retry de chamada é seguro.
5. **Logs imutáveis** em `spin_logs` (Mongo) — auditoria + análise estatística (RTP real × teórico).

---

## Estrutura técnica do módulo `GameEngine` (.NET)

```
src/Haishabet.GameEngine/
├── Rng/
│   ├── IRngStream.cs
│   ├── HmacRngStream.cs           # HMAC-SHA256 → uint pool
│   └── ProvablyFairSeeds.cs
├── Slot/
│   ├── SlotEngine.cs              # Spin(SlotConfig, bet, seeds, nonce)
│   ├── SlotConfig.cs
│   ├── Paytable.cs
│   └── Simulator.cs               # roda 10M giros, mede RTP/volatilidade
├── Crash/
│   ├── CrashEngine.cs             # GenerateCrashPoint
│   └── CrashRound.cs              # estado do round em memória + Redis
├── Mines/
│   ├── MinesEngine.cs
│   └── MinesRound.cs
└── Common/
    ├── Money.cs                   # value object (centavos)
    └── BetResult.cs
```

**Testes unitários cruciais:**
- `SlotEngine_Simulate10M_RtpInRange` — roda 10M giros, RTP deve estar em `[target − 0.5%, target + 0.5%]`.
- `CrashEngine_GenerateCrashPoint_RtpInRange` — 1M rounds, RTP no alvo.
- `MinesEngine_MultiplierMatchesExpectedTable` — comparar com tabela esperada para `(k, m)`.
- `HmacRngStream_DeterministicForSameSeeds` — mesma seed/nonce → mesmo resultado byte a byte.

---

## Integração com agregador (fase 2)

Em vez de licenciar e integrar Pragmatic, Spribe, Evolution etc. direto, contrata **agregador** (SoftSwiss, EveryMatrix, BetConstruct, SoftGamings) — uma integração para acessar centenas de jogos.

### Fluxo seamless wallet (mais comum)
1. Usuário abre jogo no app → frontend pede launch URL ao backend → backend chama agregador → recebe URL com token de sessão.
2. WebView (no app Flutter) carrega o jogo do agregador.
3. A cada bet/win, agregador chama nosso endpoint `POST /aggregator/callback` (auth HMAC) → debitamos/creditamos no ledger e respondemos com saldo novo.
4. **Idempotência obrigatória** — callbacks podem ser repetidos pelo agregador.

**Custos típicos:** 5–15% revshare sobre GGR dos jogos do agregador, ou fee fixo + revshare menor.

---

## Certificação (operação comercial)

Para licença SPA/MF, jogos próprios precisam de certificação **RNG e RTP** por laboratório credenciado (GLI — Gaming Laboratories International, BMM Testlabs, eCOGRA). Custo: **dezenas de milhares de R$ por jogo**. Por isso o caminho prático para escala é **agregador**.

Para **dev/staging local sem operação comercial**, sem essa exigência.

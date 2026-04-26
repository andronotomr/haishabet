# Arquitetura

## Stack escolhida e por quê

### Mobile — Flutter
- Codebase única Android+iOS, performance nativa (importante para animações de slots/tigrinho a 60fps), bom suporte a biometria/notificações/deep links, ecossistema maduro (Riverpod, Dio, GoRouter).
- Alternativas descartadas: PWA (Apple bloqueia pagamento real fora do IAP, biometria fraca, animações piores), React Native (Flutter rende melhor em canvas), nativo Kotlin+Swift (2× custo).

### Admin web — React + Next.js + TypeScript
- App Router, server components onde fizer sentido, client components para tabelas interativas.
- UI: shadcn/ui + Tailwind + TanStack Table + Recharts.
- Cliente SignalR (`@microsoft/signalr`) para tempo real.
- Autenticação separada da do usuário (admins têm 2FA obrigatório).

### Backend — C# / .NET 8 (ASP.NET Core)
**Por que .NET (e não Node):**
- **Performance excelente** (Kestrel + .NET 8 fica no topo do TechEmpower; importa quando há picos de spin com WebSocket aberto).
- **Tipagem rigorosa** (nullable reference types, records, exhaustive switch) — muito útil em domínio financeiro.
- Bom para sistemas de longa vida com muitas regras (DDD/Clean é cultura no .NET, casa com nosso domínio).
- Roda nativo em Linux desde .NET Core; deploy em VPS via container ou systemd, sem dor.
- `decimal` 128-bit nativo (mas convenção segura em iGaming é centavos em `long`).

**Bibliotecas principais:**
- **ASP.NET Core 8** Web API (controllers; minimal API onde fizer sentido).
- **Entity Framework Core 8** + Npgsql (Postgres).
- **MongoDB.Driver** oficial.
- **StackExchange.Redis** + IDistributedCache.
- **SignalR** para WebSocket (saldo em tempo real, jogo crash multi-jogador).
- **Hangfire** para jobs/filas (saques, KYC, reconciliação) — backed em Postgres, UI web embutida.
- **Serilog** para logs estruturados.
- **MediatR** + **FluentValidation** para CQRS-light + validação.
- **Refit** para HTTP clients tipados (PSPs, KYC providers, agregador).
- **Polly** para retry/circuit-breaker em chamadas externas.
- **xUnit** + **FluentAssertions** + **Testcontainers** para testes (Postgres/Redis/Mongo reais em container nos testes de integração).

### Banco de dados
- **PostgreSQL 16** — fonte da verdade financeira. Tudo que envolve dinheiro: `users`, `wallets`, `wallet_transactions`, `bets`, `deposits`, `withdrawals`, `payouts`. Transações ACID **obrigatórias**.
- **MongoDB** — alto volume e schema flexível: `spin_logs` (cada giro de cada jogador), `game_events`, `audit_trail`, `analytics_events`, `kyc_documents_meta`. Não substitui Postgres; complementa.
- **Redis** — sessões/JWT revogáveis, rate limiting, cache de configs de jogo, leaderboards (ZSET), locks distribuídos. Hangfire usa Postgres como storage; Redis fica para o app.

### Real-time — SignalR
Hubs para: saldo do usuário, jogo crash (multiplicador ao vivo), notificações de prêmio grande no lobby. Cliente Flutter usa `signalr_netcore`; web usa `@microsoft/signalr`.

### Filas / jobs assíncronos — Hangfire
Jobs: aprovação de saque, reconciliação PIX, geração de relatórios, e-mail/SMS, KYC assíncrono. UI web em `/hangfire` protegida por auth admin.

### Observabilidade
- Logs Serilog → arquivo JSON + console (dev). Em prod, sink para Loki ou Seq.
- Métricas: OpenTelemetry → Prometheus → Grafana.
- Sentry para exceções não tratadas (frontend + backend).

## Diagrama lógico

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ App Flutter  │    │ Admin Web    │    │ Site (web)   │
│ (usuário)    │    │ (Next.js)    │    │ futuro PWA   │
└──────┬───────┘    └──────┬───────┘    └──────┬───────┘
       │ HTTPS / WSS       │ HTTPS             │
       └─────────┬─────────┴────────┬──────────┘
                 │                  │
            ┌────▼──────────────────▼────┐
            │      Nginx (reverse proxy) │
            └────────────┬────────────────┘
                         │
                ┌────────▼──────────────┐
                │  ASP.NET Core 8       │
                │  (Web API + SignalR)  │
                └─┬──────┬──────┬───────┘
                  │      │      │
          ┌───────▼─┐ ┌──▼──┐ ┌─▼──────┐
          │Postgres │ │Redis│ │MongoDB │
          └─────────┘ └─────┘ └────────┘
                  │
                  ├─► Hangfire workers (saques, KYC, mailers)
                  └─► PSP (PIX), Agregador de jogos,
                       SERPRO/Idwall (KYC), Sentry
```

## Estrutura de projetos do backend (.NET solution)

```
backend/
├── Haishabet.sln
├── src/
│   ├── Haishabet.Api/              # entry point ASP.NET Core, controllers, hubs SignalR, middlewares
│   ├── Haishabet.Application/      # use cases (handlers MediatR), DTOs, validators
│   ├── Haishabet.Domain/           # entidades, value objects (Money, Cpf), domain events
│   ├── Haishabet.Infrastructure/   # EF DbContext, Mongo repos, Redis, clients PSP/KYC (Refit)
│   └── Haishabet.GameEngine/       # RNG, slot/crash/mines engines, paytables, simulador RTP
└── tests/
    ├── Haishabet.UnitTests/
    └── Haishabet.IntegrationTests/  # Testcontainers + Postgres/Redis/Mongo reais
```

### Módulos funcionais (organizados por feature dentro dos projetos)

| Módulo | Responsabilidade |
|---|---|
| `Auth` | login, refresh, 2FA admin, recuperação senha |
| `Users` | cadastro, perfil, validação CPF/idade, autoexclusão |
| `Kyc` | nível 1/2, integração Idwall/Datavalid, listas PEP/COAF |
| `Wallet` | saldo, extrato, transações (ledger double-entry) |
| `Deposits` | PIX in, webhook PSP, validação CPF pagador, reconciliação |
| `Withdrawals` | PIX out, fila Hangfire, antifraude, aprovação manual |
| `Games` | catálogo, sessões, lançamento (in-house ou agregador) |
| `GameEngine` | RNG, slot/crash/mines/plinko, paytables, RTP tracking |
| `Bets` | aposta, resolução, payout, idempotência |
| `Bonus` | campanhas, rollover, cashback |
| `ResponsibleGaming` | limites depósito/perda/tempo, autoexclusão |
| `Admin` | endpoints do dashboard segregados (autorização por policy) |
| `Reports` | GGR, NGR, RTP por jogo, cohort |
| `Notifications` | push (FCM/APNs), e-mail, SMS |

## Princípios não-negociáveis

1. **Wallet em ledger double-entry.** Toda movimentação tem débito + crédito, soma sempre zero. Saldo é projeção materializada, nunca campo mutável "solto".
2. **Dinheiro em centavos (`bigint` no Postgres, `long` em C#)** — ou um value object `Money`. Nunca `float`/`double`.
3. **Idempotência.** Webhooks de PSP, retry de saques, callbacks de jogo — todos com `idempotency_key` validado em índice único.
4. **Auditoria total.** Toda ação admin (ajuste de saldo, bloqueio, aprovação de saque) gera registro imutável em `audit_trail` (Mongo) + log Serilog assinado.
5. **RNG só do lado servidor.** Cliente nunca decide resultado de spin/aposta. Cliente envia "girei", servidor responde "resultado X". Animação é só visualização.
6. **Segregação de ambientes.** Dev / staging / prod isolados, com chaves de PSP distintas (sandbox vs produção).
7. **Servidores em produção no Brasil** (exigência regulatória SPA/MF para operação licenciada). Detalhes em [hospedagem.md](hospedagem.md).

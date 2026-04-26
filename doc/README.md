# Haishabet

Plataforma de apostas e cassino online (foco em jogos tipo "tigrinho"/slots) operando no mercado brasileiro regulado pela **Lei 14.790/2023**.

## Estrutura do repositório

```
haishabet/
├── mobile/      # App do usuário final (Flutter — Android v1, iOS depois)
├── web/         # Dashboard administrativo (React + Next.js)
├── backend/     # API REST + SignalR (C# / .NET 8 — ASP.NET Core)
└── doc/         # Documentação do projeto
```

> Web do **usuário** é fase 2 (após MVP mobile + admin). PWA opcional reusando código React.

## Documentação — leia nesta ordem

1. [arquitetura.md](arquitetura.md) — stack técnica e desenho do sistema
2. [mercado-brasileiro.md](mercado-brasileiro.md) — top bets, top jogos, unit economics
3. [escopo.md](escopo.md) — features completas por módulo
4. [jogos.md](jogos.md) — catálogo, mecânica de slot/crash/mines com matemática e código
5. [banco-dados.md](banco-dados.md) — schemas PostgreSQL, MongoDB, Redis
6. [pagamentos-kyc.md](pagamentos-kyc.md) — PIX, PSP, sandbox, KYC, antifraude
7. [hospedagem.md](hospedagem.md) — VPS BR vs exterior, custos, hardening
8. [regulamentacao.md](regulamentacao.md) — exigências legais BR
9. [roadmap.md](roadmap.md) — fases MVP → produção
10. [setup-dev.md](setup-dev.md) — ambiente local
11. [git-workflow.md](git-workflow.md) — estratégia de branches (main / dev / fase-N)

## Resumo executivo

| Item | Decisão |
|---|---|
| Mobile | Flutter (Android v1, iOS v2) |
| Admin web | React + Next.js + TypeScript |
| Backend | **C# / .NET 8 (ASP.NET Core)** + EF Core + Hangfire |
| DB principal | PostgreSQL (transações, dinheiro, ACID) |
| DB secundário | MongoDB (logs de spins, eventos, auditoria) |
| Cache / sessão | Redis |
| Real-time | **SignalR** (WebSocket) |
| Pagamentos | PIX via PSP (EFI ou Asaas no MVP) |
| Hospedagem (dev/staging) | VPS exterior (Hetzner / Contabo) — Docker Compose |
| Hospedagem (produção comercial) | **VPS Brasil** — exigência Lei 14.790 (Magalu / AWS sa-east-1 / Locaweb) |

## Modelo de negócio

Receita = **margem da casa** (1 - RTP). Em slots padrão, RTP fica entre 94% e 96%, ou seja a casa retém 4–6% de cada giro estatisticamente. Volume × ticket médio × margem = receita bruta. Custos principais: PSP (taxa por PIX), agregador de jogos (% receita ou fixo), infra, marketing, KYC/antifraude, licença regulatória.

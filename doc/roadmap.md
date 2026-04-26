# Roadmap

Plano em fases. Cada fase tem critério de "pronto" verificável.

---

## Fase 0 — Fundação ✅ EM CURSO

**Objetivo:** ambiente de dev funcionando, esqueleto dos 3 projetos rodando.

- [x] Repositório git inicializado, `.gitignore`, `.gitattributes`, README raiz, `.editorconfig`.
- [x] `backend/` — Solution `Haishabet` (.NET 8) com Api/Application/Domain/Infrastructure/GameEngine + UnitTests/IntegrationTests + EF Core + MongoDB.Driver + Redis + SignalR + Hangfire + MediatR + FluentValidation + Refit + Polly + Serilog + Testcontainers. `dotnet build`: 0 erro.
- [x] `mobile/` — Flutter 3.41.7 scaffold (Android + iOS) + Riverpod + GoRouter + Dio + Freezed + json_serializable + secure_storage + local_auth + Firebase + signalr_netcore. `flutter analyze`: No issues.
- [x] `web/` — Next.js 15 + TypeScript + Tailwind v4 + zustand + swr + TanStack Table + recharts + zod + react-hook-form + @microsoft/signalr. `pnpm build`: ok.
- [x] `docker-compose.dev.yml` com Postgres 16 + Redis 7 + MongoDB 7 + healthchecks.
- [x] `appsettings.Example.json` (backend) + `web/.env.example`.
- [x] Conventional Commits + git-workflow doc (main/dev/fase-N).
- [x] Helpers `scripts/dev-{up,down,logs,reset}.ps1`.
- [ ] Validação final: `docker compose up` (aguardando Docker Desktop install)
- [ ] PR `fase-0-fundacao` → `dev` no GitHub

**Pronto quando:** todas as branches mergeadas em `dev`, com `docker compose up` + `dotnet run` + `pnpm dev` + `flutter analyze` rodando sem erro.

---

## Fase 1 — Backend MVP (3–4 semanas)

**Objetivo:** API funcional com auth, wallet ledger, depósito PIX (sandbox), 1 jogo (slot).

- [ ] `auth` — cadastro, login JWT, refresh, recuperação senha.
- [ ] `users` — perfil, validação CPF, idade ≥18.
- [ ] `wallet` — ledger double-entry, projeção de saldo, extrato.
- [ ] `deposits` — integração EFI ou Asaas em **sandbox**, webhook, validação CPF pagador.
- [ ] `withdrawals` — fluxo básico, fila de aprovação manual.
- [ ] `games` — catálogo + 1 slot in-house (`tigre-da-fortuna`).
- [ ] `bets` + RNG service.
- [ ] `responsible-gaming` — limites + autoexclusão.
- [ ] WebSocket de saldo.
- [ ] Testes: unit nos services críticos (wallet, RNG), integração no fluxo deposit→play→withdraw.

**Pronto quando:** consigo via Postman/Insomnia: cadastrar → depositar (sandbox) → girar slot 100× → ver saldo coerente → solicitar saque → aprovar via SQL manual → ver fluxo completo.

---

## Fase 2 — Mobile MVP (3–4 semanas, paralelo com Fase 1 final)

**Objetivo:** app Flutter funcional Android com fluxo completo.

- [ ] Telas: splash, onboarding, cadastro, login, esqueci senha.
- [ ] Lobby com 1 jogo, modo demo.
- [ ] Tela do slot (`tigre-da-fortuna`) com animação de 3 reels.
- [ ] Carteira: saldo, depósito (QR Code PIX), extrato.
- [ ] Saque: solicitação + status.
- [ ] Configurações: limites de jogo responsável, autoexclusão, perfil.
- [ ] Notificações push (FCM).
- [ ] Build Android assinado + testar em device real.

**Pronto quando:** APK instalável roda fluxo completo apontando para backend local em VPS de staging.

---

## Fase 3 — Admin Web MVP (2–3 semanas)

**Objetivo:** dashboard funcional para operação básica.

- [ ] Login admin com 2FA (TOTP).
- [ ] Listagem e detalhe de usuários.
- [ ] Fila de saques pendentes (aprovar/rejeitar).
- [ ] Fila de KYC (upload de doc — fase 2 depois, MVP só nível 1).
- [ ] Catálogo de jogos (ativar/desativar).
- [ ] Relatório GGR/NGR diário.
- [ ] Audit trail viewer.

**Pronto quando:** operador consegue fazer fluxo financeiro completo (aprovar saque, ver relatório, bloquear conta) sem tocar no SQL.

---

## Fase 4 — Catálogo de jogos próprios (2–3 semanas)

**Objetivo:** chegar a 5 jogos in-house para soltar versão fechada para amigos/beta.

- [ ] +2 slots (boi-da-sorte, dragao-de-jade).
- [ ] Crash (voa-aviao) com WebSocket de multiplicador ao vivo + cash-out.
- [ ] Raspadinha.
- [ ] Tela de free spins, multipliers visuais.
- [ ] Tracking de RTP real × teórico (relatório admin).

---

## Fase 5 — Hardening e VPS staging (2 semanas)

**Objetivo:** subir tudo numa VPS Linux com domínio, HTTPS, monitoramento, backup.

- [ ] VPS Ubuntu 22/24 (Hetzner / Contabo / DigitalOcean / Vultr — começar com 4vCPU/8GB).
- [ ] Docker Compose de produção com Nginx reverse proxy + Let's Encrypt.
- [ ] Postgres com backup diário automatizado (pg_dump + S3-like).
- [ ] Mongo backup.
- [ ] Sentry, Prometheus + Grafana, log centralizado (Loki).
- [ ] Firewall (ufw), fail2ban, ssh com chave, sem root login.
- [ ] CI/CD (GitHub Actions): lint + test + build + deploy ssh.
- [ ] Pentest básico interno (OWASP Top 10).

---

## Fase 6 — Beta fechado (3–4 semanas)

**Objetivo:** convidar 50–200 usuários reais (ou amigos com saldo simulado) e iterar.

- [ ] Cupons de saldo "casa" para teste (sem PIX real).
- [ ] Coletar métricas: DAU, retenção D1/D7, sessão média, jogos por sessão, ARPU.
- [ ] Bug bash, ajuste de UX, balanceamento de RTP/volatilidade.
- [ ] Decisão estratégica: licenciar/parcerizar para go-live comercial.

---

## Fase 7+ — Pós-MVP (backlog)

- iOS (Flutter — geralmente 1–2 semanas extras).
- Site web do usuário (Next.js — reaproveita componentes).
- Integração com agregador de jogos (centenas de slots Pragmatic etc.).
- Bônus / cashback / VIP.
- Cassino ao vivo (via agregador).
- Sportsbook (mais complexo — exige feed de odds tipo Sportradar).
- Programa de afiliados.
- KYC nível 2 com biometria (Idwall/Unico).

---

## Estimativa total para MVP jogável (fases 0–4)

**~10–14 semanas com 2–3 devs full-stack** (1 backend, 1 mobile, 1 web/full).
Solo: **~5–7 meses**.

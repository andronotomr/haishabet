# Hospedagem — Brasil ou exterior?

## TL;DR

| Fase | Onde | Por quê |
|---|---|---|
| **Dev / staging** | **Exterior** (Hetzner DE, Contabo) | Custo baixo (€5–10/mês), sem usuário real, sem dinheiro real |
| **MVP fechado / beta sem PIX real** | Exterior ou BR | Não há exigência regulatória até receber dinheiro de jogador |
| **Produção comercial regulada** | **Brasil obrigatório** | Lei 14.790/2023 + Portarias SPA/MF exigem servidores no país |

---

## Por que produção tem que ser no Brasil

### 1. Exigência legal
A Lei 14.790/2023 e as Portarias da SPA/MF estabelecem que operadoras licenciadas devem manter:
- **Servidores em território nacional** (cópia local de logs operacionais e dados pessoais de jogadores).
- **Registro auditável** de todas as transações financeiras e apostas.
- **Acesso garantido** para fiscalização sem necessidade de cooperação internacional.

Operar para o público brasileiro com servidores fora, recebendo PIX, sem licença = **operação irregular**, sujeita a bloqueio de domínio (ANATEL/Receita), bloqueio de pagamentos (BACEN/PSPs) e responsabilização criminal/administrativa.

### 2. LGPD
Dados pessoais de brasileiros podem ser tratados no exterior, mas exigem:
- Adequação do país receptor ou cláusulas contratuais padrão (ANPD).
- Inventário de transferência internacional.
- Bem mais burocracia. **Manter no BR é simplesmente mais simples.**

### 3. Latência
| Origem | Destino BR | RTT médio |
|---|---|---|
| Hetzner Frankfurt | BR | ~190–230ms |
| Contabo USA | BR | ~110–150ms |
| AWS sa-east-1 (SP) | BR | 5–40ms |
| Magalu Cloud (SP) | BR | 5–40ms |

Para slots e crash em tempo real, latência alta degrada UX visivelmente — animações travadas, cash-out perdido por delay, WebSocket SignalR menos responsivo.

### 4. PIX e PSPs
Alguns PSPs validam IP de origem das chamadas e geram alerta se vier de fora do país. Webhooks de PIX entrega mais confiável dentro da mesma região de rede.

### 5. Imagem institucional
Para ganhar confiança ("hospedado e operado no Brasil"), aparecer no rodapé com CNPJ e localização nacional ajuda. Players tendem a confiar mais. Marca `.bet.br` reforça isso.

---

## Provedores recomendados — produção BR

### Cloud com região no BR

| Provedor | Pontos fortes | Pontos fracos | Faixa preço (4vCPU/16GB) |
|---|---|---|---|
| **AWS sa-east-1 (São Paulo)** | Maturidade total, KMS/HSM, RDS, S3, CloudFront, AWS Shield (DDoS) | Mais caro, complexidade de billing | US$ 150–300/mês |
| **GCP southamerica-east1 (SP)** | Ótimas APIs, BigQuery para analytics, preço competitivo | Menos opções regionais que AWS | US$ 130–250/mês |
| **Azure brazilsouth (SP)** | Sinergia com .NET (Application Insights, AAD), boa para shops .NET | UI complexa, pricing menos transparente | US$ 150–280/mês |
| **Magalu Cloud** | Brasileira, conformidade BR fácil, suporte PT-BR, taxa Cloud BR específica | Catálogo menor que hyperscalers | R$ 400–900/mês |
| **Locaweb Cloud** | Brasileira tradicional, suporte forte | Catálogo limitado | R$ 350–800/mês |
| **Hostinger Cloud BR** | Barato, simples | Limitado, menos features avançadas | R$ 100–300/mês |
| **HostDime BR** | Foco em compliance, atendimento jurídico | Caro, menos automação | R$ 800+/mês |

### Sobre .NET no Azure
Como vamos rodar **.NET 8**, Azure tem sinergia natural (Application Insights, Azure SQL Database, App Service). **Mas isso é opcional.** Container .NET roda igual em qualquer Linux. **Não recomendo lock-in em PaaS (App Service) no MVP** — Docker Compose em VPS Linux dá mais controle, custo menor e portabilidade entre provedores.

---

## Provedores recomendados — dev/staging exterior

| Provedor | Faixa preço | Por que |
|---|---|---|
| **Hetzner CX22 (4vCPU/8GB)** | €5,83/mês | Melhor custo-benefício do mundo. AMD Ryzen + NVMe. **Recomendação top.** |
| **Hetzner CX32 (4vCPU/16GB)** | €11,66/mês | Folga para rodar tudo (api + 3 DBs + observabilidade) |
| **Contabo VPS S** | $7/mês | RAM generosa (8GB), barato, datacenter US Central |
| **DigitalOcean Premium AMD (4vCPU/8GB)** | $48/mês | UI simples, App Platform opcional, datacenter NYC ou SP |
| **Vultr High Frequency (4vCPU/16GB)** | $96/mês | Performance alta, datacenter SP disponível |

---

## Setup de produção — arquitetura mínima sugerida

### Opção A — VPS única (custo ↓, complexidade ↓) — bom até ~10k jogadores ativos
```
[VPS BR — 8 vCPU / 32GB / 200GB NVMe]
└─ Docker Compose:
   ├─ nginx (reverse proxy + Let's Encrypt)
   ├─ haishabet-api (.NET 8 — ASP.NET Core)
   ├─ postgres (com replica de leitura local opcional)
   ├─ redis
   ├─ mongo
   └─ hangfire-worker (mesmo container ou separado)
└─ Backups diários → Backblaze B2 ou S3-compat (cifrados client-side)
└─ Monitoring: Grafana Cloud free tier + Sentry (free tier)
```
Custo: R$ 400–800/mês infra + R$ 100–200/mês backups/monit.

### Opção B — Multi-host com banco gerenciado (custo ↑, robustez ↑) — quando crescer

```
[Load balancer BR (NLB/CLB)]
        │
        ├─► [VPS api-1 .NET]  ──┐
        └─► [VPS api-2 .NET]  ──┤
                                ├─► [Postgres gerenciado (RDS / Aurora)]
                                ├─► [Redis gerenciado (ElastiCache)]
                                └─► [Mongo gerenciado (Atlas BR ou DocumentDB)]

[VPS Hangfire-workers (1–2)]
[CDN (CloudFront / Cloudflare) para estáticos do app/admin]
```
Custo: US$ 500–1500/mês.

### Opção C — Kubernetes (custo ↑↑, complexidade ↑↑↑) — só para escala muito grande
Não recomendo para os primeiros 1–2 anos. Complexidade não compensa.

---

## Checklist de hardening de VPS de produção

- [ ] Ubuntu 24.04 LTS (suporte longo).
- [ ] Usuário não-root, ssh por chave (sem senha), root login bloqueado.
- [ ] `ufw` fechando tudo exceto 22 (rate-limited), 80, 443.
- [ ] `fail2ban` para ssh + nginx.
- [ ] Atualizações automáticas de segurança (`unattended-upgrades`).
- [ ] Docker rodando como usuário não-root (rootless mode opcional).
- [ ] Imagens .NET via multi-stage build → final em `mcr.microsoft.com/dotnet/aspnet:8.0-alpine` (mínima).
- [ ] Postgres/Redis/Mongo com volumes persistentes em disco separado.
- [ ] Backups diários cifrados com `age` ou GPG antes de upload pro B2/S3.
- [ ] `pg_dump` + `mongodump` em cron, retenção 30 dias rotativa.
- [ ] Restauração testada **mensalmente** (backup que não restaura não é backup).
- [ ] Monitoramento de disco/CPU/RAM/conexões DB com alertas.
- [ ] WAF na frente (Cloudflare ou AWS WAF) — DDoS protection + rate limit por IP.
- [ ] HTTPS com HSTS preload, TLS 1.2+, ciphers modernos (Mozilla intermediate).
- [ ] CSP headers, X-Frame-Options, segredos em arquivo `.env` fora do git.
- [ ] Segredos em produção via cofre (Vault, ou AWS Secrets Manager, ou simplesmente env do systemd com permissão restrita).
- [ ] Logs centralizados, retidos 90+ dias (exigência regulatória).

---

## Servidores fora do BR — o que pode ficar

Pode ficar fora do BR, **desde que sem PII de jogador identificável**:
- Logs anonimizados → Loki/Grafana Cloud (EU/US): OK.
- Analytics agregados (sem CPF, sem ID interno) → BigQuery/AWS US: OK.
- CI/CD runners (GitHub Actions): OK.
- Repositório Git: OK.

**Dados financeiros, KYC, CPF, sessões → fica no BR.**

---

## Recomendação final por fase

| Fase | Hospedagem | Custo aproximado |
|---|---|---|
| **0–4 (dev + MVP backend/mobile/admin)** | Hetzner CX32 (Frankfurt) | €12/mês |
| **5 (staging em VPS)** | Mesma Hetzner ou +1 VPS dedicada | €12–24/mês |
| **6 (beta fechado, sem PIX real)** | Hetzner ou migrar para Hostinger Cloud BR | €12–R$ 200/mês |
| **7+ (operação comercial regulada)** | **VPS BR** — Magalu Cloud / Hostinger Cloud BR / AWS sa-east-1 | R$ 500–3.000/mês conforme escala |

> Decisão: **construir nossa própria infra no Brasil** quando chegar em operação comercial. Não dependemos de terceiros operadores — só de provedores de infra (cloud, DNS, CDN, backup), que são fornecedores comuns.

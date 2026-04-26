# Banco de dados

## Divisão de responsabilidade

| Banco | Uso | Justificativa |
|---|---|---|
| **PostgreSQL** | Dinheiro, contas, apostas, KYC | ACID forte, integridade referencial, transações multi-tabela |
| **MongoDB** | Logs de spin, eventos, auditoria, analytics | Alto volume de write, schema flexível, agregações analíticas |
| **Redis** | Sessões, cache, rate limit, filas, leaderboard | Latência sub-ms, estruturas (ZSET, LIST), TTL nativo |

> Regra de ouro: **se envolve dinheiro, é Postgres**. Mongo nunca é fonte da verdade financeira.

---

## PostgreSQL — schema principal

Apenas as tabelas críticas — esboço, vai evoluir.

### `users`
```sql
id              uuid PK
email           text UNIQUE NOT NULL
password_hash   text NOT NULL          -- argon2id
cpf             char(11) UNIQUE NOT NULL
full_name       text NOT NULL
birth_date      date NOT NULL
phone           text
status          enum('active','blocked','self_excluded','closed')
kyc_level       smallint DEFAULT 0     -- 0=none, 1=cadastro, 2=doc+selfie
created_at      timestamptz DEFAULT now()
updated_at      timestamptz
last_login_at   timestamptz
self_exclusion_until  timestamptz NULL
CONSTRAINT age_18  CHECK (birth_date <= now() - interval '18 years')
```

### `wallets`
```sql
id              uuid PK
user_id         uuid FK → users(id) UNIQUE
balance_real    bigint NOT NULL DEFAULT 0   -- centavos
balance_bonus   bigint NOT NULL DEFAULT 0
balance_locked  bigint NOT NULL DEFAULT 0   -- saque pendente
currency        char(3) NOT NULL DEFAULT 'BRL'
updated_at      timestamptz
CHECK (balance_real >= 0 AND balance_bonus >= 0 AND balance_locked >= 0)
```

### `wallet_transactions` (ledger double-entry)
```sql
id              uuid PK
wallet_id       uuid FK
type            enum('deposit','withdrawal','bet','payout','bonus','adjustment','refund','rollback')
direction       enum('credit','debit')
amount          bigint NOT NULL              -- sempre positivo, direction define sinal
bucket          enum('real','bonus','locked')
balance_after   bigint NOT NULL
ref_type        text    -- 'deposit','bet','withdrawal','admin_adjustment'
ref_id          uuid
idempotency_key text UNIQUE
created_at      timestamptz DEFAULT now()
created_by      text    -- 'system','user:<id>','admin:<id>'
INDEX (wallet_id, created_at DESC)
```

> Saldo da wallet = projeção materializada. Toda mudança passa por `wallet_transactions` em **uma transação SQL**. Job de reconciliação compara projeção × soma do ledger e alerta divergência.

### `deposits`
```sql
id              uuid PK
user_id         uuid FK
amount          bigint
psp_provider    text                        -- 'pagarme','asaas','efi'
psp_charge_id   text
pix_key         text
pix_qrcode      text
pix_payer_cpf   char(11)                    -- valida = users.cpf
status          enum('pending','paid','expired','failed','refunded')
expires_at      timestamptz
paid_at         timestamptz
created_at      timestamptz
INDEX (user_id, created_at DESC), INDEX (status)
```

### `withdrawals`
```sql
id              uuid PK
user_id         uuid FK
amount          bigint
pix_key         text                        -- chave do CPF do titular
pix_key_type    enum('cpf','email','phone','random')
status          enum('requested','reviewing','approved','paid','rejected','cancelled','failed')
risk_score      smallint                    -- 0-100
reviewed_by     uuid FK admin_users(id) NULL
reviewed_at     timestamptz NULL
reject_reason   text NULL
psp_payout_id   text NULL
created_at      timestamptz
```

### `games` (catálogo)
```sql
id              uuid PK
slug            text UNIQUE
name            text
provider        text                        -- 'inhouse','pragmatic','spribe'...
type            enum('slot','crash','live','table','scratch')
rtp_target      numeric(5,4)                -- ex 0.9500
volatility      enum('low','medium','high','very_high')
min_bet         bigint
max_bet         bigint
active          boolean DEFAULT true
config          jsonb                       -- reels, simbolos, paytable
```

### `game_sessions`
```sql
id              uuid PK
user_id         uuid FK
game_id         uuid FK
started_at      timestamptz
ended_at        timestamptz NULL
total_bet       bigint DEFAULT 0
total_win       bigint DEFAULT 0
spin_count      int DEFAULT 0
device_info     jsonb
```

### `bets`
```sql
id              uuid PK
session_id      uuid FK
user_id         uuid FK
game_id         uuid FK
amount          bigint
bucket          enum('real','bonus')        -- de qual saldo veio
result          enum('win','lose','push')
payout          bigint DEFAULT 0
multiplier      numeric(10,4) DEFAULT 0
seed_server     text                        -- provably fair
seed_client     text
nonce           bigint
created_at      timestamptz
INDEX (user_id, created_at DESC), INDEX (game_id, created_at DESC)
```

### `kyc_records`
```sql
id              uuid PK
user_id         uuid FK
level           smallint                    -- 1 ou 2
provider        text                        -- 'idwall','serasa','manual'
status          enum('pending','approved','rejected')
data            jsonb                       -- redacted
reviewed_by     uuid NULL
reviewed_at     timestamptz NULL
notes           text
created_at      timestamptz
```

### `responsible_gaming_limits`
```sql
user_id         uuid PK
deposit_daily   bigint
deposit_weekly  bigint
deposit_monthly bigint
loss_daily      bigint
loss_weekly     bigint
session_minutes int
updated_at      timestamptz
```

### `admin_users`, `admin_roles`, `admin_audit`
Padrão RBAC — não detalhado aqui.

### `bonuses`, `bonus_grants`, `bonus_rollover`
Estrutura para campanhas e rollover — fase 2.

---

## MongoDB — coleções

### `spin_logs`
Cada giro/aposta detalhado para auditoria e análise. Volume alto (milhões/dia em escala).
```json
{
  "_id": "...",
  "bet_id": "uuid",
  "user_id": "uuid",
  "game_id": "uuid",
  "timestamp": ISODate,
  "bet_amount": 1000,
  "result": {
    "reels": [[3,1,7],[2,7,5],[7,4,1]],
    "lines_won": [{"line":1,"symbols":[7,7,7],"payout":50000}],
    "multiplier": 50,
    "free_spins_triggered": 0
  },
  "rng": {"seed_server":"...","seed_client":"...","nonce":12345,"hash":"..."},
  "device": {"ua":"...","ip":"x.x.x.x","country":"BR"}
}
```
Index: `{user_id:1, timestamp:-1}`, `{game_id:1, timestamp:-1}`.

### `audit_trail`
Toda ação admin sensível.
```json
{
  "actor":{"type":"admin","id":"...","email":"...","ip":"..."},
  "action":"withdrawal.approve",
  "target":{"type":"withdrawal","id":"..."},
  "before":{...},"after":{...},
  "reason":"texto obrigatório",
  "timestamp":ISODate
}
```

### `analytics_events`
Eventos de produto: `screen_view`, `game_open`, `deposit_started`, `deposit_completed` etc. Para BI / funil.

### `kyc_documents_meta`
Metadados de docs (o arquivo em si vai pra storage S3-compatible com link assinado).

---

## Redis — chaves

| Padrão | Uso | TTL |
|---|---|---|
| `session:<jwt-jti>` | revogação de token | até exp |
| `rl:login:<ip>` | rate limit login | 15 min |
| `rl:deposit:<user>` | rate limit depósito | 1 min |
| `cache:game:<id>` | config de jogo | 10 min |
| `lb:bigwins:daily` | ZSET top ganhadores do dia | 24h |
| `lock:hangfire:*` | locks distribuídos do Hangfire (workers concorrentes) | curto |
| `lock:withdrawal:<id>` | lock distribuído na aprovação | 30s |

---

## Migrations e versionamento

- Backend: **Entity Framework Core 8** (`Npgsql.EntityFrameworkCore.PostgreSQL`) com migrations geradas via `dotnet ef migrations add` e versionadas em git.
- Mongo: schema controlado em código C# (driver oficial **MongoDB.Driver** + `BsonClassMap`) + scripts de migration manuais quando necessário.
- Hangfire cria suas próprias tabelas no Postgres (`hangfire.*`) automaticamente — schema separado.
- Nada de schema change manual em produção. Tudo via migration revisada.

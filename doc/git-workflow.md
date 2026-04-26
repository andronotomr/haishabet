# Git workflow

## Branches permanentes

| Branch | Propósito |
|---|---|
| `main` | **Produção.** O que está rodando na VPS. Só recebe merge de `dev` quando uma versão é validada para deploy. Nunca commitar direto. |
| `dev` | **Integração das fases.** Roda localmente (Docker Compose + dotnet/pnpm/flutter) para teste de ponta a ponta. Recebe merge das branches de fase à medida que ficam prontas. |

## Branches de fase (do roadmap)

Branch de trabalho de cada fase do `roadmap.md`. Sempre criadas **a partir de `dev`** e mergeadas **de volta para `dev`** quando a fase está completa.

| Branch | Conteúdo |
|---|---|
| `fase-0-fundacao` | docker-compose.dev.yml + scaffold backend (.NET 8) + web (Next.js) + mobile (Flutter) + git-workflow doc |
| `fase-1-backend-mvp` | auth, wallet ledger, deposits PIX sandbox, withdrawals, 1 jogo, RNG, responsible-gaming |
| `fase-2-mobile-mvp` | telas Flutter completas, integração com backend |
| `fase-3-admin-mvp` | dashboard Next.js (saques, KYC, relatórios, audit trail) |
| `fase-4-jogos-catalogo` | +2 slots, crash, mines, plinko, raspadinha — chega aos 5 jogos in-house |
| `fase-5-vps-staging` | deploy em VPS, hardening, CI/CD GitHub Actions |
| `fase-6-beta-fechado` | features de beta, métricas, ajustes finos |
| `fase-7-go-live` | preparação produção comercial (licença, infra BR) |

## Fluxo de uma fase

```
1) Criar branch da fase a partir de dev:
   git checkout dev && git pull
   git checkout -b fase-N-nome

2) Trabalhar localmente, commits frequentes:
   git add .
   git commit -m "feat(modulo): mensagem"
   git push -u origin fase-N-nome

3) Quando a fase está completa, abrir PR fase-N-nome → dev no GitHub.
   Revisar, validar, merge (squash ou merge commit, à escolha).

4) Em dev, rodar localmente o stack completo e testar ponta a ponta:
   docker compose -f docker-compose.dev.yml up -d
   dotnet run --project backend/src/Haishabet.Api
   pnpm --dir web dev
   cd mobile && flutter run

5) Quando dev está estável e validada, abrir PR dev → main.
   Revisar, merge, criar tag de versão (vX.Y.Z).

6) main dispara deploy para VPS (CI/CD a partir da fase 5).
```

## Convenção de commit (Conventional Commits)

```
tipo(escopo): mensagem curta no imperativo
```

Tipos:
- `feat`     — nova feature
- `fix`      — bug fix
- `chore`    — ajuste sem efeito em produção (deps, configs)
- `docs`     — documentação
- `refactor` — refatoração sem mudar comportamento
- `test`     — adicionar/corrigir testes
- `perf`     — melhoria de performance

Exemplos:
```
feat(wallet): adicionar ledger double-entry
fix(deposits): corrigir validação de CPF do pagador
chore(backend): atualizar dependências EF Core
docs(jogos): adicionar matemática do crash game
test(slot-engine): simulação 10M giros valida RTP em ±0.5%
```

## Tags de versão (SemVer)

- Começa em `v0.1.0` quando `dev` for promovida para `main` pela primeira vez.
- Schema **SemVer**: `MAJOR.MINOR.PATCH`.
  - `0.x.x` enquanto não há release pública / comercial.
  - `1.0.0` no go-live comercial regulado (após licença SPA/MF).
- Uma tag por release de produção:
  ```
  git checkout main && git pull
  git tag -a v0.1.0 -m "MVP backend funcional"
  git push origin v0.1.0
  ```

## Hotfix em produção

Bug crítico em prod (já em VPS):
```
1) git checkout main && git pull
2) git checkout -b hotfix/descricao
3) Corrigir + commit
4) PR hotfix → main, merge, tag de patch (v0.1.1)
5) Merge main → dev para sincronizar a correção
```

## Limpeza

- Branches de fase fechadas (já merged em dev) podem ser deletadas localmente e no remote.
  ```
  git branch -d fase-0-fundacao                  # local
  git push origin --delete fase-0-fundacao       # remote
  ```
- `dev` e `main` **nunca** são deletadas.
- Tags são imutáveis — não reescrever.

## Onde estamos agora

- `main` — first commit (estrutura de pastas + 11 docs).
- `dev` — esta branch, recebe esse doc de workflow.
- Próximo: `fase-0-fundacao` será criada a partir de `dev`, e nela vai todo o scaffold de docker-compose + .NET solution + Next.js + Flutter.

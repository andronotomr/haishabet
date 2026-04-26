# Escopo funcional

Lista completa de features. Marcadas como **[MVP]** (versão 1), **[F2]** (fase 2 — pós-lançamento), **[F3]** (futuro).

---

## 1. Autenticação e onboarding (app do usuário)

- **[MVP]** Cadastro com CPF + e-mail + senha + data de nascimento (≥ 18 anos — **bloqueio rígido**).
- **[MVP]** Validação de e-mail (link ou código 6 dígitos).
- **[MVP]** Login senha + biometria (FaceID/digital) opcional.
- **[MVP]** Recuperação de senha por e-mail.
- **[MVP]** Aceite de termos de uso, política de privacidade, política de jogo responsável (versionados).
- **[MVP]** KYC nível 1 no cadastro: nome completo, CPF, data nascimento, endereço (validados via API SERPRO/Receita ou bureau como Serasa/Idwall).
- **[F2]** KYC nível 2 (envio de documento + selfie com prova de vida) — exigido antes do **primeiro saque**.
- **[F2]** Login social (Google/Apple) — opcional.
- **[F2]** 2FA por SMS/TOTP.

## 2. Carteira (wallet)

- **[MVP]** Saldo em tempo real (atualizado via WebSocket).
- **[MVP]** Extrato paginado com filtros (data, tipo: depósito, aposta, prêmio, saque, bônus).
- **[MVP]** Múltiplos "buckets" de saldo: **real**, **bônus**, **liberado** (para regras de rollover).
- **[F2]** Conversão de moeda / cotação (se houver crypto futuramente).

## 3. Depósitos

- **[MVP]** PIX (QR Code estático e Copia-e-Cola) — valor mínimo R$ 10, máximo configurável (ex: R$ 5.000/dia).
- **[MVP]** Webhook do PSP confirma pagamento → credita carteira.
- **[MVP]** Histórico de depósitos.
- **[MVP]** **CPF do pagador deve bater com CPF do titular da conta** (anti-laranja, exigência regulatória).
- **[F2]** Cartão de crédito (em geral evitado em apostas — alto chargeback).
- **[F3]** Boleto.

## 4. Saques

- **[MVP]** Saque via PIX (chave precisa ser do CPF do titular).
- **[MVP]** Fila de aprovação: automático até R$ X, manual acima → admin aprova.
- **[MVP]** Antifraude: bloqueio se houver KYC pendente, padrão suspeito (depósito + saque sem jogar = "wash"), saque > 5× depósito, conta nova.
- **[MVP]** Cancelamento de saque enquanto pendente (volta pro saldo).
- **[MVP]** Notificação push/e-mail em cada mudança de status.

## 5. Catálogo de jogos

- **[MVP]** Categorias: **Slots/Tigrinhos**, **Crash** (aviator-like), **Cassino ao vivo** (depois via agregador).
- **[MVP]** Lobby com busca, filtro por provedor, "favoritos", "jogados recentemente".
- **[MVP]** Modo demo (sem dinheiro) para usuário não-logado experimentar.
- **[MVP]** Mínimo 5 jogos próprios no MVP: 3 tigrinhos com temas distintos + 1 crash + 1 dado/raspadinha.
- **[F2]** Integração com agregador (Pragmatic Play, Spribe, Evolution) — adiciona 100+ jogos.
- **[F2]** Promoções/torneios in-game.

## 6. Mecânica de jogo (slots)

Detalhado em [jogos.md](jogos.md). Resumo:

- **[MVP]** Aposta por giro configurável (ex: R$ 0,50 a R$ 100).
- **[MVP]** RNG no servidor, resultado validado por hash (provably fair opcional).
- **[MVP]** RTP teórico de 95% por padrão, configurável por jogo.
- **[MVP]** Free spins / multipliers / scatter / wild conforme jogo.
- **[MVP]** Auto-spin com limite de N giros + stop on win acima de X.
- **[MVP]** Animações fluidas (Flutter — 60fps) com possibilidade de "pular animação".

## 7. Bônus e fidelidade

- **[F2]** Bônus de boas-vindas (% match no primeiro depósito, com rollover).
- **[F2]** Cashback semanal (% das perdas líquidas devolvido).
- **[F2]** Free spins promocionais.
- **[F2]** Sistema de níveis / VIP.
- **[F3]** Programa de afiliados (rev share / CPA).

## 8. Jogo responsável (obrigatório por regulamentação)

- **[MVP]** Limite diário/semanal/mensal de depósito (usuário define).
- **[MVP]** Limite de perda.
- **[MVP]** Limite de tempo de sessão (avisos a cada X min).
- **[MVP]** **Autoexclusão** (24h, 7d, 30d, 6 meses, indeterminado) — bloqueia login e marketing.
- **[MVP]** Link visível para CVV / "Jogadores Anônimos" / canais de ajuda.
- **[MVP]** Mensagem em todas as telas de jogo: "Aposte com responsabilidade. +18".

## 9. Notificações

- **[MVP]** Push (via FCM/APNs) — saque aprovado, depósito recebido, prêmio grande.
- **[MVP]** E-mail transacional (cadastro, recuperação, comprovantes).
- **[F2]** SMS (custo alto — só para 2FA e alertas críticos).
- **[F2]** Inbox in-app.

## 10. Suporte

- **[MVP]** Chat de suporte via integração externa (Tawk.to, Zendesk, ou Intercom).
- **[MVP]** FAQ in-app.
- **[F2]** Chatbot com IA para perguntas frequentes.

---

## Dashboard administrativo (web)

### A. Acesso e governança
- **[MVP]** Login admin com 2FA obrigatório (TOTP).
- **[MVP]** Roles: `superadmin`, `financeiro`, `kyc`, `suporte`, `marketing`, `auditoria`.
- **[MVP]** Audit trail de toda ação (quem, quando, o que mudou — antes/depois).
- **[MVP]** IP allowlist opcional para roles sensíveis.

### B. Usuários
- **[MVP]** Busca por CPF/e-mail/ID.
- **[MVP]** Ver perfil, histórico de KYC, sessões, dispositivos.
- **[MVP]** Bloquear / desbloquear / forçar logout.
- **[MVP]** Aplicar autoexclusão administrativa (suspeita de vício, fraude).
- **[MVP]** Ajuste manual de saldo (com motivo obrigatório, vai pro audit trail).

### C. Financeiro
- **[MVP]** Fila de saques pendentes (aprovar/recusar com motivo).
- **[MVP]** Visão de depósitos (filtros por status, PSP, valor).
- **[MVP]** Reconciliação PSP × banco × wallet interna.
- **[MVP]** Estornos / chargebacks.
- **[F2]** Limites configuráveis por usuário (VIP).

### D. Jogos
- **[MVP]** Catálogo: ativar/desativar jogo, ajustar RTP (dentro de faixa permitida), ajustar limites de aposta.
- **[MVP]** Manutenção programada (banner + bloqueio).
- **[F2]** Configuração de torneios.

### E. KYC / compliance
- **[MVP]** Fila de KYC pendente (documento + selfie) — aprovar/recusar.
- **[MVP]** Lista de PEPs / sanções (importar e bloquear).
- **[MVP]** Relatório de operações suspeitas (COAF/AML).

### F. Bônus / marketing
- **[F2]** Criar campanha (cupom, segmento, valor, rollover, data).
- **[F2]** Segmentação (LTV, dias inativo, depósito médio).
- **[F2]** A/B teste de promoções.

### G. Relatórios
- **[MVP]** GGR (Gross Gaming Revenue) diário/semanal/mensal por jogo.
- **[MVP]** NGR (Net = GGR − bônus − chargebacks − impostos).
- **[MVP]** RTP real × teórico por jogo (com alerta se desviar).
- **[MVP]** Cohort de retenção, LTV, CAC (depois com marketing).
- **[MVP]** Top jogadores (volume, prêmio, perda).
- **[MVP]** Exportação CSV/XLSX.

### H. Configuração
- **[MVP]** Limites globais (depósito mín/máx, saque mín/máx).
- **[MVP]** Termos de uso versionados.
- **[MVP]** Feature flags.

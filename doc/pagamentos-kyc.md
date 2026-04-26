# Pagamentos, KYC e antifraude

## Pagamentos

### PIX é praticamente o único caminho viável no MVP brasileiro

- 100% dos players brasileiros têm acesso, liquidação instantânea, custo baixo (R$ 0,01–0,99 por transação dependendo do PSP).
- Cartão de crédito em apostas tem chargeback altíssimo (operadoras desencorajam, alguns bancos bloqueiam MCC 7995). Evitar no MVP.
- Boleto: lento, baixa conversão. Pular.

### PSP (Payment Service Provider)

Não dá pra integrar direto no Banco Central — precisa ser instituição autorizada. Use um PSP que abstrai isso.

| PSP | Pontos |
|---|---|
| **Pagar.me** | Robusto, doc boa, integração OK, taxa média |
| **Asaas** | Foco em PIX, taxa competitiva, bom para SaaS |
| **EFI Bank (antiga Gerencianet)** | API PIX direta, taxa baixa (R$ 0,01 PIX recebido), curva pequena |
| **Mercado Pago** | Marca forte, integração estável, taxa maior |
| **Iugu** | Estável, bom para recorrência |
| **Especialistas em iGaming** | Pagsmile, BS2, EBANX — taxas competitivas para volume alto, com features anti-fraude para apostas |

**Recomendação MVP:** começar com **EFI** ou **Asaas** (taxa baixa, doc clara, sandbox simples). Migrar para PSP especialista quando volume justificar.

### Sandbox vs produção (todos os PSPs funcionam assim)

Cada PSP expõe **duas APIs paralelas**:

| Ambiente | Para que serve | O que muda |
|---|---|---|
| **Sandbox** | Desenvolvimento e testes | Credenciais de teste, PIX gerado **não vale dinheiro real** (não é pagável em banco), webhooks vêm com dados fake, transações ficam num banco isolado do PSP |
| **Produção** | Operação real | Credenciais aprovadas após cadastro/homologação, PIX real, webhooks reais, dinheiro de verdade |

**Como funciona na prática:**
1. Você cria conta no PSP (gratuito) → recebe credenciais de **sandbox** imediatamente.
2. Desenvolve toda a integração apontando para a URL de sandbox (ex: `api-sandbox.efipay.com.br`).
3. Testa fluxos: criação de cobrança PIX, webhook de pagamento, payout PIX, erros, idempotência. Tudo sem risco financeiro.
4. Quando o app está pronto, faz o **processo de homologação** com o PSP (envia documentos da empresa, comprova fluxo correto, alguns exigem testes específicos).
5. PSP libera credenciais de **produção**.
6. Você troca a URL para `api.efipay.com.br` (e as credenciais) na sua config — `appsettings.Production.json` ou variáveis de ambiente. Mesmo código, ambiente diferente.

> **Por isso temos `PSP_SANDBOX=true` no `.env` de dev** — bate na API de sandbox. Em produção vira `false`.

### Fluxo de depósito

```
[Usuário] → toca "Depositar" → escolhe valor
[App] → POST /deposits  {amount}
[Backend] → cria registro deposits (status=pending)
         → chama PSP: cria cobrança PIX
         → recebe {qrcode, copia_e_cola, expires_at}
         → retorna ao app
[App] → exibe QR Code + Copia-e-Cola, polling status
[Usuário paga PIX no app do banco]
[PSP] → POST /webhooks/psp/pagarme  {charge_id, status:paid, payer_cpf}
[Backend] → valida HMAC do webhook
         → idempotency check (charge_id)
         → valida payer_cpf == users.cpf  (anti-laranja)
         → transação SQL: cria wallet_transactions(credit) + atualiza wallets.balance_real
         → emite evento 'deposit.completed' (WebSocket → app, push notification)
[App] → vê saldo atualizado em tempo real
```

### Fluxo de saque

```
[Usuário] → "Sacar" → informa valor + chave PIX
[App] → POST /withdrawals {amount, pix_key, pix_key_type}
[Backend] → valida saldo, KYC nível 2 OK, sem autoexclusão
         → calcula risk_score (regras + ML futuro)
         → bloqueia valor: move balance_real → balance_locked (transação SQL)
         → cria withdrawals (status=requested ou reviewing se risco alto)
         → enfileira no Hangfire
[Worker] → se auto-aprovação (risco baixo + valor pequeno): chama PSP payout PIX
         → senão: aguarda admin aprovar via dashboard
[Admin] → aprova ou rejeita (com motivo)
[Backend] → se aprovado: chama PSP payout, status=paid
         → se rejeitado: devolve balance_locked → balance_real, status=rejected
[PSP webhook] → confirma payout efetivado (ou falha → re-fila ou cancela)
```

### Reconciliação

Job diário compara:
- Total creditado em `wallets` por depósito × total recebido pelo PSP × extrato bancário do PSP.
- Total debitado por saque × total pago pelo PSP × extrato bancário.
- Divergência → alerta no Slack, pendência no admin.

---

## KYC (Know Your Customer)

### Por que existe
- **Lei 14.790/2023 + Portarias SPA/MF**: cadastro deve confirmar identidade real, ≥18 anos, não-PEP de risco, não-sancionado.
- **Lei 9.613/98 (Lavagem de Dinheiro) + Lei 12.683/12**: operadora de apostas é sujeita à comunicação de operações suspeitas ao **COAF** (controle de patrimônio/movimentações atípicas).

### Níveis

| Nível | Quando | O que valida |
|---|---|---|
| 0 | Antes do cadastro | Nada |
| **1** | No cadastro | CPF válido na Receita, nome bate, ≥18, não-falecido, endereço plausível |
| **2** | Antes do **1º saque** | Documento (RG/CNH) + selfie com prova de vida, comparação biométrica com foto do doc |
| 3 | Para limites altos / VIP | Comprovante de renda, origem dos recursos (PEP/COAF) |

### Provedores
- **Idwall**, **Unico**, **Caf.io** — biometria + OCR de documento + face match + listas restritivas (PEP, OFAC, COAF).
- **Datavalid (SERPRO)** — base oficial da Receita, valida CPF/foto (preço por consulta).
- **BigDataCorp**, **Serasa Experian** — bureaus para enriquecimento (renda, score, dívidas).

Todos têm API REST. Integração: cliente envia foto → backend chama provedor → recebe score 0–100 + flags → decide automático ou fila manual.

### Listas de risco (obrigatório consultar)
- **PEP** (Pessoas Politicamente Expostas — receita federal mantém lista).
- **COAF / Sanções ONU / OFAC** — bloquear cadastro.
- **Falecidos** (CPF cancelado).

---

## Antifraude e jogo responsável (visão técnica)

### Sinais de fraude / lavagem
- Depósito + saque sem jogar (ou apostando o mínimo só pra "lavar") → **wash trading**.
- Múltiplas contas com mesmo IP, device fingerprint, chave PIX, endereço.
- Padrão "1 grande prêmio + saque imediato" em conta nova.
- Conta dorment de repente faz movimentação alta.
- CPF do pagador PIX ≠ CPF do titular (indicador de "laranja").

### Sinais de vício (jogo problemático)
- Aumento progressivo de depósito/aposta.
- Sessões > 4h sem pausa.
- Tentativa de elevar limite logo após perda.
- Depósitos múltiplos no mesmo dia após perdas.
- Uso noturno excessivo.

Sistema de **score de risco** combinado, com triggers para:
- Avisar usuário (popup "Você está jogando há X horas").
- Bloquear novos depósitos automaticamente.
- Acionar autoexclusão sugerida.
- Alertar admin/COAF.

### Device fingerprint
Bibliotecas: FingerprintJS, ou implementação própria com hash de (UA + screen + plugins + canvas + timezone). Salvar por sessão para detectar multi-conta.

---

## Conformidade contábil (resumo)

- Toda transação fica no ledger imutável (`wallet_transactions`) — fonte para fechamento contábil mensal.
- Relatório **GGR** (Gross Gaming Revenue) = total apostado − total pago em prêmios.
- Relatório **NGR** (Net Gaming Revenue) = GGR − bônus − impostos − chargebacks.
- Brasil tributa apostas: **operadora paga ~12% sobre GGR** (Lei 14.790, contribuição de educação/saúde/segurança/turismo + IR/CSLL etc.) e jogador paga **15% de IR sobre prêmio líquido positivo anual** acima do limite. O sistema deve calcular e reter quando aplicável.

# Regulamentação — Brasil

> Este documento é referência técnica para guiar requisitos do produto. **Não é parecer jurídico**. Antes de operar comercialmente, contratar advogado especialista em iGaming.

## Marco legal

| Norma | Resumo |
|---|---|
| **Lei 14.790/2023** | Marco regulatório das apostas de quota fixa e jogos online no Brasil. Define o regime, tributação, exigências para operadoras. |
| **Portaria SPA/MF 1.330/2023** e seguintes | Regulamentam licenciamento, requisitos técnicos, jogo responsável, prevenção a lavagem. |
| **Lei 9.613/98 + 12.683/12** | Lavagem de dinheiro — operadora é sujeita a comunicar operações suspeitas ao COAF. |
| **LGPD (Lei 13.709/18)** | Tratamento de dados pessoais (CPF, biometria, financeiro). |
| **Código de Defesa do Consumidor** | Termos claros, direito de arrependimento (limitado em jogos), suporte. |
| **Estatuto da Criança e do Adolescente** | Proibição absoluta a menores de 18. |

## Quem regula

- **SPA/MF** — Secretaria de Prêmios e Apostas, vinculada ao Ministério da Fazenda. É o órgão regulador e licenciador.
- **COAF** — Conselho de Controle de Atividades Financeiras (recebe comunicações de operações suspeitas).
- **ANPD** — Autoridade Nacional de Proteção de Dados (LGPD).
- **Anatel / Receita / PF** — fiscalizam tangencialmente (publicidade, tributos, crimes).

## Para operar comercialmente — requisitos resumidos

1. **CNPJ no Brasil**, sede no país, capital social mínimo (R$ 30 milhões em valores recentes).
2. **Outorga / licença** da SPA/MF: taxa única na casa de **R$ 30 milhões por 5 anos** (operadora pode ofertar até 3 marcas).
3. **Domínio `.bet.br`** (registro reservado).
4. **Servidores no Brasil** (cópia local de logs e dados é exigida).
5. **Certificação técnica** dos sistemas de jogo (RNG, RTP) e da plataforma por laboratório credenciado (GLI, BMM, eCOGRA…).
6. **Compliance**: programa de prevenção à lavagem, oficial de compliance, política de jogo responsável, canal de denúncias.
7. **KYC obrigatório** com biometria e validação de CPF/idade.
8. **PIX vinculado ao CPF do titular** — proibido pagar/receber em CPF de terceiro.
9. **Auto-exclusão** funcional, banimento de menores de 18 e de PEPs de risco proibido.
10. **Publicidade** restrita: não direcionar a menores, não usar atletas/celebridades de forma enganosa, sempre exibir aviso de risco.

## Tributação (resumo executivo)

- **Operadora**: contribuição sobre GGR (≈12% combinando educação, saúde, segurança pública, turismo, esportes) + IR/CSLL/PIS/Cofins sobre lucro.
- **Jogador**: IR de **15%** sobre prêmio líquido positivo anual acima do limite (apurado no Carnê-Leão / declaração anual). Operadora pode/deve reter na fonte em prêmios elevados.

## Jogo responsável — exigências técnicas que entram no produto

- Mensagem visível em todas as telas: "Aposte com responsabilidade. +18".
- Limites configuráveis pelo usuário (depósito, perda, tempo).
- **Autoexclusão** funcional (24h, 7d, 30d, 6m, indeterminado) com bloqueio de login e remoção de marketing.
- Pop-up de "tempo de sessão" (X minutos jogando).
- Link para canais de ajuda (Jogadores Anônimos BR, CVV).
- Histórico de gastos visível ao usuário.
- Proibição de crédito (não emprestar saldo, não financiar apostas).

## Implicação para o desenvolvimento

Para **MVP local em ambiente de desenvolvimento**, sem operação comercial, sem aceitar dinheiro real de terceiros, **não há restrição** — você pode construir e testar livremente.

Para **lançar publicamente recebendo dinheiro real**, é mandatório ter a licença SPA/MF.

**Caminho do projeto:**
1. **Fases 0–4** — MVP local + staging em VPS (sem público real, sem dinheiro real). Sem exigência regulatória até aqui.
2. **Fase 5–6** — beta fechado com saldo simulado (cupons "casa", sem PIX real). Ainda fora do escopo regulatório.
3. **Pré-fase 7** — em paralelo ao polimento do produto, iniciar o processo de:
   - Constituição da PJ no Brasil (CNPJ, capital social, sede).
   - Levantamento de capital (R$ 30M de outorga + capital social mínimo).
   - Contratação de advogado especialista em iGaming + compliance officer.
   - Adequação técnica para certificação (math sheets, RNG audit, infra BR, KYC nível 2 com biometria).
   - Solicitação de licença SPA/MF.
   - Contratação de PSPs e bureaus de KYC em modo produção (após homologação).
5. **Fase 7+** — operação comercial regulada com nossa própria licença e infra no Brasil.

---

> **Importante:** o cenário regulatório evoluiu rápido entre 2023 e 2026. Antes de qualquer movimento comercial, validar com a versão **vigente** das portarias SPA/MF (consultar [gov.br/fazenda/pt-br/assuntos/spa](https://www.gov.br/fazenda/pt-br/assuntos/spa)) e com advogado especialista.

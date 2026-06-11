# Modelo Share-to-Unlock

> Regras canônicas do modelo de compartilhamento e desbloqueio de
> funcionalidades premium. Referência: Constitution, princípio IV.

## Conceito

Inspirado na estrutura do Passei Direto: o usuário fornece dados gratuitamente
e ganha acesso premium. Aqui, o dado é a **observação de preço** — produto,
preço, mercado, data — anonimizada e enviada por ação explícita do usuário.

- Os dados pessoais ficam no dispositivo. Compartilhar é sempre **opt-in por
  envio** (nunca automático, nunca em segundo plano).
- Cada contribuição aceita gera **créditos**.
- Créditos desbloqueiam funcionalidades premium por período ou
  permanentemente, conforme tabela abaixo.

## O que é compartilhado (payload anonimizado)

| Campo              | Compartilhado | Observação                            |
|--------------------|---------------|---------------------------------------|
| Nome do produto    | Sim           | Normalizado                           |
| Marca              | Sim           |                                       |
| Peso/volume/unidade| Sim           |                                       |
| EAN (se houver)    | Sim           | Chave de deduplicação preferencial    |
| Preço observado    | Sim           |                                       |
| Mercado (nome/cidade) | Sim        | Sem geolocalização exata do usuário   |
| Data da observação | Sim           | Truncada para o dia                   |
| Identidade do usuário | **Não**    | Apenas um ID pseudônimo p/ creditar   |
| Carrinho completo / orçamento | **Não** | Nunca sai do dispositivo         |

## Regras de Crédito

| Evento                                            | Créditos |
|---------------------------------------------------|----------|
| Observação de preço aceita (produto já conhecido) | 1        |
| Produto inédito no catálogo colaborativo          | 5        |
| Observação com EAN válido                         | +1 bônus |
| Observação rejeitada (validação/anti-fraude)      | 0        |

Anti-abuso (validação no backend): limite de envios/dia por usuário,
detecção de preços fora de faixa plausível (z-score vs. histórico do produto),
deduplicação por (EAN|produto normalizado + mercado + dia).

## Tabela de Desbloqueios

| Funcionalidade premium                       | Custo (créditos) | Duração   |
|----------------------------------------------|------------------|-----------|
| Histórico de preços do catálogo colaborativo | 10               | 30 dias   |
| Comparação de preços entre mercados          | 20               | 30 dias   |
| Exportação (CSV/planilha) de compras         | 15               | permanente|
| Listas inteligentes (sugestão por histórico) | 30               | 30 dias   |
| Estatísticas avançadas de gastos             | 25               | 30 dias   |

> Valores iniciais; calibrar com dados reais. Mudanças exigem atualização
> deste documento e da spec correspondente.

## Estados da conta

- **Anônimo (somente local):** todas as funções do fluxo principal, sem
  créditos nem catálogo colaborativo.
- **Identificado (conta leve):** pode compartilhar, acumular créditos e
  desbloquear funcionalidades. Sincronização opcional dos próprios dados.

## Invariantes

1. Nenhuma funcionalidade do fluxo principal (F1–F5 da visão) fica atrás de
   paywall/creditwall.
2. Crédito só é concedido após validação do payload no backend.
3. O usuário pode excluir sua conta; contribuições já anonimizadas permanecem
   no catálogo (são anônimas por construção).

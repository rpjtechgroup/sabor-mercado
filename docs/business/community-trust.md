# Confiança Comunitária e Gamificação

> Regras canônicas de votação, reputação, denúncias e conquistas no catálogo
> colaborativo. Complementa `share-to-unlock.md`. Referência: Constitution IV e V.

## Objetivo

Incentivar contribuições de qualidade e permitir **moderação autônoma** pela
comunidade: usuários avaliam observações compartilhadas, acumulam reputação
(por pseudônimo) e sinalizam comportamentos suspeitos — sem expor identidade
pessoal.

## Princípios

1. **Pseudônimo, não identidade**: reputação e denúncias referem-se ao
   `contributorPseudonymId`, nunca ao e-mail.
2. **Determinístico**: pontuação de confiança, ocultação e restrições seguem
   fórmulas fixas; proibido LLM em runtime (Constitution III).
3. **Opt-in e premium**: votação e denúncia exigem conta autenticada; leitura
   detalhada de observações exige desbloqueio colaborativo (mesmo gate do
   histórico colaborativo).
4. **Fluxo principal intocado**: carrinho, orçamento e catálogo local permanecem
   gratuitos e offline.

## Votação (upvote / downvote)

| Regra | Valor |
|-------|-------|
| Alvo | Observação de preço **aceita** no catálogo colaborativo |
| Quem pode votar | Usuário autenticado (1 voto por observação; pode alterar ou remover) |
| Valores | `+1` (útil/confiável) ou `-1` (duvidoso/incorreto) |
| Auto-voto | Proibido votar na própria observação |
| Efeito na observação | `netScore = upvotes − downvotes` |

### Ocultação automática de observação

| Condição | Efeito |
|----------|--------|
| `netScore ≤ −3` | Observação oculta da busca e listagens públicas |
| Observação rejeitada pelo anti-fraude | Não elegível a votos |

## Reputação do contribuidor

Agregada por `contributorPseudonymId`:

```
trustScore = clamp(0, 100,
  50
  + (totalUpvotesReceived − totalDownvotesReceived) × 2
  + min(acceptedContributions, 20)
  − reportCount × 5
  − (isRestricted ? 15 : 0)
)
```

| Faixa | Rótulo na UI |
|-------|----------------|
| 70–100 | Confiável |
| 40–69 | Neutro |
| 0–39 | Cautela |

Exibido ao lado das observações do contribuidor (sem revelar quem é).

## Denúncias (report)

Motivos catalogados (`ReportReason`):

| Código | Descrição |
|--------|-----------|
| `misleading-price` | Preço enganoso ou fora da realidade |
| `spam` | Envios repetitivos ou irrelevantes |
| `suspected-bot` | Padrão automatizado / comportamento de bot |
| `duplicate-abuse` | Abuso de duplicatas para manipular dados |
| `off-topic` | Conteúdo fora do escopo (não é observação de preço) |
| `other` | Outro (exige texto até 500 caracteres) |

| Regra | Valor |
|-------|-------|
| Quem pode denunciar | Usuário autenticado |
| Auto-denúncia | Proibido denunciar o próprio pseudônimo |
| Duplicata | Máx. 1 denúncia por (denunciante, alvo, observação?, motivo) a cada 24 h |
| Privacidade | Denunciante não é revelado ao denunciado |

### Restrição automática do contribuidor

| Condição | Efeito |
|----------|--------|
| ≥ 3 denúncias de **denunciantes distintos** em 7 dias (mesmo pseudônimo alvo) | `isRestricted = true` por 7 dias |
| Contribuidor restrito tenta compartilhar | `403 CONTRIBUTOR_RESTRICTED` |

## Conquistas (achievements)

Desbloqueio automático, sem custo de créditos. Exibidas em **Minha conta**.

| Código | Título | Critério |
|--------|--------|----------|
| `first-contribution` | Primeira contribuição | 1ª observação aceita |
| `contributor-10` | Colaborador ativo | 10 observações aceitas |
| `contributor-50` | Pilar da comunidade | 50 observações aceitas |
| `trusted-voice` | Voz confiável | `trustScore ≥ 70` |
| `community-helper` | Ajudante da comunidade | ≥ 25 upvotes recebidos em observações |
| `quality-observation` | Preço bem avaliado | Alguma observação com `netScore ≥ 5` |

Conquistas são **permanentes** e vinculadas à conta (`userId`), não ao pseudônimo
visível publicamente.

## Créditos bônus por gamificação (fase 2 — opcional)

Calibrar após dados reais. Valores iniciais sugeridos:

| Evento | Créditos extras |
|--------|-----------------|
| Desbloquear conquista `first-contribution` | +2 (uma vez) |
| Desbloquear `trusted-voice` | +5 (uma vez) |

> Implementação de bônus exige atualização desta tabela antes do código.

## Invariantes

1. Votos e denúncias nunca expõem e-mail ou `userId` de terceiros.
2. Moderação automática não remove dados; apenas oculta ou restringe temporariamente.
3. Contribuições já anonimizadas permanecem após exclusão de conta (Constitution IV).
4. Novos endpoints devem constar em `docs/standards/api-standards.md`.

# Feature Specification: Confiança Comunitária e Gamificação

**Feature Branch**: `006-community-trust`

**Created**: 2026-06-12

**Status**: Draft

**Input**: Achievements, upvotes/downvotes em informações compartilhadas, denúncia
de usuários (preço enganoso, bot, etc.) para moderação autônoma e reputação.

Regras canônicas: [`docs/business/community-trust.md`](../../docs/business/community-trust.md)

## User Scenarios & Testing

### User Story 1 — Votar em observação compartilhada (Priority: P1)

Usuário logado com histórico colaborativo desbloqueado consulta observações de um
produto e marca se o preço compartilhado parece confiável (upvote) ou duvidoso
(downvote).

**Why this priority**: É o mecanismo central de moderação autônoma pedido pelo
usuário.

**Independent Test**: Buscar produto → listar observações → votar → ver contagem
atualizada e impossibilidade de votar na própria contribuição.

**Acceptance Scenarios**:

1. **Given** observação aceita de outro contribuidor, **When** usuário envia
   upvote, **Then** `netScore` aumenta e o voto fica registrado (idempotente por
   usuário).
2. **Given** usuário já votou downvote, **When** muda para upvote, **Then** o
   voto anterior é substituído (não duplicado).
3. **Given** observação do próprio pseudônimo, **When** tenta votar, **Then**
   recebe `403 SELF_VOTE_NOT_ALLOWED`.
4. **Given** `netScore ≤ −3`, **When** outro usuário busca o produto, **Then** a
   observação não aparece na listagem pública.

---

### User Story 2 — Ver reputação do contribuidor (Priority: P1)

Ao ver uma observação, o usuário vê um indicador de confiança do contribuidor
(Confiável / Neutro / Cautela) calculado deterministicamente.

**Why this priority**: Permite saber em quem confiar sem expor identidade.

**Independent Test**: Observações de contribuidores com históricos distintos de
votos exibem faixas de reputação corretas conforme fórmula canônica.

**Acceptance Scenarios**:

1. **Given** contribuidor com muitos upvotes, **When** lista observações,
   **Then** rótulo "Confiável" (`trustScore ≥ 70`).
2. **Given** contribuidor com muitas denúncias aceitas no agregador,
   **When** lista observações, **Then** rótulo "Cautela" e score reduzido.

---

### User Story 3 — Denunciar contribuidor ou observação (Priority: P1)

Usuário reporta comportamento suspeito escolhendo motivo catalogado (preço
enganoso, bot, spam, etc.).

**Why this priority**: Complementa votos com sinal forte para abuso grave.

**Independent Test**: Enviar denúncia válida → contador do alvo incrementa →
terceira denúncia distinta em 7 dias restringe contribuidor.

**Acceptance Scenarios**:

1. **Given** motivo `misleading-price`, **When** denuncia observação alheia,
   **Then** denúncia registrada com `202 Accepted`.
2. **Given** motivo `other` sem texto, **When** denuncia, **Then** `400` validação.
3. **Given** usuário tenta denunciar a si (mesmo pseudônimo), **When** envia,
   **Then** `403 SELF_REPORT_NOT_ALLOWED`.
4. **Given** contribuidor restrito, **When** tenta novo compartilhamento,
   **Then** `403 CONTRIBUTOR_RESTRICTED`.

---

### User Story 4 — Conquistas na conta (Priority: P2)

Usuário vê badges desbloqueadas (primeira contribuição, colaborador ativo, voz
confiável, etc.) em Minha conta, como estímulo de gamificação.

**Why this priority**: Reforça contribuição positiva sem bloquear fluxo principal.

**Independent Test**: Após N contribuições aceitas, badge correspondente aparece
em `/conta` e persiste após logout/login.

**Acceptance Scenarios**:

1. **Given** primeira observação aceita, **When** abre Minha conta, **Then** vê
   conquista "Primeira contribuição".
2. **Given** 10 observações aceitas, **When** abre Minha conta, **Then** vê
   "Colaborador ativo".
3. **Given** conquista já desbloqueada, **When** critério se repete, **Then** não
   duplica badge.

---

### User Story 5 — Listagem de observações no catálogo colaborativo (Priority: P2)

Na página de detalhe do produto colaborativo, usuário vê observações recentes com
preço, mercado, data, reputação, votos e ações (votar / denunciar).

**Why this priority**: Superfície única que une votação, reputação e denúncia.

**Independent Test**: Navegar de busca colaborativa → detalhe → interagir com
cada observação.

**Acceptance Scenarios**:

1. **Given** desbloqueio colaborativo ativo, **When** abre detalhe do produto,
   **Then** vê até 20 observações visíveis ordenadas por data (mais recentes).
2. **Given** sem desbloqueio, **When** chama API de observações, **Then**
   `403 PREMIUM_REQUIRED`.

---

### Edge Cases

- Observação oculta por score negativo continua no banco para auditoria; não
  aparece em buscas.
- Usuário remove voto (`DELETE`): score recalculado.
- Denúncia duplicada em 24 h: `409 REPORT_ALREADY_SUBMITTED`.
- Contribuidor sem votos nem denúncias: reputação "Neutro" (score base 50 + contribuições).
- Exclusão de conta: conquistas removidas; votos e denúncias permanecem
  anonimizados.

## Requirements

### Functional Requirements

- **FR-001**: Sistema MUST permitir upvote/downvote em observações aceitas por
  usuários autenticados (1 voto por usuário por observação).
- **FR-002**: Sistema MUST impedir auto-voto e auto-denúncia.
- **FR-003**: Sistema MUST calcular `trustScore` e rótulo conforme fórmula em
  `community-trust.md`.
- **FR-004**: Sistema MUST ocultar observações com `netScore ≤ −3` das listagens.
- **FR-005**: Sistema MUST aceitar denúncias com motivos catalogados e validar
  `other` com detalhes obrigatórios.
- **FR-006**: Sistema MUST restringir contribuidor após 3 denúncias distintas em
  7 dias.
- **FR-007**: Sistema MUST desbloquear conquistas automaticamente conforme
  tabela canônica.
- **FR-008**: PWA MUST exibir conquistas em Minha conta e ações de voto/denúncia
  no detalhe colaborativo.
- **FR-009**: APIs MUST usar `ProblemDetails` com códigos estáveis.
- **FR-010**: Fluxo principal (F1–F5) MUST permanecer gratuito e offline.

### Key Entities

- **ObservationVote**: vínculo votante (`userId`) → observação → valor (+1/−1).
- **ContributorTrust**: agregado por `contributorPseudonymId` (score, contadores,
  restrição).
- **ContributorReport**: denúncia (denunciante, alvo, observação opcional, motivo).
- **UserAchievement**: conquista desbloqueada por `userId`.

## Success Criteria

- **SC-001**: 90% dos fluxos P1 (voto, reputação, denúncia) cobertos por testes
  de integração.
- **SC-002**: Usuário identifica reputação do contribuidor em menos de 5 segundos
  na tela de detalhe.
- **SC-003**: Observação com score ≤ −3 deixa de aparecer na busca em até 1
  requisição após o voto decisivo.
- **SC-004**: Conquistas visíveis em Minha conta após critério atingido, sem
  ação manual do usuário.
- **SC-005**: Nenhum endpoint expõe e-mail de terceiros.

## Assumptions

- Votação e denúncia exigem rede e conta (não offline).
- Leitura de observações segue mesmo gate premium do histórico colaborativo.
- Bônus de créditos por conquista fica para fase 2 (documentado, não no MVP).
- Moderação humana/admin fica fora do escopo; apenas regras automáticas.

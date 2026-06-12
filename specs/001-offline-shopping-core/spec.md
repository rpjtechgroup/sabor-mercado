# Feature Specification: Núcleo Offline de Compras (Shopping + Catalog)

**Feature Branch**: `001-offline-shopping-core`

**Created**: 2026-06-11

**Status**: Draft

**Input**: User description: "Núcleo offline de compras: sessão de compra com meta de orçamento (F3), carrinho virtual com adição/edição/remoção manual de itens e ajuste de quantidade +1/+5/digitar (F2), barra de orçamento com faixas de cor e alertas determinísticos conforme o catálogo de mensagens (F4), CRUD manual de produtos do catálogo pessoal com histórico de preços (F5), persistência local (IndexedDB para entidades, localStorage para preferências). Modo Foto/OCR (F1) e backend ficam fora desta feature."

> Esta spec descreve **o quê** a feature faz. As regras de negócio canônicas vivem
> nos documentos abaixo e são referenciadas, nunca copiadas
> (`docs/standards/documentation-standards.md`):
>
> - Visão e fluxos F2–F5: [`docs/business/vision.md`](../../docs/business/vision.md)
> - Entidades e fronteiras (contextos Shopping e Catalog): [`docs/domain/domain-model.md`](../../docs/domain/domain-model.md)
> - Catálogo fechado de mensagens de status e regras de emissão: [`docs/domain/status-messages.md`](../../docs/domain/status-messages.md)
> - Princípios inegociáveis: [`.specify/memory/constitution.md`](../../.specify/memory/constitution.md)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Sessão de compra com carrinho e orçamento (Priority: P1)

A pessoa chega ao mercado, abre o app e inicia uma sessão de compra informando
opcionalmente o nome do mercado e quanto pretende gastar (meta de orçamento,
fluxo F3 da [visão](../../docs/business/vision.md)). Durante a compra, adiciona
itens manualmente ao carrinho virtual (nome, preço unitário, quantidade e
detalhes opcionais), ajusta quantidades com ações rápidas `+1` / `+5` /
`Digitar quantidade` (F2), edita ou remove itens, e vê o total e o restante do
orçamento atualizados instantaneamente. Ao final, encerra a sessão e vê o resumo.

**Why this priority**: É o coração do produto — sem sessão + carrinho não existe
controle de gasto. Tudo o mais (alertas, catálogo) se apoia nesse fluxo.

**Independent Test**: Iniciar sessão com meta R$ 300,00, adicionar 3 itens
manualmente, ajustar quantidades, remover 1 item, encerrar a sessão e conferir
total, restante e resumo — tudo com o dispositivo em modo avião.

**Acceptance Scenarios**:

1. **Given** nenhuma sessão ativa, **When** o usuário inicia uma sessão informando meta de R$ 300,00, **Then** a sessão fica ativa com total R$ 0,00 e restante R$ 300,00.
2. **Given** sessão ativa, **When** o usuário adiciona "Óleo de Soja Liza 900ml" a R$ 8,99, **Then** o item entra no carrinho com quantidade 1 e o total passa a R$ 8,99.
3. **Given** item com quantidade 1 e preço R$ 8,99, **When** o usuário toca `+1` e depois `+1`, **Then** a quantidade vira 3 e o subtotal vira R$ 26,97 (cálculo `quantidade × preço unitário` conforme F2).
4. **Given** item no carrinho, **When** o usuário toca `+5`, **Then** a quantidade aumenta em 5 e o total recalcula imediatamente.
5. **Given** item no carrinho, **When** o usuário escolhe `Digitar quantidade` e informa 12, **Then** a quantidade vira 12.
6. **Given** item no carrinho, **When** o usuário remove o item, **Then** o total recalcula sem o item.
7. **Given** sessão ativa com itens, **When** o usuário encerra a sessão, **Then** a sessão fica com status finalizado e a mensagem `SESSION_FINISHED` é exibida conforme o [catálogo](../../docs/domain/status-messages.md).
8. **Given** sessão ativa, **When** o usuário fecha e reabre o app (sem rede), **Then** a sessão e o carrinho reaparecem exatamente como estavam.

---

### User Story 2 - Barra de orçamento e alertas determinísticos (Priority: P2)

Durante a compra com meta definida, o usuário vê uma barra de progresso com
faixas de cor (verde/amarelo/laranja/vermelho) e recebe mensagens preventivas
quando cruza limiares do orçamento — todas determinísticas, vindas
exclusivamente do [catálogo de mensagens](../../docs/domain/status-messages.md)
(Constitution III; fluxo F4).

**Why this priority**: É o diferencial de valor (evitar sustos no caixa), mas
depende do carrinho da User Story 1 para existir.

**Independent Test**: Com meta R$ 100,00, adicionar itens que levem o total a
50%, 75%, 90%, 100% e acima, verificando cor da barra e mensagem emitida em
cada cruzamento, e que remover itens rearma os gatilhos conforme as regras de
emissão do catálogo.

**Acceptance Scenarios**:

1. **Given** meta definida, **When** `P` (percentual utilizado) está em cada faixa da tabela "Faixas da barra de orçamento" do [catálogo](../../docs/domain/status-messages.md), **Then** a barra exibe a cor/código correspondente (`budget-ok`, `budget-warn`, `budget-high`, `budget-over`).
2. **Given** usuário define a meta no início da sessão, **Then** a mensagem `BUDGET_SET` é exibida.
3. **Given** total abaixo de 50%, **When** uma adição faz `P` cruzar 50%, **Then** `BUDGET_HALF` é emitida uma única vez na sessão (idem `BUDGET_WARN_75`, `BUDGET_HIGH_90`, `BUDGET_REACHED` nos respectivos limiares).
4. **Given** orçamento estourado (`T > B`), **When** novo item é adicionado, **Then** `BUDGET_EXCEEDED` é emitida com o valor excedido.
5. **Given** múltiplos gatilhos na mesma mutação, **Then** apenas a mensagem de maior prioridade é emitida (ordem de prioridade do catálogo).
6. **Given** `BUDGET_HALF` já emitida, **When** remoções fazem `P` voltar abaixo de 50% e nova adição cruza 50% de novo, **Then** `BUDGET_HALF` é emitida novamente (rearme conforme regra de emissão nº 1).
7. **Given** sessão sem meta definida, **Then** nenhuma mensagem `BUDGET_*`/`PACE_*` é emitida (regra de emissão nº 4) e a barra de orçamento não é exibida.
8. **Given** condições de projeção satisfeitas (fórmulas de `E` do catálogo), **Then** `PACE_PROJECTION_OVER`/`PACE_PROJECTION_OK` são emitidas respeitando o cooldown de 5 itens ou 5 minutos.

---

### User Story 3 - Catálogo pessoal de produtos com histórico de preços (Priority: P3)

O usuário mantém seu cadastro pessoal de produtos (fluxo F5): cria, consulta,
edita e exclui produtos com nome, marca, peso/volume, unidade, categoria, EAN e
observações. Cada produto acumula um histórico de preços observados (preço,
mercado, data). Durante uma compra, o usuário pode adicionar um produto do
catálogo direto ao carrinho, aproveitando o último preço conhecido.

**Why this priority**: Acelera compras recorrentes e alimenta o futuro
share-to-unlock, mas a compra funciona sem ele.

**Independent Test**: Cadastrar um produto, registrar dois preços em datas
diferentes, editar o produto, adicioná-lo ao carrinho a partir do catálogo e
excluí-lo — tudo offline.

**Acceptance Scenarios**:

1. **Given** catálogo vazio, **When** o usuário cadastra produto com nome obrigatório e demais campos opcionais (conforme entidade `Product` do [modelo de domínio](../../docs/domain/domain-model.md)), **Then** o produto aparece na listagem.
2. **Given** produto cadastrado, **When** o usuário registra um preço com mercado e data, **Then** o histórico do produto exibe o registro em ordem cronológica decrescente.
3. **Given** produto com preço registrado, **When** o usuário o adiciona ao carrinho a partir do catálogo, **Then** o item entra com o último preço conhecido e origem `Catalog`.
4. **Given** produto cadastrado, **When** o usuário edita ou exclui o produto, **Then** a listagem reflete a mudança imediatamente.
5. **Given** item adicionado ao carrinho a partir do catálogo, **When** o produto é excluído do catálogo depois, **Then** o item do carrinho permanece intacto (snapshot do produto, conforme `CartItem.productSnapshot`).

---

### Edge Cases

- Quantidade deve ser sempre > 0 (invariante do domínio); tentativa de zerar quantidade equivale a remover o item (com confirmação).
- Preço unitário R$ 0,00 é permitido (brinde/promoção); preço negativo é rejeitado.
- Meta de orçamento é opcional; sem meta, só mensagens permitidas pela regra de emissão nº 4 do catálogo.
- Reabrir o app com sessão ativa antiga: a sessão continua ativa (o usuário decide encerrar ou abandonar).
- Iniciar nova sessão com outra ativa: o app pede para encerrar/abandonar a atual primeiro (uma sessão ativa por vez).
- Armazenamento local indisponível/corrompido: o app informa o problema e segue operável em memória durante a sessão (sem perder o fluxo da compra em andamento).
- Valores monetários sempre com 2 casas, cultura `pt-BR` (ex.: `R$ 8,99`).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Sistema MUST permitir iniciar uma sessão de compra com nome do mercado (opcional) e meta de orçamento (opcional), conforme `ShoppingSession` do [modelo de domínio](../../docs/domain/domain-model.md).
- **FR-002**: Sistema MUST permitir adicionar item manualmente ao carrinho com nome, preço unitário e quantidade (≥ 1), e campos opcionais de marca/volume/unidade; o item registra origem (`Manual` ou `Catalog` nesta feature; `Ocr` previsto no modelo para feature futura).
- **FR-003**: Sistema MUST oferecer ações rápidas `+1`, `+5` e `Digitar quantidade` em cada item, com recálculo instantâneo de subtotal e total (F2).
- **FR-004**: Sistema MUST permitir editar e remover itens do carrinho.
- **FR-005**: Sistema MUST exibir, durante sessão com meta: total atual, restante e barra de progresso com as faixas de cor exatas do [catálogo](../../docs/domain/status-messages.md) (F3).
- **FR-006**: Sistema MUST avaliar e emitir mensagens de status a cada mutação do carrinho usando exclusivamente os códigos, gatilhos, prioridades, cooldowns e regras de rearme do [catálogo](../../docs/domain/status-messages.md); no máximo 1 mensagem por mutação. Proibido criar mensagem fora do catálogo (Constitution III).
- **FR-007**: Sistema MUST permitir encerrar a sessão, exibindo `SESSION_FINISHED` com a variação economia/estouro definida no catálogo, e permitir abandonar a sessão.
- **FR-008**: Sistema MUST oferecer CRUD completo de produtos do catálogo pessoal (entidade `Product`) e registro/consulta de histórico de preços (`PriceRecord`) por produto (F5).
- **FR-009**: Sistema MUST permitir adicionar produto do catálogo ao carrinho com o último preço conhecido pré-preenchido e origem `Catalog`; o item guarda snapshot do produto (não referência viva).
- **FR-010**: Sistema MUST persistir todas as entidades localmente de forma imediata a cada mutação, e restaurar o estado completo ao reabrir o app, 100% offline (Constitution I; [`docs/standards/data-standards.md`](../../docs/standards/data-standards.md)).
- **FR-011**: Sistema MUST guardar preferências leves (ex.: última meta usada como sugestão) separadas dos dados de domínio, conforme data-standards.
- **FR-012**: Sistema MUST exibir todos os valores monetários com 2 casas decimais na cultura `pt-BR` e aceitar entrada de preço com vírgula decimal.
- **FR-013**: Toda a UI MUST estar em PT-BR; textos de mensagens de status vêm de recursos de localização, nunca fixos em tela (regra de frontend-standards).

### Key Entities

Definidas canonicamente em [`docs/domain/domain-model.md`](../../docs/domain/domain-model.md) — contextos **Shopping** e **Catalog** (cliente):

- **ShoppingSession**: sessão de compra (raiz do agregado Shopping).
- **CartItem**: item do carrinho com snapshot do produto, preço, quantidade e origem.
- **BudgetAlertState**: estado dos alertas emitidos na sessão (controle de emissão única/rearme/cooldown).
- **Product**: produto do catálogo pessoal (raiz do agregado Catalog).
- **PriceRecord**: observação de preço de um produto.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Usuário conclui uma compra completa (iniciar sessão → adicionar/ajustar itens → encerrar) 100% offline, sem nenhum erro visível (métrica do MVP na [visão](../../docs/business/vision.md)).
- **SC-002**: Total e restante do orçamento refletem qualquer mutação do carrinho de forma percebida como instantânea (< 100 ms).
- **SC-003**: 100% das mensagens exibidas pertencem ao catálogo de `status-messages.md`; todos os códigos aplicáveis a esta feature têm teste automatizado cobrindo gatilho, prioridade, emissão única, rearme e cooldown.
- **SC-004**: Após fechar e reabrir o app offline, 100% dos dados (sessão ativa, itens, produtos, histórico de preços, preferências) são restaurados.
- **SC-005**: Usuário adiciona um item manual em até 15 segundos e registra produto no catálogo em até 60 segundos em dispositivo móvel.

## Assumptions

Decisões tomadas em nome do usuário (não havia usuário disponível; fonte de
verdade: docs canônicos):

- **Uma sessão ativa por vez**: o domain-model define `BudgetAlertState` por sessão e os fluxos F2–F4 assumem uma compra corrente; múltiplas sessões simultâneas não agregam valor no MVP.
- **Quantidade inteira**: F2 define ações `+1`/`+5`/digitar; itens pesáveis podem ser registrados com preço total e quantidade 1 (peso/volume vai no snapshot do produto). Quantidade fracionada fica como evolução futura.
- **`SESSION_FINISHED` com `B` indefinido**: exibe apenas "Compra finalizada: {n} itens, total {T}." (variações de economia/estouro exigem `B`, conforme texto do catálogo).
- **Projeção sem lista planejada**: `N` (tamanho esperado da lista) não faz parte desta feature; a projeção `E` usa a regra 2 do catálogo (ritmo temporal, `n ≥ 5`, default 40 min) — a regra 1 fica latente no avaliador para quando houver lista planejada.
- **`ITEM_ADDED_OCR*` e `OCR_UNAVAILABLE`**: códigos do catálogo cujo gatilho (fluxo F1) está fora desta feature; o avaliador os reconhece como códigos válidos, mas nenhum fluxo desta feature os emite.
- **Abandono de sessão**: status `Abandoned` do domain-model é acionável pelo usuário ao tentar iniciar nova sessão com outra ativa (ou explicitamente na sessão atual); não emite mensagem (não há código no catálogo para abandono — registrado como pendência, não inventado).
- **Exclusão de produto**: exclui também seu histórico de preços (dado pessoal, local); itens de carrinho existentes não são afetados (snapshot).
- **Fora de escopo**: Modo Foto/OCR (F1), backend, share-to-unlock, exportação/backup JSON (fica para feature de dados pessoais), múltiplos dispositivos/sync.

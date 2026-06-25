# Gamificação baseada em uso

> Substitui o modelo anterior de créditos e desbloqueios premium
> (`docs/business/share-to-unlock.md`, obsoleto).

## Objetivo

Incentivar o uso real do app (cadastro de produtos, comércios, compras e login)
com conquistas e rankings, sem bloquear funcionalidades por créditos.

## Métricas coletadas (cliente)

| Métrica | Origem local |
|---------|--------------|
| Produtos cadastrados | `CatalogService.CreateProductAsync` |
| Comércios cadastrados | `StoreService.CreateStoreAsync` |
| Compras finalizadas | `ShoppingService.FinishSessionAsync` |
| Compras com orçamento respeitado | mesma sessão, `total <= budget` |
| Itens via OCR | `ShoppingService.AddItemFromOcrAsync` |
| Produtos com histórico de preço | `CatalogService.AddPriceRecordAsync` |
| Sequência de login | `AccountService` (dias consecutivos) |

Métricas persistem em IndexedDB (`gamificationMetrics`) e são sincronizadas
com o servidor via `POST /api/v1/metrics/sync` quando o usuário está logado.

## Conquistas

Catálogo canônico: `AchievementCodes` em `SaborMercado.Shared`.

Conquistas de contribuição colaborativa (compartilhamento de preços) permanecem
ativas. Conquistas de uso local são avaliadas após cada sincronização de métricas.

## Rankings

Categorias: `products`, `stores`, `purchases`, `login-streak`, `achievements`.

- Snapshots pré-calculados em `rewards_ranking_snapshots`.
- Exibição com pseudônimo anônimo (`Usuario#XXXX`).
- Top 100 por categoria + posição do usuário atual.

## Anti-fraude (MVP)

- Incremento máximo por sincronização: 25 unidades por métrica.
- Métricas derivadas são recalculadas localmente antes do envio.
- Compras com orçamento OK não podem exceder total de compras.

## Privacidade

- Dados de compra permanecem no dispositivo.
- Apenas contadores agregados são enviados ao servidor.
- Rankings não expõem e-mail nem identificadores reais.

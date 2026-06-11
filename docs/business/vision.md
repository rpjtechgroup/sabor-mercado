# Visão de Negócio — Sabor Mercado

> Documento canônico da ideia de negócio. Specs (`specs/`) devem referenciar
> este documento, não redefini-lo.

## Problema

A maioria das pessoas não quer digitar produto por produto enquanto faz
compras. Quem tenta controlar o gasto no mercado desiste porque o registro é
lento, e descobre que estourou o orçamento apenas no caixa.

## Solução

Um PWA gratuito e offline-first onde o usuário registra produtos apontando a
câmera para a etiqueta da prateleira (OCR via IA) ou, em fallback, digitando
manualmente. O app mantém carrinho virtual, meta de orçamento e alertas
preventivos em tempo real — tudo no dispositivo, sem custo.

## Fluxos Principais

### F1 — Modo Foto Inteligente
1. Usuário aponta a câmera para a etiqueta da prateleira ou para o produto.
2. A IA (OCR) identifica: **nome do produto, marca, peso/volume, preço** e
   demais detalhes disponíveis (EAN, categoria, unidade de medida).
3. O item é adicionado automaticamente ao carrinho virtual com quantidade 1.

Exemplo — foto da etiqueta `"Óleo de Soja Liza 900ml - R$ 8,99"` registra:

| Campo            | Valor                  |
|------------------|------------------------|
| Produto          | Óleo de Soja Liza      |
| Volume           | 900ml                  |
| Quantidade       | 1                      |
| Preço unitário   | R$ 8,99                |
| Total do carrinho| R$ 8,99                |

**Fallback obrigatório (Constitution II):** se o OCR falhar ou estiver
indisponível, abre o formulário manual pré-preenchido com o que foi extraído.

### F2 — Ajuste de Quantidade
Ao tocar no item do carrinho, o usuário vê ações rápidas:
- `+1`
- `+5`
- `Digitar quantidade`

O cálculo é automático e instantâneo: `quantidade × preço unitário`, com o
total do carrinho atualizado imediatamente (ex.: 3 × R$ 8,99 = R$ 26,97).

### F3 — Meta de Orçamento
Antes de entrar no mercado o app pergunta: **"Quanto pretende gastar?"**
(ex.: R$ 300). Durante a compra exibe:
- Total atual (ex.: R$ 187,42)
- Restante do orçamento (ex.: R$ 112,58)
- Barra visual de progresso com faixas de cor
  (ex.: 🟩🟩🟩🟩🟩🟩🟨⬜⬜⬜ — 62% do orçamento utilizado)

### F4 — Alertas Preventivos
Mensagens **determinísticas, pré-estabelecidas no sistema** (Constitution III),
nunca geradas por LLM em tempo real. Exemplos:
- Projeção: "Se continuar nesse ritmo, sua compra deve terminar em
  aproximadamente R$ 347."
- Estouro: "Seu orçamento foi ultrapassado em R$ 27."

Catálogo completo de mensagens e regras de disparo:
[`docs/domain/status-messages.md`](../domain/status-messages.md).

### F5 — Controle de Produtos (CRUD manual)
Independente do OCR, o usuário mantém seu cadastro pessoal de produtos com
todos os detalhes: nome, marca, peso/volume, unidade, preço, categoria, EAN,
mercado onde comprou, data, observações. Histórico de preços por produto.

## Modelo de Negócio

Gratuito no fluxo principal (dados em localStorage/IndexedDB). Receita e
crescimento vêm do modelo **share-to-unlock** (estilo Passei Direto): o
usuário que compartilha dados anonimizados de produtos/preços com o catálogo
colaborativo ganha créditos que desbloqueiam funcionalidades premium.
Detalhes: [`docs/business/share-to-unlock.md`](share-to-unlock.md).

## Métricas de Sucesso (MVP)

- Tempo médio para registrar um item por foto < 5s (incluindo OCR).
- ≥ 70% dos itens adicionados sem edição manual após OCR.
- Usuário conclui uma compra completa 100% offline.
- VM OCI (1GB RAM) sustenta o backend com p95 < 800ms nas rotas de OCR proxy.

## Fora de Escopo (MVP)

- Pagamentos / checkout real.
- Integração com programas de fidelidade de mercados.
- Apps nativos (somente PWA).
- Geração de mensagens por LLM em runtime.

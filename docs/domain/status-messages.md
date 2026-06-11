# Catálogo de Mensagens de Status

> Mensagens determinísticas pré-estabelecidas no sistema (Constitution III).
> Nenhuma mensagem deste catálogo é gerada por LLM em runtime. O cálculo
> roda no cliente, a cada mutação do carrinho.

## Variáveis de cálculo

| Símbolo | Definição                                                       |
|---------|-----------------------------------------------------------------|
| `B`     | Meta de orçamento da sessão (`budgetAmount`)                    |
| `T`     | Total atual do carrinho = Σ(preço unitário × quantidade)        |
| `R`     | Restante = `B − T`                                              |
| `P`     | Percentual utilizado = `T / B` (0 quando `B` não definido)      |
| `n`     | Quantidade de itens distintos no carrinho                       |
| `N`     | Tamanho esperado da lista (itens planejados; opcional)          |
| `E`     | Projeção de gasto final (ver fórmula abaixo)                    |

### Projeção de gasto final (`E`)

1. **Com lista planejada (`N` conhecido, `n ≥ 3`):**
   `E = (T / n) × N` — ticket médio por item × itens planejados.
2. **Sem lista (`n ≥ 5`):** projeção por ritmo temporal:
   `E = T × (duracaoMediaSessao / tempoDecorrido)`, limitada a `3 × T`,
   onde `duracaoMediaSessao` vem do histórico do usuário (default: 40 min).
3. Caso contrário: não exibir projeção (dados insuficientes).

Arredondamento: valores monetários sempre `decimal`, exibidos com 2 casas,
cultura `pt-BR`.

## Faixas da barra de orçamento

| Faixa de `P`     | Cor da barra | Código        |
|------------------|--------------|---------------|
| 0% – 59,99%      | Verde 🟩     | `budget-ok`   |
| 60% – 84,99%     | Amarelo 🟨   | `budget-warn` |
| 85% – 99,99%     | Laranja 🟧   | `budget-high` |
| ≥ 100%           | Vermelho 🟥  | `budget-over` |

## Mensagens (códigos estáveis)

Os textos são recursos de localização (`pt-BR` default). O código da mensagem
é o contrato; o texto pode ser refinado sem mudar a lógica.

| Código                 | Gatilho (avaliado em ordem; emite no máx. 1 por mutação)             | Texto (pt-BR)                                                                 |
|------------------------|----------------------------------------------------------------------|-------------------------------------------------------------------------------|
| `BUDGET_SET`           | Usuário define `B` no início da sessão                              | "Meta definida: {B}. Boa compra!"                                             |
| `BUDGET_HALF`          | `P` cruza 50% (de baixo para cima)                                  | "Você usou metade do orçamento. Restam {R}."                                  |
| `BUDGET_WARN_75`       | `P` cruza 75%                                                       | "Atenção: 75% do orçamento utilizado. Restam {R}."                            |
| `BUDGET_HIGH_90`       | `P` cruza 90%                                                       | "Quase no limite: 90% do orçamento utilizado. Restam {R}."                    |
| `BUDGET_REACHED`       | `P` cruza 100%                                                      | "Você atingiu sua meta de {B}."                                               |
| `BUDGET_EXCEEDED`      | `T > B` (a cada novo item enquanto estourado)                       | "Seu orçamento foi ultrapassado em {T−B}."                                    |
| `PACE_PROJECTION_OVER` | `E` disponível e `E > B × 1,05` e `P < 100%`                        | "Se continuar nesse ritmo, sua compra deve terminar em aproximadamente {E}."  |
| `PACE_PROJECTION_OK`   | `E` disponível e `E ≤ B` e `P ≥ 60%`                                | "No ritmo atual, você deve fechar a compra em {E}, dentro da meta."           |
| `ITEM_ADDED_OCR`       | Item adicionado via foto com `confidence ≥ 0,8`                     | "✅ {produto} adicionado. Total: {T}."                                        |
| `ITEM_ADDED_OCR_REVIEW`| Item adicionado via foto com `confidence < 0,8`                     | "Adicionei {produto}, confira os dados. Total: {T}."                          |
| `OCR_UNAVAILABLE`      | Falha de comunicação/quota com o serviço de OCR                     | "Não consegui ler a etiqueta agora. Preencha os dados manualmente."           |
| `SESSION_FINISHED`     | Usuário encerra a sessão                                            | "Compra finalizada: {n} itens, total {T}{, economia de {B−T}\|, {T−B} acima da meta}." |

### Regras de emissão

1. Mensagens de cruzamento de faixa (`BUDGET_*`) disparam **uma única vez por
   sessão** por código (controle em `BudgetAlertState`); remover itens pode
   rearmar o gatilho se `P` voltar para a faixa anterior.
2. `PACE_PROJECTION_*` tem cooldown de 5 itens ou 5 minutos entre emissões.
3. Prioridade quando múltiplos gatilhos na mesma mutação:
   `BUDGET_EXCEEDED` > `BUDGET_REACHED` > `BUDGET_HIGH_90` > `BUDGET_WARN_75`
   > `BUDGET_HALF` > `PACE_*` > `ITEM_*`.
4. Sem `B` definido: somente `ITEM_*`, `OCR_UNAVAILABLE`, `SESSION_FINISHED`.

## Implementação

- Avaliador puro: `(estadoAnterior, carrinho, orçamento) → mensagem?` —
  função sem efeitos colaterais, 100% testável (xUnit/bUnit).
- Textos em arquivos de recursos (`.resx` ou JSON i18n no cliente Blazor).
- Novos códigos exigem atualização desta tabela **antes** da implementação.

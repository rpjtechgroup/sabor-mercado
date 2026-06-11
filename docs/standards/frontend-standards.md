# Padrões de Frontend (Blazor WebAssembly PWA)

> Obrigatório para `src/SaborMercado.Web`. Reforçado pela rule
> `.cursor/rules/blazor-standards.mdc`.

## Stack

- Blazor WebAssembly standalone com template PWA (service worker + manifest).
- Mobile-first: layout projetado para uma mão, botões grandes, uso no
  corredor do mercado (luz variável, pressa).
- UI em PT-BR; cultura `pt-BR` para moeda e números.

## Estrutura

```
src/SaborMercado.Web/
  Features/
    Shopping/        # Sessão, carrinho, orçamento, alertas (F2–F4)
    Catalog/         # CRUD de produtos e histórico (F5)
    Recognition/     # Câmera, upload, revisão do OCR (F1)
    Rewards/         # Créditos e desbloqueios
  Shared/            # Componentes reutilizáveis (BudgetBar, MoneyInput...)
  Storage/           # Abstração IndexedDB/localStorage
  Interop/           # Wrappers JS (canvas resize, câmera) — único lugar com JS
  Domain/            # Modelos locais + avaliador de mensagens de status
```

## Regras

1. **Offline-first:** nenhuma feature de Shopping/Catalog pode depender de
   rede. Chamada HTTP só em Recognition, Rewards e sincronização — sempre com
   tratamento de falha que preserva o fluxo local.
2. **Persistência:** IndexedDB para entidades (sessões, produtos, preços),
   localStorage apenas para preferências leves. Toda escrita é imediata
   (usuário pode fechar o app a qualquer momento no mercado).
3. **Mensagens de status:** implementar exatamente o catálogo de
   `docs/domain/status-messages.md` — avaliador puro em `Domain/`, textos em
   recursos i18n. Proibido texto de alerta hardcoded em componente.
4. **Dinheiro:** `decimal` + formatação `pt-BR` (`R$ 8,99`). Entrada de preço
   com máscara de vírgula.
5. **Câmera:** `InputFile` com `capture="environment"`; compressão via
   interop de canvas (~1024px JPEG) antes do upload (ADR-0005).
6. **JS interop:** somente dentro de `Interop/`, com wrapper C# tipado e
   módulo JS isolado (`export function ...`). Proibido `eval`/JS inline em
   componentes.
7. **Componentes:** 1 componente = 1 responsabilidade; code-behind
   (`.razor.cs`) quando a lógica passar de ~30 linhas; parâmetros documentados.
8. **Estado:** serviços com `ObservableState`/eventos por feature; sem
   bibliotecas de state management pesadas no MVP.

## Performance do PWA

- Publish com compressão Brotli (servida pelo Caddy) e trimming habilitado.
- Lazy loading de assemblies de features fora do fluxo principal (Rewards).
- Service worker: cache-first para assets, network-only para `/api`.

## Testes

- bUnit para componentes com lógica (BudgetBar, avaliador de alertas via UI).
- xUnit puro para `Domain/` (avaliador de mensagens é o alvo nº 1 de testes).

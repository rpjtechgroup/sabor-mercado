# Quickstart — 001 Núcleo Offline de Compras

> Guia de validação ponta-a-ponta. Pré-requisito: SDK .NET 8 (`dotnet --list-sdks`).

## Build e testes

```powershell
dotnet build SaborMercado.sln      # deve compilar sem warnings (TreatWarningsAsErrors)
dotnet test SaborMercado.sln       # xUnit (avaliador/carrinho) + bUnit (componentes)
```

## Rodar o app

```powershell
dotnet run --project src/SaborMercado.Web
```

Abrir a URL informada (ex.: `http://localhost:5xxx`). Para validar offline:
DevTools → Network → Offline após o primeiro load (service worker em modo
publish: `dotnet publish` + servidor estático).

## Cenários de validação (espelham a spec)

1. **Sessão + carrinho (US1)**: iniciar sessão com meta `R$ 300,00` →
   mensagem `BUDGET_SET`. Adicionar "Óleo de Soja Liza 900ml" a `R$ 8,99` →
   total `R$ 8,99`. Tocar `+1` 2× → quantidade 3, subtotal `R$ 26,97`.
   `+5` → 8. `Digitar quantidade` 12 → 12. Remover item → total `R$ 0,00`.
2. **Barra e alertas (US2)**: com meta `R$ 100,00`, adicionar item de
   `R$ 55,00` → barra verde, `BUDGET_HALF`. Adicionar `R$ 22,00` →
   barra amarela (77%), `BUDGET_WARN_75`. Adicionar `R$ 15,00` → barra
   laranja (92%), `BUDGET_HIGH_90`. Adicionar `R$ 10,00` → barra vermelha,
   `BUDGET_REACHED`. Adicionar mais um item → `BUDGET_EXCEEDED` com o
   excedente. Remover itens até ~40% e readicionar até cruzar 50% →
   `BUDGET_HALF` de novo (rearme).
3. **Persistência (FR-010)**: com sessão ativa e itens, recarregar a página
   (F5) → sessão, itens e produtos restaurados; preferência de meta sugerida
   no próximo início de sessão.
4. **Catálogo (US3)**: cadastrar produto, registrar 2 preços, ver histórico
   decrescente, adicionar ao carrinho a partir do catálogo (origem `Catalog`,
   último preço pré-preenchido), editar e excluir o produto (item do carrinho
   permanece).
5. **Encerramento**: encerrar sessão → `SESSION_FINISHED` com variação
   economia/estouro conforme o caso.

## Referências

- Regras de mensagens: [contracts/status-evaluator.md](contracts/status-evaluator.md)
  e [`docs/domain/status-messages.md`](../../docs/domain/status-messages.md)
- Entidades: [data-model.md](data-model.md)

# Implementation Plan: Histórico de Compras (004)

**Branch**: `004-purchase-history-grid` | **Date**: 2026-06-11

## Summary

Histórico offline de compras (3 meses) em grid com comparação de preços por cor,
catálogo alimentado automaticamente durante compras e autocomplete no formulário de item.

## Stack

| Camada | Tecnologia |
|--------|------------|
| PWA | Blazor WASM, IndexedDB |
| Domínio | `PurchaseHistoryService`, `ProductIdentity` |
| UI | `/historico`, `ProductAutocomplete` |

## Rotas

- `/historico` — abas Por compra / Consolidado
- `/historico/{sessionId}` — detalhe de uma compra

## Comandos

```powershell
dotnet run --project src/SaborMercado.Web
dotnet test tests/SaborMercado.Web.Tests
```

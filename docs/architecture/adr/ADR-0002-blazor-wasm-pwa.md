# ADR-0002 — Blazor WebAssembly como PWA

**Status:** Aceito — 2026-06-11

## Contexto

O produto é um PWA mobile-first, offline-first, com captura de foto pela
câmera. O time é especialista em C#/.NET; React seria aceito apenas se o
Blazor não atendesse os requisitos de câmera/PWA.

## Decisão

Blazor WebAssembly standalone com template PWA (service worker + manifest):

- **Câmera:** atendida sem JS pesado — `InputFile` com
  `accept="image/*" capture="environment"` abre a câmera nativa do mobile
  para fotografar a etiqueta (modelo de captura por foto, não streaming de
  vídeo, que é exatamente o fluxo F1). Redimensionamento via pequeno interop
  de canvas.
- **Offline/armazenamento:** localStorage para preferências e IndexedDB para
  dados (interop via biblioteca fina), service worker para assets.
- **Custo de servidor:** estáticos puros — zero RAM na VM além do Caddy.
- **Stack única C#:** modelos de domínio do cliente em C#, testáveis com
  bUnit/xUnit; nada de manter dois ecossistemas.

## Consequências

- (+) C# de ponta a ponta; reuso de tipos de contrato com a API.
- (+) PWA instalável com aparência de app (manifest, ícones, standalone).
- (−) Payload inicial maior que React (~mitigado por Brotli + lazy loading +
  trimming); aceitável pois o app é instalado e cacheado pelo service worker.
- (−) Acesso a APIs de browser exige JS interop pontual (canvas, IndexedDB) —
  isolado em wrappers no projeto Web.

## Alternativas consideradas

- **React + TypeScript:** maior ecossistema PWA, porém quebra a stack única
  C# sem necessidade — os requisitos de câmera são atendidos por captura de
  foto nativa, não exigem streaming WebRTC.
- **Blazor Hybrid / MAUI:** fora de escopo, o requisito é PWA web.

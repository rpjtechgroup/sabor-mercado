# ADR-0005 — OCR via Gemini (chave do usuário no cliente)

**Status:** Aceito — emendado 2026-06-11 (revoga proxy obrigatório no servidor)

## Contexto

O Modo Foto Inteligente (F1) usa modelos gratuitos do Google. A decisão inicial
previa proxy no servidor com `GEMINI_API_KEY` centralizada. O produto evoluiu
para que **cada usuário use sua própria chave** no free tier, sem custo de
infra para OCR e sem depender do backend.

## Decisão

1. **Chave por usuário** em `localStorage` (`saborMercado.preferences.geminiApiKey`).
2. **Chamada direta** do PWA ao `generativelanguage.googleapis.com` com a chave
   do usuário (`GeminiShelfLabelClient`).
3. **Normalização** no cliente (`RecognitionNormalizer` no projeto Web).
4. **Backend Recognition** mantido como código legado/opcional, não usado pelo
   fluxo principal do PWA.
5. **Fallback manual** obrigatório quando não há chave ou a chamada falha.

## Consequências

- (+) OCR funciona sem servidor; quota é do usuário no Google.
- (+) Alinhado ao offline-first e gratuito por padrão.
- (−) Chave exposta no dispositivo do usuário (aceitável — é credencial dele).
- (−) Usuário precisa criar chave no Google AI Studio (UX em `/configuracoes`).

## Alternativas consideradas

- **Proxy no servidor com env var:** descartado — custo e quota centralizados.
- **Enviar chave do cliente ao backend:** descartado — chave trafegaria pelo
  nosso servidor sem necessidade.

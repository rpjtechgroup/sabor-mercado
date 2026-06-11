# ADR-0005 — OCR via Gemini free tier com proxy no servidor

**Status:** Aceito — 2026-06-11

## Contexto

O Modo Foto Inteligente (F1) exige extração estruturada de etiquetas de
preço. O requisito de negócio é usar os modelos gratuitos do Google, com
fallback manual obrigatório quando a comunicação falhar (Constitution II).

## Decisão

1. **Modelo:** família Gemini Flash no free tier, com saída estruturada
   (`responseSchema` JSON). Nome do modelo é configuração, não código.
2. **Proxy obrigatório no servidor:** o PWA nunca chama o Gemini; o módulo
   Recognition recebe a imagem, aplica rate-limit (por usuário e global) e
   repassa. A API key vive apenas no servidor.
3. **Compressão no cliente:** a imagem é redimensionada (~1024px JPEG) no
   dispositivo antes do upload — preserva o orçamento de memória e banda da VM.
4. **Orçamento de quota:** o rate-limit global é dimensionado abaixo da quota
   diária do free tier; quota esgotada responde `503 OCR_UNAVAILABLE` sem
   chamar o Gemini.
5. **Fallback manual:** sempre disponível e acionado automaticamente em falha,
   pré-preenchido com extração parcial quando houver.

Detalhes operacionais em `docs/architecture/ocr-integration.md`.

## Consequências

- (+) Custo zero de IA no MVP; chave protegida; abuso controlado.
- (+) Trocar de modelo (ou de provedor) é mudança de configuração/adapter.
- (−) Quota gratuita limita o volume diário de OCR — mitigado pelo fallback
  manual e, na escala, por tier pago (decisão futura com dados de uso).

## Alternativas consideradas

- **Chamada direta do cliente ao Gemini:** exporia a API key; inviável.
- **OCR local no dispositivo (Tesseract WASM):** sem custo de quota, porém
  qualidade insuficiente para etiquetas heterogêneas e payload WASM pesado.
  Pode virar fallback adicional no futuro (novo ADR).
- **Google Cloud Vision API:** melhor OCR puro, mas exige billing ativo; o
  requisito é o free tier dos modelos Google.

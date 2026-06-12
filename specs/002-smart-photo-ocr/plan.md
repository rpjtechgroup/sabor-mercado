# Plan: Modo Foto Inteligente (OCR)

## Constitution Check

- **II Degradação graciosa**: fallback manual obrigatório — PASS  
- **III Mensagens determinísticas**: códigos do catálogo, sem LLM em runtime — PASS  
- **V Caber no MVP**: módulo Recognition isolado, cache/quota in-process — PASS  
- **VI C#/.NET**: Api + Modules.Recognition + Shared — PASS  

## Estrutura implementada

```
src/SaborMercado.Shared/Recognition/     # DTOs e códigos de erro
src/SaborMercado.Modules.Recognition/    # Gemini proxy, normalização, endpoint
src/SaborMercado.Api/                    # Host + CORS
src/SaborMercado.Web/Features/Recognition/  # /foto, compressão, HTTP client
```

## Decisões

- SQLite para `recognition_logs` no desenvolvimento (schema `recognition`).
- Chave Gemini do usuário em `localStorage` (`/configuracoes`); chamada direta
  ao Google do PWA (sem backend obrigatório).
- Compressão de imagem no cliente (~1024px JPEG) antes do envio ao Gemini.

# Padrões de API (HTTP)

> Contrato entre o PWA e o backend, e entre módulos quando extraídos
> (Fase 2+). Mudanças aqui exigem versionamento.

## Convenções gerais

- Base path versionado: `/api/v1/...`. Breaking change → `/api/v2`, mantendo
  `v1` durante a janela de migração (clientes PWA instalados atualizam com
  atraso).
- Recursos em inglês, plural, kebab-case: `/api/v1/recognitions`,
  `/api/v1/price-observations`, `/api/v1/unlocks`.
- JSON camelCase; datas ISO 8601 UTC; dinheiro como número decimal
  (`8.99`) + moeda implícita BRL no MVP.
- Idempotência: POSTs de contribuição aceitam header `Idempotency-Key`.

## Erros — sempre `ProblemDetails` (RFC 9457)

```json
{
  "type": "https://sabormercado.app/errors/ocr-unavailable",
  "title": "Serviço de leitura indisponível",
  "status": 503,
  "code": "OCR_UNAVAILABLE",
  "detail": "Limite diário de leituras atingido. Use o cadastro manual."
}
```

- `code` é estável e é o que o cliente usa para decidir comportamento
  (ex.: `OCR_UNAVAILABLE` → abrir formulário manual).
- Códigos catalogados por módulo em `Contracts/ErrorCodes.cs`.

## Endpoints do MVP (visão de contrato)

| Método/rota                          | Módulo        | Descrição                                  | Auth      |
|--------------------------------------|---------------|--------------------------------------------|-----------|
| `POST /api/v1/recognitions`          | Recognition   | Foto → `RecognitionResult`                 | opcional* |
| `GET  /api/v1/starter-catalog`       | SharedCatalog | Catálogo curado inicial (JSON)               | —         |
| `POST /api/v1/price-observations`    | SharedCatalog | Contribuição anonimizada → `202 Accepted`  | requerida |
| `GET  /api/v1/shared-products?query=`| SharedCatalog | Busca no catálogo colaborativo             | premium   |
| `GET  /api/v1/shared-products/{id}/markets` | SharedCatalog | Comparação de preços por mercado    | premium   |
| `GET  /api/v1/shared-products/{id}/observations` | SharedCatalog | Observações com reputação e votos | premium   |
| `POST /api/v1/price-observations/{id}/vote` | SharedCatalog | Upvote (+1) ou downvote (−1)       | requerida |
| `DELETE /api/v1/price-observations/{id}/vote` | SharedCatalog | Remove voto do usuário          | requerida |
| `POST /api/v1/contributor-reports`   | SharedCatalog | Denúncia de contribuidor/observação        | requerida |
| `GET  /api/v1/achievements`          | Rewards       | Conquistas desbloqueadas do usuário        | requerida |
| `GET  /api/v1/credits`               | Rewards       | Saldo + extrato                            | requerida |
| `POST /api/v1/unlocks`               | Rewards       | Gastar créditos em funcionalidade          | requerida |
| `POST /api/v1/auth/register|login`   | Identity      | Conta leve (email+senha)                   | —         |
| `POST /api/v1/auth/refresh`          | Identity      | Renovar access token                       | refresh   |
| `DELETE /api/v1/auth/me`             | Identity      | Excluir conta                              | requerida |

\* anônimo com rate-limit por IP mais restrito.

## Status codes

- `200` leitura ok; `201` criado; `202` aceito p/ processamento assíncrono;
  `400` validação; `401/403` auth; `404`; `409` conflito/idempotência;
  `422` regra de negócio (ex.: créditos insuficientes → `INSUFFICIENT_CREDITS`);
  `429` rate-limit (com `Retry-After`); `503` dependência indisponível.

## Autenticação

- JWT Bearer curto (1h) + refresh token; emissão pelo módulo Identity.
- `pseudonymId` (GUID estável) é o que circula para crédito de contribuições;
  o e-mail nunca aparece em payloads de outros módulos.

## Documentação viva

- OpenAPI gerado pelo ASP.NET Core, publicado em `/api/v1/openapi.json` (dev).
- Toda spec de feature que cria/altera endpoint referencia esta tabela e a
  atualiza no mesmo PR.

# Tasks: Catálogo Inicial e Preenchimento por Voz (007)

## Phase 1 — Dados e contrato

- [X] T001 Criar `data/starter-catalog.pt-BR.json` com redes e produtos curados
- [X] T002 Adicionar `StarterCatalogDtos` em `SaborMercado.Shared`
- [X] T003 Configurar cópia do JSON para `wwwroot` e embedded resource na API

## Phase 2 — Backend

- [X] T004 Implementar `GET /api/v1/starter-catalog`
- [X] T005 Atualizar `docs/standards/api-standards.md`

## Phase 3 — Cliente bootstrap

- [X] T006 Adicionar `StarterKey` em `Store` e `Product`
- [X] T007 Implementar `StarterCatalogBootstrapService`
- [X] T008 UX onboarding em `MainLayout` e `CatalogPage`

## Phase 4 — Voz

- [X] T009 `speechRecognition.js` + `SpeechRecognitionInterop`
- [X] T010 `VoiceInputButton.razor`
- [X] T011 `VoiceUtteranceParser` determinístico PT-BR
- [X] T012 Integrar em `ProductAutocomplete` e `CartItemForm`

## Phase 5 — Testes

- [X] T013 Testes xUnit parser e bootstrap
- [X] T014 Teste bUnit `VoiceInputButton`
- [X] T015 Teste API `GET /starter-catalog`

# Implementation Plan: Catálogo Inicial e Preenchimento por Voz (007)

## Rotas

Sem rotas novas no PWA. Endpoint API: `GET /api/v1/starter-catalog`.

## Comandos

```powershell
dotnet run --project src/SaborMercado.Web
dotnet run --project src/SaborMercado.Api
dotnet test tests/SaborMercado.Web.Tests
dotnet test tests/SaborMercado.Api.Tests
```

## Artefatos

- `data/starter-catalog.pt-BR.json` — fonte única
- `SaborMercado.Shared/StarterCatalog/StarterCatalogDtos.cs`
- `StarterCatalogEndpoints.cs` + `StarterCatalogReader.cs`
- `StarterCatalogBootstrapService.cs`
- `SpeechRecognitionInterop.cs` + `speechRecognition.js`
- `VoiceUtteranceParser.cs` + `VoiceInputButton.razor`

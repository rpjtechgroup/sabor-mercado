using System.Text.Json.Serialization;

namespace SaborMercado.Web.Domain.Shopping;

/// <summary>
/// Origem do item no carrinho (docs/domain/domain-model.md). `Ocr` é emitido
/// apenas pelo fluxo F1 (feature futura), mas faz parte do contrato do modelo.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CartItemSource
{
    Ocr,
    Manual,
    Catalog,
}

using System.Text.Json.Serialization;

namespace SaborMercado.Web.Domain.Shopping;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CartItemSource
{
    Ocr,
    Manual,
    Catalog,
}

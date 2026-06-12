using System.Text.Json.Serialization;

namespace SaborMercado.Web.Domain.Catalog;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PriceSource
{
    Ocr,
    Manual,
}

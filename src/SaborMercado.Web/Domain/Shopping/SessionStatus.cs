using System.Text.Json.Serialization;

namespace SaborMercado.Web.Domain.Shopping;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SessionStatus
{
    Active,
    Finished,
    Abandoned,
}

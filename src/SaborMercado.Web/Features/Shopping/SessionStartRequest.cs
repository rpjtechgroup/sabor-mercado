using SaborMercado.Web.Domain.Shopping;

namespace SaborMercado.Web.Features.Shopping;

public sealed record SessionStartRequest(SessionKind Kind, decimal? Budget, string? MarketName);

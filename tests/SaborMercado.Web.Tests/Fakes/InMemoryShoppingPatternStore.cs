using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Tests.Fakes;

public sealed class InMemoryShoppingPatternStore : IShoppingPatternStore
{
    public Dictionary<Guid, ShoppingPattern> Patterns { get; } = [];

    public Task<ShoppingPattern?> GetAsync(Guid id) =>
        Task.FromResult(Patterns.TryGetValue(id, out var pattern) ? pattern : null);

    public Task SaveAsync(ShoppingPattern pattern)
    {
        Patterns[pattern.Id] = pattern;
        return Task.CompletedTask;
    }
}

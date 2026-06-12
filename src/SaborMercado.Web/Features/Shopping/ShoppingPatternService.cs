using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Shopping;

public sealed class ShoppingPatternService(
    IShoppingPatternStore store,
    CatalogService catalog,
    TimeProvider clock)
{
    public event Action? StateChanged;

    public async Task<ShoppingPattern> GetOrCreateAsync()
    {
        var existing = await store.GetAsync(StorageSchema.DefaultPatternId);
        if (existing is not null)
        {
            return existing;
        }

        var pattern = new ShoppingPattern
        {
            Id = StorageSchema.DefaultPatternId,
            UpdatedAt = clock.GetUtcNow(),
        };
        await store.SaveAsync(pattern);
        return pattern;
    }

    public async Task AddProductAsync(Guid productId, int defaultQuantity = 1)
    {
        await catalog.InitializeAsync();
        if (catalog.GetProduct(productId) is null)
        {
            throw new InvalidOperationException("Produto não encontrado no catálogo.");
        }

        var pattern = await GetOrCreateAsync();
        if (pattern.Items.Any(i => i.ProductId == productId))
        {
            return;
        }

        pattern.Items.Add(new PatternItem { ProductId = productId, DefaultQuantity = defaultQuantity });
        pattern.UpdatedAt = clock.GetUtcNow();
        await store.SaveAsync(pattern);
        NotifyStateChanged();
    }

    public async Task RemoveProductAsync(Guid productId)
    {
        var pattern = await GetOrCreateAsync();
        pattern.Items.RemoveAll(i => i.ProductId == productId);
        pattern.UpdatedAt = clock.GetUtcNow();
        await store.SaveAsync(pattern);
        NotifyStateChanged();
    }

    public async Task UpdateQuantityAsync(Guid productId, int quantity)
    {
        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        var pattern = await GetOrCreateAsync();
        var item = pattern.Items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new InvalidOperationException("Produto não está na lista mensal.");

        item.DefaultQuantity = quantity;
        pattern.UpdatedAt = clock.GetUtcNow();
        await store.SaveAsync(pattern);
        NotifyStateChanged();
    }

    public async Task<IReadOnlyList<PatternLineView>> GetLinesAsync()
    {
        await catalog.InitializeAsync();
        var pattern = await GetOrCreateAsync();
        var lines = new List<PatternLineView>();

        foreach (var item in pattern.Items)
        {
            var product = catalog.GetProduct(item.ProductId);
            if (product is null)
            {
                continue;
            }

            var lastPrice = await catalog.GetLastKnownPriceAsync(item.ProductId);
            lines.Add(new PatternLineView(
                product,
                item.DefaultQuantity,
                lastPrice?.Price));
        }

        return lines.OrderBy(l => l.Product.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}

public sealed record PatternLineView(Product Product, int DefaultQuantity, decimal? LastPrice);

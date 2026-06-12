using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Features.Shopping;
using SaborMercado.Web.Storage;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Features;

public class ShoppingPatternServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryShoppingPatternStore _patternStore = new();
    private readonly InMemoryCatalogStore _catalogStore = new();
    private readonly FixedTimeProvider _clock = new(Now);

    private ShoppingPatternService CreateService()
    {
        var catalog = new CatalogService(_catalogStore, _clock);
        return new ShoppingPatternService(_patternStore, catalog, _clock);
    }

    [Fact]
    public async Task GetOrCreateAsync_CreatesDefaultPattern()
    {
        var pattern = await CreateService().GetOrCreateAsync();

        Assert.Equal(StorageSchema.DefaultPatternId, pattern.Id);
        Assert.Empty(pattern.Items);
    }

    [Fact]
    public async Task AddAndRemoveProduct_UpdatesPattern()
    {
        var catalog = new CatalogService(_catalogStore, _clock);
        var product = await catalog.CreateProductAsync(new Product { Name = "Arroz" });
        var service = new ShoppingPatternService(_patternStore, catalog, _clock);

        await service.AddProductAsync(product.Id, defaultQuantity: 2);
        var lines = await service.GetLinesAsync();

        Assert.Single(lines);
        Assert.Equal(2, lines[0].DefaultQuantity);

        await service.RemoveProductAsync(product.Id);
        Assert.Empty(await service.GetLinesAsync());
    }
}

using SaborMercado.Shared.StarterCatalog;

namespace SaborMercado.Api.Tests;

public class StarterCatalogProviderTests
{
    [Fact]
    public void GetCatalog_LoadsEmbeddedJson()
    {
        var provider = new StarterCatalogProvider();
        var catalog = provider.GetCatalog();

        Assert.Equal(1, catalog.Version);
        Assert.NotEmpty(catalog.Stores);
        Assert.Contains(catalog.Products, p => p.Key == "oleo-soja");
    }
}

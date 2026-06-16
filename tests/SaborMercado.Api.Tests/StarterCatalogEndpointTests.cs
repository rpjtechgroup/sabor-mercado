using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SaborMercado.Shared.StarterCatalog;

namespace SaborMercado.Api.Tests;

public class StarterCatalogEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StarterCatalogEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory;

    [Fact]
    public async Task GetStarterCatalog_ReturnsCuratedJson()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/starter-catalog");
        response.EnsureSuccessStatusCode();

        var catalog = await response.Content.ReadFromJsonAsync<StarterCatalogDto>();
        Assert.NotNull(catalog);
        Assert.Equal(1, catalog.Version);
        Assert.Equal("pt-BR", catalog.Locale);
        Assert.NotEmpty(catalog.Stores);
        Assert.NotEmpty(catalog.Products);
        Assert.Contains(catalog.Stores, s => s.Key == "carrefour");
        Assert.Contains(catalog.Products, p => p.Key == "oleo-soja");
    }
}

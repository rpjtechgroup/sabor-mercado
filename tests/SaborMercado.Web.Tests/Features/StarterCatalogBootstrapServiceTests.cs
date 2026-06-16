using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SaborMercado.Shared.StarterCatalog;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Infrastructure;
using SaborMercado.Web.Interop;
using SaborMercado.Web.Shared;
using SaborMercado.Web.Storage;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Features;

public class StarterCatalogBootstrapServiceTests
{
    private static readonly DateTimeOffset T0 = new(2026, 6, 16, 12, 0, 0, TimeSpan.Zero);

  private static readonly StarterCatalogDto SampleCatalog = new()
    {
        Version = 1,
        Locale = "pt-BR",
        Stores =
        [
            new StarterStoreDto { Key = "carrefour", Name = "Carrefour" },
        ],
        Products =
        [
            new StarterProductDto
            {
                Key = "oleo-soja",
                Name = "Óleo de Soja",
                Category = "Mercearia",
                QuantityValue = 900,
                QuantityUnit = "ml",
                DefaultStoreKey = "carrefour",
            },
        ],
    };

    [Fact]
    public async Task ImportAsync_PopulatesStoresAndProducts()
    {
        var harness = CreateService(SampleCatalog);

        var result = await harness.Bootstrap.ImportAsync(showToast: false);

        Assert.Equal(1, result.StoresAdded);
        Assert.Equal(1, result.ProductsAdded);
        Assert.Single(harness.Stores.Stores);
        Assert.Single(harness.Catalog.Products);
        Assert.Equal("carrefour", harness.Stores.Stores[0].StarterKey);
        Assert.Equal("oleo-soja", harness.Catalog.Products[0].StarterKey);
    }

    [Fact]
    public async Task ImportAsync_SecondRun_IsIdempotent()
    {
        var harness = CreateService(SampleCatalog);
        await harness.Bootstrap.ImportAsync(showToast: false);

        var second = await harness.Bootstrap.ImportAsync(showToast: false);

        Assert.Equal(0, second.StoresAdded);
        Assert.Equal(0, second.ProductsAdded);
        Assert.Single(harness.Catalog.Products);
    }

    [Fact]
    public async Task TryImportIfNeededAsync_WhenCatalogNotEmpty_SkipsImport()
    {
        var harness = CreateService(SampleCatalog);
        await harness.Stores.CreateStoreAsync(new SaborMercado.Web.Domain.Catalog.Store { Name = "Manual" });

        var result = await harness.Bootstrap.TryImportIfNeededAsync();

        Assert.Null(result);
        Assert.Empty(harness.Catalog.Products);
    }

    private BootstrapHarness CreateService(StarterCatalogDto sample)
    {
        var storeStore = new InMemoryStoreStore();
        var catalogStore = new InMemoryCatalogStore();
        var clock = new FixedTimeProvider(T0);
        var stores = new StoreService(storeStore, catalogStore, clock);
        var catalogService = new CatalogService(catalogStore, stores, clock);
        var json = JsonSerializer.Serialize(sample);
        var staticClient = new HttpClient(new StaticJsonHandler(json))
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        var apiClient = new SaborMercadoApiClient(
            new HttpClient(new NotFoundHandler()) { BaseAddress = new Uri("http://api.test/") },
            new InMemoryPreferencesStore());
        var bootstrap = new StarterCatalogBootstrapService(
            stores,
            catalogService,
            storeStore,
            catalogStore,
            apiClient,
            staticClient,
            new InMemoryLocalStorageInterop(),
            new ToastService(clock),
            clock);

        return new BootstrapHarness(bootstrap, stores, catalogService);
    }

    private sealed record BootstrapHarness(
        StarterCatalogBootstrapService Bootstrap,
        StoreService Stores,
        CatalogService Catalog);

    private sealed class StaticJsonHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
    }

    private sealed class NotFoundHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}

using System.Text.Json;
using SaborMercado.Shared.StarterCatalog;
using SaborMercado.Web.Domain;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Infrastructure;
using SaborMercado.Web.Interop;
using SaborMercado.Web.Shared;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Catalog;

public sealed class StarterCatalogBootstrapService(
    StoreService stores,
    CatalogService catalog,
    IStoreStore storeStore,
    ICatalogStore catalogStore,
    SaborMercadoApiClient apiClient,
    HttpClient staticHttpClient,
    ILocalStorageInterop localStorage,
    ToastService toast,
    TimeProvider clock)
{
    private const string ImportedVersionKey = "starterCatalogImportedVersion";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public async Task<StarterImportResult?> TryImportIfNeededAsync(CancellationToken cancellationToken = default)
    {
        await stores.InitializeAsync();
        await catalog.InitializeAsync();

        if (!IsCatalogEmpty())
        {
            return null;
        }

        return await ImportAsync(showToast: true, cancellationToken);
    }

    public async Task<StarterImportResult?> TryMergeNewStarterStoresAsync(CancellationToken cancellationToken = default)
    {
        await stores.InitializeAsync();
        await catalog.InitializeAsync();

        if (IsCatalogEmpty())
        {
            return await ImportAsync(showToast: true, cancellationToken);
        }

        return await ImportAsync(showToast: false, cancellationToken);
    }

    public async Task<StarterImportResult> ImportFromDtoAsync(
        StarterCatalogDto dto,
        CancellationToken cancellationToken = default)
    {
        await stores.InitializeAsync();
        await catalog.InitializeAsync();
        return await MergeCatalogAsync(dto, cancellationToken);
    }

    public async Task<StarterImportResult> ImportAsync(
        bool showToast = true,
        CancellationToken cancellationToken = default)
    {
        await stores.InitializeAsync();
        await catalog.InitializeAsync();

        var dto = await LoadCatalogAsync(cancellationToken);
        var result = await MergeCatalogAsync(dto, cancellationToken);

        await localStorage.SetItemAsync(ImportedVersionKey, dto.Version.ToString());

        if (showToast && (result.StoresAdded > 0 || result.ProductsAdded > 0))
        {
            toast.Show(
                $"Catálogo sugerido importado: {result.StoresAdded} comércio(s), {result.ProductsAdded} produto(s).",
                ToastSeverity.Success);
        }

        return result;
    }

    public bool IsCatalogEmpty() =>
        stores.Stores.Count == 0 && catalog.Products.Count == 0;

    private async Task<StarterCatalogDto> LoadCatalogAsync(CancellationToken cancellationToken)
    {
        if (apiClient.IsConfigured)
        {
            try
            {
                var response = await apiClient.SendAsync(
                    HttpMethod.Get,
                    "/api/v1/starter-catalog",
                    requireAuth: false,
                    cancellationToken: cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var fromApi = await apiClient.ReadJsonAsync<StarterCatalogDto>(response, cancellationToken);
                    if (fromApi is not null)
                    {
                        return fromApi;
                    }
                }
            }
            catch
            {
                
            }
        }

        var staticResponse = await staticHttpClient.GetAsync("data/starter-catalog.pt-BR.json", cancellationToken);
        staticResponse.EnsureSuccessStatusCode();
        await using var stream = await staticResponse.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<StarterCatalogDto>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Starter catalog JSON is invalid.");
    }

    private async Task<StarterImportResult> MergeCatalogAsync(
        StarterCatalogDto dto,
        CancellationToken cancellationToken)
    {
        var storeIdByKey = new Dictionary<string, Guid>(StringComparer.Ordinal);
        var storesAdded = 0;
        var productsAdded = 0;

        foreach (var existing in stores.Stores.Where(s => !string.IsNullOrWhiteSpace(s.StarterKey)))
        {
            storeIdByKey[existing.StarterKey!] = existing.Id;
        }

        foreach (var storeDto in dto.Stores)
        {
            if (storeIdByKey.ContainsKey(storeDto.Key))
            {
                continue;
            }

            var store = new Store
            {
                Name = storeDto.Name,
                City = storeDto.City,
                State = storeDto.State,
                StarterKey = storeDto.Key,
                CreatedAt = clock.GetUtcNow(),
            };

            await storeStore.SaveStoreAsync(store);
            storeIdByKey[storeDto.Key] = store.Id;
            storesAdded++;
        }

        if (storesAdded > 0)
        {
            await stores.ReloadAsync();
        }

        var existingProductKeys = catalog.Products
            .Where(p => !string.IsNullOrWhiteSpace(p.StarterKey))
            .Select(p => p.StarterKey!)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var productDto in dto.Products)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (existingProductKeys.Contains(productDto.Key))
            {
                continue;
            }

            if (!storeIdByKey.TryGetValue(productDto.DefaultStoreKey, out var storeId))
            {
                continue;
            }

            var product = new Product
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                QuantityValue = productDto.QuantityValue,
                QuantityUnit = MapQuantityUnit(productDto.QuantityUnit),
                StoreId = storeId,
                StarterKey = productDto.Key,
                CreatedAt = clock.GetUtcNow(),
            };

            await catalogStore.SaveProductAsync(product);
            existingProductKeys.Add(productDto.Key);
            productsAdded++;
        }

        if (productsAdded > 0)
        {
            await catalog.ReloadAsync();
        }

        return new StarterImportResult(storesAdded, productsAdded);
    }

    private static QuantityUnit? MapQuantityUnit(string? unit) => unit?.ToLowerInvariant() switch
    {
        "g" => QuantityUnit.G,
        "kg" => QuantityUnit.Kg,
        "ml" => QuantityUnit.Ml,
        "l" => QuantityUnit.L,
        "un" => QuantityUnit.Un,
        _ => null,
    };
}

public sealed record StarterImportResult(int StoresAdded, int ProductsAdded);

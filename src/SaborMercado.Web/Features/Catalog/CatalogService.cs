using System.Globalization;
using SaborMercado.Web.Domain;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Shared;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Catalog;

public sealed class CatalogService(ICatalogStore store, StoreService stores, TimeProvider clock)
{
    private readonly List<Product> _products = [];
    private bool _initialized;

    public event Action? StateChanged;

    public IReadOnlyList<Product> Products => _products;

    public bool StorageUnavailable { get; private set; }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await stores.InitializeAsync();

        try
        {
            var products = await store.GetAllProductsAsync();
            _products.Clear();
            _products.AddRange(products.OrderBy(p => p.Name, StringComparer.Create(MoneyFormat.Culture, ignoreCase: true)));
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }

        _initialized = true;
        NotifyStateChanged();
    }

    public async Task ReloadAsync()
    {
        await stores.InitializeAsync();

        try
        {
            var products = await store.GetAllProductsAsync();
            _products.Clear();
            _products.AddRange(products.OrderBy(p => p.Name, StringComparer.Create(MoneyFormat.Culture, ignoreCase: true)));
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }

        _initialized = true;
        NotifyStateChanged();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        await stores.InitializeAsync();
        ValidateProduct(product);
        product.Id = Ids.NewId();
        product.CreatedAt = clock.GetUtcNow();

        _products.Add(product);
        SortProducts();
        await PersistProductAsync(product);
        NotifyStateChanged();
        return product;
    }

    public async Task UpdateProductAsync(Product product)
    {
        await stores.InitializeAsync();
        ValidateProduct(product);
        var index = _products.FindIndex(p => p.Id == product.Id);
        if (index < 0)
        {
            throw new InvalidOperationException("Produto não encontrado.");
        }

        _products[index] = product;
        SortProducts();
        await PersistProductAsync(product);
        NotifyStateChanged();
    }

    
    public async Task DeleteProductAsync(Guid productId)
    {
        _products.RemoveAll(p => p.Id == productId);

        try
        {
            await store.DeleteProductAsync(productId);
            await store.DeletePriceRecordsAsync(productId);
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }

        NotifyStateChanged();
    }

    public Product? GetProduct(Guid productId) => _products.FirstOrDefault(p => p.Id == productId);

    public async Task<List<PriceRecord>> GetPriceHistoryAsync(Guid productId)
    {
        try
        {
            var records = await store.GetPriceRecordsAsync(productId);
            StorageUnavailable = false;
            return records.OrderByDescending(r => r.ObservedAt).ToList();
        }
        catch
        {
            StorageUnavailable = true;
            return [];
        }
    }

    public async Task<PriceRecord> AddPriceRecordAsync(
        Guid productId,
        decimal price,
        Guid storeId,
        DateTimeOffset? observedAt = null,
        PriceSource source = PriceSource.Manual)
    {
        await stores.InitializeAsync();
        stores.RequireStore(storeId);

        if (GetProduct(productId) is null)
        {
            throw new InvalidOperationException("Produto não encontrado.");
        }

        if (price < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Preço não pode ser negativo.");
        }

        var record = new PriceRecord
        {
            Id = Ids.NewId(),
            ProductId = productId,
            Price = price,
            StoreId = storeId,
            MarketName = stores.GetStoreName(storeId),
            ObservedAt = observedAt ?? clock.GetUtcNow(),
            Source = source,
        };

        try
        {
            await store.SavePriceRecordAsync(record);
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }

        NotifyStateChanged();
        return record;
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetLastPricesByProductIdAsync()
    {
        await InitializeAsync();
        var result = new Dictionary<Guid, decimal>();

        foreach (var product in _products)
        {
            var last = await GetLastKnownPriceAsync(product.Id);
            if (last is not null)
            {
                result[product.Id] = last.Price;
            }
        }

        return result;
    }

    
    public async Task<PriceRecord?> GetLastKnownPriceAsync(Guid productId)
    {
        var history = await GetPriceHistoryAsync(productId);
        return history.FirstOrDefault();
    }

    public async Task<Product> EnsureProductAsync(ProductSnapshot snapshot, Guid? storeId = null)
    {
        if (string.IsNullOrWhiteSpace(snapshot.Name))
        {
            throw new ArgumentException("Nome do produto é obrigatório.", nameof(snapshot));
        }

        await InitializeAsync();

        var existing = _products.FirstOrDefault(p => ProductIdentity.Matches(p, snapshot));
        if (existing is not null)
        {
            return existing;
        }

        if (storeId is null || storeId == Guid.Empty)
        {
            throw new InvalidOperationException("Selecione um comércio para cadastrar o produto.");
        }

        var product = new Product
        {
            Name = snapshot.Name.Trim(),
            Brand = string.IsNullOrWhiteSpace(snapshot.Brand) ? null : snapshot.Brand.Trim(),
            QuantityValue = snapshot.QuantityValue,
            QuantityUnit = snapshot.QuantityUnit,
            StoreId = storeId.Value,
            CreatedAt = clock.GetUtcNow(),
        };

        return await CreateProductAsync(product);
    }

    public async Task<IReadOnlyList<Product>> SearchProductsAsync(string prefix, int limit = 8)
    {
        await InitializeAsync();

        if (string.IsNullOrWhiteSpace(prefix))
        {
            return _products
                .OrderBy(p => p.Name, StringComparer.Create(MoneyFormat.Culture, CompareOptions.IgnoreCase))
                .Take(limit)
                .ToList();
        }

        var term = prefix.Trim();
        return _products
            .Where(p =>
                p.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                (p.Brand?.Contains(term, StringComparison.CurrentCultureIgnoreCase) ?? false))
            .OrderBy(p => p.Name, StringComparer.Create(MoneyFormat.Culture, CompareOptions.IgnoreCase))
            .Take(limit)
            .ToList();
    }

    public async Task TouchPriceFromPurchaseAsync(
        Guid productId,
        decimal price,
        Guid? storeId,
        DateTimeOffset observedAt)
    {
        if (GetProduct(productId) is null)
        {
            return;
        }

        if (price < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Preço não pode ser negativo.");
        }

        if (storeId is null || storeId == Guid.Empty)
        {
            return;
        }

        var history = await GetPriceHistoryAsync(productId);
        var sameDay = DateOnly.FromDateTime(observedAt.UtcDateTime);
        var duplicate = history.Any(r =>
            DateOnly.FromDateTime(r.ObservedAt.UtcDateTime) == sameDay &&
            r.Price == price &&
            r.StoreId == storeId);

        if (duplicate)
        {
            return;
        }

        await AddPriceRecordAsync(productId, price, storeId.Value, observedAt, PriceSource.Manual);
    }

    private void ValidateProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ArgumentException("Nome do produto é obrigatório.", nameof(product));
        }

        if (product.QuantityValue is <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(product), "Peso/volume deve ser maior que zero.");
        }

        if (product.StoreId == Guid.Empty)
        {
            throw new ArgumentException("Selecione o comércio onde você compra este produto.", nameof(product));
        }

        stores.RequireStore(product.StoreId);
    }

    private void SortProducts() =>
        _products.Sort((a, b) => string.Compare(a.Name, b.Name, MoneyFormat.Culture, CompareOptions.IgnoreCase));

    private async Task PersistProductAsync(Product product)
    {
        try
        {
            await store.SaveProductAsync(product);
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}

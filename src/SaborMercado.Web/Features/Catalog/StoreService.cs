using System.Globalization;
using SaborMercado.Web.Domain;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Shared;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Catalog;

/// <summary>
/// Cadastro local de comércios (F5). Pré-requisito para vincular produtos e preços.
/// </summary>
public sealed class StoreService(IStoreStore store, ICatalogStore catalogStore, TimeProvider clock)
{
    private readonly List<Store> _stores = [];
    private bool _initialized;

    public event Action? StateChanged;

    public IReadOnlyList<Store> Stores => _stores;

    public bool StorageUnavailable { get; private set; }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            var stores = await store.GetAllStoresAsync();
            _stores.Clear();
            _stores.AddRange(stores.OrderBy(s => s.Name, StringComparer.Create(MoneyFormat.Culture, CompareOptions.IgnoreCase)));
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }

        _initialized = true;
        NotifyStateChanged();
    }

    public Store? GetStore(Guid storeId) => _stores.FirstOrDefault(s => s.Id == storeId);

    public string? GetStoreName(Guid? storeId)
    {
        if (storeId is null || storeId == Guid.Empty)
        {
            return null;
        }

        return GetStore(storeId.Value)?.Name;
    }

    public async Task<Store> CreateStoreAsync(Store entity)
    {
        ValidateStore(entity);
        entity.Id = Ids.NewId();
        entity.CreatedAt = clock.GetUtcNow();

        _stores.Add(entity);
        SortStores();
        await PersistStoreAsync(entity);
        NotifyStateChanged();
        return entity;
    }

    public async Task UpdateStoreAsync(Store entity)
    {
        ValidateStore(entity);
        var index = _stores.FindIndex(s => s.Id == entity.Id);
        if (index < 0)
        {
            throw new InvalidOperationException("Comércio não encontrado.");
        }

        _stores[index] = entity;
        SortStores();
        await PersistStoreAsync(entity);
        NotifyStateChanged();
    }

    public async Task DeleteStoreAsync(Guid storeId)
    {
        await InitializeAsync();
        var products = await catalogStore.GetAllProductsAsync();
        if (products.Any(p => p.StoreId == storeId))
        {
            throw new InvalidOperationException(
                "Não é possível excluir: há produtos vinculados a este comércio.");
        }

        _stores.RemoveAll(s => s.Id == storeId);

        try
        {
            await store.DeleteStoreAsync(storeId);
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }

        NotifyStateChanged();
    }

    public void RequireStore(Guid storeId)
    {
        if (GetStore(storeId) is null)
        {
            throw new InvalidOperationException("Comércio não encontrado. Cadastre-o antes de continuar.");
        }
    }

    private static void ValidateStore(Store entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            throw new ArgumentException("Nome do comércio é obrigatório.", nameof(entity));
        }

        if (entity.Latitude is < -90m or > 90m)
        {
            throw new ArgumentOutOfRangeException(nameof(entity), "Latitude deve estar entre -90 e 90.");
        }

        if (entity.Longitude is < -180m or > 180m)
        {
            throw new ArgumentOutOfRangeException(nameof(entity), "Longitude deve estar entre -180 e 180.");
        }
    }

    private void SortStores() =>
        _stores.Sort((a, b) => string.Compare(a.Name, b.Name, MoneyFormat.Culture, CompareOptions.IgnoreCase));

    private async Task PersistStoreAsync(Store entity)
    {
        try
        {
            await store.SaveStoreAsync(entity);
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
